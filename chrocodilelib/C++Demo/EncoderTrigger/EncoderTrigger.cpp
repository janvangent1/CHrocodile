/* 
This demo is used to illustrate how does user use CHRocodileLib to set CHR device in encoder trigger mode.
Synchronous communication with the CHR device is demonstrated here
After device is set up properly, "AutoBufferMode" is used to collect data sample.
*/

#include <string>
#include <sstream>
#include <fstream>
#include <iostream>
#include <vector>
#include <memory>
#include <chrono>
#include <thread>
#ifdef _WIN32
#include <windows.h>
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

int main(int argc, char *argv[])
{
    std::string strConnectionInfo = "192.168.170.2";

    std::ofstream oStr("EncoderTriggerData.txt");

    if (argc > 1)
    {
        strConnectionInfo = argv[1];
    }
    std::cout << "Opening CHRocodile " << strConnectionInfo << std::endl;
    uint32_t hCHR;
    // Open connection in synchronous mode
    //here when connection setting is based on serial connection, program will regard device as first generation CHRocodile device
    //otherwise CHR² device
    //Device buffer size needs to be power of 2, when device buffer size is 0, Lib takes the default buffersize: 32MB
    Res_t nRes;
    if (strConnectionInfo.find("COM") != std::string::npos)
        nRes = OpenConnection(strConnectionInfo.c_str(), CHR_1_Device, Connection_Synchronous, 0, &hCHR);
    else
        nRes = OpenConnection(strConnectionInfo.c_str(), CHR_2_Device, Connection_Synchronous, 0, &hCHR);

    if (!CHR_SUCCESS(nRes))
    {
        std::cout << "Error in connecting to device: " << strConnectionInfo << std::endl;
        return -1;
    }

    //first set up device
    Cmd_h hCmd;
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

    int an[] = {83, 66, 256, 257}; // get sample counter, Y axis encoder counter, distance and intensity
    NewCommand(CmdID_Output_Signals, false, &hCmd);
    AddCommandIntArrayArg(hCmd, an, 4);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in setting output signals: " << nRes << std::endl;

    //move axis to the starting position

    //now begin to set up encoder trigger, here we use Y axis to trigger device
    //initialise current encoder counter
    int nIniPos = 0;
    NewCommand(CmdID_Encoder_Counter, false, &hCmd);
    AddCommandIntArg(hCmd, 1);
    AddCommandIntArg(hCmd, nIniPos);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in setting encoder counter: " << nRes << std::endl;

    //enable encoder trigger
    int nEnable = 1;
    NewCommand(CmdID_Encoder_Trigger_Enabled, false, &hCmd);
    AddCommandIntArg(hCmd, nEnable);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in enabling encoder trigger: " << nRes << std::endl;

    //set encoder trigger property: start position, stop position, trigger interval...
    //here set Y axis trigger, start Pos: 500, Stop Pos: 5400, interval: 100, no trigger on return move
    //if the encoder counter at the start Pos. is larger than stop Pos, the trigger interval should be minus
    int nAxis = 1;
    int nStartPos = 500;
    int nStopPos = 5400;
    float nInterval = 100.0f;
    int nReturnTrigger = 0;
    NewCommand(CmdID_Encoder_Trigger_Property, false, &hCmd);
    AddCommandIntArg(hCmd, nAxis);
    AddCommandIntArg(hCmd, nStartPos);
    AddCommandIntArg(hCmd, nStopPos);
    AddCommandFloatArg(hCmd, nInterval);
    AddCommandIntArg(hCmd, nReturnTrigger);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in setting encoder trigger property: " << nRes << std::endl;

    //set device to "trigger each" mode
    NewCommand(CmdID_Device_Trigger_Mode, false, &hCmd);
    AddCommandIntArg(hCmd, Device_Trigger_Mode_Trigger_Each);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in setting trigger each mode: " << nRes << std::endl;

    
    //device will not send any data, waiting for trigger...

    //read triggered data, here we use auto buffer save to save the data
    LibSize_t nSize = 0;
    //calculate number of samples, from trigger start Pos. to stop Pos.
    LibSize_t const nSampleCount = (5400 + 100 - 500) / 100;
    //get the minimum size of the buffer
    ActivateAutoBufferMode(hCHR, nullptr, nSampleCount, &nSize);
    std::cout << "Min buffer size: " << nSize << std::endl;
    nSize /= sizeof(double);
    //allocate buffer
    std::vector<double> aSamples;
    aSamples.resize(size_t(nSize));
    double * pData = aSamples.data();
    //simulate 5 times scan
    //every time axis passes start position and moves to the direction of stop position, encoder will start trigger device
    for (int j = 0; j < 5; j++)
    {
        //give over the pointer to target buffer and number of required sample, start auto. save to the newest sample.....
        ActivateAutoBufferMode(hCHR, pData, nSampleCount, nullptr);
        //checking whether the auto. save is finished, i.e. whether DLL has collected all the triggered data. If so, it means axis has pass the trigger stop Pos.
        while (GetAutoBufferStatus(hCHR) == Auto_Buffer_Saving)
        {
            std::this_thread::sleep_for(std::chrono::milliseconds(10));
        }
        int nOffset = 0;
        int64_t nSampleCountRead = 0;
        std::cout << "Auto buffer save status: " << GetAutoBufferStatus(hCHR) << std::endl;
        nRes = GetAutoBufferSavedSampleCount(hCHR, &nSampleCountRead);
        if (CHR_SUCCESS(nRes))
            std::cout << "Saved sample number: " << nSampleCountRead << std::endl;
        else
            std::cout << "Failed to read saved sample number. Error code: " << nRes << std::endl;
        //save data to file, finish one scan
        //here only valid for single channel device, multi-channel device data have to be read differently
        //Details of how to read multi-channel device data, please refer to manual or multi-channel basic demo
        oStr << "Save " << j << " scan data: "
             << "\n";
        for (int i = 0; i < nSampleCount; i++)
        {
            std::string str = "";
            str += "\t" + ToString(*(pData + nOffset)); // sample counter output by device
            nOffset++;
            str += "\t" + ToString(*(pData + nOffset)); // Y axis encoder counter
            nOffset++;
            str += "\t" + ToString(*(pData + nOffset)); // distance in micrometer
            nOffset++;
            str += "\t" + ToString(*(pData + nOffset)); // intensity in percent
            nOffset++;
            oStr << str << "\n";
        }
        oStr << "\n";
        oStr << "\n";
        oStr << "\n";

        //wait for axis to go back to start position, not necessary to ::Sleep. Device will not be triggered during the backward movement
        std::this_thread::sleep_for(std::chrono::milliseconds(2000));
    }

    oStr.close();
    //set device back to free run mode...
    NewCommand(CmdID_Device_Trigger_Mode, false, &hCmd);
    AddCommandIntArg(hCmd, Device_Trigger_Mode_Free_Run);
    nRes = ExecCommand(hCHR, hCmd, nullptr);
    if (!CHR_SUCCESS(nRes))
        std::cout << "Error in setting free run mode: " << nRes << std::endl;

    CloseConnection(hCHR);

    return 0;
}
