/*
This demo demonstates how to pass a data buffer to and let CHRocodileLib to fill it up with samples.
After connecting to and setting up the device, a data buffer is allocated with proper size and passed over to CHRocodileLib.
During CHRocodileLib filling up the buffer with predifined sample count, the already saved samples are written to a file.
Automatic data buffer filling/saving can be used for the application like scanning. 
Data buffer is used to save the data for one scan.
*/




#include <vector>
#include <string>
#include <fstream>
#include <sstream>
#include <iostream>
#include <chrono>
#include <thread>
#ifdef _WIN32
#include <windows.h>
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
        Res_t nRes = OpenConnection(strConnectionInfo.c_str(), CHR_Multi_Channel_Device, Connection_Synchronous, 0, &hCHR);
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
        // Set sample rate to 2kHz:
        NewCommand(CmdID_Scan_Rate, false, &hCmd);
        AddCommandFloatArg(hCmd, 2000.0f);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting scan rate: " << nRes << std::endl;
        int32_t an[] = {83, 16640, 16641}; // get sample counter, distance and intensity. CLS device can only output 16bit integer data, MPS and DPS can choose to output float data (the signal ID then would be 83 256 257)
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

        // open a buffer to save 1000 samples
        int64_t nSampleCount = 1000;
        int64_t nSize = 0;
        // get the minimum size of the buffer
        // since no buffer is passed to the function, an error is returned by the function. At the same time, minimum required size is written in nSize
        // we use this way to query the minimum size
        ActivateAutoBufferMode(hCHR, nullptr, nSampleCount, &nSize);
        std::cout << "Min buffer size: " << nSize << std::endl;
        // allocate buffer
        std::vector<double> aSamples;
        aSamples.resize(size_t(nSize) / sizeof(double));
        // give over the pointer to target buffer and number of required sample, activate CHRocodileLib to save the data after device setting up to the buffer
        nRes = ActivateAutoBufferMode(hCHR, aSamples.data(), nSampleCount, &nSize);
        if (!CHR_SUCCESS(nRes))
        {
            std::cout << "Error in activating auto buffer: " << nRes << std::endl;
            return -1;
        }
        std::ofstream oStr("tmp.txt");

        std::string str;
        int64_t nSavedSampleCount = 0;
        double *pData = aSamples.data();
        // signal number in each sample
        LibSize_t nSignalNr = size_t(nSize) / sizeof(double) / nSampleCount;
        int32_t nOffset = 0;
        // write the samples which are already saved in the buffer to a file
        do
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
            int64_t nNewCount;
            GetAutoBufferSavedSampleCount(hCHR, &nNewCount);
            // write the newly saved data
            for (auto i = nSavedSampleCount; i < nNewCount; i++)
            {
                str = "";
                str += "\t" + ToString(int(*(pData + nOffset))); // sample counter output by device
                nOffset++;
                for (int j = 1; j < nSignalNr; j++)
                {
                    str += "\t" + ToString(int(*(pData + nOffset))); // output peak signal of all the channels
                    nOffset++;
                }
                oStr << str << "\n";
            }
            nSavedSampleCount = nNewCount;
        } while (nSavedSampleCount < nSampleCount);

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
