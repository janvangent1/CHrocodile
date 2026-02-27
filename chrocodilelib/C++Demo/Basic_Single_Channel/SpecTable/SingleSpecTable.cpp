/*
This demo demonstates how to download spectrum, wavelength and confocal calibration table
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
        Rsp_h hRsp;
        // Set measurement mode to confocal mode:
        NewCommand(CmdID_Measuring_Method, false, &hCmd);
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting measuring mode: " << nRes << std::endl;
        // Select optical probe
        NewCommand(CmdID_Optical_Probe, false, &hCmd);
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, nullptr);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in setting optical probe: " << nRes << std::endl;

        std::ofstream oStr("tmp.txt");

        std::cout << "Download conformcal spectrum and save to file" << std::endl;
        oStr << "Confocal Spectrum" << "\n";

        //download spectrum, spectrum type: 0, raw spectrum; 1, processed spectrum in confocal mode; 2, FT spectrum;
        const int8_t *pTemp = nullptr;
        int32_t nLength = 0;
        NewCommand(CmdID_Download_Spectrum, false, &hCmd);
        AddCommandIntArg(hCmd, Spectrum_Confocal);
        nRes = ExecCommand(hCHR, hCmd, &hRsp);
        //spectrum download response, parameters:
        //the first: spectrum type, second: start channel index, third: channel count
        //fourth: exposure number, fifth: micromters per bin (interferometric mode), 
        //sixth: Block exponent of spectrum data, seventh: spectrum data
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 6, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in spectrum downloading!" << std::endl;
        else
        {
            //Spectrum is in short format
            int32_t nSpecLength = nLength / 2;
            const int16_t *pSpectrum = (const int16_t *)pTemp;
            std::string str = "";
            for (int32_t i = 0; i < nSpecLength; i++)
            {
                str = ToString(*(pSpectrum++));
                oStr << str << "\n";
            }
        }

        oStr << "\n";
        oStr << "\n";
        oStr << "\n";

        std::cout << "Download wavelength table and save to file" << std::endl;
        oStr << "Wavelength table"
             << "\n";
        //download wavelength table from normal CHRocodile device or CHRocodile² device
        NewCommand(CmdID_Download_Upload_Table, true, &hCmd);
        AddCommandIntArg(hCmd, Table_WaveLength);
        nRes = ExecCommand(hCHR, hCmd, &hRsp);
        //the fifth parameter is the table, the first: table type, second: table index, third: table offset, fourth:  table length
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 4, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in wavelength table downloading!" << std::endl;
        else
        {
            std::string str = "";
            for (int32_t i = 0; i < nLength / 4; i++)
            {
                str = ToString<float>(*((float *)(pTemp)));
                oStr << str << "\n";
                pTemp += 4;
            }
        }
        oStr << "\n";
        oStr << "\n";
        oStr << "\n";

        std::cout << "Download current calibration table and save to file" << std::endl;
        oStr << "Calibration table"
             << "\n";

        //download Calibration table from normal CHRocodile device or CHRocodile² device
        NewCommand(CmdID_Download_Upload_Table, true, &hCmd);
        AddCommandIntArg(hCmd, Table_Confocal_Calibration);
        //table index
        AddCommandIntArg(hCmd, 0);
        nRes = ExecCommand(hCHR, hCmd, &hRsp);
        //the fifth parameter is the table, the first: table type, second: table index, third: table offset, fourth:  table length
        if (CHR_SUCCESS(nRes))
            nRes = GetResponseBlobArg(hRsp, 4, &pTemp, &nLength);
        if (!CHR_SUCCESS(nRes))
            std::cout << "Error in confocal calibration table downloading!" << std::endl;
        else
        {
            int32_t nDeviceType = GetDeviceType(hCHR);
            //normal CHRocodile Device
            if (nDeviceType == CHR_1_Device)
            {
                std::string str = "";
                for (int32_t i = 0; i < 1024; i++)
                {
                    str = ToString<float>(*((float *)(pTemp)));
                    pTemp += 4;
                    oStr << str << "\n";
                }
            }
            //CHRocodile² device or CHRocodile C device
            else
            {
                //in CHR², the last four data in calibration table has special meaning and in integer form, instead of float
                //For details, please refer to related document
                std::string str = "";
                for (int32_t i = 0; i < 1020; i++)
                {
                    str = ToString<float>(*((float *)(pTemp)));
                    pTemp += 4;
                    oStr << str << "\n";
                }
                for (int32_t i = 1020; i < 1024; i++)
                {
                    str = ToString<uint32_t>(*((uint32_t *)pTemp));
                    pTemp += 4;
                    oStr << str << "\n";
                }
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
