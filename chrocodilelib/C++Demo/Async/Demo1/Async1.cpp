/*
AsyncDemo1 uses two threads. The first thread constantly sends commands to the device without waiting for the responses. 
Second thread reads data and command responses from the device by actively calling function "ProcessDeviceOutput".
Data and responses/updates are both delievered through calllback functions.
The first thread can also be regarded as the up stream to the device and the second thread as the down stream from the device.
A software, which is used to monitor CHR device and occasionaly sets device parameters, can use the similar routines as in this demo.
This demo will run indefinitely.
*/
#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <vector>
#include <mutex>
#include <memory>
#include <thread>
#include <atomic>
#include "../../../libcore/include/CHRocodileLib.h"
#ifdef _WIN32
#include <windows.h>
#include <conio.h> // _kbhit()
#else
#include <sys/select.h>

//check whether there is console input
int _kbhit() {
  timeval tv;
  fd_set readfd;
  tv.tv_sec = 0;
  tv.tv_usec = 0;
  FD_ZERO( &readfd );
  FD_SET( 0, &readfd );
  return select( 1, &readfd, nullptr, nullptr, &tv );
}
#endif



std::ofstream oStr("AsyncDemo1.txt");
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
std::mutex CSConsole;

//response general call back function
void GenCBFct(const TRspCallbackInfo _sInfo, Rsp_h _hRsp)
{
    TResponseInfo sRspInfo;
    GetResponseInfo(_hRsp, &sRspInfo);

    // if it is confocal detection threadshold command, get and save the threshold
    if (sRspInfo.CmdID == CmdID_Confocal_Detection_Threshold)
    {
        float nTHR;
        GetResponseFloatArg(_hRsp, 0, &nTHR);
        oStr << "Confocal Detection Threashold: " << nTHR << "\n";
    }
    // if it is other command, save general response information
    else
    {
        LibSize_t nSize;
        std::string strRsp;
        ResponseToString(_hRsp, nullptr, &nSize);
        if (nSize>0)
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
    }
}

// data sample call back function
void OnReceiveSample(void * /*_pUser */, int32_t _nStatus, int64_t _nSampleCount, const double *_pSampleBuffer,
                     LibSize_t _nSizePerSample, TSampleSignalGeneralInfo _sGenInfo, TSampleSignalInfo *)
{
    std::string str;
    if ((_nSampleCount > 0) && (_nSizePerSample > 0))
    {
        int nSigCount = _sGenInfo.GlobalSignalCount + _sGenInfo.PeakSignalCount * _sGenInfo.ChannelCount;
        for (int32_t j = 0; j < _nSampleCount; j++)
        {
            str = "";
            //here to demonstate to write every signal to the file, however for high scan rate, it could be too slow. User should put data processing/buffering here
            for (int32_t k = 0; k < nSigCount; k++)
                str += "\t" + ToString(*(_pSampleBuffer++)); // intensity in percent
            oStr << str << "\n";
        }
    }

    if (CHR_WARNING(_nStatus))
        std::cout << "Lib auto flushed input buffer!! " << std::endl;
}

//Thread used to read data sample and reponses from CHR device, both response and data sample are passed over through call back functions
void CHRReadThread()
{
    while (true)
    {
        //input any key, terminate thread
        std::unique_lock<std::mutex> uqlock(CSConsole);
        if (_kbhit())
            break;
        uqlock.unlock();

        if (Cancel)
            break;

        // trigger Lib to process device output
        Res_t nRes = ProcessDeviceOutput(hCHR);
        //not enough data sleep
        if (nRes == Read_Data_Not_Enough)
            std::this_thread::sleep_for(std::chrono::milliseconds(1));
        else if (CHR_ERROR(nRes))
        {
            std::cout << "Error in ProcessDeviceOutput" << std::endl;
            break;
        }
    }
    oStr.close();
    std::cout << "Quit read thread" << std::endl;
}

//thread used to constantly send command to the device
void CHRWriteThread()
{
    //set up related device parameters first
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
    NewCommand(CmdID_Output_Signals, false, &hCmd);
    AddCommandIntArrayArg(hCmd, an, 3);
    ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);

    std::this_thread::sleep_for(std::chrono::milliseconds(10));
    float nTHR = 1.0f;
    bool bQuery = false;
    //here constantly (between 50ms to 100ms) send dummy command "THR" to the device
    while (true)
    {
        //input any key, terminate thread
        std::unique_lock<std::mutex> uqlock(CSConsole);
        if (_kbhit())
            break;
        uqlock.unlock();

        if (Cancel)
            break;

        if (bQuery)
        {
            NewCommand(CmdID_Confocal_Detection_Threshold, true, &hCmd);
            ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
        }
        else
        {
            NewCommand(CmdID_Confocal_Detection_Threshold, false, &hCmd);
            AddCommandFloatArg(hCmd, nTHR);
            ExecCommandAsync(hCHR, hCmd, nullptr, nullptr, nullptr);
            nTHR += 1;
            if (nTHR > 50)
                nTHR = 1;
        }
        bQuery = !bQuery;
        std::this_thread::sleep_for(std::chrono::milliseconds(rand() % 50 + 50));
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

    //the first parameter is connection info
    if (argc > 1)
    {
        strConnectionInfo = argv[1];
    }
    //second parameter is execute time in seconds
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

    //Open connection in asynchronous mode
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

    //"CHRReadThread" reads CHR device output
    //"CHRWriteThread" sends commands to the device
    std::vector<std::thread> aThreads;
    aThreads.push_back(std::thread(CHRReadThread));
    aThreads.push_back(std::thread(CHRWriteThread));
    // Wait until all threads have terminated//
    if (Time > 0)
    {
        //if time is set, automatic stop the demo
        std::this_thread::sleep_for(std::chrono::seconds(Time));
        Cancel = true;
    }
    else
        std::this_thread::sleep_for(std::chrono::seconds(1));
    for (uint32_t i=0; i<aThreads.size(); i++)
    {
        if (aThreads[i].joinable())
            aThreads[i].join();
    }
    CloseConnection(hCHR);
    return 0;
}
