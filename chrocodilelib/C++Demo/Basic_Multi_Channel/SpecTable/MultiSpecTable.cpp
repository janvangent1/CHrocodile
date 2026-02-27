/*
This demo demonstates how to download spectrum and confocal calibration table
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

#include "../../libcore/include/CHRocodileLib.h"

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
        Rsp_h hRsp;
        // Select optical probe
        NewCommand(CmdID_Optical_Probe, false, &hCmd);
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting optical probe: " << nRes << std::endl;

        std::ofstream oStr("tmp.txt");

        std::cout << "Download confocal spectrum and save to file" << std::endl;
        oStr << "Confocal Spectrum"
             << "\n";

        //download spectrum, spectrum type: 0, raw spectrum; 1, processed spectrum in confocal mode
        //Note, spectra for multiple channels cannot be downloaded from CLS2, only single channel is possible 
        const int8_t *pTemp = nullptr;
        int32_t nLength = 0;
        //download spectrum from CLS
        NewCommand(CmdID_Download_Spectrum, false, &hCmd);
        //Spectrum type: confocal
        AddCommandIntArg(hCmd, Spectrum_Confocal);
        //Start channel index
        AddCommandIntArg(hCmd, 0);
        //Channel count,  here to download spectrum for all the channels
        //however for CLS2 device, only spectrum for 1 channel can be downloaded, so nChannelCount = 1
        int32_t nChannelCount = GetDeviceChannelCount(hCHR);
        AddCommandIntArg(hCmd, nChannelCount);
        nRes = ExecCommand(hCHR, hCmd, &hRsp);
        //spectrum download response, parameters:
        //the first: spectrum type, second: start channel index, third: channel count
        //fourth: exposure number, fifth: micromters per bin (interferometric mode),
        //sixth: Block exponent of spectrum data, seventh: spectrum data
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 6, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in spectrum downloading!" << std::endl;

        //Spectrum is in short format
        int32_t nSingleSpecLength = nLength / 2 / nChannelCount;
        const int16_t *pSpectrum = (const int16_t *)pTemp;
        if (CHR_SUCCESS(nRes))
        {
            for (int j = 0; j < nChannelCount; j++)
            {
                oStr << "Spectrum: " << j << "\n";
                std::string str = "";
                for (int i = 0; i < nSingleSpecLength; i++)
                {
                    str += ToString(*(pSpectrum++)) + "\t";
                }
                oStr << str << "\n";
                oStr << "\n";
                oStr << "\n";
            }
        }



        std::cout << "Download current calibration table and save to file" << std::endl;
        oStr << "Calibration table"
             << "\n";

        // download Calibration table from CLS/MPS device, CLS2 has different table index: Table_CLS2_Confocal_Calibration
        NewCommand(CmdID_Download_Upload_Table, true, &hCmd);
        AddCommandIntArg(hCmd,  Table_Confocal_Calibration_Multi_Channel);
        // table index
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, &hRsp);
        // the fifth parameter is the table, the first: table type, second: table index, third: table offset, fourth:  table length
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 4, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in confocal calibration table downloading!" << std::endl;
        else
        {
            std::cout << "Received table with length: " << nLength << std::endl;
            //user should interprete table as the definition in device manual...
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
