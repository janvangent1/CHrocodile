/*
This demo illustates the use of shared connection for one CHR device.
Two connections(clients) for one CHR device are opened up with CHRocodileLib.
One connection sends THR command and read data/response from CHR device asynchronously.
One connection synchronously downloads spectrums constantly.
Demo shows athat the two connections works independent from each other well.
Of course the response from THR and spectrum downloading commands are sends to both connections.
*/

#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <ctime>
#include <mutex>
#include <memory>
#include <chrono>
#include <thread>
#include <vector>
#include <atomic>

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

#include "../../libcore/include/CHRocodileLib.h"

// int, float, double to string:
template<typename T>
static inline std::string ToString(const T &_val)
{
    std::stringstream oStr;
    oStr << _val;
    if (oStr.fail())
        throw std::runtime_error("string conversion failed.");
    return oStr.str();
}

uint32_t hCHR, hCHR2;

// tmpMultiAll.txt saves all the responses first connection receives
std::ofstream oStrAll("tmpMultiAll.txt");
// tmpMultiMissingSample.txt saves the information of the missing samples of the first connection (if it ever happens)
std::ofstream oStrSample("tmpMultiMissingSample.txt");
// tmpMultiSpec.txt saves the spectrum length from the downloaded spectrum of the second second connection
std::ofstream oStrSpec("tmpMultiSpec.txt");

std::mutex g_oCinCS;

int32_t Time = -1;
std::atomic_bool Cancel{false};

//General call back function, it is called whenever a response has been received from the device
void GenCBFct(const TRspCallbackInfo _sInfo, Rsp_h _hRsp)
{
    std::ofstream &oStr = *(std::ofstream *)_sInfo.User;

    TResponseInfo sRspInfo;
    GetResponseInfo(_hRsp, &sRspInfo);

    // if it is confocal detection threadshold command, get and save the threshold
    if (sRspInfo.CmdID == CmdID_Confocal_Detection_Threshold)
    {
        float nTHR;
        GetResponseFloatArg(_hRsp, 0, &nTHR);
        oStr << ": Confocal Detection Threashold: " << nTHR << "; Ticket: " << _sInfo.Ticket << "\n";
    }
    // if it is other command, save general response information
    else
    {
        LibSize_t nSize = 0;
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
            oStr  << ": Get Response: " << strRsp << " Ticket: " << _sInfo.Ticket << " with error \n";
        else
            oStr  << ": Get Response: " << strRsp << "; Ticket: " << _sInfo.Ticket << "\n";
    }
    oStr.flush();
}

// Data sample call back
void ReceiveSampleCBFct(void *_pContext, int32_t _nStatus, int64_t _nSampleCount,
                        const double *_pSampleBuffer, LibSize_t _nSizePerSample, TSampleSignalGeneralInfo _sGenInfo, TSampleSignalInfo *)
{
    int nCurrentCounter;
    static int nOldCounter = -1;
    std::string str;
    if ((_nSampleCount == 0) || (_nSizePerSample == 0))
        return;
    const double *p = _pSampleBuffer;
    std::ofstream *oStr = (std::ofstream *)_pContext;
    int nSignalCount = _sGenInfo.GlobalSignalCount + _sGenInfo.PeakSignalCount * _sGenInfo.ChannelCount;
    if ((_nSampleCount > 0) && (nSignalCount > 0))
    {
        for (int j = 0; j < _nSampleCount; j++)
        {
            //first signal is sample counter
            nCurrentCounter = int(*p);
            //if sample count jumps in between, there is a sample(s) missing, save the information
            if ((nOldCounter != -1) && (nCurrentCounter - nOldCounter > 1))
            {
                str = ToString(*(p));
                str += "\t" + ToString(nCurrentCounter) + "\t" + ToString(nOldCounter) + "\t"; // sample counter output by device
                *oStr << str << "\n";
                std::cout << "Missing Sample: " << str << "Signal Counter: " << nSignalCount << std::endl;
            }
            nOldCounter = nCurrentCounter;
            p += nSignalCount;
        }
    }

    if (CHR_WARNING(_nStatus))
        std::cout << "Lib auto flushed input buffer!! " << std::endl;
    else if (CHR_ERROR(_nStatus))
    {
        std::cout << "Error in ProcessDeviceOutput" << std::endl;
    }
}

// The thread used to send THR command asynchronously to device
void CHRWriteThread()
{
    //Setup device
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

    float nTHR = 1;
    bool bQuery = false;

    //send dummy confocal detection threshold command
    while (true)
    {
        //input any key, terminate thread
        std::unique_lock<std::mutex> uqlock(g_oCinCS);
        if (_kbhit())
            break;
        uqlock.unlock();

        if (Cancel)
            break;

        Cmd_h hCmd;
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
    std::cout << "Quit write thread 1 " << std::endl;
}



// the thread used to download spectrum synchronously
void CHRWriteThread2()
{
    while (true)
    {
        //input any key, terminate thread
        std::unique_lock<std::mutex> uqlock(g_oCinCS);
        if (_kbhit())
            break;
        uqlock.unlock();

        if (Cancel)
            break;

        Cmd_h hCmd;
        Rsp_h hRsp;
        // download spectrum, spectrum type: 0, raw spectrum; 1, processed spectrum in confocal mode; 2, FT spectrum;
        const int8_t *pTemp = nullptr;
        int32_t nLength = 0;
        // download spectrum from normal CHRocodile device or CHRocodile² device
        NewCommand(CmdID_Download_Spectrum, false, &hCmd);
        // download confocal spectrum
        AddCommandIntArg(hCmd, Spectrum_Confocal);
        auto nRes = ExecCommand(hCHR2, hCmd, &hRsp);
        // spectrum download response, parameters:
        // the first: spectrum type, second: start channel index, third: channel count
        // fourth: exposure number, fifth: micromters per bin (interferometric mode),
        // sixth: Block exponent of spectrum data, seventh: spectrum data
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 6, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in spectrum downloading!" << std::endl;
        else
        {
            // Spectrum is in short format
            int32_t nSpecLength = nLength / 2;
            oStrSpec << "Receive spectrum length: " << ToString(nSpecLength) << "\n";
        }
        std::this_thread::sleep_for(std::chrono::milliseconds(rand() % 50 + 50));
    }
    std::cout << "Quit write thread 2 " << std::endl;
}

int main(int argc, char *argv[])
{
    std::string strConnectionInfo = "COM1";
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

#ifdef _WIN32
        SetLibLogFileDirectory(L".", 500, 50);
#else
        SetLibLogFileDirectory(".", 500, 50);
#endif

    //Open first connection in asynchronous mode
    int nRes = 0;
    if (strConnectionInfo.find("COM") != std::string::npos)
        nRes = OpenConnection(strConnectionInfo.c_str(), CHR_1_Device, Connection_Asynchronous, 0, &hCHR);
    else
        nRes = OpenConnection(strConnectionInfo.c_str(), CHR_2_Device, Connection_Asynchronous, 0, &hCHR);
    if (nRes < 0)
    {
        std::cout << "Error in connecting to device: " << strConnectionInfo << std::endl;
        return -1;
    }
    // register call back function for the first connection
    RegisterGeneralResponseAndUpdateCallback(hCHR, &oStrAll, GenCBFct);
    RegisterSampleDataCallback(hCHR, 5000, 10, &oStrSample, &ReceiveSampleCBFct);
    // set up DLL to automatically process device output, i.e. all the callback functions are called automatically from DLL
    StartAutomaticDeviceOutputProcessing(hCHR);

    // Open up the second connection based on the first connection (shared connection),
    // this connection downloads spectrum synchronously, therefore is in synchronous mode
    nRes = OpenSharedConnection(hCHR, Connection_Synchronous, &hCHR2);
    if (nRes < 0)
    {
        std::cout << "Error in extra connecting to device: " << std::endl;
        return -1;
    }

    //"CHRReadThread" reads CHR device output
    //"CHRWriteThread" sends commands to the device
    std::vector<std::thread> aThreads;
    aThreads.push_back(std::thread(CHRWriteThread));
    aThreads.push_back(std::thread(CHRWriteThread2));
    

    // Wait until all threads have terminated//
    if (Time > 0)
    {
        // if time is set, automatic stop the demo
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
    CloseConnection(hCHR2);

    return 0;
}
