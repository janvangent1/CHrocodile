/**
 *  \file
 *
 *  \copyright    Copyright (C) @CHRLIB_GIT_TIME_YEAR@ by Precitec Optronik GmbH
 *  \brief Error definitions for the CHRocodileLib
 *
 */

using System;

public static class CHRLibError
{
    /*********************************************************************
     * Message: Operation was successful
     * Value: 0 (0b00000000000000000000000000000000)
     *********************************************************************/
    public const Int32 SCS_SUCCESS = 0;

    /*********************************************************************
     * Message: Physical connection attempt already underway. Wait
     * Value: -1610345472 (0b10100000000001000001010000000000)
     *********************************************************************/
    public const Int32 WRN_CONNECTION_ATTEMPT_UNDERWAY = unchecked((Int32)0xA0041400);

    /*********************************************************************
     * Message: Input buffer was automatically flushed, due to out-of-sync condition
     * Value: -1610320384 (0b10100000000001000111011000000000)
     *********************************************************************/
    public const Int32 WRN_AUTOFLUSH_OCCURRED = unchecked((Int32)0xA0047600);

    /*********************************************************************
     * Message: Output signals have been adjusted according to device output
     * Value: -1610313728 (0b10100000000001001001000000000000)
     *********************************************************************/
    public const Int32 WRN_SIGNAL_CHANGED = unchecked((Int32)0xA0049000L); //!< Output signals have been adjusted according to device output

    /*********************************************************************
     * Message: Failed to allocate memory/resource
     * Value: -536867840 (0b11100000000000000000110000000000)
     *********************************************************************/
    public const Int32 ERR_ALLOCATE = unchecked((Int32)0xE0000C00);

    /*********************************************************************
     * Message: Failed to free memory/resource
     * Value: -536867584 (0b11100000000000000000110100000000)
     *********************************************************************/
    public const Int32 ERR_FREE = unchecked((Int32)0xE0000D00);

    /*********************************************************************
     * Message: Failed to reallocate (bigger) memory/resource
     * Value: -536867328 (0b11100000000000000000111000000000)
     *********************************************************************/
    public const Int32 ERR_REALLOCATE = unchecked((Int32)0xE0000E00);

    /*********************************************************************
     * Message: A passed handle was of the wrong type
     * Value: -536863232 (0b11100000000000000001111000000000)
     *********************************************************************/
    public const Int32 ERR_WRONG_HANDLE_TYPE = unchecked((Int32)0xE0001E00);

    /*********************************************************************
     * Message: An invalid handle was encountered
     * Value: -536862976 (0b11100000000000000001111100000000)
     *********************************************************************/
    public const Int32 ERR_INVALID_HANDLE = unchecked((Int32)0xE0001F00);

    /*********************************************************************
     * Message: A null pointer was encountered
     * Value: -536862720 (0b11100000000000000010000000000000)
     *********************************************************************/
    public const Int32 ERR_NULLPTR = unchecked((Int32)0xE0002000);

    /*********************************************************************
     * Message: Buffer too small
     * Value: -536862464 (0b11100000000000000010000100000000)
     *********************************************************************/
    public const Int32 ERR_BUFSIZE_TOO_SMALL = unchecked((Int32)0xE0002100);

    /*********************************************************************
     * Message: Uncaught exception
     * Value: -536845568 (0b11100000000000000110001100000000)
     *********************************************************************/
    public const Int32 ERR_UNCAUGHT = unchecked((Int32)0xE0006300);

    /*********************************************************************
     * Message: Buffer size must be a power of two
     * Value: -536836608 (0b11100000000000001000011000000000)
     *********************************************************************/
    public const Int32 ERR_BUFSIZE_POWER_OF_TWO = unchecked((Int32)0xE0008600);

    /*********************************************************************
     * Message: Unknown error
     * Value: -536805632 (0b11100000000000001111111100000000)
     *********************************************************************/
    public const Int32 ERR_UNKNOWN = unchecked((Int32)0xE000FF00);

    /*********************************************************************
     * Message: Failed to open file
     * Value: -536802816 (0b11100000000000010000101000000000)
     *********************************************************************/
    public const Int32 ERR_OPEN_FILE = unchecked((Int32)0xE0010A00);

    /*********************************************************************
     * Message: File does already exist
     * Value: -536800256 (0b11100000000000010001010000000000)
     *********************************************************************/
    public const Int32 ERR_FILE_ALREADY_EXISTS = unchecked((Int32)0xE0011400);

    /*********************************************************************
     * Message: File does not exist
     * Value: -536800000 (0b11100000000000010001010100000000)
     *********************************************************************/
    public const Int32 ERR_NO_SUCH_FILE = unchecked((Int32)0xE0011500);

    /*********************************************************************
     * Message: Failed to open connection to serial device
     * Value: -536737280 (0b11100000000000100000101000000000)
     *********************************************************************/
    public const Int32 ERR_OPEN_COMPORT = unchecked((Int32)0xE0020A00);

    /*********************************************************************
     * Message: Encountered serial device write operation timeout
     * Value: -536729600 (0b11100000000000100010100000000000)
     *********************************************************************/
    public const Int32 ERR_COMPORT_WRITE_TIMEOUT = unchecked((Int32)0xE0022800);

    /*********************************************************************
     * Message: Failed to read from serial device
     * Value: -536727040 (0b11100000000000100011001000000000)
     *********************************************************************/
    public const Int32 ERR_READ_COMPORT = unchecked((Int32)0xE0023200);

    /*********************************************************************
     * Message: Failed to write to serial device
     * Value: -536726784 (0b11100000000000100011001100000000)
     *********************************************************************/
    public const Int32 ERR_WRITE_COMPORT = unchecked((Int32)0xE0023300);

    /*********************************************************************
     * Message: Failed to get state of serial device
     * Value: -536726528 (0b11100000000000100011010000000000)
     *********************************************************************/
    public const Int32 ERR_GET_STATE_COMPORT = unchecked((Int32)0xE0023400);

    /*********************************************************************
     * Message: Failed to set state of serial device
     * Value: -536726272 (0b11100000000000100011010100000000)
     *********************************************************************/
    public const Int32 ERR_SET_STATE_COMPORT = unchecked((Int32)0xE0023500);

    /*********************************************************************
     * Message: Device output stream does not stop on '$' (wrong Baud rate?)
     * Value: -536713984 (0b11100000000000100110010100000000)
     *********************************************************************/
    public const Int32 ERR_FLUSH_SERIAL_CONNECTION_STREAM = unchecked((Int32)0xE0026500);

    /*********************************************************************
     * Message: Failed to initiate connection on socket
     * Value: -536671744 (0b11100000000000110000101000000000)
     *********************************************************************/
    public const Int32 ERR_CONNECT_SOCKET = unchecked((Int32)0xE0030A00);

    /*********************************************************************
     * Message: Failed to allocate socket
     * Value: -536671232 (0b11100000000000110000110000000000)
     *********************************************************************/
    public const Int32 ERR_ALLOCATE_SOCKET = unchecked((Int32)0xE0030C00);

    /*********************************************************************
     * Message: Failed to read from socket, is it already closed?
     * Value: -536668672 (0b11100000000000110001011000000000)
     *********************************************************************/
    public const Int32 ERR_READ_CLOSED_SOCKET = unchecked((Int32)0xE0031600);

    /*********************************************************************
     * Message: Encountered socket timeout
     * Value: -536664064 (0b11100000000000110010100000000000)
     *********************************************************************/
    public const Int32 ERR_SOCKET_TIMEOUT = unchecked((Int32)0xE0032800);

    /*********************************************************************
     * Message: Failed to read from socket
     * Value: -536661504 (0b11100000000000110011001000000000)
     *********************************************************************/
    public const Int32 ERR_READ_SOCKET = unchecked((Int32)0xE0033200);

    /*********************************************************************
     * Message: Failed to write to socket
     * Value: -536661248 (0b11100000000000110011001100000000)
     *********************************************************************/
    public const Int32 ERR_WRITE_SOCKET = unchecked((Int32)0xE0033300);

    /*********************************************************************
     * Message: Failed to get state of socket
     * Value: -536660992 (0b11100000000000110011010000000000)
     *********************************************************************/
    public const Int32 ERR_GET_STATE_SOCKET = unchecked((Int32)0xE0033400);

    /*********************************************************************
     * Message: Failed to set state of socket
     * Value: -536660736 (0b11100000000000110011010100000000)
     *********************************************************************/
    public const Int32 ERR_SET_STATE_SOCKET = unchecked((Int32)0xE0033500);

    /*********************************************************************
     * Message: The select() call on a socket failed
     * Value: -536648192 (0b11100000000000110110011000000000)
     *********************************************************************/
    public const Int32 ERR_SELECT_SOCKET = unchecked((Int32)0xE0036600);

    /*********************************************************************
     * Message: Connection not open
     * Value: -536603392 (0b11100000000001000001010100000000)
     *********************************************************************/
    public const Int32 ERR_CONNECTION_NOT_OPEN = unchecked((Int32)0xE0041500);

    /*********************************************************************
     * Message: Wrong connection mode
     * Value: -536601088 (0b11100000000001000001111000000000)
     *********************************************************************/
    public const Int32 ERR_WRONG_CONNECTION_MODE = unchecked((Int32)0xE0041E00);

    /*********************************************************************
     * Message: The requested functionality is not supported by the device
     * Value: -536600064 (0b11100000000001000010001000000000)
     *********************************************************************/
    public const Int32 ERR_UNSUPPORTED_DEVICE_FUNCTIONALITY = unchecked((Int32)0xE0042200);

    /*********************************************************************
     * Message: Timeout when reading command response
     * Value: -536598272 (0b11100000000001000010100100000000)
     *********************************************************************/
    public const Int32 ERR_RESPONSE_TIMEOUT = unchecked((Int32)0xE0042900);

    /*********************************************************************
     * Message: Timeout when sending command
     * Value: -536598016 (0b11100000000001000010101000000000)
     *********************************************************************/
    public const Int32 ERR_COMMAND_SEND_TIMEOUT = unchecked((Int32)0xE0042A00);

    /*********************************************************************
     * Message: Exhausted retries on resync
     * Value: -536581888 (0b11100000000001000110100100000000)
     *********************************************************************/
    public const Int32 ERR_RESYNC_FAILED = unchecked((Int32)0xE0046900);

    /*********************************************************************
     * Message: The ordered signal was not received from the device
     * Value: -536581632 (0b11100000000001000110101000000000)
     *********************************************************************/
    public const Int32 ERR_SIGNAL_MISSING = unchecked((Int32)0xE0046A00);

    /*********************************************************************
     * Message: Input buffer is out-of-sync
     * Value: -536580864 (0b11100000000001000110110100000000)
     *********************************************************************/
    public const Int32 ERR_CONNECTION_OUT_OF_SYNC = unchecked((Int32)0xE0046D00);

    /*********************************************************************
     * Message: Exhausted retries attempting to automatically flush input buffer
     * Value: -536578304 (0b11100000000001000111011100000000)
     *********************************************************************/
    public const Int32 ERR_AUTOFLUSH_FAILED = unchecked((Int32)0xE0047700);

    /*********************************************************************
     * Message: Automatic device output processing active. User device output processing not permissible
     * Value: -536574976 (0b11100000000001001000010000000000)
     *********************************************************************/
    public const Int32 ERR_AUTO_PROCESS_ACTIVE = unchecked((Int32)0xE0048400);

    /*********************************************************************
     * Message: Automatic buffer saving active. Conflicting operation not permissible
     * Value: -536574720 (0b11100000000001001000010100000000)
     *********************************************************************/
    public const Int32 ERR_AUTO_SAVE_ACTIVE = unchecked((Int32)0xE0048500);

    /*********************************************************************
     * Message: Invalid parameter during connection attempt
     * Value: -536574208 (0b11100000000001001000011100000000)
     *********************************************************************/
    public const Int32 ERR_CONNECTION_INVALID_PARAMETER = unchecked((Int32)0xE0048700);

    /*********************************************************************
     * Message: Unknown device type during connection attempt
     * Value: -536573952 (0b11100000000001001000100000000000)
     *********************************************************************/
    public const Int32 ERR_CONNECTION_UNKNOWN_DEVTYPE = unchecked((Int32)0xE0048800);

    /*********************************************************************
     * Message: Data stream stopped, unable to read data or auto-save
     * Value: -536573696 (0b11100000000001001000100100000000)
     *********************************************************************/
    public const Int32 ERR_DATASTREAM_STOPPED = unchecked((Int32)0xE0048900);

    /*********************************************************************
     * Message: No signals selected for connection, select some first
     * Value: -536573440 (0b11100000000001001000101000000000)
     *********************************************************************/
    public const Int32 ERR_NO_SIGNALS_SELECTED = unchecked((Int32)0xE0048A00);

    /*********************************************************************
     * Message: Failed to allocate memory required for device search
     * Value: -536540160 (0b11100000000001010000110000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_ALLOCATE = unchecked((Int32)0xE0050C00);

    /*********************************************************************
     * Message: Failed to reallocate memory required for device search
     * Value: -536539648 (0b11100000000001010000111000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_REALLOCATE = unchecked((Int32)0xE0050E00);

    /*********************************************************************
     * Message: A device search is already underway
     * Value: -536538112 (0b11100000000001010001010000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UNDERWAY = unchecked((Int32)0xE0051400);

    /*********************************************************************
     * Message: Device search was canceled
     * Value: -536532224 (0b11100000000001010010101100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_CANCELED = unchecked((Int32)0xE0052B00);

    /*********************************************************************
     * Message: The call of getifaddrs()/GetAdaptersInfo() failed
     * Value: -536515072 (0b11100000000001010110111000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_GETIFADDRS = unchecked((Int32)0xE0056E00);

    /*********************************************************************
     * Message: Failed to allocate send socket for UPnP broadcast
     * Value: -536514816 (0b11100000000001010110111100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_ALLOCATE_SEND_SOCKET = unchecked((Int32)0xE0056F00);

    /*********************************************************************
     * Message: Failed to allocate receive socket for UPnP response
     * Value: -536514560 (0b11100000000001010111000000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_ALLOCATE_RECV_SOCKET = unchecked((Int32)0xE0057000);

    /*********************************************************************
     * Message: Failed to set default interface address for UPnP broadcast
     * Value: -536514304 (0b11100000000001010111000100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_SET_IFADDR = unchecked((Int32)0xE0057100);

    /*********************************************************************
     * Message: Failed to bind to multicast address
     * Value: -536514048 (0b11100000000001010111001000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_BIND_MULTICAST = unchecked((Int32)0xE0057200);

    /*********************************************************************
     * Message: Failed to set reuse port option
     * Value: -536513792 (0b11100000000001010111001100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_REUSE_PORT = unchecked((Int32)0xE0057300);

    /*********************************************************************
     * Message: Failed setting UPnP group membership
     * Value: -536513536 (0b11100000000001010111010000000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_ADD_MEMBERSHIP = unchecked((Int32)0xE0057400);

    /*********************************************************************
     * Message: Failed sending UPnP search
     * Value: -536513280 (0b11100000000001010111010100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UPNP_SEARCH = unchecked((Int32)0xE0057500);

    /*********************************************************************
     * Message: Unknown error condition during device search
     * Value: -536477952 (0b11100000000001011111111100000000)
     *********************************************************************/
    public const Int32 ERR_DEVSEARCH_UNKNOWN = unchecked((Int32)0xE005FF00);

    /*********************************************************************
     * Message: The requested functionality is not supported
     * Value: -536468992 (0b11100000000001100010001000000000)
     *********************************************************************/
    public const Int32 ERR_UNSUPPORTED_FUNCTIONALITY = unchecked((Int32)0xE0062200);

    /*********************************************************************
     * Message: Client reader thread error
     * Value: -536451328 (0b11100000000001100110011100000000)
     *********************************************************************/
    public const Int32 ERR_CLIENT_READER_ERROR = unchecked((Int32)0xE0066700);

    /*********************************************************************
     * Message: Internal reader thread error
     * Value: -536451072 (0b11100000000001100110100000000000)
     *********************************************************************/
    public const Int32 ERR_INTERNAL_READER_ERROR = unchecked((Int32)0xE0066800);

    /*********************************************************************
     * Message: Device data format packet is missing
     * Value: -536450304 (0b11100000000001100110101100000000)
     *********************************************************************/
    public const Int32 ERR_DATAFMT_MISSING = unchecked((Int32)0xE0066B00);

    /*********************************************************************
     * Message: No valid data format received for current data
     * Value: -536450048 (0b11100000000001100110110000000000)
     *********************************************************************/
    public const Int32 ERR_DATAFMT_INVALID = unchecked((Int32)0xE0066C00);

    /*********************************************************************
     * Message: Library data stream out of sync with device. Cannot send commands or read data
     * Value: -536449792 (0b11100000000001100110110100000000)
     *********************************************************************/
    public const Int32 ERR_LIB_STREAM_OUT_OF_SYNC = unchecked((Int32)0xE0066D00);

    /*********************************************************************
     * Message: Functionality not supported in debug configuration/build
     * Value: -536445696 (0b11100000000001100111110100000000)
     *********************************************************************/
    public const Int32 ERR_UNSUPPORTED_FUNCTIONALITY_DEBUG = unchecked((Int32)0xE0067D00);

    /*********************************************************************
     * Message: Function was called from a different thread context than before where not permissible
     * Value: -536445440 (0b11100000000001100111111000000000)
     *********************************************************************/
    public const Int32 ERR_WRONG_THREAD = unchecked((Int32)0xE0067E00);

    /*********************************************************************
     * Message: Spline calculation error
     * Value: -536444928 (0b11100000000001101000000000000000)
     *********************************************************************/
    public const Int32 ERR_SPLINE_CALC_FAILED = unchecked((Int32)0xE0068000);

    /*********************************************************************
     * Message: WSAStartup (WinSock) failed
     * Value: -536386560 (0b11100000000001110110010000000000)
     *********************************************************************/
    public const Int32 ERR_WSASTARTUP = unchecked((Int32)0xE0076400);

    /*********************************************************************
     * Message: Failed to convert to number
     * Value: -536331264 (0b11100000000010000011110000000000)
     *********************************************************************/
    public const Int32 ERR_TONUMBER = unchecked((Int32)0xE0083C00);

    /*********************************************************************
     * Message: Failed to convert to string
     * Value: -536331008 (0b11100000000010000011110100000000)
     *********************************************************************/
    public const Int32 ERR_TOSTRING = unchecked((Int32)0xE0083D00);

    /*********************************************************************
     * Message: Error in response to device command
     * Value: -536250368 (0b11100000000010010111100000000000)
     *********************************************************************/
    public const Int32 ERR_CMD_RESPONSE = unchecked((Int32)0xE0097800);

    /*********************************************************************
     * Message: Error in response to device command: wrong parameter index
     * Value: -536249856 (0b11100000000010010111101000000000)
     *********************************************************************/
    public const Int32 ERR_CMD_RESPONSE_PARAM_INDEX = unchecked((Int32)0xE0097A00);

    /*********************************************************************
     * Message: Error in response to device command: wrong parameter type
     * Value: -536249600 (0b11100000000010010111101100000000)
     *********************************************************************/
    public const Int32 ERR_CMD_RESPONSE_PARAM_TYPE = unchecked((Int32)0xE0097B00);

    /*********************************************************************
     * Message: Error in response to device command: ID differs from the one sent
     * Value: -536249344 (0b11100000000010010111110000000000)
     *********************************************************************/
    public const Int32 ERR_CMD_RESPONSE_ID_DIFFERENT = unchecked((Int32)0xE0097C00);

    /*********************************************************************
     * Message: Error response from attempted firmware upload
     * Value: -536248576 (0b11100000000010010111111100000000)
     *********************************************************************/
    public const Int32 ERR_FIRMWARE_UPLOAD = unchecked((Int32)0xE0097F00);

    /*********************************************************************
     * Message: Device command buffer is empty
     * Value: -536248064 (0b11100000000010011000000100000000)
     *********************************************************************/
    public const Int32 ERR_CMDBUF_EMPTY = unchecked((Int32)0xE0098100);

    /*********************************************************************
     * Message: Invalid device buffer read position
     * Value: -536247808 (0b11100000000010011000001000000000)
     *********************************************************************/
    public const Int32 ERR_CMDBUF_INVALID_READPOS = unchecked((Int32)0xE0098200);

    /*********************************************************************
     * Message: Invalid command ticket encountered
     * Value: -536247552 (0b11100000000010011000001100000000)
     *********************************************************************/
    public const Int32 ERR_CMDBUF_INVALID_TICKET = unchecked((Int32)0xE0098300);

    /*********************************************************************
     * Message: Invalid device command parameter
     * Value: -536245504 (0b11100000000010011000101100000000)
     *********************************************************************/
    public const Int32 ERR_CMD_INVALID_PARAM = unchecked((Int32)0xE0098B00);

    /*********************************************************************
     * Message: Invalid file for device (upload) command
     * Value: -536245248 (0b11100000000010011000110000000000)
     *********************************************************************/
    public const Int32 ERR_CMD_INVALID_FILE = unchecked((Int32)0xE0098C00);

    /*********************************************************************
     * Message: Unknown device command error
     * Value: -536215808 (0b11100000000010011111111100000000)
     *********************************************************************/
    public const Int32 ERR_CMD_UNKNOWN = unchecked((Int32)0xE009FF00);
}
