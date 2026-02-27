/*
This demo demonstates how to constantly aquire data with CHRocodileLib from single channel device.
After connecting to the device, firstly a few commands are sent to the device in order to setup device properly.
Afterwards, "GetNextSamples" are used to constantly collect 10000 samples.
This way of collecting data can be used to application like online-inspectrion
*/

#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
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

// This project expects to be invoked like "CHRocodileLibDemo COMx"
// With the COM port passed as command line argument.
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

        std::string strConnectionInfo = "COM1";
        if (argc > 1)
        {
            strConnectionInfo = argv[1];
        }
        std::cout << "Opening CHRocodile " << strConnectionInfo << std::endl;

        Conn_h hCHR;
        // Open connection in synchronous mode
        //here when connection setting is based on serial connection, program will regard device as first generation CHRocodile device
        //otherwise CHR² device
        //Device buffer size needs to be power of 2, when device buffer size is 0, Lib takes the default buffersize: 32MB
        Res_t nRes = 0;
        if (strConnectionInfo.find("COM") != std::string::npos)
            nRes = OpenConnection(strConnectionInfo.c_str(), CHR_1_Device, Connection_Synchronous, 0, &hCHR); //buffer size 0, use default buffer size 32MB
        else
            nRes = OpenConnection(strConnectionInfo.c_str(), CHR_2_Device, Connection_Synchronous, 0, &hCHR); //buffer size 0, use default buffer size 32MB
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
            std::cout << "Error in stopping data stream: "<< nRes << std::endl;
        // Set measurement mode to confocal mode:
        NewCommand(CmdID_Measuring_Method, false, &hCmd);
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting measuring mode: " << nRes << std::endl;
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
        // Set sample rate to 4kHz:
        NewCommand(CmdID_Scan_Rate, false, &hCmd);
        AddCommandFloatArg(hCmd, 4000.0f);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting scan rate: " << nRes << std::endl;
        // Set Averaging to 1 (data written to file):
        NewCommand(CmdID_Data_Average, false, &hCmd);
        AddCommandIntArg(hCmd, 1);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting data average: " << nRes << std::endl;
        int32_t an[] = {83, 256, 257}; // get sample counter, distance and intensity
        NewCommand(CmdID_Output_Signals, false, &hCmd);
        AddCommandIntArrayArg(hCmd, an, 3);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting output signals: " << nRes << std::endl;
        //after setting up device, start data stream again
        NewCommand(CmdID_Start_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in starting data stream: " << nRes << std::endl;

        std::cout << "Parametrized." << std::endl;

        //begin to collect all the data after device setting up
        //here to demonstrate, write the collected data to a file (if the sample rate is high, writing to a file may be too slow)
        std::ofstream oStr("tmp.txt");

        const double *p = nullptr;
        LibSize_t nSampleSize;
        TSampleSignalGeneralInfo sGenInfo;
        TSampleSignalInfo *pSignalInfo;
        LibSize_t nSampleCount = 10000;
        while (nSampleCount>0)
        {
            //jump out of loop, when a key is pressed
            if (_kbhit())
                break;
            
            // Read next samples
            // Every sample comes as an array of double.
            // try to get maximum 1000 samples (this number can be defined differently, here is only an arbitary number) samples.
            int64_t nCount = nSampleCount;
            if (nSampleCount > 1000)
                nCount = 1000;
            nRes = GetNextSamples(hCHR, &nCount, &p, &nSampleSize, &sGenInfo, &pSignalInfo);
            // no error appears
            if (CHR_SUCCESS(nRes))
            {
                //if get new sample, output the sample
                if (nCount > 0)
                {
                    std::string str;
                    for (int32_t i = 0; i < nCount; i++)
                    {
                        str = "";
                        //for every signal, output 
                        for (int32_t j=0; j<sGenInfo.GlobalSignalCount+sGenInfo.PeakSignalCount; j++)
                             str += "\t" + ToString(*(p++));          
                        oStr << str << "\n";
                    }
                    nSampleCount -= nCount;
                }
                //if not enough new sample, wait
                if (nRes == Read_Data_Not_Enough)
                {
                    std::this_thread::sleep_for(std::chrono::milliseconds(1));
                }
            }
            //error in getting new sample...
            else
            {
                throw std::runtime_error("Error in getting CHR sample!");
            }
        }
        oStr.close();
        std::cout << "Finished. Please check tmp.txt" << std::endl;       
        CloseConnection(hCHR);
    }
    catch (std::runtime_error &_e)
    {
        std::cout << "Terminating: " << _e.what() << std::endl;
        return -1;
    }
}
