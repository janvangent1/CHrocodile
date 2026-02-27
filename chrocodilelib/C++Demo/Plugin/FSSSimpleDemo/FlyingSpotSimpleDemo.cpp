
#include <fstream>
#include <iostream>
#include <thread>
#include <atomic>

#include "FlyingSpotSimpleDemo.h"

#ifdef _WIN32
#include <windows.h>
#endif

using namespace std::chrono_literals;
using namespace chr;

static std::atomic_bool g_done = false;

#define TIMEOUT 10000 // 10 secs timeout

/*
 * In this small FSS demo project we compile and execute a small scanner script
 * that just scans a rectangular area.
 *
 * Data from that scan is interpolated to form a bitmap with equidistant
 * pixels
 *
 * The DLL is operating in
 * ASYNCHRONOUS mode
 * (see CHRocodileLib API reference).
 *
 * "asynchronous" means here that the exec function used below
 * only initiates commands and does not wait for any result.
 * Instead, a callback function is passed in to receive any
 * command results or updates.
 *
 * Currently, the asynchronous mode is mandatory for FSS
 * applications.
 */

DataReader::~DataReader()
{
    try {
        m_conn.exec(Cmd(CmdID_Stop_Data_Stream));
        CHR_CHECK(StopAutomaticDeviceOutputProcessing(m_conn.handle()));
    }
    catch(...) {
         std::cerr << "~DataReader exception occurred!\n";
    }
}

// compiles a script and returns its handle if successful
// if callback is not provided, the function will wait until the compilation completes
uint32_t DataReader::compile(std::string_view script, int32_t type,
                                   chr::Connection::ResponseCallback compileCallback) {

    if(compileCallback != nullptr) {
        return m_conn.exec(m_pluginH, Cmd(CmdID_FlyingSpot_Compile).
                        add(type, std::string(script)), compileCallback);
    }

    std::string_view name = type == FSS_PROG_InputFile ? script : "raw string";

    auto r = m_conn.execWaiting(m_pluginH, Cmd(CmdID_FlyingSpot_Compile).
            add(type, std::string(script)), TIMEOUT);

    if(r.isError()) {
        throw std::runtime_error(std::string(name) + ": compile error: " + r.stringArg(0));
    }
    auto handle = r.intArg(0);
    std::cout << "Compiled program ID: " << std::hex << handle << std::dec << '\n';
    return handle;
}

void DataReader::startScript(uint32_t ID, chr::Connection::ResponseCallback responseFunc)
{
    std::cout << "Executing script with handle: " << std::hex << ID << std::dec << '\n';

    CHR_CHECK(m_conn.exec(m_pluginH, Cmd(CmdID_FlyingSpot_Exec).add(ID), responseFunc));
}

// saves the scanned figure for future processing: expensive data processing directly in the
// callback function shall be avoided..
void DataReader::saveData(const FSS_PluginShapeData& s) {

    const char *typ = s.shapeType == FSS_PluginRawData ? "1D data" : "2D rect";
    std::cout << "Got figure: " << s.label << " of type: " << typ << '\n';

    ShapeData shape = {s, s.label, {s.info, s.info + s.numSignals}, {}};
    shape.metaBuf.resize(s.numSignals);

    for(uint32_t i = 0; i < s.numSignals; i++)
    {
        auto beg = s.metaBuf[i];
        auto sz = SignalSize(s.info[i].DataType);
        shape.metaBuf[i] = {beg, beg + s.numSamples * sz};
    }
    m_shapes.emplace_back(std::move(shape));
}

void DataReader::pluginSetup()
{
    /*
     * Sets output data mode:
     *  - Output_Data_Format_Double: all signals of a data sample are stored as as double-precision values
     *    (easier for processing but requires more memory for data storage / movement)
     *  - Output_Data_Format_Raw: every signal can have a different data type: see TSampleSignalInfo::DataType from FSS_PluginShapeData
     */
    SetOutputDataFormatMode(m_conn.handle(), USE_RAW_DATA_FORMAT ?
                                Output_Data_Format_Raw : Output_Data_Format_Double);


    /*
     * Add FSS plug-in to connection
     */
    CHR_CHECK(AddConnectionPlugIn(m_conn.handle(), FlyingSpot_Plugin_Name, &m_pluginH))

    /*
     * Asynchronous mode:
     * Tell CHRocodileLib that we do not intend to call ProcessDeviceOutput(...) ourselves.
     * Instead, the library will start a thread itself to call ProcessDeviceOutput and
     * thus push data and command responses into the callback handlers below.
     */
    CHR_CHECK(StartAutomaticDeviceOutputProcessing(m_conn.handle()));

    std::cout << "Starting data stream..\n";
    CHR_CHECK(m_conn.exec(Cmd(CmdID_Start_Data_Stream)));

    /*
     * Configure FSS plugin: this shall be done only once at the beginning
     * afterwards the plugin can compile / execute any number of scripts
     */
    std::string cfgPath = PLUGIN_PATH "/ScannerGlobalConfig.cfg";
    if(!std::ifstream(cfgPath)) {
        cfgPath = "ScannerGlobalConfig.cfg"; // otherwise look into CWD
        if(!std::ifstream(cfgPath)) {
            throw std::runtime_error("Unable to find FSS config file!");
        }
    }
    m_conn.execWaiting(m_pluginH, Cmd(CmdID_FlyingSpot_Cfg).add(cfgPath));
}

void DataReader::runSimpleDemo()
{
    std::string fpath(__FILE__); // search for file in the source directory
    auto pos = fpath.find_last_of("\\/");
    fpath = fpath.substr(0, pos+1);

    std::array< uint32_t, 3 > progIDs{}; // a list of compiled program handles

    // set output control & data signals
    CHR_CHECK(m_conn.exec(Cmd(CmdID_Output_Signals).add(256, 65, 66, 69)));

    /*
     * compile scanner script from file
     */
    progIDs[0] = compile(fpath + "TestRecipe.rs", FSS_PROG_InputFile);

    std::exception_ptr response_exception;
    auto responseFunc = [&](const Response & _r)
    {
        try
        {
            //std::cout << "Prog response with #params: " << _r.info().ParamCount << '\n';

            //if (_r.isError())
            //    throw std::runtime_error("Error in FSS command response: " + _r.toString());

            //std::cout << "response to " << CmdID2Str(_r.info().CmdID) << "\n";
            if (_r.argTypes() != "B") // check if we received a binary blob in response
                return;

            auto blob = _r.blobArg(0);

            if(blob.s != sizeof(FSS_PluginShapeData))
                throw std::runtime_error("Error in FSS command response: unexpected output data format");

            auto pdata = (const FSS_PluginShapeData *)blob.p;
            if(pdata->shapeType == FSS_PluginRecipeTerminate)
            {
                std::cout << "Scan finished.\n";
                g_done = true;
                return;
            }

            saveData(*pdata);
        }
        catch(...)
        {
            response_exception = std::current_exception(); // capture
            g_done = true;
        }
    }; // responseFunc


    /*
     * Example 1: run the script, collect data
     */

    startScript(progIDs[0], responseFunc);

    std::cout << "Press Ctrl+C to stop recipe execution..\n";

    // wait for responses:
    while(!g_done)
        std::this_thread::sleep_for(100ms);

    std::cout << "Writing collected figures to files..\n";
    writeToFile();

    // handle exception if any:
    if (response_exception)
        std::rethrow_exception(response_exception);


    /*
     * Example 2: run several scripts in a loop
     */

    /*
     * precompile necessary scripts to save to during execution:
     * FSS Plugin maintains the database of 16 least recently compiled programs
     */
    // compile a scanner script as a raw string
    const char *recipeStr[] = { R"(
init {
    $SHZ 30000;
}
fn main(scanFreq=30000) {
    rect(-10,-20,20,10, 100, 100, label="rect400")
}
)",

    R"(
init {
    $SHZ 30000;
}
fn main(scanFreq=30000) {

    let nPts = 200, N = 10, x0 = 0, y0 = 0, R = 2

    shape(label = "poly") {
         moveTo(x0 + R, y0)
         waitUsec(10000)
         startMeasure()
         for i in range(1, N+1) {
             let ang = M_PI*2*i / N
             let X = x0 + R*cos(ang), Y = y0 + R*sin(ang)
             lineTo(X, Y, nPts)
        }
    }
}
)"};


    progIDs[1] = compile(recipeStr[0], FSS_PROG_InputString);
    progIDs[2] = compile(recipeStr[1], FSS_PROG_InputString);

    for(int num = 0; num < 3; num++) {

        for(auto ID : progIDs) {
            g_done = false; // reset "done" flag

            startScript(ID, responseFunc);

            // wait for responses:
            while(!g_done)
                std::this_thread::sleep_for(100ms);

            // handle exception if any:
            if (response_exception)
                std::rethrow_exception(response_exception);
        }

        std::cout << num << ": writing collected figures to files..\n";
        writeToFile();
    }


    /*
     * Example 3: premature termination of the running script and staring another one
     */
    {
        g_done = false; // reset "done" flag

        startScript(progIDs[1], responseFunc);

        std::this_thread::sleep_for(70ms);
        std::cout << "Stopping the current script and starting a new one..\n";

        g_done = false; // reset "done" flag
        startScript(progIDs[2], responseFunc);

        // wait for responses:
        while(!g_done)
            std::this_thread::sleep_for(100ms);

        std::cout << "writing collected figures to files..\n";
        writeToFile();
    }
}

/*
 * running a script with an infinite loop and wait trigger in between
 */
void DataReader::runInfLoopDemo()
{
    std::string fpath(__FILE__); // search for file in the source directory
    auto pos = fpath.find_last_of("\\/");
    fpath = fpath.substr(0, pos+1);

    std::condition_variable cv;
    std::mutex mtx;
    bool stepDone = false;

    std::exception_ptr response_exception;
    auto responseFunc = [&](const Response & rsp)
    {
        try
        {
            if (rsp.argTypes() != "B") // check if we received a binary blob in response
                return;

            auto blob = rsp.blobArg(0);

            if(blob.s != sizeof(FSS_PluginShapeData))
                throw std::runtime_error("Error in FSS command response: unexpected output data format");

            auto pdata = (const FSS_PluginShapeData *)blob.p;
            if(pdata->shapeType == FSS_PluginRawData) {

                std::string label(pdata->label);
                if(label == "dummy")
                {
                    std::lock_guard lk(mtx);
                    stepDone = true;    // signal the main thread that one step is finished
                    cv.notify_one();
                } else {
                    std::cout << "Got data for shape: " << label << " #" << pdata->shapeCounter << std::endl;
                }
            }
        }
        catch(...)
        {
            response_exception = std::current_exception(); // capture
            g_done = true;
        }
    }; // responseFunc

    /*
     * compile scanner script from file and start it
     */
    auto ID = compile(fpath + "49_points_inf.rs", FSS_PROG_InputFile);

    startScript(ID, responseFunc);

    g_done = false;
    // run infinite loop until Ctrl+C
    for(int step = 0; !g_done; step++) {

        {
            // wait until one scanning sequence is finished
            std::unique_lock lk(mtx);
            long timeoutMs = 100000; // sufficiently long timeout for one scanning sequence
            if(!cv.wait_for(lk, std::chrono::milliseconds(timeoutMs),
                    [&stepDone](){ return stepDone || g_done; })) {

                throw std::runtime_error("The command's response timed out!");
            }
            stepDone = false; // reset stepDone for the next iteration
        }

        std::cout << "============== scanning step " << step << " done.. processing data" << std::endl;

        // process exceptions if any occurred
        if (response_exception)
            std::rethrow_exception(response_exception);

        // do whatever needed to process currently collected data
        std::this_thread::sleep_for(1s);

        std::cout << "Starting a new scan cycle.." << std::endl;

        // use software trigger command to start a new iteration
        m_conn.exec(Cmd("STR"), [&](const Response &rsp){
            if(rsp.isError()) {
                std::cerr << "STR command failed! Aborting execution!" << std::endl;
                g_done = true;
            }
        });
    }
    // abort infinite script execution
    m_conn.execWaiting(m_pluginH, Cmd(CmdID_FlyingSpot_Stop));
    std::cout << "Execution stopped with Ctrl+C.." << std::endl;
}


#ifdef _WIN32
static BOOL WINAPI ctrlHandler(DWORD)
{
    g_done = true;
    std::cout << "Execution interrupted.." << std::endl;
    return TRUE;
}
#endif

int main(int argc, char *argv[])
{
    /*
     * Sets Log-level for CHRocodile library: setting a high log-level might be useful for debugging
     */
    //SetLibLogLevel(4);

    #ifdef _WIN32
        FilePath_t path(L".");
        SetConsoleCtrlHandler(ctrlHandler, TRUE);
        std::atexit([]() { system("PAUSE"); });
    #else
        FilePath_t path(".");
    #endif
    (void)path;

    /*
     * Instantiate / open connection
     * in ASYNCHRONOUS MODE (currently mandatory for FSS applications),
     * then call run() to compile and execute scan script
     */

    try
    {
        /*
         * sets directory where CHRocodile library log files shall be created:
         * useful for debugging when SetLibLogLevel is set
         */
        //SetLibLogFileDirectory(path, 1024, 1024);

        std::string deviceAddr(argc > 1 ? argv[1] : "IP:192.168.170.2");
        std::cout << "Opening device " << deviceAddr << std::endl;

        Connection conn;
        conn.open(deviceAddr, CHR_2_Device, Connection_Asynchronous);

        DataReader reader(conn);

        // setup plugin
        reader.pluginSetup();

        reader.runSimpleDemo();
        reader.runInfLoopDemo();

        return 0;
    }
    catch (std::exception &e)
    {
        std::cerr << "Exception: " << e.what() << std::endl;
        return -1;
    }
    catch (...)
    {
        std::cerr << "Unknown exception occurred!" << std::endl;
        return -1;
    }
}
