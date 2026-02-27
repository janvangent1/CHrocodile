
## Flying Spot CSharp Demo Description

The demo package contains the following subprojects:
1. *Common*: small support library built providing features common to all demos, such as:
   - **FlyingSpotSensor**: class built on top of CSharp CHRocodile Wrapper to support basic Flying Spot functionality: opening/closing CHRocodile connection, **CFG**, **EXEC** and **STOP** FSS Plugin commands, user-defined callbacks, etc.
   - **ColorHeatMap**: supporting class to map floating-point values to RGB colors
   - **DataProcessor**: the collection of routines to save data signals in CSV, BCRF or RGB bitmap formats
2. *AreaScan*: demonstrates 2D interpolated rectangular scan, displays the data as an RGB bitmap
3. *DataAcquisition*: demonstrates 1D data scans, displays the data as XY-plots and saves to CSV 
4. *LargeAreaScan*: demonstrates 2D interpolated large area scan, save data in BCRF format or displays as an RGB bitmap 
  
### LargeAreaScan short manual

The basic program usage consists of the following steps:

1. *BtConnection_Click()*: opens up connection to the CHRocodile device and Flying Spot Sensor. Installs callback functions and configures FSS with the CFG file path and internal ring-buffer size

2. Use *btLoadScript_Click()* to load script to be executed: by default, the program loads 'LargeAreaScanInline.rs' file from 'Scripts' subfolder located in the binary files directory.

3. *BtRun_Click()*: compiles the loaded file and starts script execution. For each new shape scanned, the program receives **OnScanProgramCallback** function call with a shape data in command response. The shape data gets decoded into *FSS_PluginShape* and stored for later usage. For performance reasons, no data copy occurs here: instead, *FSS_PluginShape* refers to the internal FSS ring-buffer holding the actual signal data.

4. Once the scan is finished, **OnScanProgramCallback** gets called with a dummy shape of type *FSS_PluginDataType.RecipeTerminate*. At this point, the data processing might begin. The shape data is saved in BCRF file format using *DataManipuilator.SaveAsBCRF()* function or converted to an RGB bitmap with the help of *DataManipuilator.GridDataToBitmapRGB()*.

5. The current script execution can be prematurely stopped using *BtStop_Click()* function. 