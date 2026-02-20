# CHRocodile Film Thickness Measurement GUI

A Python GUI application for controlling CHRocodile 2 devices (LR, IT RW, IT DW, and other CHR 2 variants) for film thickness measurements using interferometric mode.

## Features

- **Device Connection**: Connect to CHRocodile 2 devices (LR, IT RW, IT DW, etc.) via IP address
- **Single Measurement**: Perform one-time thickness measurements on demand
- **Continuous Measurement**: Automatically measure thickness at configurable intervals (in milliseconds)
- **Live Plotting**: Real-time visualization of thickness measurements over time
- **Raw Data Visualization**: Display interferometric spectrum with peak markers
- **Simulation Mode**: Test the application without physical device connection
- **Data Export**: Export measurement data to CSV format
- **Beckhoff PLC Integration**: TCP/IP interface for triggering measurements from TwinCAT PLC/HMI

## Compatible Devices

This application is compatible with all CHRocodile 2 variants, including:
- **CHRocodile 2 LR** (Long Range)
- **CHRocodile 2 IT RW** (Interferometric, Red/White)
- **CHRocodile 2 IT DW** (Interferometric, Dual Wavelength)
- **CHRocodile 2 IT** (Interferometric)
- Other CHRocodile 2 variants using the standard CHR 2 protocol

The application uses `DeviceType.CHR_2` which provides universal support for all CHRocodile 2 devices. All commands used (MMD, NOP, SODX, SHZ, AVD, AVS, LIA, SRI, DRK, DNLD) are universal across CHR 2 variants.

## Requirements

- Python 3.7 or higher
- CHRocodile library (included in `chrocodilelib/` folder)
- Required Python packages (see `requirements.txt`)

## Installation

### Option 1: Using Virtual Environment (Recommended)

**Windows (PowerShell):**
```powershell
# Run the setup script
.\setup_venv.ps1

# If you get an execution policy error, run this first:
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser

# Activate the virtual environment
.\venv\Scripts\Activate.ps1
```

**Windows (Command Prompt):**
```cmd
# Run the setup script
setup_venv.bat

# Activate the virtual environment
venv\Scripts\activate.bat
```

**Linux/Mac:**
```bash
# Make script executable
chmod +x setup_venv.sh

# Run the setup script
./setup_venv.sh

# Activate the virtual environment
source venv/bin/activate
```

### Option 2: Manual Installation

1. Ensure Python 3.7+ is installed
2. Create a virtual environment (recommended):
   ```bash
   python -m venv venv
   # Activate it:
   # Windows: venv\Scripts\activate
   # Linux/Mac: source venv/bin/activate
   ```
3. Install required packages:
   ```bash
   pip install -r requirements.txt
   ```

4. The CHRocodile Python library should be available in the `chrocodilelib/libcore/` directory. If not, extract it from `chrocodilelib.zip`.

**Note:** A virtual environment has been created in this project. To use it:
- **Windows PowerShell**: `.\venv\Scripts\Activate.ps1`
- **Windows CMD**: `venv\Scripts\activate.bat`
- **Linux/Mac**: `source venv/bin/activate`

## Building Executable

To create a single-file executable (.exe) for distribution:

**Windows:**
```powershell
.\build_exe.ps1
```
or
```cmd
build_exe.bat
```

**Manual:**
```bash
pip install pyinstaller
python -m PyInstaller CHRocodileGUI.spec --clean
```

The executable will be created in `dist/CHRocodileGUI.exe`.

For detailed build instructions, see [BUILD_EXECUTABLE.md](docs/BUILD_EXECUTABLE.md).

## Usage

### Running the Application

**With virtual environment activated:**
```bash
python chrocodile_gui.py
```

**Without virtual environment (not recommended):**
```bash
python chrocodile_gui.py
```

### Connecting to Device

1. Enter the IP address of your CHRocodile device (default: `192.168.170.3`)
   - **Note**: Different CHRocodile 2 variants may use different default IP addresses
   - You can configure the device IP address after connecting using the "Configure Device IP" button
2. Click "Connect" button
3. Wait for connection status to show "Connected" (green)

### Performing Measurements

**Single Measurement:**
- Click "Single Measurement" button to perform one measurement
- Results are displayed immediately and added to the plot

**Continuous Measurement:**
- Enter the measurement interval in milliseconds (e.g., 100 for 100ms = 10 measurements/second)
- Click "Start Continuous" to begin automatic measurements
- Click "Stop Continuous" to stop

**Download Raw Spectrum:**
- Click "Download Spectrum" to get the raw interferometric pattern
- The spectrum will be displayed in the lower plot with peak markers

### Simulation Mode

- Check "Simulation Mode" checkbox to test the application without a physical device
- Simulation generates realistic thickness values in the 60-120 micron range
- All features work in simulation mode

### Exporting Data

- Click "Export Data" to save measurement history to a CSV file
- The file contains timestamps and thickness values

## File Structure

```
CHRocodile/
├── chrocodile_gui.py          # Main GUI application
├── device_controller.py        # Device communication logic
├── simulator.py                # Simulation functionality
├── plotter.py                  # Plotting utilities
├── requirements.txt            # Python dependencies
├── README.md                   # This file
└── chrocodilelib/              # CHRocodile library files
    └── libcore/
        └── chrpy/              # Python bindings
```

## Signal IDs

The application uses the following signal IDs:
- **256**: Thickness value (Thickness 1 in float format, in microns, already corrected for refractive index)
- **83**: Sample counter (global signal, optional, for tracking sample numbers)
- **16640**: First peak signal (from interferometric pattern)
- **16641**: Second peak signal (from interferometric pattern)

## Troubleshooting

### Connection Issues

- Verify the device IP address is correct
- Ensure the device is powered on and connected to the network
- Check firewall settings if connection fails
- Verify the CHRocodile library DLL is accessible

### Import Errors

If you get import errors for `chrpy`:
- Ensure the `chrocodilelib/libcore/` directory exists
- Check that all required library files are present
- Verify Python can access the library path

### Measurement Issues

- Ensure the probe is properly positioned
- Check that the film sample is within the measurement range (60-120 microns)
- Verify device settings are configured correctly

## Beckhoff PLC/HMI Integration

The application uses **Beckhoff's native ADS protocol** via the `pyads` library for integration with TwinCAT PLC/HMI systems.

### Requirements

- **pyads library**: `pip install pyads` (already installed in venv)
- **TwinCAT ADS DLL**: On Windows, requires TwinCAT installation (`TcAdsDll.dll`)
  - The DLL is typically installed with TwinCAT Runtime
  - If TwinCAT is not installed, you may need to install TwinCAT Runtime or copy the DLL manually
  - See [PYADS_INSTALLATION.md](docs/PYADS_INSTALLATION.md) for details
- **Linux**: Requires `libads.so` library

**Note**: If you see a warning about pyads/TwinCAT DLL, the application will still work for manual measurements, but the Beckhoff ADS interface will be disabled.

### Enabling the Interface

1. Install pyads: `pip install pyads`
2. Check the "Beckhoff ADS Interface" checkbox in the connection panel
3. Status indicator shows connection status (ADS: On/Off)

### How It Works

The interface uses a **handshake mechanism** to ensure reliable communication:

**Handshake Flow**:
1. PLC sets `bTriggerMeasurement := TRUE`
2. Python detects rising edge, sets `bMeasurementBusy := TRUE` (measurement started)
3. Python performs measurement
4. Python writes results and sets `bMeasurementBusy := FALSE`, `bMeasurementReady := TRUE` (complete)
5. PLC reads results and resets `bMeasurementReady := FALSE`

**PLC Variables**:

- **Trigger variables** (read by Python):
  - `GVL_CHRocodile.bTriggerMeasurement` - BOOL: Trigger single measurement (cyclically checked)
  - `GVL_CHRocodile.bStartContinuous` - BOOL: Start continuous measurement
  - `GVL_CHRocodile.bStopContinuous` - BOOL: Stop continuous measurement
  - `GVL_CHRocodile.nIntervalMs` - UDINT: Continuous measurement interval

- **Handshake variables** (written by Python):
  - `GVL_CHRocodile.bMeasurementBusy` - BOOL: TRUE while measurement in progress
  - `GVL_CHRocodile.bMeasurementReady` - BOOL: TRUE when measurement complete

- **Result variables** (written by Python):
  - `GVL_CHRocodile.rThickness` - REAL: Measurement thickness (μm)
  - `GVL_CHRocodile.rPeak1` - REAL: First peak position
  - `GVL_CHRocodile.rPeak2` - REAL: Second peak position
  - `GVL_CHRocodile.nMeasurementCount` - UDINT: Total measurement count
  - `GVL_CHRocodile.sError` - STRING: Error message if measurement fails

### Configuration

- **AMS NetID**: Default is `127.0.0.1.1.1` (localhost). Change in code if needed.
- **Variable Prefix**: Default is `GVL_CHRocodile.`. Can be customized.

### Documentation

For detailed integration guide and example PLC code, see [BECKHOFF_ADS_INTEGRATION.md](docs/BECKHOFF_ADS_INTEGRATION.md).

## Notes

- The application uses synchronous mode for device communication for simplicity
- Measurements are performed in a separate thread to prevent GUI freezing
- The live plot shows a rolling window of the last 1000 measurements
- Raw spectrum data shows the interferometric pattern with detected peaks
- Beckhoff integration uses native ADS protocol via pyads library

## License

This application is provided as-is for use with Precitec CHRocodile devices.

