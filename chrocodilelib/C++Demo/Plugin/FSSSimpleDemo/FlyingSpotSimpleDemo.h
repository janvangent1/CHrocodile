#include <list>
#include "CHRocodileLibPluginDef.h"
#include "cxx/chroco_micro.hxx"

// whether to use raw data format or "all doubles" data format
#define USE_RAW_DATA_FORMAT 0

class DataReader
{
public:
    explicit DataReader(chr::Connection& conn) : m_conn(conn) {}

    ~DataReader();

    // initial plugin setup
    void pluginSetup();

    // compiles a script and returns its handle if successful
    // if callback is not provided, the function will wait until the compilation completes
    uint32_t compile(std::string_view script, int32_t type,
                           chr::Connection::ResponseCallback callback = nullptr);

    // starts script execution
    void startScript(uint32_t ID, chr::Connection::ResponseCallback responseFunc);

    // several simple demos on how to compile / run / stop scripts
    void runSimpleDemo();

    // running a script with an infinite loop and wait trigger in between
    void runInfLoopDemo();

    void writeToFile();

private:
    // simple struct to hold shape data for further processing
    struct ShapeData {
        FSS_PluginShapeData sdata;
        std::string label;
        std::vector< TSampleSignalInfo > sigs;
        std::vector< std::vector< uint8_t >> metaBuf;
    };

    // saves the data of the currently scanned shape for future processing
    void saveData(const FSS_PluginShapeData& s);

    // obtains the data size in bytes based on the signal type
    size_t SignalSize(int type)
    {
        switch (type)
        {
        case Data_Type_Unsigned_Char:
        case Data_Type_Signed_Char:
            return 1;
        case Data_Type_Unsigned_Short:
        case Data_Type_Signed_Short:
            return 2;
        case Data_Type_Unsigned_Int32:
        case Data_Type_Signed_Int32:
        case Data_Type_Float:
            return 4;
        case Data_Type_Double:
            return 8;
        default:
            throw std::runtime_error("Unexpected data type: " + std::to_string(type));
        }
    }

    chr::Connection& m_conn;
    Plugin_h m_pluginH{};

    uint32_t m_counter = 0;
    std::list< ShapeData > m_shapes;
};
