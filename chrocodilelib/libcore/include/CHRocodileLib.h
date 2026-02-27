/**
 *  \file
 *
 *  \copyright    Copyright (C) @CHRLIB_GIT_TIME_YEAR@ by Precitec Optronik GmbH
 *  \brief Main header file for the CHRocodile Library (\\CHRLIB)
 */

#ifndef __CHROCODILELIB_H__
#define __CHROCODILELIB_H__ @CHRLIB_GIT_HEADERDEF@ /// Latest change: @CHRLIB_GIT_ISODATE@
/* Latest Git commit hash: @CHRLIB_GIT_REVID@ */
#if (defined(_MSC_VER) && (_MSC_VER >= 1020)) || (defined(__GNUC_VERSION__) && __GNUC_VERSION__ >= 30400) || defined(__MCPP) || defined(__clang__)
#    pragma once
#endif /* Check for "#pragma once" support */

#ifndef RC_INVOKED
#    ifdef __cplusplus
/// \cond nohide
#        define cxxstatic static
/// \endcond
extern "C"
{
#    else
/// \cond nohide
#        define cxxstatic
/// \endcond
#    endif

#    if (__cplusplus >= 201103L)
#        include <cstdint>
#    else
#        if defined(_MSC_VER) && (_MSC_VER >= 1900)
#            include <stdint.h>
#        elif defined(_MSC_VER) /* versions of MSVC older than VS2015 need this */
typedef unsigned __int64 uint64_t;
typedef signed __int64 int64_t;
typedef unsigned int uint32_t;
typedef signed int int32_t;
typedef unsigned short int uint16_t;
typedef signed short int int16_t;
#        else
#            include <stdint.h>
#        endif
#    endif
#endif // RC_INVOKED

/// \cond nohide
#ifndef RC_INVOKED
#    ifndef CHRLIB_API
#        ifdef _WIN32
#            if defined(CHROCODILEDEMOLIB_EXPORTS) || defined(CHRLIB_EXPORTS)
#                define CHRLIB_API __declspec(dllexport) /// Tell the linker to export from DLL
#            else
#                define CHRLIB_API __declspec(dllimport) /// Tell the linker to import from DLL
#            endif
#        else
#            define CHRLIB_API /// Not used on non-Windows targets
#        endif
#    endif
#endif // RC_INVOKED

#define CHRLIB_VERSION_TYPE @JENKINS_BUILD_TYPE@
#define CHRLIB_VERSION_MAJOR 5
#define CHRLIB_VERSION_MINOR 1
#define CHRLIB_VERSION_PATCH 0
#define CHRLIB_VERSION_BUILD @JENKINS_BUILD_NUM@
#define CHRLIB_GIT_REVID "@CHRLIB_GIT_REVID@"
#define CHRLIB_GIT_REVDATE "@CHRLIB_GIT_ISODATE@"
    /// \endcond

#ifndef RC_INVOKED

    typedef uint32_t Handle_t;
    typedef Handle_t Conn_h;
    typedef Handle_t Cmd_h;
    typedef Handle_t Rsp_h;
    typedef Handle_t Plugin_h;
    typedef int64_t LibSize_t;
#    ifdef _WIN32
    typedef wchar_t const *FilePath_t;
#    else
    typedef char const *FilePath_t;
#    endif

#    include "./CHRocodileLibErrorDef.h"

    /** \defgroup HandleConst Handle Types
     *  \ingroup Handle
     *  @{
     */
    cxxstatic const Handle_t Invalid_Handle = ((Handle_t)-1);
    cxxstatic const int32_t Handle_Unknown_Type = -1;
    cxxstatic const int32_t Handle_Connection = 0;
    cxxstatic const int32_t Handle_Command = 1;
    cxxstatic const int32_t Handle_Response = 2;
    cxxstatic const int32_t Handle_Plugin = 3;
    /** @} */
    
    /**
     * \ingroup Handle
     * \brief Get handle type.
     *
     * \param[in] _Handle Handle whose type to be checked
     *
     *
     * \return handle type \\ref{sec:HandleConst}..
     */
    CHRLIB_API int32_t GetHandleType(Handle_t _Handle);

    /**
     * \ingroup Handle
     * \brief Release object related to the handle.
     *
     * \param[in] _Handle Handle which to be released
     *
     *
     */
    CHRLIB_API void DestroyHandle(Handle_t _Handle);

    /** \defgroup DataTypeConst Sample Data Types
     *  \ingroup SampleData
     *  @{
     */
    cxxstatic const int16_t Data_Type_Unsigned_Char = 0;
    cxxstatic const int16_t Data_Type_Signed_Char = 1;
    cxxstatic const int16_t Data_Type_Unsigned_Short = 2;
    cxxstatic const int16_t Data_Type_Signed_Short = 3;
    cxxstatic const int16_t Data_Type_Unsigned_Int32 = 4;
    cxxstatic const int16_t Data_Type_Signed_Int32 = 5;
    cxxstatic const int16_t Data_Type_Float = 6;
    cxxstatic const int16_t Data_Type_Double = 255;
    /** @} */

    /**
     * \ingroup SampleData
     * \brief Structure to represent the detailed information of each output signal.
     *
     */
    typedef struct
    {
        uint16_t SignalID; ///< Signal ID.
        int16_t DataType;  ///< Data type. For a detailed definition see section \\ref{sec:DataTypeConst}.
    } TSampleSignalInfo;

    /**
     * \ingroup SampleData
     * \brief Structure to represent the general information of output signals.
     *
     */
    typedef struct
    {
        uint32_t InfoIndex;        ///< Index of sample signal data format; increases with changing of the data format.
        int32_t PeakSignalCount;   ///< Number of peak signals in data sample. For single channel device, all the signals
                                   ///< are regarded as global signal.
        int32_t GlobalSignalCount; ///< Global signal count in data sample.
        int32_t ChannelCount;      ///< Device channel count.
    } TSampleSignalGeneralInfo;

    /**
     * \ingroup DataCallback
     * \brief Data sample callback function, which is called to pass device data samples
     * \param[in] _pUser Value as passed to RegisterSampleDataCallback(). Forwarded but not interpreted by the library.
     See also section \\ref{puser} of the reference manual.
     * \param[in] _nState Data reading status: negative: error or warning code, 0: time out in waiting for preset amount of data to be read,
     1: successfully read in preset amount of the data, 2: command response has been received, 3: data Sample
     buffer is not large enough to store preset amount of data.
     * \param[in] _nSampleCount Number of data samples received.
     * \param[in] _pSampleBuffer Buffer which stores received data sample. Buffer is maintained by library.
     * \param[in] _nSizePerSample Size of each data sample.
     * \param[in] _sSignalGeneralInfo General information of signals.
     * \param[in] _pSignalInfo Array which contains the detailed description of each signal.
     *
     */
    typedef void (*SampleDataCallback)(
        void *_pUser,
        int32_t _nState,
        int64_t _nSampleCount,
        const double *_pSampleBuffer,
        LibSize_t _nSizePerSample,
        const TSampleSignalGeneralInfo _sSignalGeneralInfo,
        TSampleSignalInfo *_pSignalInfo);

    /**
     * \ingroup CmdCallback
     * \brief General command response information in callback function.
     *
     */
    typedef struct
    {
        void *User;                ///< Value as passed to RegisterGeneralResponseAndUpdateCallback() or ExecCommandAsync() function. Forwarded
                                   ///< but not interpreted by the library. See also section \\ref{puser} of the documentation.
        int32_t *State;            ///< Status of command response: negative: error or warning code, `0`: no error. If `*_State` is set to `1` by the
                                   ///< callback function, general command callback function for this response will not be called.
        int32_t Ticket;            ///< Internal ticket for command.
        Handle_t SourceConnection; ///< Handle of the connection/plugin which has originally issued the command for current
                                   ///< response. If response has been sent out by device as an update (for example in
                                   ///< \\CHRtwo~device), `0xFFFFFFFF` will be given out.
    } TRspCallbackInfo;

    /** \defgroup RspFlagConst Response flag
    *  \ingroup DeviceRsp
    *  @{
    */
    const uint32_t Rsp_Flag_Query = 0x0001;
    const uint32_t Rsp_Flag_Error = 0x8000;
    const uint32_t Rsp_Flag_Warning = 0x4000;
    const uint32_t Rsp_Flag_Update = 0x2000;
    /** @} */

    /**
     * \ingroup DeviceRsp
     * \brief Command response information.
     *
     */
    typedef struct
    {
        uint32_t CmdID;      ///< Command/response ID.
        int32_t Ticket;      ///< Internal ticket for the original command.
        uint32_t Flag;       ///< Response flag see \\ref{sec:RspFlagConst}
        uint32_t ParamCount; ///< Number of parameters in the response.
    } TResponseInfo;

    /** \defgroup DeviceParamType Device Response Parameter Types
     *  \ingroup DeviceRsp
     *  @{
     */
    cxxstatic const int32_t Rsp_Param_Type_Integer = 0;
    cxxstatic const int32_t Rsp_Param_Type_Float = 1;
    cxxstatic const int32_t Rsp_Param_Type_String = 2;
    cxxstatic const int32_t Rsp_Param_Type_Byte_Array = 4;
    cxxstatic const int32_t Rsp_Param_Type_Integer_Array = 254;
    cxxstatic const int32_t Rsp_Param_Type_Float_Array = 255;

    /** @} */

    /**
     * \ingroup CmdCallback
     * \brief The format of callback function upon receiving command response.
     *
     * \param[in] _sInfo Struct contains the general information about the response (see TRspCallbackInfo)
     * \param[in] _hRsp Response handle.
     *
     *
     */

    typedef void (*ResponseAndUpdateCallback)(TRspCallbackInfo _sInfo, Rsp_h _hRsp);

    /**
     * \ingroup ErrorHandling
     * \brief Information of each single error stored in the buffer returned by LastErrors()
     */
    typedef struct
    {
        Handle_t SourceHandle; ///< The handle of the connection where the error occurs.
        int32_t ErrorCode;       ///< Error code.
    } TErrorInfo;


    /**
     * \ingroup Plugin
     * \brief Plugin information returned by GetConnectionPlugInInfo()
     */
    typedef struct
    {
        Plugin_h PluginHandle; ///< Plugin handle
        uint32_t TypeID;     ///<  Type ID of the plugin
    } TPluginInfo;


    /**
     * \ingroup ErrorHandling
     * \brief Get the information of the last errors (maximum 1024 for one connection)
     *
     * \param[in] _hConnection When the valid connection handle is given, function returns the errors of that specific connection.
        When Invalid_Handle is passed, the errors of all the connections will be returned.
     * \param[in] _aErrorInfos Pointer to the buffer where error information should be save to (in the form of an array of
     * TErrorInfo (alignment 4)).
     * \param[in, out] _pnBufSizeInBytes Size of the given error buffer in bytes. The minimum required size is written back.
     *
     * \return number of errors, or negative in case of an error or warning.
     */

    CHRLIB_API Res_t LastErrors(Conn_h _hConnection, TErrorInfo *_aErrorInfos, LibSize_t *_pnBufSizeInBytes);

    /**
     * \ingroup ErrorHandling
     * \brief Clear errors.
     *
     * \param[in] _hConnection When the valid connection handle is given, the error of that specific connection is cleared.
     * When Invalid_Handle is passed, the errors of all the connections are cleared.
     *
     * \return Zero on success, negative in case of an error or warning.
     */
    CHRLIB_API Res_t ClearErrors(Conn_h _hConnection);

    /**
    * \ingroup ErrorHandling
    * \brief Get error string from error code.
    *
    * \param[in] _nErrorCode  Error code.
    * \param[in] _aErrorStr Pointer to the string buffer where the zero-terminated error string should be saved to (zero termination included).
    * \param[in, out] _pnSize Size of the given string buffer. The minimum required size is written back (including zero termination).
    *
    * \return Zero on success, negative in case of an error or warning.
    */
    CHRLIB_API Res_t ErrorCodeToString(int32_t _nErrorCode, char *_aErrorStr, LibSize_t *_pnSize);

    /** \defgroup DeviceTypeConst Connection Constants
     *  \ingroup Connection
     *  These values are returned by GetDeviceType() and consumed by OpenConnection().
     *  @{
     */
    cxxstatic const int32_t CHR_Unspecified = -1;
    cxxstatic const int32_t CHR_1_Device = 0;
    cxxstatic const int32_t CHR_2_Device = 1;
    cxxstatic const int32_t CHR_Multi_Channel_Device = 2;
    cxxstatic const int32_t CHR_Compact_Device = 3;
    /** @} */

    /** \defgroup ConnModeConst Connection Mode
     *  \ingroup Connection
     *  @{
     */
    cxxstatic const int32_t Connection_Synchronous = 0;
    cxxstatic const int32_t Connection_Asynchronous = 1;
    /** @} */

    /**
     * \ingroup Connection
     * \brief Connect to \\CHR~device.
     *
     * \param[in] _strConnectionInfo A zero-terminated string that contains the connection information.
     * \param[in] _nDeviceType For a list of device types see \\ref{sec:DeviceTypeConst}.
     * \param[in] _eConnectionMode Connection mode: synchornous or asynchronous \\ref{sec:ConnModeConst}.
     * \param[in] _nDevBufSizeInBytes The size of library internal buffer for storing device output (only powers of 2 allowed
     (e.g. 16x1024x1024)). By default (when _nDevBufSizeInBytes<=0 ), 32MB.
     * \param[out] _pHandle A pointer to a variable receiving the connection handle. This handle
                   is passed when calling other functions related to this connection.
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t OpenConnection(
        const char *_strConnectionInfo,
        int32_t _nDeviceType,
        int32_t _eConnectionMode,
        LibSize_t _nDevBufSizeInBytes,
        Conn_h *_pHandle);

    /**
     * \ingroup Connection
     * \brief Open an extra connection shared with the exising one.
     *
     * \param[in] _nExistingConnection Handle of existing connection, which to be shared with.
     * \param[in] _eConnectionMode Connection mode: synchornous or asynchronous \\ref{sec:ConnModeConst}.
     * \param[out] _pHandle A pointer to a variable receiving the connection handle. This handle
                    is passed when calling other functions related to this connection.
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t OpenSharedConnection(Conn_h _nExistingConnection, int32_t _eConnectionMode, Conn_h *_pHandle);

    /**
     * \ingroup Connection
     * \brief Close connection to \\CHR~device.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t CloseConnection(Conn_h _hConnection);

    /**
     * \ingroup ConnectionInfo
     * \brief Get device connection information.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _aConnInfoStr Pointer to the string buffer where the connection information string should be saved to (zero termination included).
     * \param[in, out] _pnSize Size of the given string buffer. The minimum required size is written back (including zero termination).
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t GetDeviceConnectionInfo(Conn_h _hConnection, char *_aConnInfoStr, LibSize_t *_pnSize);

    /**
     * \ingroup ConnectionInfo
     * \brief Get device connection mode.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return negative: error or warning code; otherwise see Appendix \\ref{sec:ConnModeConst}
     */

    CHRLIB_API int32_t GetConnectionMode(Conn_h _hConnection);

    /**
     * \ingroup ConnectionInfo
     * \brief Get device type.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return negative: error or warning code; otherwise see Appendix \\ref{sec:DeviceTypeConst}
     */
    CHRLIB_API int32_t GetDeviceType(Conn_h _hConnection);

    /**
     * \ingroup DeviceInfo
     * \brief Get device measuring channel count.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Negative: error or warning code.;
     *          1: \\CHRone, \\CHRC, \\CHRtwo;
     *          n: multi-channel devices, number of measurement channels (MPS) / points (CLS).
     *
     * This function is used to get the number of measurement channels for multi-channel devices.
     * For all the other devices, it will return 1.
     */
    CHRLIB_API Res_t GetDeviceChannelCount(Conn_h _hConnection);

    /**
     * \ingroup DeviceInfo
     * \brief Get IDs of the signals output by the device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _aSignalIDs Pointer to the integer buffer where ID of device output signals should be save to.
     * \param[in, out] _pSignalCount In: the Size of the given integer buffer, out: the number of signals output by the
     * device
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t GetDeviceOutputSignals(Conn_h _hConnection, int32_t *_aSignalIDs, int32_t *_pSignalCount);

    /**
     * \ingroup SampleData SynchronousMode
     * \brief Get the next available samples from the device.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in, out] _pSampleCount User passes over the number of samples to be read, lib gives back the actual number of read samples.
     * \param[out] _ppData A pointer to the double sample buffer where data is saved.
     * \param[out] _pnSizePerSample A pointer to an integer variable which stores the size of each data sample in byte.
     * \param[out] _pSignalGeneralInfo A pointer to the struct, which contains the general information of signals in data sample (see
     TSampleSignalGeneralInfo).
     * \param[out] _pSignalInfo A pointer to the array, where detailed description of each signal is stored one by one.
     *

     * \return negative: error or warning code, 0 : present available data is fewer than the required number, 1 : successfully read in required
     * number of the data, 2 : command response has been received, 3 : library data sample buffer is not large enough to
     * store preset amount of data (\\ref{sec:DataReadConst}).
     *
     */
    CHRLIB_API Res_t GetNextSamples(
        Conn_h _hConnection,
        int64_t *_pSampleCount,
        const double **_ppData,
        LibSize_t *_pnSizePerSample,
        TSampleSignalGeneralInfo *_pSignalGeneralInfo,
        TSampleSignalInfo **_pSignalInfo);

    // read the newest sample from device, read pointer will automatically move to the newest sample.....

    /**
     * \ingroup SampleData SynchronousMode
     * \brief Get the last valid sample from device.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[out] _ppData A pointer to the double sample buffer where data is saved.
     * \param[out] _pnSizePerSample A pointer to an integer variable which stores the size of each data sample.
     * \param[out] _pSignalGeneralInfo A pointer to the struct, which contains the general information of signals in data sample (see
     TSampleSignalGeneralInfo).
     * \param[out] _pSignalInfo A pointer to the array, where detailed description of each signal is stored one by one.
     *
     *
     * \return 1: sample is available; 0: no sample has been received from device;
     *         negative: error or warning code.
     */
    CHRLIB_API Res_t GetLastSample(
        Conn_h _hConnection,
        const double **_ppData,
        LibSize_t *_pnSizePerSample,
        TSampleSignalGeneralInfo *_pSignalGeneralInfo,
        TSampleSignalInfo **_pSignalInfo);

    /**
     * \ingroup DataCallback
     * \brief Register data sample callback function and set data processing related settings.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _nMaxSampleCount Maximum number of samples to be read during single device output processing.
     * \param[in] _nReadSampleTimeOut Timeout in millisecond for waiting for _nReadSampleCount of samples. If
     * _nReadSampleTimeOut is set to 0, only available samples are read without waiting.
     * \param[in] _pUser Can be used to carry user information (see \\ref{puser}). It is not interpreted by the library.
     * When _pOnReadSample is called, this value will be passed into it.
     * \param[in] _pOnReadSample Sample callback function to pass over samples (format see SampleDataCallback()), which is called either by function
     * ProcessDeviceOutput() or by library automatic output processing.
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t RegisterSampleDataCallback(
        Conn_h _hConnection,
        int64_t _nMaxSampleCount,
        int32_t _nReadSampleTimeOut,
        void *_pUser,
        SampleDataCallback _pOnReadSample);

    /**
     * \ingroup SampleData
     * \brief Number of current available samples to be read.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[out] _pnSampleCount Current available sample count.
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t GetCurrentAvailableSampleCount(Conn_h _hConnection, int64_t *_pnSampleCount);

    /**
     * \ingroup SampleData
     * \brief Get the size of each data sample output by the library.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[out] _pnSampleSize A pointer to an integer variable which stores the size of each data sample.
     *
     * \return Zero on success. Negative error or warning code..
     *
     */

    CHRLIB_API Res_t GetSingleOutputSampleSize(Conn_h _hConnection, LibSize_t *_pnSampleSize);

    /**
    * \ingroup SampleData
    * \brief Get the signal information of the data sample output by the library.
    * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
    * \param[out] _pSignalGeneralInfo A pointer to the struct, where the general information of signals in data sample
    (see TSampleSignalGeneralInfo) is returned.
    * \param[in] _pSignalInfoBuf Pointer to the buffer where detailed description of each signal should be saved to.
    * \param[in, out] _pnSignalInfoBufSizeInBytes Size of the given buffer. The minimum required size is written back.
    *
    * \return Zero on success. Negative error or warning code..
    */
    CHRLIB_API Res_t GetConnectionOutputSignalInfos(
        Conn_h _hConnection,
        TSampleSignalGeneralInfo *_pSignalGeneralInfo,
        TSampleSignalInfo *_pSignalInfoBuf,
        LibSize_t *_pnSignalInfoBufSizeInBytes);

    /**
     * \ingroup SampleData
     * \brief Stop data stream for one specific connection. 
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t StopConnectionDataStream(Conn_h _hConnection);

    /**
     * \ingroup SampleData
     * \brief Start data stream for one specific connection.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t StartConnectionDataStream(Conn_h _hConnection);

    /** \defgroup LibDataFormatConst Library Data Formats
     *  \ingroup SampleData
     *  @{
     */
    cxxstatic const int32_t Output_Data_Format_Double = 0;
    cxxstatic const int32_t Output_Data_Format_Raw = 1;

    /** @} */

    /**
     * \ingroup SampleData
     * \brief Set the mode of library output data format {\\bf (Use carefully!)}
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _eMode Output data format mode: 0 - double; 1 - raw.
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t SetOutputDataFormatMode(Conn_h _hConnection, int32_t _eMode);

    /**
     * \ingroup SampleData
     * \brief Get the mode of library output data format.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return negative: error or warning code; 0 - double; 1 - raw.
     */
    CHRLIB_API Res_t GetOutputDataFormatMode(Conn_h _hConnection);

    /**
     * \ingroup AutoBufferSave
     * \brief Automatically save data from device to the target buffer.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _pBuffer A double pointer to the buffer where the data is written to.
     * \param[in] _nSampleCount Number of the sample to be written to the buffer.
     * \param[in, out] _pBufferSizeInBytes Size of the target buffer in byte. If the target buffer is not large enough
     * or _pBuffer is not assigned, required minimum size is written to *_pBufferSizeInBytes.
     *
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t ActivateAutoBufferMode(
        Conn_h _hConnection, double *_pBuffer, int64_t _nSampleCount, LibSize_t *_pBufferSizeInBytes);

    /**
     * \ingroup AutoBufferSave
     * \brief Get the number of the sample already saved into the buffer in the automatic buffer data save mode.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[out] _pnSampleCount Pointer to a variable receiving the number of saved samples.
     *
     * \return Zero on success. Negative value: error or warning code.
     */
    CHRLIB_API Res_t GetAutoBufferSavedSampleCount(Conn_h _hConnection, int64_t *_pnSampleCount);

    /**
     * \ingroup AutoBufferSave
     * \brief Manually quit automatic buffer data save mode.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative value: error or warning code.
     */
    CHRLIB_API Res_t DeactivateAutoBufferMode(Conn_h _hConnection);

    /** \defgroup AutoBufferSaveStatus Automatic Buffer Saving Constants
     *  \ingroup AutoBufferSave
     *  @{
     */
    cxxstatic const int32_t Auto_Buffer_Error = -1;
    cxxstatic const int32_t Auto_Buffer_Saving = 0;
    cxxstatic const int32_t Auto_Buffer_Finished = 1;
    cxxstatic const int32_t Auto_Buffer_Received_Response = 2;
    cxxstatic const int32_t Auto_Buffer_Deactivated = 3;
    cxxstatic const int32_t Auto_Buffer_UnInit = 4;
    /** @} */

    /**
     * \ingroup AutoBufferSave
     * \brief Get the status of automatic buffer data save.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return negative: error or warning code; otherwise the status of automatic buffer data save, see Appendix \\ref{sec:AutoBufferSaveStatus}
     */
    CHRLIB_API int32_t GetAutoBufferStatus(Conn_h _hConnection);

    /** \defgroup DataReadConst Return value of data read
     *  \ingroup Return
     *  @{
     */
    const int32_t Read_Data_Not_Enough = 0;
    const int32_t Read_Data_Success = 1;
    const int32_t Read_Data_Response = 2;
    const int32_t Read_Data_Buffer_Small = 3;
    const int32_t Read_Data_User_Buf_Full = 4;
    const int32_t Read_Data_Format_Change = 5;

    /** @} */

    /**
     * \ingroup AsynchronousMode
     * \brief Process device output. All the data samples and command responses are passed through callback functions.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return negative: error or warning code, 0: timeout in waiting for preset amount of data to be read, 1:
     * successfully read in preset amount of the data, 2: command response has been received, 3: data sample
     * buffer is not large enough to store preset amount of data, 4: set user bufer is full, 5: data format has been changed  (\\ref{sec:DataReadConst}).
     *
     */
    CHRLIB_API Res_t ProcessDeviceOutput(Conn_h _hConnection);

    /**
     * \ingroup AutoProcessing
     * \brief Start library automatic processing of device output.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t StartAutomaticDeviceOutputProcessing(Conn_h _hConnection);

    /**
     * \ingroup AutoProcessing
     * \brief Stop library automatic processing of device output.
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code..
     *
     */
    CHRLIB_API Res_t StopAutomaticDeviceOutputProcessing(Conn_h _hConnection);

    /**
     * \ingroup AsynchronousMode
     * \brief in asychronous mode save data from device to the user target buffer.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _pBuffer A double pointer to the buffer where the data is written to.
     * \param[in] _nSampleCount Number of the sample to be written to the buffer.
     * \param[in, out] _pBufferSizeInBytes Size of the target buffer in byte. If the target buffer is not large enough
     * or _pBuffer is not assigned, required minimum size is written to *_pBufferSizeInBytes.
     *
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t ActivateAsyncDataUserBuffer(
        Conn_h _hConnection, double *_pBuffer, int64_t _nSampleCount, LibSize_t *_pBufferSizeInBytes);


    /**
     * \ingroup AsynchronousMode
     * \brief in asychronous mode reset to save data to library internal buffer.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative value: error or warning code.
     */
    CHRLIB_API Res_t DeactivateAsyncDataUserBuffer(Conn_h _hConnection);


    /**
     * \ingroup AsynchronousMode
     * \brief Get the number of the sample already saved into the preset user buffer in the asynchronous mode.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[out] _pnSampleCount Pointer to a variable receiving the number of saved samples.
     *
     * \return Zero on success. Negative value: error or warning code.
     */
    CHRLIB_API Res_t GetAsyncDataUserBufferSavedSampleCount(Conn_h _hConnection, int64_t *_pnSampleCount);

    /**
     * \ingroup SynchronousMode
     * \brief Flush all the received data and command response, move the read pointer to the last valid position in the
     * data stream from the device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t FlushConnectionBuffer(Conn_h _hConnection);

    /** \defgroup DeviceCmdConst Device Command Specifier ID Constants
     *  \ingroup DeviceCmd
     *  Parameter definition of each command, except CHRocodileLib internal commands, is the same as the corresponding CHR device command. 
     * 
     *  @{
     */
    cxxstatic const uint32_t CmdID_Output_Signals = 0x58444f53;
    cxxstatic const uint32_t CmdID_Firmware_Version = 0x00524556;
    cxxstatic const uint32_t CmdID_Measuring_Method = 0x00444d4d;
    cxxstatic const uint32_t CmdID_Full_Scale = 0x00414353;
    cxxstatic const uint32_t CmdID_Scan_Rate = 0x005a4853;
    cxxstatic const uint32_t CmdID_Data_Average = 0x00445641;
    cxxstatic const uint32_t CmdID_Spectrum_Average = 0x00535641;
    cxxstatic const uint32_t CmdID_Serial_Data_Average = 0x53445641;
    cxxstatic const uint32_t CmdID_Refractive_Indices = 0x00495253;
    cxxstatic const uint32_t CmdID_Abbe_Numbers = 0x00454241;
    cxxstatic const uint32_t CmdID_Refractive_Index_Tables = 0x00545253;
    cxxstatic const uint32_t CmdID_Lamp_Intensity = 0x0049414c;
    cxxstatic const uint32_t CmdID_Optical_Probe = 0x004e4553;
    cxxstatic const uint32_t CmdID_Confocal_Detection_Threshold = 0x00524854;
    cxxstatic const uint32_t CmdID_Peak_Separation_Min = 0x004D5350;
    cxxstatic const uint32_t CmdID_Interferometric_Quality_Threshold = 0x00485451;
    cxxstatic const uint32_t CmdID_Duty_Cycle = 0x00594344;
    cxxstatic const uint32_t CmdID_Detection_Window_Active = 0x00414d4c;
    cxxstatic const uint32_t CmdID_Detection_Window = 0x00445744;
    cxxstatic const uint32_t CmdID_Number_Of_Peaks = 0x00504f4e;
    cxxstatic const uint32_t CmdID_Peak_Ordering = 0x00444f50;
    cxxstatic const uint32_t CmdID_Dark_Reference = 0x004b5244;
    cxxstatic const uint32_t CmdID_Continuous_Dark_Reference = 0x4b445243;
    cxxstatic const uint32_t CmdID_Start_Data_Stream = 0x00415453;
    cxxstatic const uint32_t CmdID_Stop_Data_Stream = 0x004f5453;
    cxxstatic const uint32_t CmdID_Light_Source_Auto_Adapt = 0x004c4141;
    cxxstatic const uint32_t CmdID_CCD_Range = 0x00415243;
    cxxstatic const uint32_t CmdID_Median = 0x5844454d;
    cxxstatic const uint32_t CmdID_Analog_Output = 0x58414e41;
    cxxstatic const uint32_t CmdID_Encoder_Counter = 0x53504525;
    cxxstatic const uint32_t CmdID_Encoder_Counter_Source = 0x53434525;
    cxxstatic const uint32_t CmdID_Encoder_Preload_Function = 0x46504525;
    cxxstatic const uint32_t CmdID_Encoder_Trigger_Enabled = 0x45544525;
    cxxstatic const uint32_t CmdID_Encoder_Trigger_Property = 0x50544525;
    cxxstatic const uint32_t CmdID_Device_Trigger_Mode = 0x4d525425;
    cxxstatic const uint32_t CmdID_Download_Spectrum = 0x444c4e44;
    cxxstatic const uint32_t CmdID_Save_Settings = 0x00555353;
    cxxstatic const uint32_t CmdID_Download_Upload_Table = 0x4C424154;
    /** @} */

    /** \defgroup SpectraConst Spectra Types
     *  \ingroup DeviceCmd
     *  @{
     */
    cxxstatic const int32_t Spectrum_Raw = 0;
    cxxstatic const int32_t Spectrum_Confocal = 1;
    cxxstatic const int32_t Spectrum_FT = 2;
    cxxstatic const int32_t Spectrum_2D_Image = 3;
    cxxstatic const int32_t Spectrum_Capture = 4;
    /** @} */

    /** \defgroup TableConst Table Types
     *  \ingroup DeviceCmd
     *  @{
     */
    cxxstatic const int32_t Table_Confocal_Calibration = 1;
    cxxstatic const int32_t Table_WaveLength = 2;
    cxxstatic const int32_t Table_Refractive_Index = 3;
    cxxstatic const int32_t Table_Dark_Correction = 4;
    cxxstatic const int32_t Table_Confocal_Calibration_Multi_Channel = 5;
    cxxstatic const int32_t Table_CLS_Mask = 6;
    cxxstatic const int32_t Table_MPS_Mask = 8;
    cxxstatic const int32_t Table_CLS2_Confocal_Calibration = 18;
    /** @} */

    /** \defgroup TriggerModeConst Device Trigger Modes
     *  \ingroup DeviceCmd
     *  @{
     */
    cxxstatic const int32_t Device_Trigger_Mode_Free_Run = 0;
    cxxstatic const int32_t Device_Trigger_Mode_Wait_Trigger = 1;
    cxxstatic const int32_t Device_Trigger_Mode_Trigger_Each = 2;
    cxxstatic const int32_t Device_Trigger_Mode_Trigger_Window = 3;
    /** @} */

    /** \defgroup EncoderCounterTriggerConst Encoder Counter Input Source
     *  Encoder Trigger Source constants used for SetEncoderPreloadFunctionExtended() and so on.
     *  \ingroup EncoderPreload
     *  @{
     */
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_A0 = 0;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_B0 = 1;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_A1 = 2;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_B1 = 3;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_A2 = 4;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_B2 = 5;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_A3 = 6;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_B3 = 7;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_A4 = 8;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_B4 = 9;
    cxxstatic const int32_t Encoder_Counter_Trigger_Source_SyncIn = 10;
    cxxstatic const int32_t Encoder_Counter_Source_Quadrature = 15;
    cxxstatic const int32_t Encode_Trigger_Source_Immediate = 15;
    /** @} */

    /** \defgroup EncoderPreloadConst Encoder Preload Event Property Constants
     *  Encoder preload configuration flags used for the SetEncoderPreloadFunction() group of functions.
     *  \ingroup EncoderPreload
     *  @{
     */
    cxxstatic const int32_t Encoder_Preload_Config_Once = 0;
    cxxstatic const int32_t Encoder_Preload_Config_Eachtime = 1;
    cxxstatic const int32_t Encoder_Preload_Config_Trigger_RisingEdge = 0;
    cxxstatic const int32_t Encoder_Preload_Config_Trigger_FallingEdge = 2;
    cxxstatic const int32_t Encoder_Preload_Config_Trigger_OnEdge = 0;
    cxxstatic const int32_t Encoder_Preload_Config_Trigger_OnLevel = 4;
    cxxstatic const int32_t Encoder_Preload_Config_Active = 8;
    /** @} */

    /**
     * \ingroup DeviceCmd SynchronousMode
     * \brief Send command to device and wait for the response, i.e. synchronous command sending.
     *
     * \param[in] _hHandle Connection handle (returned by OpenConnection() or OpenSharedConnection()) or plugin handle (returned by AddConnectionPlugin()).
     * \param[in] _hCmd Command handle returned by NewCommand().
     * \param[out] _phRsp Pointer to a variable receiving the response handle.
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t ExecCommand(Handle_t _hHandle, Cmd_h _hCmd, Rsp_h *_phRsp);

    /**
     * \ingroup DeviceCmd AsynchronousMode
     * \brief Send command to device without waiting for the response, i.e. asynchronous command sending.
     *
     * \param[in] _hHandle Connection handle (returned by OpenConnection() or OpenSharedConnection()) or plugin handle (returned by AddConnectionPlugin()).
     * \param[in] _hCmd Command handle returned by NewCommand().
     * \param[in] _pUser Can be freely used to forward user information to the function _pCB points to. \\
     *            Passed to _pCB but not interpreted by the library. See also section \\ref{puser} of the reference
     * manual.
     * \param[in] _pCB Callback function for current command when response from device comes back.
     * \param[out] _pTicket Pointer to a variable receiving the internal ticket for the issued device command.
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t ExecCommandAsync(
        Handle_t _hHandle, Cmd_h _hCmd, void *_pUser, ResponseAndUpdateCallback _pCB, int32_t *_pTicket);

    /**
     * \ingroup CmdCallback
     * \brief Register callback function for all the commands.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _pUser Can be freely used to forward user information to the function _pCB points to. \\
     *            Passed to _pCB() but not interpreted by the library. See also section \\ref{puser} of the reference manual. 
     * \param[in] _pGenCBFct Callback function for all the responses/updates (see ResponseAndUpdateCallback()).
     *
     * \return Zero on success. Negative error or warning code.
     */
    CHRLIB_API Res_t
    RegisterGeneralResponseAndUpdateCallback(Conn_h _hConnection, void *_pUser, ResponseAndUpdateCallback _pGenCBFct);

    /**
    * \ingroup DeviceCmd
    * \brief Translate device command name to Lib command ID.
    *
    * \param[in] _strCmd Device command name in zero-terminated string.
    * \param[in] _nLength Length of the command name.
    *
    * \return Corresponding command ID
    *
    */
    CHRLIB_API int32_t CmdNameToID(const char *_strCmd, int32_t _nLength);

    /**
    * \ingroup DeviceCmd
    * \brief Create new command.
    *
    * \param[in] _nCmdID Command ID.
    * \param[in] _bQuery Whether command is query or not.
    * \param[out] _hCmd A pointer to a variable receiving the command handle.
    *
    * \return Zero on success. Negative error or warning code..
    *
    */
    CHRLIB_API Res_t NewCommand(uint32_t _nCmdID, int32_t _bQuery, Cmd_h *_hCmd);

    /**
    * \ingroup DeviceCmd
    * \brief Add integer argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _nArg Integer argument.
    *
    * \return Zero on success. Negative error or warning code..
    *
    */
    CHRLIB_API Res_t AddCommandIntArg(Cmd_h _hCmd, int32_t _nArg);

    /**
    * \ingroup DeviceCmd
    * \brief Add float argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _nArg Float argument.
    *
    * \return Zero on success. Negative error or warning code..
    *
    */
    CHRLIB_API Res_t AddCommandFloatArg(Cmd_h _hCmd, float _nArg);

    /**
    * \ingroup DeviceCmd
    * \brief Add integer array argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _pArg Pointer to the integer array argument.
    * \param[in] _nLength Length of the integer array.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t AddCommandIntArrayArg(Cmd_h _hCmd, const int32_t *_pArg, int32_t _nLength);

    /**
    * \ingroup DeviceCmd
    * \brief Add float array argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _pArg Pointer to the float array argument.
    * \param[in] _nLength Length of the float array.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t AddCommandFloatArrayArg(Cmd_h _hCmd, const float *_pArg, int32_t _nLength);

    /**
    * \ingroup DeviceCmd
    * \brief Add string argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _pArg A zero-terminated argument string.
    * \param[in] _nLength Length of the string.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t AddCommandStringArg(Cmd_h _hCmd, const char *_pArg, int32_t _nLength);

    /**
    * \ingroup DeviceCmd
    * \brief Add binray data argument to the command.
    *
    * \param[in] _hCmd Command handle returned by NewCommand().
    * \param[in] _pArg A pointer, pointing to where binary data resides
    * \param[in] _nLength Size of the binary data
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t AddCommandBlobArg(Cmd_h _hCmd, const int8_t *_pArg, int32_t _nLength);

    /**
     * \ingroup DeviceCmd
     * \brief Translate string command to internal command and give back related command handle.
     *
     * \param[in] _strCommand Zero terminated string command.
     * \param[out] _hCmd A pointer to an unsigned integer variable receiving the command handle.
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    CHRLIB_API int32_t NewCommandFromString(const char *_strCommand, Cmd_h *_hCmd);
    /**
    * \ingroup DeviceRsp
    * \brief Get response information.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[out] _pRspInfo A pointer to the struct, where the response information (see TResponseInfo) is returned.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseInfo(Rsp_h _hRsp, TResponseInfo *_pRspInfo);

    /**
    * \ingroup DeviceRsp
    * \brief Get response argument type.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArgType A pointer to the integer variable receiving the argument type.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseArgType(Rsp_h _hRsp, uint32_t _nIndex, int32_t *_pArgType);

    /**
    * \ingroup DeviceRsp
    * \brief Get integer response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the integer variable receiving the argument value.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseIntArg(Rsp_h _hRsp, uint32_t _nIndex, int32_t *_pArg);

    /**
    * \ingroup DeviceRsp
    * \brief Get float response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the float variable receiving the argument value.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseFloatArg(Rsp_h _hRsp, uint32_t _nIndex, float *_pArg);

    /**
    * \ingroup DeviceRsp
    * \brief Get integer array response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the array, where the integer array argument is returned. Array is maintained by the library.
    * \param[out] _pLength A pointer to the integer variable receiving the length of the integer array argument.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseIntArrayArg(Rsp_h _hRsp, uint32_t _nIndex, const int32_t **_pArg, int32_t *_pLength);

    /**
    * \ingroup DeviceRsp
    * \brief Get float array response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the array, where the float array argument is returned. Array is maintained by the library.
    * \param[out] _pLength A pointer to the integer variable receiving the length of the float array argument.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseFloatArrayArg(Rsp_h _hRsp, uint32_t _nIndex, const float **_pArg, int32_t *_pLength);

    /**
    * \ingroup DeviceRsp
    * \brief Get string response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the zero-terminated string containing the argument value
    * \param[out] _pLength A pointer to the integer variable, where the length of the string - including the terminating zero - is returned.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseStringArg(Rsp_h _hRsp, uint32_t _nIndex, const char **_pArg, int32_t *_pLength);

    /**
    * \ingroup DeviceRsp
    * \brief Get binary response argument.
    *
    * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
    * \param[in] _nIndex Argument index, start with 0
    * \param[out] _pArg A pointer to the buffer where the binary data is restored. The buffer is maintained by the library.
    * \param[out] _pLength A pointer to the integer variable, where the size of the binary data.
    *
    * \return Zero on success. Negative error or warning code.
    *
    */
    CHRLIB_API Res_t GetResponseBlobArg(Rsp_h _hRsp, uint32_t _nIndex, const int8_t **_pArg, int32_t *_pLength);

    /**
     * \ingroup DeviceRsp
     * \brief Output response in string format.
     *
     * \param[in] _hRsp Response handle, either returned by ExecCommand() or passed through response and update callback function.
     * \param[in] _aResponseStr Pointer to the buffer where the response string should be saved to (zero-termination is included).
     * \param[in, out] _pnSize Size of the given string buffer. If it is not large enough or _aResponseStr is not assigned,
     * the minimum required size is written back (including zero termination).
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    CHRLIB_API Res_t ResponseToString(Rsp_h _hRsp, char *_aResponseStr, LibSize_t *_pnSize);

    /**
     * \ingroup UploadTables
     * \brief Synchronously upload confocal calibration table from file to device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strFullFileName Calibration table file name (full name including file path).
     * \param[in] _nProbeSerialNumber Sensor probe serial number.
     * \param[in] _nCHRTableIndex Index of calibration table stored in device (from 0 to 15).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t UploadConfocalCalibrationTableFromFile(
        Conn_h _hConnection, FilePath_t _strFullFileName, uint32_t _nProbeSerialNumber, uint32_t _nCHRTableIndex);

    /**
     * \ingroup UploadTables
     * \brief Synchronously upload wavelength table from file to device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strFullFileName Wavelength table file name (full name including file path).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t UploadWaveLengthTableFromFile(Conn_h _hConnection, FilePath_t _strFullFileName);

    /**
     * \ingroup UploadTables
     * \brief Synchronously upload refractive index table from file to device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strFullFileName Refractive index table file name (full name including file path).
     * \param[in] _strTableName Table name.
     * \param[in] _nCHRTableIndex Table index stored in device (from 1 to 16).
     * \param[in] _nRefSRI Reference refractive index (see below).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t UploadRefractiveIndexTableFromFile(
        Conn_h _hConnection,
        FilePath_t _strFullFileName,
        const char *_strTableName,
        uint32_t _nCHRTableIndex,
        float _nRefSRI);

    /**
     * \ingroup UploadTables
     * \brief Synchronously upload mask table from file to multi-channel device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strFullFileName Mask table file name (full name including file path).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t UploadMultiChannelMaskTable(Conn_h _hConnection, FilePath_t _strFullFileName);

    /**
     * \ingroup UploadTables
     * \brief Synchronously upload firmware file to device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strFullFileName Firmware file name (full name including file path).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t UploadFirmware(Conn_h _hConnection, FilePath_t _strFullFileName);

    /**
     * \ingroup SampleData
     * \brief Set profile interpolation parameter for CLS device.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _nMaxHoleSize Maximum hole size/missing sample point number supported by the interpolation.
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t SetMultiChannelProfileInterpolation(Conn_h _hConnection, int32_t _nMaxHoleSize);

    /**
     * \ingroup LibLogConfig
     * \brief Set log file directory property.
     *
     * \param[in] _pstrDirectory Log file directory.
     * \param[in] _nMaxFileSizeInKiB Maximum size of one log file in KiB.
     * \param[in] _nMaxFileNumber Maximum number of log files in the directory.
     * If in the log directory, the number of log files are larger than _nMaxFileNumber, the oldest log file(s) will be automatically deleted.
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t
    SetLibLogFileDirectory(FilePath_t _pstrDirectory, LibSize_t _nMaxFileSizeInKiB, int32_t _nMaxFileNumber);

    /**
     * \ingroup LibLogConfig
     * \brief Set the level for logging library messages. The message which has the higher or equal level will be
     * logged.
     *
     * \param[in] _nLevel Minimum logging level. By default, the logging level is 2.
     * \return Zero on success. Non-zero otherwise.
     */
    CHRLIB_API Res_t SetLibLogLevel(int32_t _nLevel);

    /**
     * \ingroup LibConfig
     * \brief Set the ini file for the Lib. This function should be called right after loading the library.
     * If another ini file was previously loaded or a connection has been openend, results in an error. 
     *
     * \param[in] _pstrIniFile The ini file to be used.
     * \return Zero on success. Non-zero otherwise.
     */
    CHRLIB_API Res_t SetIniFile(FilePath_t _pstrIniFile);

    /** \defgroup LibConfigFlags Library Configuration Flags
     *  \ingroup LibraryConfiguration
     *  @{
     */
    cxxstatic const uint32_t CHRLib_Flag_Rsp_Deactivate_Auto_Buffer = 1;
    cxxstatic const uint32_t CHRLib_Flag_Auto_Change_Data_Buffer_Size = 2;
    cxxstatic const uint32_t CHRLib_Flag_DataFormat_Deactivate_Auto_Buffer = 8;
    /** @} */
    /**
     * \ingroup LibraryConfiguration
     * \brief Set library configuration flags
     *
     * \param[in] _nFlag:  CHRLib_Flag_Rsp_Deactivate_Auto_Buffer: set library to automatically quit auto buffer save mode
     when response/update has been received from the device, by default this flag is not set.
                           CHRLib_Flag_Auto_Change_Data_Buffer_Size: set library to automatically resize library internal data sample
     buffer to store preset number of data samples in RegisterSampleDataCallback() or GetNextSamples(), by default this flag is set.
                           CHRLib_Flag_DataFormat_Deactivate_Auto_Buffer. set library to automatically quit auto buffer save mode
     when data format packet has been received from the device, by default this flag is not set.
     */

    CHRLIB_API void SetLibConfigFlags(uint32_t _nFlag);

    /**
     * \ingroup LibraryConfiguration
     * \brief Set the size of buffer to store data sample.
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * If Invalid_Handle is passed, ths buffer size is applied to all the connections.
     * \param[in] _nBufferSizeInBytes Size of data sample buffer.
     *
     * \return Zero on success. Negative error or warning code..
     */
    CHRLIB_API Res_t SetSampleDataBufferSize(Conn_h _hConnection, LibSize_t _nBufferSizeInBytes);

    /** \defgroup DeviceSearchConst Device Search Constants
     *  \ingroup DeviceSearch
     *  @{
     */
    cxxstatic const int32_t Search_Both_Serial_TCPIP_Connection = 0;
    cxxstatic const int32_t Search_Only_Serial_Connection = 1;
    cxxstatic const int32_t Search_Only_TCPIP_Connection = 2;
    /** @} */

    /**
     * \ingroup DeviceSearch
     * \brief Start automatic \\CHR~device search.
     *
     * \param[in] _nConnectionType 0: search both serial and TCPIP connection; 1: search only serial connection;
                2: search only TCPIP connection for Chrocodile connection (see \\ref{sec:DeviceSearchConst}).
     * \param[in] _bSBlockingSearch Whether start search function return immediately (unblocking search) or wait until
     search is finished (blocking search).
     *
     * \return Zero on success. Negative: error or warning code.
     */
    CHRLIB_API Res_t StartCHRDeviceAutoSearch(int32_t _nConnectionType, int32_t _bSBlockingSearch);

    /**
     * \ingroup DeviceSearch
     * \brief Cancel automatic \\CHR~device search in unblocking search mode.
     *
     */
    CHRLIB_API void CancelCHRDeviceAutoSearch(void);

    /**
     * \ingroup DeviceSearch
     * \brief Query whether \\CHR~device search in unblocking mode has been finished.
     *
     * \return 1: automatic search has been finished; 0: library is still searching for device.
     */
    CHRLIB_API Res_t IsCHRDeviceAutoSearchFinished(void);

    /**
     * \ingroup DeviceSearch
     * \brief Get connection information of automatically detected \\CHR~devices.
     *
     * \param[in] _aDeviceInfos Pointer to the buffer where the connection information string of detected CHR devices should be saved to
     * (zero-termination is included). Information for different devices is separated by ";".
     * \param[in, out] _pnSize Size of the given string buffer. If it is not large enough or _aDeviceInfos is not assigned,
     * the minimum required size is written back (including zero termination).
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    CHRLIB_API Res_t DetectedCHRDeviceInfo(char *_aDeviceInfos, LibSize_t *_pnSize);

    /**
     * \ingroup Plugin
     * \brief Add plugin by name to one specific connection
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _strPluginName Plugin name. The name of each available plugin is defined in CHRocodileLibPluginDef.h
     * \param[out] _hPlugin Returned plugin handle on success
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    
    CHRLIB_API Res_t AddConnectionPlugIn(Conn_h _hConnection, const char *_strPluginName, Plugin_h *_hPlugin);
    
    /**
     * \ingroup Plugin
     * \brief Add plugin by ID to one specific connection
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _nTypeID Plugin type ID. The type ID of each available plugin is defined in CHRocodileLibPluginDef.h
     * \param[out] _hPlugin Returned plugin handle on success
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    CHRLIB_API Res_t AddConnectionPlugInByID(Conn_h _hConnection, uint32_t _nTypeID, Plugin_h *_hPlugin);
    
    /**
     * \ingroup Plugin
     * \brief Get connection plugin information
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection().
     * \param[in] _aPluginInfos Pointer to the buffer where plugin information should be save to (in the form of an array of
     * TPluginInfo (alignment 4)).
     * \param[in, out] _pnBufSizeInBytes Size of the given plugin information buffer in bytes. The minimum required size is written back.
     *
     * \return number of plugins, or negative in case of an error or warning.
     */
    CHRLIB_API Res_t GetConnectionPlugInInfo(Conn_h _hConnection, TPluginInfo *_aPluginInfos, LibSize_t *_pnBufSizeInBytes);

    /**
     * \ingroup Plugin
     * \brief Get plugin name string from plugin type.
    *
    * \param[in] _nTypeID  Plugin type ID.
    * \param[in] _aPluginNameStr Pointer to the string buffer where the zero-terminated plugin name string should be saved to (zero termination included).
    * \param[in, out] _pnSize Size of the given string buffer. The minimum required size is written back (including zero termination).
    *
    * \return Zero on success, negative in case of an error or warning.
    */
    CHRLIB_API Res_t PluginTypeIDToName(uint32_t _nTypeID, char *_aPluginNameStr, LibSize_t *_pnSize);
    
    /** \defgroup LibOPSavePerformance Performance save setting
     *  \ingroup LibOperation
     *  @{
     */
    cxxstatic const uint32_t OPID_Save_Performance = 0x5356504D;
    cxxstatic const uint32_t OPID_Last_Error = 0x4C534552;
    cxxstatic const uint32_t OPID_Device_Search = 0x44565345;
    cxxstatic const uint32_t OPID_Device_Search_Result = 0x44565352;
    cxxstatic const uint32_t OPID_Log_Level = 0x4C474C56;
    cxxstatic const uint32_t OPID_Log_Dir = 0x4C474452;
    cxxstatic const uint32_t OPID_Conf_Flag = 0x43464647;

    cxxstatic const int32_t Performance_Snapshot_Mode = 0;
    cxxstatic const int32_t Performance_Complete_Mode = 1;

   
    
    /**
     * \ingroup LibOperation
     * \brief Perform CHRocodileLib internal operations
     *
     * \param[in] _hConnection Connection handle returned by OpenConnection() or OpenSharedConnection(), 
     * if Invalid_Handle is passed, the operation is performed in the CHRocodileLib, not related to any connection.
     * \param[in] _hCmd Command handle returned by NewCommand().
     * \param[out] _phRsp Pointer to a variable receiving the response handle.
     *
     * \return Zero on success. Negative error or warning code.
     *
     */
    
    CHRLIB_API Res_t PerformLibOperation(Conn_h _hConnection, Cmd_h _hCmd, Rsp_h *_phRsp);

    // The following are the groups we define, along with a title and optionally a description

    /** \defgroup Topics This is a meta-group of sorts
     *
     *  This group will be populated with (sub)groups that should be documented
     *  in our API reference "by topic".
     *
     *  The group itself will get deleted, so it doesn't matter what you note
     *  down here.
     */

    /** \defgroup AsynchronousMode Asynchronous Mode
     *  \ingroup Topics
     *
     *  Functions, structures, constants and other entities related to
     *  using the asynchronous mode.
     */

    /** \defgroup AutoBufferSave Automatic Buffer Saving
     *  \ingroup Topics SynchronousMode SampleData
     *
     *  Functions, structures, constants and other entities related to
     *  using the automatic buffer saving feature.
     */

    /** \defgroup AutoProcessing Automatic Device Output Processing
     *  \ingroup Topics AsynchronousMode
     *
     */

    /** \defgroup Callback Callback Functions
     *  \ingroup Topics AsynchronousMode
     *
     *  Everything related to callback functions.
     */

    /** \defgroup Connection Connection
     *  \ingroup Topics
     *
     *  Functionality related to establishing a connection, retrieving
     *  information about it and closing it.
     */

    /** \defgroup ConnectionInfo Connection Information
     *  \ingroup Topics Connection
     */

    /** \defgroup DataCallback DataCallback Function
     *  \ingroup Topics Callback SampleData
     */

    /** \defgroup DeviceCmd Device Command
     *  \ingroup Topics
     *
     *  Functions that map directly to a particular \\CHR~device command
     *  or other entities related to those.
     */

    /** \defgroup CmdCallback Update and Response Callback Function
     *  \ingroup Topics Callback DeviceCmd DeviceRsp
     */

    /** \defgroup DeviceRsp Command Response
     *  \ingroup Topics
     *
     *  Functions that relate to get the information from responses 
     */

    /** \defgroup DeviceInfo Device Information
     *  \ingroup Topics
     *
     *  Functions that to get the device information 
     */

    /** \defgroup UploadTables Table or Firmware Upload
     *  \ingroup DeviceCmd SynchronousMode
     *
     */

    /** \defgroup DeviceSearch Automatic Device Search
     *  \ingroup Topics
     *
     *  Functions for automatic device search.
     */

    /** \defgroup LibLogConfig Logging Configuration
     *  \ingroup  LibraryConfiguration
     *
     *  Functions to set logging functionality
     */

    /** \defgroup LibraryConfiguration Library Configuration
     *  \ingroup Topics
     *
     *  Functions to set CHRocodileLib configuration
     */

    /** \defgroup SampleData Sample Data
     *  \ingroup Topics
     *
     *  Functions, structures, constants and other entities related to
     *  retrieve data.
     */

    /** \defgroup SynchronousMode Synchronous Mode
     *  \ingroup Topics
     *
     *  Functions, structures, constants and other entities related to
     *  using the synchronous mode.
     */

    /** \defgroup ErrorHandling Error Handling
     *  \ingroup Topics
     *
     *  Functions, structures, constants and other entities related to
     *  error information
     */

    /** \defgroup Handle Handles
     *  \ingroup Topics
     *
     *  Functions, structures, constants and other entities related to
     *  all kinds of handle
     */

    /** \defgroup Plugin Plugins
     *  \ingroup Topics
     *
     *  Functions to configure plugins
     */

    /** \defgroup LibOperation CHRocodileLib Operation
     *  \ingroup Topics LibraryConfiguration
     *
     *  Functions and constance related to perform CHRocdileLib internal operation
     */

#    ifdef __cplusplus
} // extern "C"
#    endif

#endif // RC_INVOKED
#endif // __CHROCODILELIB_H__
