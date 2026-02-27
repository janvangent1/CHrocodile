/**
 *  \file
 *
 *  \copyright    Copyright (C) @CHRLIB_GIT_TIME_YEAR@ by Precitec Optronik GmbH
 *  \brief Result code definitions for the CHRocodileLib
 *
 *  This file gets generated from result-codes.yaml! Don't modify this file, modify
 *  the source instead and re-generate this file using "preprocess-result-codes.py"
 *  (Python 3.5 or newer with PyYAML required).
 */

#ifndef __CHROCODILELIBERRORDEF_H__
#define __CHROCODILELIBERRORDEF_H__ @CHRLIB_GIT_HEADERDEF@ /// Latest change: @CHRLIB_GIT_ISODATE@
/* Latest Git commit hash: @CHRLIB_GIT_REVID@ */
#if (defined(_MSC_VER) && (_MSC_VER >= 1020)) || (defined(__GNUC_VERSION__) && __GNUC_VERSION__ >= 30400) || defined(__MCPP) || defined(__clang__)
#    pragma once
#endif /* Check for "#pragma once" support */

/** \ingroup ErrorHandling
 *  @{
 */
typedef int32_t Res_t;

#define RES_LEVEL_SUCCESS 0
#define RES_LEVEL_INFORMATION 1
#define RES_LEVEL_WARNING 2
#define RES_LEVEL_ERROR 3


#define CHR_SUCCESS(res) ((res) >= 0)
#define CHR_INFORMATION(res) ((((uint32_t)res) >> 30) == RES_LEVEL_INFORMATION)
#define CHR_WARNING(res) ((((uint32_t)res) >> 30) == RES_LEVEL_WARNING)
#define CHR_ERROR(res) ((((uint32_t)res) >> 30) == RES_LEVEL_ERROR)
/** @} */

/** \defgroup ResultCodes Result Code Constants
 *  \ingroup ErrorHandling
 *  @{
 */

/*********************************************************************
 * Message: Operation was successful
 * Value: 0 (0b00000000000000000000000000000000)
 *********************************************************************/
#define SCS_SUCCESS ((int32_t)0x00000000L) //!< Operation was successful

/*********************************************************************
 * Message: Physical connection attempt already underway. Wait
 * Value: -1610345472 (0b10100000000001000001010000000000)
 *********************************************************************/
#define WRN_CONNECTION_ATTEMPT_UNDERWAY ((int32_t)0xA0041400L) //!< Physical connection attempt already underway. Wait

/*********************************************************************
 * Message: Input buffer was automatically flushed, due to out-of-sync condition
 * Value: -1610320384 (0b10100000000001000111011000000000)
 *********************************************************************/
#define WRN_AUTOFLUSH_OCCURRED ((int32_t)0xA0047600L) //!< Input buffer was automatically flushed, due to out-of-sync condition

/*********************************************************************
 * Message: Output signals have been adjusted according to device output
 * Value: -1610313728 (0b10100000000001001001000000000000)
 *********************************************************************/
#define WRN_SIGNAL_CHANGED ((int32_t)0xA0049000L) //!< Output signals have been adjusted according to device output

/*********************************************************************
 * Message: Failed to allocate memory/resource
 * Value: -536867840 (0b11100000000000000000110000000000)
 *********************************************************************/
#define ERR_ALLOCATE ((int32_t)0xE0000C00L) //!< Failed to allocate memory/resource

/*********************************************************************
 * Message: Failed to free memory/resource
 * Value: -536867584 (0b11100000000000000000110100000000)
 *********************************************************************/
#define ERR_FREE ((int32_t)0xE0000D00L) //!< Failed to free memory/resource

/*********************************************************************
 * Message: Failed to reallocate (bigger) memory/resource
 * Value: -536867328 (0b11100000000000000000111000000000)
 *********************************************************************/
#define ERR_REALLOCATE ((int32_t)0xE0000E00L) //!< Failed to reallocate (bigger) memory/resource

/*********************************************************************
 * Message: A passed handle was of the wrong type
 * Value: -536863232 (0b11100000000000000001111000000000)
 *********************************************************************/
#define ERR_WRONG_HANDLE_TYPE ((int32_t)0xE0001E00L) //!< A passed handle was of the wrong type

/*********************************************************************
 * Message: An invalid handle was encountered
 * Value: -536862976 (0b11100000000000000001111100000000)
 *********************************************************************/
#define ERR_INVALID_HANDLE ((int32_t)0xE0001F00L) //!< An invalid handle was encountered

/*********************************************************************
 * Message: A null pointer was encountered
 * Value: -536862720 (0b11100000000000000010000000000000)
 *********************************************************************/
#define ERR_NULLPTR ((int32_t)0xE0002000L) //!< A null pointer was encountered

/*********************************************************************
 * Message: Buffer too small
 * Value: -536862464 (0b11100000000000000010000100000000)
 *********************************************************************/
#define ERR_BUFSIZE_TOO_SMALL ((int32_t)0xE0002100L) //!< Buffer too small

/*********************************************************************
 * Message: Uncaught exception
 * Value: -536845568 (0b11100000000000000110001100000000)
 *********************************************************************/
#define ERR_UNCAUGHT ((int32_t)0xE0006300L) //!< Uncaught exception

/*********************************************************************
 * Message: Buffer size must be a power of two
 * Value: -536836608 (0b11100000000000001000011000000000)
 *********************************************************************/
#define ERR_BUFSIZE_POWER_OF_TWO ((int32_t)0xE0008600L) //!< Buffer size must be a power of two

/*********************************************************************
 * Message: Unknown error
 * Value: -536805632 (0b11100000000000001111111100000000)
 *********************************************************************/
#define ERR_UNKNOWN ((int32_t)0xE000FF00L) //!< Unknown error

/*********************************************************************
 * Message: Failed to open file
 * Value: -536802816 (0b11100000000000010000101000000000)
 *********************************************************************/
#define ERR_OPEN_FILE ((int32_t)0xE0010A00L) //!< Failed to open file

/*********************************************************************
 * Message: File does already exist
 * Value: -536800256 (0b11100000000000010001010000000000)
 *********************************************************************/
#define ERR_FILE_ALREADY_EXISTS ((int32_t)0xE0011400L) //!< File does already exist

/*********************************************************************
 * Message: File does not exist
 * Value: -536800000 (0b11100000000000010001010100000000)
 *********************************************************************/
#define ERR_NO_SUCH_FILE ((int32_t)0xE0011500L) //!< File does not exist

/*********************************************************************
 * Message: Failed to open connection to serial device
 * Value: -536737280 (0b11100000000000100000101000000000)
 *********************************************************************/
#define ERR_OPEN_COMPORT ((int32_t)0xE0020A00L) //!< Failed to open connection to serial device

/*********************************************************************
 * Message: Encountered serial device write operation timeout
 * Value: -536729600 (0b11100000000000100010100000000000)
 *********************************************************************/
#define ERR_COMPORT_WRITE_TIMEOUT ((int32_t)0xE0022800L) //!< Encountered serial device write operation timeout

/*********************************************************************
 * Message: Failed to read from serial device
 * Value: -536727040 (0b11100000000000100011001000000000)
 *********************************************************************/
#define ERR_READ_COMPORT ((int32_t)0xE0023200L) //!< Failed to read from serial device

/*********************************************************************
 * Message: Failed to write to serial device
 * Value: -536726784 (0b11100000000000100011001100000000)
 *********************************************************************/
#define ERR_WRITE_COMPORT ((int32_t)0xE0023300L) //!< Failed to write to serial device

/*********************************************************************
 * Message: Failed to get state of serial device
 * Value: -536726528 (0b11100000000000100011010000000000)
 *********************************************************************/
#define ERR_GET_STATE_COMPORT ((int32_t)0xE0023400L) //!< Failed to get state of serial device

/*********************************************************************
 * Message: Failed to set state of serial device
 * Value: -536726272 (0b11100000000000100011010100000000)
 *********************************************************************/
#define ERR_SET_STATE_COMPORT ((int32_t)0xE0023500L) //!< Failed to set state of serial device

/*********************************************************************
 * Message: Device output stream does not stop on '$' (wrong Baud rate?)
 * Value: -536713984 (0b11100000000000100110010100000000)
 *********************************************************************/
#define ERR_FLUSH_SERIAL_CONNECTION_STREAM ((int32_t)0xE0026500L) //!< Device output stream does not stop on '$' (wrong Baud rate?)

/*********************************************************************
 * Message: Failed to initiate connection on socket
 * Value: -536671744 (0b11100000000000110000101000000000)
 *********************************************************************/
#define ERR_CONNECT_SOCKET ((int32_t)0xE0030A00L) //!< Failed to initiate connection on socket

/*********************************************************************
 * Message: Failed to allocate socket
 * Value: -536671232 (0b11100000000000110000110000000000)
 *********************************************************************/
#define ERR_ALLOCATE_SOCKET ((int32_t)0xE0030C00L) //!< Failed to allocate socket

/*********************************************************************
 * Message: Failed to read from socket, is it already closed?
 * Value: -536668672 (0b11100000000000110001011000000000)
 *********************************************************************/
#define ERR_READ_CLOSED_SOCKET ((int32_t)0xE0031600L) //!< Failed to read from socket, is it already closed?

/*********************************************************************
 * Message: Encountered socket timeout
 * Value: -536664064 (0b11100000000000110010100000000000)
 *********************************************************************/
#define ERR_SOCKET_TIMEOUT ((int32_t)0xE0032800L) //!< Encountered socket timeout

/*********************************************************************
 * Message: Failed to read from socket
 * Value: -536661504 (0b11100000000000110011001000000000)
 *********************************************************************/
#define ERR_READ_SOCKET ((int32_t)0xE0033200L) //!< Failed to read from socket

/*********************************************************************
 * Message: Failed to write to socket
 * Value: -536661248 (0b11100000000000110011001100000000)
 *********************************************************************/
#define ERR_WRITE_SOCKET ((int32_t)0xE0033300L) //!< Failed to write to socket

/*********************************************************************
 * Message: Failed to get state of socket
 * Value: -536660992 (0b11100000000000110011010000000000)
 *********************************************************************/
#define ERR_GET_STATE_SOCKET ((int32_t)0xE0033400L) //!< Failed to get state of socket

/*********************************************************************
 * Message: Failed to set state of socket
 * Value: -536660736 (0b11100000000000110011010100000000)
 *********************************************************************/
#define ERR_SET_STATE_SOCKET ((int32_t)0xE0033500L) //!< Failed to set state of socket

/*********************************************************************
 * Message: The select() call on a socket failed
 * Value: -536648192 (0b11100000000000110110011000000000)
 *********************************************************************/
#define ERR_SELECT_SOCKET ((int32_t)0xE0036600L) //!< The select() call on a socket failed

/*********************************************************************
 * Message: Connection not open
 * Value: -536603392 (0b11100000000001000001010100000000)
 *********************************************************************/
#define ERR_CONNECTION_NOT_OPEN ((int32_t)0xE0041500L) //!< Connection not open

/*********************************************************************
 * Message: Wrong connection mode
 * Value: -536601088 (0b11100000000001000001111000000000)
 *********************************************************************/
#define ERR_WRONG_CONNECTION_MODE ((int32_t)0xE0041E00L) //!< Wrong connection mode

/*********************************************************************
 * Message: The requested functionality is not supported by the device
 * Value: -536600064 (0b11100000000001000010001000000000)
 *********************************************************************/
#define ERR_UNSUPPORTED_DEVICE_FUNCTIONALITY ((int32_t)0xE0042200L) //!< The requested functionality is not supported by the device

/*********************************************************************
 * Message: Timeout when reading command response
 * Value: -536598272 (0b11100000000001000010100100000000)
 *********************************************************************/
#define ERR_RESPONSE_TIMEOUT ((int32_t)0xE0042900L) //!< Timeout when reading command response

/*********************************************************************
 * Message: Timeout when sending command
 * Value: -536598016 (0b11100000000001000010101000000000)
 *********************************************************************/
#define ERR_COMMAND_SEND_TIMEOUT ((int32_t)0xE0042A00L) //!< Timeout when sending command

/*********************************************************************
 * Message: Exhausted retries on resync
 * Value: -536581888 (0b11100000000001000110100100000000)
 *********************************************************************/
#define ERR_RESYNC_FAILED ((int32_t)0xE0046900L) //!< Exhausted retries on resync

/*********************************************************************
 * Message: The ordered signal was not received from the device
 * Value: -536581632 (0b11100000000001000110101000000000)
 *********************************************************************/
#define ERR_SIGNAL_MISSING ((int32_t)0xE0046A00L) //!< The ordered signal was not received from the device

/*********************************************************************
 * Message: Input buffer is out-of-sync
 * Value: -536580864 (0b11100000000001000110110100000000)
 *********************************************************************/
#define ERR_CONNECTION_OUT_OF_SYNC ((int32_t)0xE0046D00L) //!< Input buffer is out-of-sync

/*********************************************************************
 * Message: Exhausted retries attempting to automatically flush input buffer
 * Value: -536578304 (0b11100000000001000111011100000000)
 *********************************************************************/
#define ERR_AUTOFLUSH_FAILED ((int32_t)0xE0047700L) //!< Exhausted retries attempting to automatically flush input buffer

/*********************************************************************
 * Message: Automatic device output processing active. User device output processing not permissible
 * Value: -536574976 (0b11100000000001001000010000000000)
 *********************************************************************/
#define ERR_AUTO_PROCESS_ACTIVE ((int32_t)0xE0048400L) //!< Automatic device output processing active. User device output processing not permissible

/*********************************************************************
 * Message: Automatic buffer saving active. Conflicting operation not permissible
 * Value: -536574720 (0b11100000000001001000010100000000)
 *********************************************************************/
#define ERR_AUTO_SAVE_ACTIVE ((int32_t)0xE0048500L) //!< Automatic buffer saving active. Conflicting operation not permissible

/*********************************************************************
 * Message: Invalid parameter during connection attempt
 * Value: -536574208 (0b11100000000001001000011100000000)
 *********************************************************************/
#define ERR_CONNECTION_INVALID_PARAMETER ((int32_t)0xE0048700L) //!< Invalid parameter during connection attempt

/*********************************************************************
 * Message: Unknown device type during connection attempt
 * Value: -536573952 (0b11100000000001001000100000000000)
 *********************************************************************/
#define ERR_CONNECTION_UNKNOWN_DEVTYPE ((int32_t)0xE0048800L) //!< Unknown device type during connection attempt

/*********************************************************************
 * Message: Data stream stopped, unable to read data or auto-save
 * Value: -536573696 (0b11100000000001001000100100000000)
 *********************************************************************/
#define ERR_DATASTREAM_STOPPED ((int32_t)0xE0048900L) //!< Data stream stopped, unable to read data or auto-save

/*********************************************************************
 * Message: No signals selected for connection, select some first
 * Value: -536573440 (0b11100000000001001000101000000000)
 *********************************************************************/
#define ERR_NO_SIGNALS_SELECTED ((int32_t)0xE0048A00L) //!< No signals selected for connection, select some first

/*********************************************************************
 * Message: Failed to allocate memory required for device search
 * Value: -536540160 (0b11100000000001010000110000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_ALLOCATE ((int32_t)0xE0050C00L) //!< Failed to allocate memory required for device search

/*********************************************************************
 * Message: Failed to reallocate memory required for device search
 * Value: -536539648 (0b11100000000001010000111000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_REALLOCATE ((int32_t)0xE0050E00L) //!< Failed to reallocate memory required for device search

/*********************************************************************
 * Message: A device search is already underway
 * Value: -536538112 (0b11100000000001010001010000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UNDERWAY ((int32_t)0xE0051400L) //!< A device search is already underway

/*********************************************************************
 * Message: Device search was canceled
 * Value: -536532224 (0b11100000000001010010101100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_CANCELED ((int32_t)0xE0052B00L) //!< Device search was canceled

/*********************************************************************
 * Message: The call of getifaddrs()/GetAdaptersInfo() failed
 * Value: -536515072 (0b11100000000001010110111000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_GETIFADDRS ((int32_t)0xE0056E00L) //!< The call of getifaddrs()/GetAdaptersInfo() failed

/*********************************************************************
 * Message: Failed to allocate send socket for UPnP broadcast
 * Value: -536514816 (0b11100000000001010110111100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_ALLOCATE_SEND_SOCKET ((int32_t)0xE0056F00L) //!< Failed to allocate send socket for UPnP broadcast

/*********************************************************************
 * Message: Failed to allocate receive socket for UPnP response
 * Value: -536514560 (0b11100000000001010111000000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_ALLOCATE_RECV_SOCKET ((int32_t)0xE0057000L) //!< Failed to allocate receive socket for UPnP response

/*********************************************************************
 * Message: Failed to set default interface address for UPnP broadcast
 * Value: -536514304 (0b11100000000001010111000100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_SET_IFADDR ((int32_t)0xE0057100L) //!< Failed to set default interface address for UPnP broadcast

/*********************************************************************
 * Message: Failed to bind to multicast address
 * Value: -536514048 (0b11100000000001010111001000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_BIND_MULTICAST ((int32_t)0xE0057200L) //!< Failed to bind to multicast address

/*********************************************************************
 * Message: Failed to set reuse port option
 * Value: -536513792 (0b11100000000001010111001100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_REUSE_PORT ((int32_t)0xE0057300L) //!< Failed to set reuse port option

/*********************************************************************
 * Message: Failed setting UPnP group membership
 * Value: -536513536 (0b11100000000001010111010000000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_ADD_MEMBERSHIP ((int32_t)0xE0057400L) //!< Failed setting UPnP group membership

/*********************************************************************
 * Message: Failed sending UPnP search
 * Value: -536513280 (0b11100000000001010111010100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UPNP_SEARCH ((int32_t)0xE0057500L) //!< Failed sending UPnP search

/*********************************************************************
 * Message: Unknown error condition during device search
 * Value: -536477952 (0b11100000000001011111111100000000)
 *********************************************************************/
#define ERR_DEVSEARCH_UNKNOWN ((int32_t)0xE005FF00L) //!< Unknown error condition during device search

/*********************************************************************
 * Message: The requested functionality is not supported
 * Value: -536468992 (0b11100000000001100010001000000000)
 *********************************************************************/
#define ERR_UNSUPPORTED_FUNCTIONALITY ((int32_t)0xE0062200L) //!< The requested functionality is not supported

/*********************************************************************
 * Message: Client reader thread error
 * Value: -536451328 (0b11100000000001100110011100000000)
 *********************************************************************/
#define ERR_CLIENT_READER_ERROR ((int32_t)0xE0066700L) //!< Client reader thread error

/*********************************************************************
 * Message: Internal reader thread error
 * Value: -536451072 (0b11100000000001100110100000000000)
 *********************************************************************/
#define ERR_INTERNAL_READER_ERROR ((int32_t)0xE0066800L) //!< Internal reader thread error

/*********************************************************************
 * Message: Device data format packet is missing
 * Value: -536450304 (0b11100000000001100110101100000000)
 *********************************************************************/
#define ERR_DATAFMT_MISSING ((int32_t)0xE0066B00L) //!< Device data format packet is missing

/*********************************************************************
 * Message: No valid data format received for current data
 * Value: -536450048 (0b11100000000001100110110000000000)
 *********************************************************************/
#define ERR_DATAFMT_INVALID ((int32_t)0xE0066C00L) //!< No valid data format received for current data

/*********************************************************************
 * Message: Library data stream out of sync with device. Cannot send commands or read data
 * Value: -536449792 (0b11100000000001100110110100000000)
 *********************************************************************/
#define ERR_LIB_STREAM_OUT_OF_SYNC ((int32_t)0xE0066D00L) //!< Library data stream out of sync with device. Cannot send commands or read data

/*********************************************************************
 * Message: Functionality not supported in debug configuration/build
 * Value: -536445696 (0b11100000000001100111110100000000)
 *********************************************************************/
#define ERR_UNSUPPORTED_FUNCTIONALITY_DEBUG ((int32_t)0xE0067D00L) //!< Functionality not supported in debug configuration/build

/*********************************************************************
 * Message: Function was called from a different thread context than before where not permissible
 * Value: -536445440 (0b11100000000001100111111000000000)
 *********************************************************************/
#define ERR_WRONG_THREAD ((int32_t)0xE0067E00L) //!< Function was called from a different thread context than before where not permissible

/*********************************************************************
 * Message: Spline calculation error
 * Value: -536444928 (0b11100000000001101000000000000000)
 *********************************************************************/
#define ERR_SPLINE_CALC_FAILED ((int32_t)0xE0068000L) //!< Spline calculation error

/*********************************************************************
 * Message: Timeout in (de)activating autobuffer mode
 * Value: -536441600 (0b11100000000001101000110100000000)
 *********************************************************************/
#define ERR_AUTO_BUFFER_MODE_TIMEOUT ((int32_t)0xE0068D00L) //!< Timeout in (de)activating autobuffer mode

/*********************************************************************
 * Message: Plugin not supported
 * Value: -536441344 (0b11100000000001101000111000000000)
 *********************************************************************/
#define ERR_UNSUPPORTED_PLUGIN ((int32_t)0xE0068E00L) //!< Plugin not supported

/*********************************************************************
 * Message: Ini file load not allowed
 * Value: -536441088 (0b11100000000001101000111100000000)
 *********************************************************************/
#define ERR_INI_FILE_LOAD_DISALLOWED ((int32_t)0xE0068F00L) //!< Ini file load not allowed

/*********************************************************************
 * Message: WSAStartup (WinSock) failed
 * Value: -536386560 (0b11100000000001110110010000000000)
 *********************************************************************/
#define ERR_WSASTARTUP ((int32_t)0xE0076400L) //!< WSAStartup (WinSock) failed

/*********************************************************************
 * Message: Failed to convert to number
 * Value: -536331264 (0b11100000000010000011110000000000)
 *********************************************************************/
#define ERR_TONUMBER ((int32_t)0xE0083C00L) //!< Failed to convert to number

/*********************************************************************
 * Message: Failed to convert to string
 * Value: -536331008 (0b11100000000010000011110100000000)
 *********************************************************************/
#define ERR_TOSTRING ((int32_t)0xE0083D00L) //!< Failed to convert to string

/*********************************************************************
 * Message: Error in response to device command
 * Value: -536250368 (0b11100000000010010111100000000000)
 *********************************************************************/
#define ERR_CMD_RESPONSE ((int32_t)0xE0097800L) //!< Error in response to device command

/*********************************************************************
 * Message: Error in response to device command: wrong parameter index
 * Value: -536249856 (0b11100000000010010111101000000000)
 *********************************************************************/
#define ERR_CMD_RESPONSE_PARAM_INDEX ((int32_t)0xE0097A00L) //!< Error in response to device command: wrong parameter index

/*********************************************************************
 * Message: Error in response to device command: wrong parameter type
 * Value: -536249600 (0b11100000000010010111101100000000)
 *********************************************************************/
#define ERR_CMD_RESPONSE_PARAM_TYPE ((int32_t)0xE0097B00L) //!< Error in response to device command: wrong parameter type

/*********************************************************************
 * Message: Error in response to device command: ID differs from the one sent
 * Value: -536249344 (0b11100000000010010111110000000000)
 *********************************************************************/
#define ERR_CMD_RESPONSE_ID_DIFFERENT ((int32_t)0xE0097C00L) //!< Error in response to device command: ID differs from the one sent

/*********************************************************************
 * Message: Error response from attempted firmware upload
 * Value: -536248576 (0b11100000000010010111111100000000)
 *********************************************************************/
#define ERR_FIRMWARE_UPLOAD ((int32_t)0xE0097F00L) //!< Error response from attempted firmware upload

/*********************************************************************
 * Message: Device command buffer is empty
 * Value: -536248064 (0b11100000000010011000000100000000)
 *********************************************************************/
#define ERR_CMDBUF_EMPTY ((int32_t)0xE0098100L) //!< Device command buffer is empty

/*********************************************************************
 * Message: Invalid device buffer read position
 * Value: -536247808 (0b11100000000010011000001000000000)
 *********************************************************************/
#define ERR_CMDBUF_INVALID_READPOS ((int32_t)0xE0098200L) //!< Invalid device buffer read position

/*********************************************************************
 * Message: Invalid command ticket encountered
 * Value: -536247552 (0b11100000000010011000001100000000)
 *********************************************************************/
#define ERR_CMDBUF_INVALID_TICKET ((int32_t)0xE0098300L) //!< Invalid command ticket encountered

/*********************************************************************
 * Message: Invalid device command parameter
 * Value: -536245504 (0b11100000000010011000101100000000)
 *********************************************************************/
#define ERR_CMD_INVALID_PARAM ((int32_t)0xE0098B00L) //!< Invalid device command parameter

/*********************************************************************
 * Message: Invalid file for device (upload) command
 * Value: -536245248 (0b11100000000010011000110000000000)
 *********************************************************************/
#define ERR_CMD_INVALID_FILE ((int32_t)0xE0098C00L) //!< Invalid file for device (upload) command

/*********************************************************************
 * Message: Unknown device command error
 * Value: -536215808 (0b11100000000010011111111100000000)
 *********************************************************************/
#define ERR_CMD_UNKNOWN ((int32_t)0xE009FF00L) //!< Unknown device command error

/** @} */

#endif // __CHROCODILELIBERRORDEF_H__
