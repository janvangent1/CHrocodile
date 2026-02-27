/*
This demo demonstates how to use CLS2 Calibration plugin
After opening up the connection, the plugin is added to the connection based on the plugin name.
Then calibration file, which is used in the plugin, is set through command.
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
#include "CHRocodileLibPluginDef.h"

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

        std::string strConnectionInfo = "192.168.170.3";
        if (argc > 1)
        {
            strConnectionInfo = argv[1];
        }
        std::cout << "Opening CLS2 device " << strConnectionInfo << std::endl;

        Conn_h hCHR;
        // Open connection in synchronous mode, CLS/MPS is connected through ethernet
        //Here wer set device buffer size to 128MB
        Res_t nRes = OpenConnection(strConnectionInfo.c_str(), 2, Connection_Synchronous, 128*1024*1024, &hCHR);    
        if (!CHR_SUCCESS(nRes))
        {
            std::cout << "Error in connecting to device: " << strConnectionInfo << std::endl;
            return -1;
        }
        //Add CLS2 calibration plugin
        Plugin_h hPlugin;
        AddConnectionPlugIn(hCHR, CLS2_Calib_Plugin_Name, &hPlugin);

        Cmd_h hCmd;
        // Since we do not need the data, stop data stream to set device parameters
        NewCommand(CmdID_Stop_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in stopping data stream: " << nRes << std::endl;
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
        // Set sample rate to 2kHz:
        NewCommand(CmdID_Scan_Rate, false, &hCmd);
        AddCommandFloatArg(hCmd, 2000.0f);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting scan rate: " << nRes << std::endl;
        // Set Averaging to 1 (data written to file):
        NewCommand(CmdID_Data_Average, false, &hCmd);
        AddCommandIntArg(hCmd, 1);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting data average: " << nRes << std::endl;
        int32_t an[] = {83, 16640, 16641}; // get sample counter, distance and intensity
        NewCommand(CmdID_Output_Signals, false, &hCmd);
        AddCommandIntArrayArg(hCmd, an, 3);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting output signals: " << nRes << std::endl;

        //Set Calibration File
        NewCommand(CmdID_CalibFile_Name, false, &hCmd);
        //dummy calibration file, should be real calibration file name
        std::string strFileName = "Calib.csv";
        AddCommandStringArg(hCmd, strFileName.c_str(), static_cast<int32_t>(strFileName.length()));
        //Send command to plugin
        nRes = ExecCommand(hPlugin, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting calibration file: " << nRes << std::endl;
        
        //start data stream
        NewCommand(CmdID_Start_Data_Stream, false, &hCmd);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in starting data stream: " << nRes << std::endl;

        std::cout << "Parametrized." << std::endl;


        std::ofstream oStr("tmp.txt");
        std::string str;
        std::cout << "first acquires 1000 samples, retrieves them one by one and saved to tmp.txt" << std::endl;
        std::cout << "Please wait..." << std::endl;
        oStr << "First 1000 samples, acquired by one by one retrieving"
             << "\n";

        int64_t nSampleCount = 1000;

        //vector to save all 1000 samples
        std::vector<CHR_Sample> aSamples;
        const double *p = nullptr;
        TSampleSignalGeneralInfo sSignalGeneralInfo;
        int64_t nCount;
        while (nSampleCount > 0)
        {
            // Read next samples
            // Every sample comes as an array of double.
            // try to get maximum 100 samples (this number can be defined differently, here is only an arbitary number) samples.
            nCount = nSampleCount;
            if (nCount > 100)
                nCount = 100;
            nRes = GetNextSamples(hCHR, &nCount, &p, nullptr, &sSignalGeneralInfo, nullptr);
            //if get new sample, output the sample
            if (CHR_SUCCESS(nRes))
            {
                if (nCount > 0)
                {
                    for (int32_t j = 0; j < nCount; j++)
                    {
                        CHR_Sample aSampleData;
                        //first is the global signal in the double array,  in this case is the sample Counter;
                        for (int32_t i = 0; i < sSignalGeneralInfo.GlobalSignalCount; i++)
                        {
                            aSampleData.push_back(int(*(p++))); // sample counter output by device, convert to int, because it is integer
                        }

                        //then for all the channels read out distance and intenstiy, ordered integer type
                        for (int32_t i = 0; i < sSignalGeneralInfo.ChannelCount; i++)
                        {
                            //here should be corrected distance
                            aSampleData.push_back(int(*(p++))); // distance
                            aSampleData.push_back(int(*(p++))); // intensity
                        }
                        aSamples.push_back(aSampleData);
                        nSampleCount--;
                    }
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

        //save 1000 samples to the file
        for (unsigned int i = 0; i < aSamples.size(); i++)
        {
            str = "";
            CHR_Sample aSampleData = aSamples[i];
            for (unsigned int j = 0; j < aSampleData.size(); j++)
                str += ToString(aSampleData[j]) + "\t";
            oStr << str << "\n";
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
