#ifndef CHROCO_MICRO_HXX
#define CHROCO_MICRO_HXX

#include <sstream>
#include <iostream>
#include <memory>
#include <chrono>
#include <condition_variable>
#include <string.h>

#include "cxx/chroco_micro_internal.hxx"



#define CHR_CHECK(x) \
    if(auto _res = (x); !CHR_SUCCESS(_res)) { \
        std::ostringstream ss;  \
        ss << __LINE__ << " " #x " failed: " << chr::internal::chrErrorString(_res) << " (" << std::hex << _res << ")"; \
        throw std::runtime_error(ss.str()); \
    }


namespace chr {


    inline const char *CmdID2Str(uint32_t cmdID) 
    {
        thread_local union {
            char sbuf[8] = {};
            uint32_t cmdID;
        } U;
        U.cmdID = cmdID;
        return U.sbuf;
    }

    inline constexpr uint32_t Str2CmdID(const char *str)
    {
        uint32_t z = 0, sh = 0;
        for (auto s = str; s[0] != '\0'; s++, sh += 8) {
            z += (uint32_t)(s[0] << sh);
            if (sh > 24)
                throw std::logic_error("String must be at most 4 characters long!");
        }
        return z;
    }

    class Connection;

    template<typename ElemType>
    struct Span { const ElemType* p; int32_t s; };

    class Response
    {
        void initArgTypes()
        {
            int32_t t = 0;
            for (uint32_t i = 0; i < m_info.ParamCount; i++)
            {
                CHR_CHECK(GetResponseArgType(m_hRes, 0, &t));
                switch(t)
                {
                case Rsp_Param_Type_Integer: m_argTypes += 'i'; break;
                case Rsp_Param_Type_Float: m_argTypes += 'f'; break;
                case Rsp_Param_Type_String: m_argTypes += 's'; break;
                case Rsp_Param_Type_Byte_Array: m_argTypes += 'B'; break;
                case Rsp_Param_Type_Integer_Array: m_argTypes += 'I'; break;
                case Rsp_Param_Type_Float_Array: m_argTypes += 'F'; break;
                }
            }
        }

        friend class Connection;
    public:
        explicit Response(Rsp_h _hRes) : m_hRes(_hRes)
        {
            CHR_CHECK(GetResponseInfo(m_hRes, &m_info));
            initArgTypes();
        }

        int32_t intArg(uint32_t _idx) const
        {
            int32_t res;
            CHR_CHECK(GetResponseIntArg(m_hRes, _idx, &res))
            return res;
        }

        float floatArg(uint32_t _idx) const
        {
            float res;
            CHR_CHECK(GetResponseFloatArg(m_hRes, _idx, &res))
            return res;
        }
        std::string stringArg(uint32_t _idx) const
        {
            char *str = nullptr;
            int len = 0;
            CHR_CHECK(GetResponseStringArg(m_hRes, _idx, (const char **)&str, &len))
            if(str != nullptr && len > 0) {
                str[len-1] = '\0';
                return str;
            }
            return "";
        }

        Span<int32_t> intArrayArg(uint32_t _idx) const
        {
            Span<int32_t> res;
            CHR_CHECK(GetResponseIntArrayArg(m_hRes, _idx, &res.p, &res.s))
            return res;
        }

        Span<float> floatArrayArg(uint32_t _idx) const
        {
            Span<float> res;
            CHR_CHECK(GetResponseFloatArrayArg(m_hRes, _idx, &res.p, &res.s))
            return res;
        }

        Span<int8_t> blobArg(uint32_t _idx) const
        {
            Span<int8_t> res;
            CHR_CHECK(GetResponseBlobArg(m_hRes, _idx, &res.p, &res.s));
            return res;
        }
        const TResponseInfo & info() const { return m_info; }

        [[nodiscard]] bool isError() const {
            return (info().Flag & Rsp_Flag_Error) != 0;
        }
        [[nodiscard]] bool isWarning() const {
            return (info().Flag & Rsp_Flag_Warning) != 0;
        }
        [[nodiscard]] bool isUpdate() const {
            return (info().Flag & Rsp_Flag_Update) != 0;
        }
        [[nodiscard]] bool isQuery() const {
            return (info().Flag & Rsp_Flag_Query) != 0;
        }
        Rsp_h handle() const { return m_hRes; }

        [[nodiscard]] std::string textFlags() const {
            std::string res;
            if (isError()) res += 'E';
            if (isWarning()) res += 'W';
            if (isUpdate()) res += 'U';
            if (isQuery()) res += 'Q';
            return res;
        }
        std::string toString() const {
            LibSize_t s = 0;
            ResponseToString(m_hRes, nullptr, &s);
            std::string res;
            res.resize((size_t)s);
            ResponseToString(m_hRes, res.data(), &s);
            return res;
        }
        const std::string & argTypes() const { return m_argTypes; }
    private:
        Rsp_h m_hRes;
        std::string m_argTypes;
        TResponseInfo m_info;
    };


    class Command;

    Command Cmd(const char *_scmdID);
    Command Query(const char *_scmdID);
    Command Cmd(uint32_t _cmdID);
    Command Query(uint32_t _cmdID);

    class Command
    {
    private:
        explicit Command(uint32_t _cmdID, bool _isQuery = false)
        {
            CHR_CHECK(NewCommand(_cmdID, _isQuery, &m_hCmd))
        }

        explicit Command(const char *str, bool _isQuery = false)
        {
            CHR_CHECK(NewCommand(Str2CmdID(str), _isQuery, &m_hCmd));
        }

        Cmd_h m_hCmd = Invalid_Handle;
    public:

        friend Command Cmd(const char *_scmdID) { return Command(_scmdID, false); }
        friend Command Query(const char *_scmdID) { return Command(_scmdID, true); }
        friend Command Cmd(uint32_t _cmdID) { return Command(_cmdID, false); }
        friend Command Query(uint32_t _cmdID) { return Command(_cmdID, true); }

        Command(const Command &) = delete;
        Command(Command && _src) = default;

        template < class ... Args >
        Command & add(Args... _args) {
            (add(_args), ...);
            return *this;
        }

        template<class NT>
        Command& add(NT)
        {
            throw std::runtime_error("Command::add: unsupported parameter type!");
        }

        Command & add(int32_t _n) { AddCommandIntArg(m_hCmd, _n); return *this; }
        Command & add(uint32_t _n) { AddCommandIntArg(m_hCmd, _n); return *this; }
        Command & add(float _n) { AddCommandFloatArg(m_hCmd, _n); return *this; }
        Command & add(const char *s) { AddCommandStringArg(m_hCmd, s, (int32_t)strlen(s)); return *this; }
        Command & add(const std::string& s) { AddCommandStringArg(m_hCmd, s.c_str(), (int32_t)s.length()); return *this; }
        Command & add(const std::vector< int32_t >& v) { AddCommandIntArrayArg(m_hCmd, v.data(), (int32_t)v.size()); return *this; }
        Command & add(const std::vector< float >& v) { AddCommandFloatArrayArg(m_hCmd, v.data(), (int32_t)v.size()); return *this; }
        template < class T >
        Command & add(const Span<T>& span) { AddCommandBlobArg(m_hCmd, span.p, span.s * sizeof(T)); return *this; }

        Cmd_h handle() const { return m_hCmd; }
    };

    class Connection
    {
    public:
        using ResponseCallback = std::function<void(const Response &)>;
        using DataCallback = std::function<void(internal::DataSamplesReader&)>;

        Connection() = default;
        ~Connection() { close(); }

        void open(const std::string & _conString, int deviceType = CHR_2_Device,
            int connectionMode = Connection_Asynchronous, DataCallback _onData = nullptr,
            int64_t _maxSampleCount = 1000, int32_t _readSampleTimeout = 0)
        {
            if (isConnected()) {
                throw std::runtime_error("The current connection must be closed before opening a new one!");
            }
            m_isAsync = (connectionMode == Connection_Asynchronous);
            reopen(OpenConnection(_conString.c_str(), deviceType, connectionMode, 16 * 1024 * 1024, &m_hCon),
                _onData, _maxSampleCount, _readSampleTimeout);
        }

        void openShared(Conn_h handle, int connectionMode = Connection_Asynchronous, DataCallback _onData = nullptr,
            int64_t _maxSampleCount = 1000, int32_t _readSampleTimeout = 0)
        {
            if (isConnected()) {
                throw std::runtime_error("The current connection must be closed before opening a new one!");
            }
            m_isAsync = (connectionMode == Connection_Asynchronous);
            reopen(OpenSharedConnection(handle, connectionMode, &m_hCon),
                _onData, _maxSampleCount, _readSampleTimeout);
        }

        void setDataCallback(DataCallback _onData = nullptr, int64_t _maxSampleCount = 1000, int32_t _readSampleTimeout = 0)
        {
            if (m_isAsync) {
                m_cbData = _onData; // no need to set static callback function if user function is not set
                CHR_CHECK(RegisterSampleDataCallback(m_hCon, _maxSampleCount, _readSampleTimeout,
                    this, m_cbData != nullptr ? staticSampleDataCallback : nullptr));
            }
        }

        bool isConnected() const {
            return m_hCon != Invalid_Handle;
        }

        void close() {
            if (isConnected()) {
                CloseConnection(m_hCon);
            }
            m_hCon = Invalid_Handle;
        }

        // executes a command synchronously (waits for response)
        Response execWaiting(const Command &cmd, size_t timeoutMs = 1000) {
            return execWaiting(m_hCon, cmd, timeoutMs);
        }

        Response execWaiting(Handle_t ctx, const Command &cmd, size_t timeoutMs = 1000) {

            struct {
                std::condition_variable cv;
                std::mutex mtx;
                Rsp_h rsp = Invalid_Handle;
            } S;

            if(!m_isAsync) {
                CHR_CHECK(ExecCommand(ctx, cmd.handle(), &S.rsp));
                return Response(S.rsp);
            }

            CHR_CHECK(ExecCommandAsync(ctx, cmd.handle(), &S,
                     [](TRspCallbackInfo info, Rsp_h rsp) {
                auto ps = (decltype(S) *)info.User;
                std::lock_guard<std::mutex> guard(ps->mtx);
				ps->rsp = rsp;
                ps->cv.notify_one();
            }, nullptr));

            std::unique_lock lk(S.mtx);
            if(!S.cv.wait_for(lk, std::chrono::milliseconds(timeoutMs),
                        [&S](){ return S.rsp != Invalid_Handle; })) {

                throw std::runtime_error("The command's response timed out!");
            }
            return Response(S.rsp);
        }

        int32_t exec(const Command &_cmd, ResponseCallback callback = nullptr)
        {
            return exec(m_hCon, _cmd, callback);
        }

        int32_t exec(Handle_t ctx, const Command &_cmd, ResponseCallback callback = nullptr)
        {
            if (!m_isAsync) {
                Rsp_h rsp{};
                CHR_CHECK(ExecCommand(ctx, _cmd.handle(), callback ? &rsp : nullptr))
                if (callback) {
                    std::invoke(callback, Response(rsp));
                }
                return 0;
            }

            int32_t ticket = -1;
            ResponseCallback *pItem = nullptr;
            if (callback != nullptr) {
                pItem = &(*m_responseQueue)[m_queuePos++ & queue_mask];
                *pItem = callback;
            }
            CHR_CHECK(ExecCommandAsync(ctx, _cmd.handle(), (void *)pItem,
                callback ? &staticOnResponseHandler : nullptr, &ticket))
            return ticket;
        }

        Conn_h handle() const {
            return m_hCon;
        }

        int32_t processDeviceOutput() {
            auto _res = ProcessDeviceOutput(m_hCon);
            if (!CHR_SUCCESS(_res)) {
                auto s = std::string("processDeviceOutput failed: ") + internal::chrErrorString(_res);
                throw std::runtime_error(s);
            }
            return _res;
        }

    private:
        Conn_h m_hCon = Invalid_Handle;
        bool m_isAsync = true; // this is an asynchronous connection

        static constexpr uint32_t queue_len = 16; // must be power of 2
        static constexpr uint32_t queue_mask = queue_len - 1;
        using ResponseQueue = std::array<ResponseCallback, queue_len>;
        /*
         *  we have to put the response pointer queue on the heap
         *  to keep absolute addresses valid even on connection moves etc...
         */
        std::unique_ptr<ResponseQueue> m_responseQueue;
        uint32_t m_queuePos = 0;
        DataCallback m_cbData = nullptr;
        internal::DataSamplesReader m_reader;


        void reopen(Res_t res, DataCallback _onData, int64_t _maxSampleCount, int32_t _readSampleTimeout)
        {
            m_responseQueue.reset(new ResponseQueue);
            m_reader.reset();
            m_queuePos = 0;
            if (!CHR_SUCCESS(res)) {
                m_hCon = Invalid_Handle; // this marks that we are not connected
                throw std::runtime_error(std::string("Shared connection failed: ") + internal::chrErrorString(res));
            }
            if (m_isAsync)
                setDataCallback(_onData, _maxSampleCount, _readSampleTimeout);
        }

        static void staticOnResponseHandler(TRspCallbackInfo _sInfo, Rsp_h _hRsp)
        {
            Response r(_hRsp);
            if(auto p = (ResponseCallback *)_sInfo.User; p)
                std::invoke(*p, Response(_hRsp));
        }

        static void staticSampleDataCallback(
            void *_pUser,
            int32_t _nState,
            int64_t _nSampleCount,
            const double *_pSampleBuffer,
            LibSize_t _nSizePerSample,
            const TSampleSignalGeneralInfo genSigInfo,
            TSampleSignalInfo *_pSignalInfo) try
        {
            (void)_nState;
            if (_nSampleCount <= 0 || _nSizePerSample <= 0 || !_pSignalInfo ||
                genSigInfo.GlobalSignalCount < 0 || genSigInfo.PeakSignalCount < 0)
            {
                return; // no samples or no signal information means it's useless
            }

            auto self = reinterpret_cast<Connection *>(_pUser);
            self->m_reader.update(genSigInfo, _pSignalInfo,
                (const uint8_t *)_pSampleBuffer, _nSampleCount, _nSizePerSample);

            /** TODO need to analyze nState field!
             *
             * \param[in] _nState Data reading status: negative: error or warning code, 0: time out in waiting for preset amount of data to be read,
            1: successfully read in preset amount of the data, 2: command response has been received, 3: library data Sample */

            // we have already checked for null pointer while initializing m_cbData field..
            std::invoke(self->m_cbData, self->m_reader);
        }
        catch (std::exception &ex)
        {
            std::cerr << "staticSampleDataCallback exception: " << ex.what() << std::endl;
        }
    };

} // namespace chr

#endif // CHROCO_MICRO_HXX
