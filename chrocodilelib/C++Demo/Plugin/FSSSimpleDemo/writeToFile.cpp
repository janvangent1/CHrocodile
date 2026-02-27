
#include <string>
#include <fstream>
#include "FlyingSpotSimpleDemo.h"


void DataReader::writeToFile() {

    std::cout << m_shapes.size() << " figures collected..\n";

    for(const auto& [sdata, label, sigs, metaBuf] : m_shapes) {

        auto name = std::to_string(m_counter++) + "_" + label + "_" + std::to_string(sdata.shapeCounter);
        std::ofstream ofs(name + ".csv");

        const auto& xdata = sdata;
        auto printLambda = [&xdata, &ofs](auto ptr) {

            ofs << " (" << typeid(*ptr).name() << ")\n";

            if(xdata.shapeType == FSS_PluginInterpolated2D) {
                for(size_t y = 0; y < xdata.imageH; y++) {

                    for(size_t x = 0; x < xdata.imageW; x++, ptr++) {
                        ofs << ptr[0] << ';';
                    }
                    ofs << '\n';
                } // for y

            } else { // FSS_PluginRawData

                for(size_t j = 0; j < xdata.numSamples; j++, ptr++) {
                    ofs << ';' << ptr[0] << ";\n";
                }
            }
        };

        if(sdata.shapeType == FSS_PluginInterpolated2D) {
            ofs << "bitmap ; " << sdata.imageW << "x" << sdata.imageH << " ; ";
        } else {
            ofs << "raw\n";
        }

        ofs << " signals ; ";
        for(const auto& s : sigs) {
            ofs << s.SignalID << " ; ";
        }
        ofs << '\n';

        for(size_t j = 0; j < sdata.numSignals; j++) {
            ofs << "signal #" << sigs[j].SignalID;

            auto ptr = metaBuf[j].data();

#if USE_RAW_DATA_FORMAT
            switch(sigs[j].DataType) {
            case Data_Type_Unsigned_Char:
                printLambda((const uint8_t *)ptr);
                break;
            case Data_Type_Signed_Char:
                printLambda((const int8_t *)ptr);
                break;
            case Data_Type_Unsigned_Short:
                printLambda((const uint16_t *)ptr);
                break;
            case Data_Type_Signed_Short:
                printLambda((const int16_t *)ptr);
                break;
            case Data_Type_Unsigned_Int32:
                printLambda((const uint32_t *)ptr);
                break;
            case Data_Type_Signed_Int32:
                printLambda((const int32_t *)ptr);
                break;
            case Data_Type_Float:
                printLambda((const float *)ptr);
                break;
            case Data_Type_Double:
                printLambda((const double *)ptr);
                break;
            }
#else
            // for double-data mode: all signals are doubles
            printLambda((const double *)ptr);
#endif
        }
        std::cout << "saving file " << name << '\n';
    } // for shapes

    m_shapes.clear();
}


