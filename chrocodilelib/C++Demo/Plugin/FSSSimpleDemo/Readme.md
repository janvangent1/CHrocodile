

## FSS FileManager Tool issues

There is a number of *global* CHRocodile settings (for example, like **DWD** settings) which are 
*not* transferred to FSS DLL script by the FileManager. The settings *must* be set manually (e.g., using CHRocodile Explorer) before executing the script. 

FSS FileManager currently puts only the following CHRocodile settings into FSS DLL script (which you might see in the **init**{} section of the script):
1. CHRSampleRate: SHZ,
2. LightIntensity: LAI
3. DataAverage: AVD
4. SpectrumAverage: AVS
5. RefractiveIndex: SRI
6. AbbeNumber: ABE
7. RefractiveIndexTable: SRT
8. NumberOfPeaks: NOP
9. PeakOrdering: POD
10. DistanceModeActive: OFN

## FSS DLL output data format change

The new version of FSS DLL returns the data in **double-precision** 
format where each output signal is placed into a **separate** buffer. 
The FSS DLL Callback recieves a data structure **FSS_PluginShapeData** as shown below
where **metaBuf** is an array of pointers to individual signal buffers.
For example, if the user orders the signals 65, 66 and 256, the **metaBuf** 
will be pointing to 3 buffers (of **double** data type) containing the **numSamples**
data samples each for 3 signals, respectively. Please see C++ and C# demo codes for details. 

```C++
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

    FSS_PluginDataType type; ///< output data type
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
```