/*
AsyncDemo2 sets CHRocodileLib to automatically process CHR device output in asynchronous mode. 
After the device initialisation is finished, a user buffer is passed to the DLL, instead of DLL internal buffer, for saving data.
In this way, user could let DLL directly write sample data to their specified buffer under asynchronous mode. 
He does not need to copy over the data delivered by the sample callback function any more. 
In this case, Sample callback function gives back current data position in the user buffer and last read sample data count.
Here if the user buffer is full, i.e. Sample callback with Read_Data_User_Buf_Full as status, user buffer will be re-passed over to the DLL 
and DLL rewrites the buffer from beginning with the new data.
*/

#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <atomic>
#include <mutex>
#include <memory>
#include <thread>
#include <vector>
#include <cmath>
#include <atomic>

#include "CHRocodileLib.h"

#ifdef _WIN32
#    include <windows.h>
#    include <conio.h> // _kbhit()
#else
#    include <sys/select.h>

//check whether there is console input
int _kbhit()
{
    timeval tv;
    fd_set readfd;
    tv.tv_sec = 0;
    tv.tv_usec = 0;
    FD_ZERO(&readfd);
    FD_SET(0, &readfd);
    return select(1, &readfd, nullptr, nullptr, &tv);
}
#endif

std::ofstream oStr("AsyncDemo2.txt");
int32_t Time = -1;
std::atomic_bool Cancel{false};

// int, float, double to string:
template<typename T>
static inline std::string ToString(const T &_val)
{
    std::stringstream ostm;
    ostm << _val;
    if (ostm.fail())
        throw std::runtime_error("string conversion failed.");
    return ostm.str();
}

Conn_h hCHR;
//Flag whether the device has been setup
bool DeviceInit = false;
//Ticket of the last command by device setup
std::atomic<int32_t> SetUpCmdTicket(-1);
std::vector<double> UserBuffer;
int64_t SampleCount = 4000;

//response general call back function
void GenCBFct(const TRspCallbackInfo _sInfo, Rsp_h _hRsp)
{
    TResponseInfo sRspInfo;
    GetResponseInfo(_hRsp, &sRspInfo);

    // if the response of the last command of the device setup stage is there, device setup is finished
    if (!DeviceInit)
    {
        DeviceInit = sRspInfo.Ticket == SetUpCmdTicket;
        if (DeviceInit)
        {
            std::cout << "Finish Device Initialization." << std::endl;
            //after finishing device initialization, use extern buffer to save data
            LibSize_t nSize = 0;
            //get necessary buffer size
            ActivateAsyncDataUserBuffer(hCHR, nullptr, SampleCount, &nSize);
            nSize = static_cast<LibSize_t>(ceil(double(nSize) / sizeof(double)));
            UserBuffer.resize(nSize);
            //set extern buffer
            ActivateAsyncDataUserBuffer(hCHR, UserBuffer.data(), SampleCount, nullptr);
        }
    }

    //save response information
    LibSize_t nSize{0};
    std::string strRsp;
    ResponseToString(_hRsp, nullptr, &nSize);
    if (nSize > 0)
    {
        std::vector<char> aRsp;
        aRsp.resize(nSize);
        ResponseToString(_hRsp, aRsp.data(), &nSize);
        strRsp = aRsp.data();
    }
    if (*_sInfo.State == -1)
        oStr << "Get Response: " << strRsp << "Ticket: " << _sInfo.Ticket << " with error \n";
    else
        oStr << "Get Response: " << strRsp << "; Ticket: " << _sInfo.Ticket << "\n";
    oStr.flush();
}

// data sample call back function
void OnReceiveSample(void * /*_pUser */, int32_t _nStatus, int64_t _nSampleCount, const double *_pSampleBuffer,
                     LibSize_t _nSizePerSample, TSampleSignalGeneralInfo _sGenInfo, TSampleSignalInfo *)
{
    //after device is properly setup, begin to save the data
    if (DeviceInit)
    {    
        //user buffer is full
        if (_nStatus == Read_Data_User_Buf_Full)
        {           
            //user could choose here to process the data inside the buffer
            //here to demonstate to write every signal to the file, however for high scan rate, it could be too slow.
            std::string str;
            int nSigCount = _sGenInfo.GlobalSignalCount + _sGenInfo.PeakSignalCount * _sGenInfo.ChannelCount;
            double *pData = UserBuffer.data();
            for (int32_t j = 0; j < SampleCount; j++)
            {
                str = "";
                for (int32_t k = 0; k < nSigCount; k++)
                    str += "\t" + ToString(*(pData++)); // intensity in percent
                oStr << str << "\n";
            }
            // reset the buffer saving again
            ActivateAsyncDataUserBuffer(hCHR, UserBuffer.data(), SampleCount, nullptr);
        }
    }

    if (CHR_WARNING(_nStatus))
        std::cout << "Lib auto flushed input buffer!! " << std::endl;
    else if (CHR_ERROR(_nStatus))
    {
        std::cout << "Error in ProcessDeviceOutput" << std::endl;
    }
}

//thread used to constantly send command to the device
void CHRWriteThread()
{
    //setup device first
    Cmd_h hCmd;
    NewCommand(CmdID_Measuring_Method, false, &hCmd);
    AddCommandIntArg(hCmd, 0);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
    NewCommand(CmdID_Number_Of_Peaks, false, &hCmd);
    AddCommandIntArg(hCmd, 1);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
    NewCommand(CmdID_Scan_Rate, false, &hCmd);
    AddCommandFloatArg(hCmd, 4000.0f);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
    NewCommand(CmdID_Data_Average, false, &hCmd);
    AddCommandIntArg(hCmd, 1);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
    int32_t an[] = {83, 256, 257}; // order sample counter, distance and intensity signal from CHR device
    int32_t nTicket;
    NewCommand(CmdID_Output_Signals, false, &hCmd);
    AddCommandIntArrayArg(hCmd, an, 3);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, &nTicket);
    // last command of the setup stage
    SetUpCmdTicket = nTicket;
    while (true)
    {
        if (_kbhit())
            break;
        if (Cancel)
            break;
        std::this_thread::sleep_for(std::chrono::seconds(1));
    }
    std::cout << "Quit write thread " << std::endl;
}

int main(int argc, char *argv[])
{
    //Set DLL log directory. It is not a "must" to set log directory, but it is easier to find problem later
#ifdef _WIN32
    SetLibLogFileDirectory(L".", 500, 50);
#else
    SetLibLogFileDirectory(".", 500, 50);
#endif

    std::string strConnectionInfo = "192.168.170.2";

    // the first parameter is connection info
    if (argc > 1)
    {
        strConnectionInfo = argv[1];
    }
    // second parameter is execute time in seconds
    if (argc > 2)
    {
        std::string strTemp = argv[2];
        try
        {
            int32_t nTemp = std::stoi(strTemp);
            Time = nTemp;
        }
        catch (...)
        {
        }
    }


    std::cout << "Opening CHRocodile " << strConnectionInfo << std::endl;

    // Open connection in asynchronous mode
    //here when connection setting is based on serial connection, program will regard device as first generation CHRocodile device
    //otherwise CHR² device
    //Device buffer size needs to be power of 2, when device buffer size is 0, Lib takes the default buffersize: 32MB
    Res_t Res;
    if (strConnectionInfo.find("COM") != std::string::npos)
        Res = OpenConnection(strConnectionInfo.c_str(), CHR_1_Device, Connection_Asynchronous, 0, &hCHR);
    else
        Res = OpenConnection(strConnectionInfo.c_str(), CHR_2_Device, Connection_Asynchronous, 0, &hCHR);

    if (!CHR_SUCCESS(Res))
    {
        std::cout << "Error in connecting to device: " << strConnectionInfo << std::endl;
        return -1;
    }
    //register call back functions
    //all command responses/updates from the CHR device are delievered from Lib to function "GenCBFct"
    RegisterGeneralResponseAndUpdateCallback(hCHR, nullptr, &GenCBFct);
    //Data from  the CHR device is delivered to function "OnReceiveSample"
    RegisterSampleDataCallback(hCHR, 4000, 10, nullptr, &OnReceiveSample);

    //Trigger Lib to automatically process device output
    StartAutomaticDeviceOutputProcessing(hCHR);

    std::thread oThread(CHRWriteThread);
    
    if (Time > 0)
    {
        // if time is set, automatic stop the demo
        std::this_thread::sleep_for(std::chrono::seconds(Time));
        Cancel = true;
    }
    else
        std::this_thread::sleep_for(std::chrono::seconds(1));

    if (oThread.joinable())
        oThread.join();

    CloseConnection(hCHR);

    return 0;
}
