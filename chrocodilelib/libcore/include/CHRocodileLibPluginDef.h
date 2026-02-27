
/**
 *  \file
 *
 *  \copyright    Copyright (C) @CHRLIB_GIT_TIME_YEAR@ by Precitec Optronik GmbH
 *  \brief Plugin header file for the CHRocodile Library (\\CHRLIB)
 */

#ifndef __CHROCODILELIBPLUGINDEF_H__
#define __CHROCODILELIBPLUGINDEF_H__ @CHRLIB_GIT_HEADERDEF@ /// Latest change: @CHRLIB_GIT_ISODATE@
/* Latest Git commit hash: @CHRLIB_GIT_REVID@ */
#if (defined(_MSC_VER) && (_MSC_VER >= 1020)) || (defined(__GNUC_VERSION__) && __GNUC_VERSION__ >= 30400) || defined(__MCPP) || defined(__clang__)
#    pragma once
#endif /* Check for "#pragma once" support */

#if (__cplusplus >= 201103L)
#    include <cstdint>
#else
#    if defined(_MSC_VER) && (_MSC_VER >= 1900)
#        include <stdint.h>
#    elif defined(_MSC_VER) /* versions of MSVC older than VS2015 need this */
typedef unsigned __int64 uint64_t;
typedef signed __int64 int64_t;
typedef unsigned int uint32_t;
typedef signed int int32_t;
typedef unsigned short int uint16_t;
typedef signed short int int16_t;
#    else
#        include <stdint.h>
#    endif
#endif

#include <CHRocodileLib.h>

static const char Test_Plugin_Name[] = "TestPlugin";
//maximum 128 different plugins are possible, first 8 bits for index
static const uint32_t Test_Plugin_ID = 0x545354; //TST

static const uint32_t CmdID_TestPlugin_CMD_A = 0x41545354;
static const uint32_t CmdID_TestPlugin_CMD_B = 0x42545354;
static const uint32_t CmdID_TestPlugin_CMD_C = 0x43545354;
static const uint32_t CmdID_TestPlugin_CMD_D = 0x44545354;
static const uint32_t CmdID_TestPlugin_CMD_E = 0x45545354;

static const char DownSample_Plugin_Name[] = "DownSamplePlugin";
static const uint32_t DownSample_Plugin_ID = 0x14D5344; //DSM

static const uint32_t CmdID_DownSample_Rate = 0x54525344; //DSRT
static const uint32_t CmdID_DownSample_Output_Rate = 0x5452544F; //OTRT
static const uint32_t CmdID_DownSample_Output_Max_Min = 0x4D4D544F; //OTMM

static const char CLS2_Calib_Plugin_Name[] = "CLS2CalibrationPlugin";
static const uint32_t CLS2_Calib_Plugin_ID = 0x24C4143;  //CAL

static const uint32_t CmdID_CalibFile_Name = 0x464C4143;
static const uint32_t CmdID_CalibFile_List = 0x4C464143;
static const uint32_t CmdID_Calib_Active = 0x414C4143;
static const uint32_t CmdID_Calib_Mode = 0x444D4143;
static const uint32_t CmdID_Calib_Prepare_Status = 0x53504143;
static const uint32_t CmdID_Calib_Resample_Count = 0x43524143;

static const uint32_t Calib_Prepare_Status_Idle = 0;
static const uint32_t Calib_Prepare_Status_Download = 1;
static const uint32_t Calib_Prepare_Status_Set_SEN = 2;

// user try to upload more than 1 calibration file
#define ERR_CALIB_MULTIPLE_FILE ((int32_t)0xE4200000L)
#define ERR_CALIB_INVALID_FILE ((int32_t)0xE4200001L)

static const char TriggerScan_Plugin_Name[] = "TriggerScanPlugin";
static const uint32_t TriggerScan_Plugin_ID = 0x44E5354;  //TSN

static const uint32_t CmdID_TriggerScan_Cfg = 0x474643;    // CFG

static const char Record_Plugin_Name[] = "RecordPlugin";
static const uint32_t Record_Plugin_ID = 0x54D4C52; //RCD

static const uint32_t CmdID_Record_Start = 0x41545352;              //RSTA
static const uint32_t CmdID_Record_Cancel = 0x454C4352;             //RCLE
static const uint32_t CmdID_Record_Status = 0x54534352;             //RCST
static const uint32_t CmdID_Record_Retrieve = 0x56545252;           //RRTV
static const uint32_t CmdID_Record_Delete = 0x4C454452;             //RDEL
static const uint32_t CmdID_Record_Save = 0x56415352;               //RSAV
static const uint32_t CmdID_Record_Default_Max_Sample = 0x584D4452; //RDMX

static const int32_t Record_Status_Saving = 1; 
static const int32_t Record_Status_Finish = 2;
static const int32_t Record_Status_Canceled = 3; 
static const int32_t Record_Status_Format_Unkompatible = 4; 



/*! ************************************* FSS Plugin *************************************/

static const char FlyingSpot_Plugin_Name[] = "FlyingSpotPlugin";
static const uint32_t FlyingSpot_Plugin_ID = 0x3594C46; //FLY

static const uint32_t CmdID_FlyingSpot_Cfg = 0x474643;    // CFG
static const uint32_t CmdID_FlyingSpot_Compile = 0x474f5250; // PROG
static const uint32_t CmdID_FlyingSpot_Exec = 0x43455845; // EXEC
static const uint32_t CmdID_FlyingSpot_Stop = 0x504f5453; // STOP
static const uint32_t CmdID_FlyingSpot_Blob = 0x424f4c42; // BLOB
static const uint32_t CmdID_FlyingSpot_SDCard = 0x44524143; // CARD

#define ERR_FSS_INVALID_PARAMS 0xC4300001       ///< invalid input parameters
#define ERR_FSS_INVALID_CONFIG_FILE 0xC4300002  ///< unable to open/parse FSS config file
#define ERR_FSS_NOT_CONFIGURED 0xC4300003       ///< scan cannot be started since the system is not properly configured
#define ERR_FSS_STATE_FORBIDDEN 0xC4300004      ///< state transition is forbidden
#define ERR_FSS_DEVICE_ERROR 0xC4300005         ///< internal device error
#define ERR_FSS_COMPILE_ERROR 0xC4300006        ///< script compile error


static const int32_t FSS_PROG_InputString = 0; ///< script to be compiled is given as a null-terminated string
static const int32_t FSS_PROG_InputFile = 1;   ///< script to be compiled is given as a path to a text file

/**
 * @brief The FSS_PluginDataType enum defines figure's output data format
 */
static const int32_t FSS_PluginRawData = 0;         ///< raw data returned as an array of tuples: { x, y, sig1, sig2, ... sigN }
static const int32_t FSS_PluginInterpolated2D = 1;  ///< interpolated 2D bitmap where each pixel is given as a tuple: { sig1, sig2, ... sigN }
static const int32_t FSS_PluginRecipeTerminate = 2; ///< special data callback type indicating that the recipe execution has been terminated

/**
 * @brief The FSS_PluginShapeData struct defines the output shape data for FSS command queries / FSS async updates
 *
 * The fields x0, y0, x1, y1, imageW and imageH are only relevant for #FSS_PluginInterpolated2D data type
 */
typedef struct FSS_PluginShapeData
{
    const char *label;                ///< pointer to a null-terminated string holding a user-defined label of this shape
    const TSampleSignalInfo *info;    ///< an array describing the output data signals and its ordering in the \c data field
    const uint8_t * const * metaBuf;  ///< an array of pointers to data buffers (\c numSignals in total). One buffer corresponds to
                                      ///< one data signal. The exact buffer format is determined by \c type

    int32_t shapeType;       ///< output data type (FSS_PluginRawData, FSS_PluginInterpolated2D, etc.)
    uint32_t shapeCounter;   ///< counter defining how many times this shape appeared in a data stream
    uint32_t numSignals;     ///< the number of elements in \c info array
    uint32_t numSamples;     ///< the number of data items in each buffer of \c metabuf
                             ///< for #FSS_PluginRawData: \c numSamples equals the total number of data samples collected for a given shape
                             ///< for #FSS_PluginInterpolated2D: \c numSamples is equal \c imageW * \c imageH

    double x0;       ///< left corner of the interpolated bitmap in mm (#FSS_PluginInterpolated2D only)
    double y0;       ///< top corner of the interpolated bitmap in mm (#FSS_PluginInterpolated2D only)
    double x1;       ///< right corner of the interpolated bitmap in mm (#FSS_PluginInterpolated2D only)
    double y1;       ///< bottom corner of the interpolated bitmap in mm (#FSS_PluginInterpolated2D only)
    uint32_t imageW; ///< width of the interpolated bitmap in pixels (#FSS_PluginInterpolated2D only)
    uint32_t imageH; ///< height of the interpolated bitmap in pixels (#FSS_PluginInterpolated2D only)

} FSS_PluginShapeData;

/*! ************************************* ********* *************************************/

#endif // __CHROCODILELIBPLUGINDEF_H__
