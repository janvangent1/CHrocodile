/*
This demo demonstates how to constantly aquire data with CHRocodileLib from multi-channel channel device.
After connecting to the device, firstly a few commands are sent to the device in order to setup device properly.
Afterwards, "GetNextSamples" are used to constantly collect 1000 samples.
This way of collecting data can be used to application like online-inspectrion
*/



#include <vector>
#include <string>
#include <fstream>
#include <sstream>
#include <iostream>
#include <chrono>
#include <thread>
#ifdef _WIN32
#    include <windows.h>
#    include <conio.h> // _kbhit()
#else
#    include <sys/select.h>

// check whether there is console input
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

#include "CHRocodileLib.h"

//vector to save one sample
typedef std::vector<int> CHR_Sample;

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

// This project expects to be invoked like "CHRocodileLibCLSDemo IP:192.168.170.2"
int main(int argc, char *argv[])
{
    try
    {
        //Set log directory as current working directory, maximum file size is 500KB,
        //maximum 50 log files, the oldest log file will be automatically deleted.
        //It is not a "must" to set log directory, but it is easier to find problem later
#ifdef _WIN32
        SetLibLogFileDirectory(L".", 500, 50);
#else
        SetLibLogFileDirectory(".", 500, 50);
#endif

        std::string strConnectionInfo = "192.168.170.2";
        if (argc > 1)
        {
            strConnectionInfo = argv[1];
        }
        std::cout << "Opening CLS/MPS device " << strConnectionInfo << std::endl;

        Conn_h hCHR;
        // Open connection in synchronous mode, CLS/MPS is connected through ethernet
        //Device buffer size needs to be power of 2, when device buffer size is 0, Lib takes the default buffersize: 32MB
        Res_t nRes = OpenConnection(strConnectionInfo.c_str(), CHR_Multi_Channel_Device, Connection_Synchronous, 512*1024*1024, &hCHR);
        if (!CHR_SUCCESS(nRes))
        {
            std::cout << "Error in connecting to device: " << strConnectionInfo << std::endl;
            return -1;
        }

        Cmd_h hCmd;
        //Since we do not need the data, stop data stream to set device parameters
        NewCommand(CmdID_Stop_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in stopping data stream: " << nRes << std::endl;
        // set to only detect one peaks
        NewCommand(CmdID_Number_Of_Peaks, false, &hCmd);
        AddCommandIntArg(hCmd, 1);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting number of peaks: " << nRes << std::endl;
        // Select optical probe
        NewCommand(CmdID_Optical_Probe, false, &hCmd);
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting optical probe: " << nRes << std::endl;
        // Set sample rate to 1kHz:
        NewCommand(CmdID_Scan_Rate, false, &hCmd);
        AddCommandFloatArg(hCmd, 1000.0f);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting scan rate: " << nRes << std::endl;
        int32_t an[] = {83, 16640, 16641}; // get sample counter, distance and intensity. CLS(2) device can only output 16bit integer data, MPS and DPS can choose to output float data (the signal ID then would be 83 256 257)
        NewCommand(CmdID_Output_Signals, false, &hCmd);
        AddCommandIntArrayArg(hCmd, an, 3);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting output signals: " << nRes << std::endl;
        //Start data stream again
        NewCommand(CmdID_Start_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in starting data stream: " << nRes << std::endl;


        // begin to collect all the data after device setting up
        // here to demonstrate, write the collected data to a file (for CLS2 device, writing to a file may be too slow)
        std::ofstream oStr("tmp.txt");

        const double *p = nullptr;
        LibSize_t nSampleSize;
        TSampleSignalGeneralInfo sGenInfo;
        TSampleSignalInfo *pSignalInfo;
        LibSize_t nSampleCount = 1000;
        while (nSampleCount>0)
        {
            // jump out of loop, when a key is pressed
            if (_kbhit())
                break;

            // Read next samples
            // Every sample comes as an array of double.
            // try to get maximum 100 samples (this number can be defined differently, here is only an arbitary number) samples.
            int64_t nCount = nSampleCount;
            if (nCount>100)
                nCount = 100;
            nRes = GetNextSamples(hCHR, &nCount, &p, &nSampleSize, &sGenInfo, &pSignalInfo);
            // no error appears
            if (CHR_SUCCESS(nRes))
            {
                // if get new sample, output the sample
                if (nCount > 0)
                {
                    std::string str;
                    for (int32_t k = 0; k < nCount; k++)
                    {
                        str = "";
                        // for every signal, output
                        // first is the global signal in the double array,  in this case is the sample Counter;
                        for (int32_t i = 0; i < sGenInfo.GlobalSignalCount; i++)
                        {
                            str += "\t" + ToString(*(p++)); // sample counter output by device, convert to int, because it is integer
                        }

                        // then for all the channels read out distance and intenstiy, ordered integer type
                        for (int32_t i = 0; i < sGenInfo.ChannelCount; i++)
                        {
                            str += "\t" + ToString(*(p++));     // distance
                            str += "\t" + ToString(*(p++));     // intensity
                        }
                        oStr << str << "\n";
                    }
                    nSampleCount -= nCount;
                }
                // if not enough new sample, wait
                if (nRes == Read_Data_Not_Enough)
                {
                    std::this_thread::sleep_for(std::chrono::milliseconds(1));
                }
            }
            // error in getting new sample...
            else
            {
                throw std::runtime_error("Error in getting CHR sample!");
            }
        }
        std::cout << "Finished. Please check tmp.txt" << std::endl;
        oStr.close();
        CloseConnection(hCHR);
    }
    catch (std::runtime_error &_e)
    {
        std::cout << "Terminating: " << _e.what() << std::endl;
        return -1;
    }
}
