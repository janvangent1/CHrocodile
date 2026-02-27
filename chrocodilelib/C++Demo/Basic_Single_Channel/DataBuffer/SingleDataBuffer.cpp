/*
This demo demonstates how to pass a data buffer to and let CHRocodileLib to fill it up with samples.
After connecting to and setting up the device, a data buffer is allocated with proper size and passed over to CHRocodileLib.
During CHRocodileLib filling up the buffer with predifined sample count, the already saved samples are written to a file.
Automatic data buffer filling/saving can be used for the application like scanning. 
Data buffer is used to save the data for one scan.
*/

#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <chrono>
#include <thread>
#include <vector>
#ifdef _WIN32
#include <windows.h>
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
        //Start data stream again
        NewCommand(CmdID_Start_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in starting data stream: " << nRes << std::endl;

        std::cout << "Parametrized." << std::endl;


        //open a buffer to save 10000 samples
        int64_t nSampleCount = 10000;
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
        int32_t nOffset = 0;
        // write the samples which are already saved in the buffer to a file
        do
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
            int64_t nNewCount;
            GetAutoBufferSavedSampleCount(hCHR, &nNewCount);
            //write the newly saved data
            for (auto i=nSavedSampleCount; i<nNewCount; i++)
            {
                str = "";
                str += "\t" + ToString(int32_t(*(pData + nOffset))); // sample counter output by device
                nOffset++;
                str += "\t" + ToString(*(pData + nOffset)); // distance in micrometer
                nOffset++;
                str += "\t" + ToString(*(pData + nOffset)); // intensity in percent
                nOffset++;
                str += "\t" + ToString(nSampleCount - i); // number of the samples still need to get
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
