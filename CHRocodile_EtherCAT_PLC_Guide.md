# CHRocodile 2 — EtherCAT/PLC Integration Guide

**Based on:** existing Python GUI program review + CHRocodile 2 Command Reference (Precitec, R1.5.2, 01/2025)

---

## 1. What the Current Python Program Does (What to Replicate on the PLC)

### 1.1 Architecture Today vs. Tomorrow

**Today (ADS workaround):**
```
CHRocodile 2 LR ──Ethernet/TCP─→ PC (Python GUI) ──ADS/pyads─→ Beckhoff PLC
      192.168.170.2                 device_controller.py               TwinCAT
```
The PC is the middleman: it connects to the sensor over TCP port 7891 (binary packet protocol), requests measurement data, then writes results to the PLC via ADS variables. The PLC can also trigger measurements through the ADS handshake.

**Tomorrow (EtherCAT direct):**
```
CHRocodile 2 (EtherCAT) ──EtherCAT────→ Beckhoff PLC
                                          TwinCAT master
```
The PLC becomes the EtherCAT master. No PC middleman. Data arrives cyclically in the process image every bus cycle. Configuration happens via SDO at startup.

---

### 1.2 Sensor Configuration Applied by the Python Program

These settings must be replicated on the PLC via EtherCAT SDO writes at startup:

| Setting | Python command | Value used | EtherCAT SDO | Notes |
|---|---|---|---|---|
| Measuring mode | `MMD 1` | Interferometric | `0x200B = 1` | **Critical — must be set first** |
| Number of peaks | `NOP 2` | 2 peaks | `0x2003 = 2` | Needed for film thickness |
| Sample frequency | `SHZ 1000` | 1000 Hz default | `0x2004 = 1000.0` (f32) | Configurable by user |
| Data averaging | `AVD 1` | 1 (no averaging) | `0x200D = 1` (u16) | 1 = raw, higher = slower but smoother |
| Spectrum averaging | `AVS 1` | 1 | `0x2010 = 1` (u16) | |
| Lamp intensity | `LAI 50` | 50% | `0x2002 = 50.0` (f32) | Percent |
| Refractive index | `SRI 1.5 1.5` | 1.5 | `0x2011:02 = 1.5`, then `0x2011:01 = 1` | Write value(s) to :02…:11, then write count to :01 to execute |
| Dark reference | `DRK` | (on demand) | `0x2001:01 = 1` (u8) | Initiate; poll `:01` until ≠ 1 to confirm complete |
| Save to flash | `SSU` | (optional) | `0x2008 = 1` (u8) | Persists config across power cycles |

---

### 1.3 Measurement Signals Used

The Python program reads these signals from the sensor data stream. On EtherCAT these become PDO-mapped variables:

| Signal ID | Name | EtherCAT Object | Type | Description |
|---|---|---|---|---|
| **256** | Distance/Thickness Peak 1 (float) | `0x2700:01` | f32 | **Primary thickness result in µm. Already includes refractive index correction (geometrical thickness). This is the main output.** |
| **260** | Median of Peak 1 (float) | Arbitrary signal via `0x2503:01` | f32 | Median-filtered thickness. The Python program can use this instead of raw thickness. Map by writing 260 to `0x2503:01` |
| **257** | Intensity/Quality Peak 1 | `0x2708:01` | f32 | FFT peak quality (interferometric mode). Used for quality filtering. |
| **82** | InterferomIntensity | `0x2652:00` | f32 | Highest intensity on detector, in % of full well. Used as "intensity" display value. |
| **83** | SampleCounter | `0x2653:00` | u16 | Increments with each processed sample. Useful to detect missed samples. |
| **80** | NumberOfValidPeaks | `0x2650:00` | u16 | Number of peaks found in spectrum. 0 = no valid measurement. |

> **Important:** In the current Python code, Signal 256 is described as already providing **geometrical thickness in µm** with refractive index already applied. No manual post-processing is needed. The same applies via EtherCAT: `0x2700:01` gives the result directly in µm.

---

### 1.4 Quality Threshold Logic

The Python program has a configurable quality threshold. When the quality signal (Signal 257, FFT peak quality) falls below the threshold, the reported thickness value is **forced to 0.0** before being passed to the PLC. This prevents the PLC from acting on unreliable measurements.

**Replicate this in PLC Structured Text:**
```pascal
IF rQuality < rQualityThreshold THEN
    rThicknessOut := 0.0;
ELSE
    rThicknessOut := rThicknessRaw;
END_IF
```

The default quality threshold in the Python program is **0.0** (disabled). Tune this based on your process — typical useful values are in the range 0.1–0.5 depending on material and measurement conditions.

---

### 1.5 PLC Handshake Protocol (Current ADS Approach — for Reference)

The current ADS interface uses this polling-based trigger/handshake with the following PLC variables (prefix `GVL_CHRocodile.`):

| Variable | Type | Direction | Purpose |
|---|---|---|---|
| `bTriggerMeasurement` | BOOL | PLC→PC | Rising edge = trigger single measurement |
| `bStartContinuous` | BOOL | PLC→PC | Rising edge = start continuous |
| `bStopContinuous` | BOOL | PLC→PC | Rising edge = stop |
| `nIntervalMs` | UDINT | PLC→PC | Interval in ms for continuous mode |
| `bMeasurementBusy` | BOOL | PC→PLC | Measurement in progress |
| `bMeasurementReady` | BOOL | PC→PLC | Result is valid |
| `bMeasurementAck` | BOOL | PLC→PC | PLC acknowledged result |
| `rThickness` | REAL | PC→PLC | Thickness result (µm) |
| `rPeak1`, `rPeak2` | REAL | PC→PLC | Peak positions |
| `nMeasurementCount` | UDINT | PC→PLC | Running counter |
| `sError` | STRING | PC→PLC | Error description |

**On EtherCAT, you no longer need any of this.** Data flows automatically every bus cycle.

---

## 2. Beckhoff PLC Hardware and TwinCAT Setup

### 2.1 Hardware Connection

1. Connect the CHRocodile 2 EtherCAT **IN** port to your Beckhoff EtherCAT master port (or to the OUT port of the previous slave in chain).
2. If the sensor is the last device in the chain, the EtherCAT **OUT** port is not connected.
3. The sensor still has its Ethernet (TCP/IP) port — you can use this simultaneously for configuration/diagnostics without disrupting EtherCAT.

### 2.2 Get the ESI File

The EtherCAT Slave Information (ESI) XML file is stored on the sensor itself. Download it **before** switching to pure EtherCAT operation while the sensor is still reachable via TCP:

```
Open browser → http://192.168.170.2/esi   (replace with your sensor IP)
Save the XML file to disk.
```

Copy this `.xml` file to the TwinCAT ESI directory:
```
C:\TwinCAT\3.1\Config\Io\EtherCAT\
```

Restart TwinCAT XAE / TwinCAT System to load the new ESI.

### 2.3 Scan and Add the Slave in TwinCAT

1. In **TwinCAT XAE** → **I/O** → **Devices** → right-click → **Scan**.
2. The CHRocodile 2 should appear as an EtherCAT slave.
3. Accept the scan result. The device appears in the I/O tree with its PDOs.

### 2.4 Configure PDO Mapping

This is the most critical step — it replaces the Python `SODX` command.

> **Important:** SODX is **not supported** on EtherCAT. Signal selection is done through PDO mapping in TwinCAT.

PDO mapping can only be changed when the device is in **Pre-Operational (PREOP)** state. TwinCAT handles this automatically during configuration mode.

In TwinCAT XAE → select the CHRocodile slave → **Process Data** tab:

**Control PDO (0x1A00) — Always first, always include:**

| SubIndex | Object | Type | Signal |
|---|---|---|---|
| :01 | 0x2406 | u8 | Frame counter |
| :02 | 0x2400 | u8 | Number of configured samples |
| :03 | 0x2401 | u8 | Number of recorded samples |
| :04 | 0x2407 | u8 | Flags (bit 0 = sample lost) |

**Data PDO (0x1A01) — Map your measurement signals here:**

| Object | Type | Signal | Description |
|---|---|---|---|
| 0x2700:01 | f32 | 256_Distance/Thickness Peak 1 | **Primary thickness (µm)** |
| 0x2708:01 | f32 | 257_Intensity/Quality Peak 1 | Quality for threshold filtering |
| 0x2652:00 | f32 | 82_InterferomIntensity | Detector intensity (%) |
| 0x2653:00 | u16 | 83_SampleCounter | Sample counter |
| 0x2650:00 | u16 | 80_NumberOfValidPeaks | Valid peaks count |

**For Median 1 (Signal 260) — Arbitrary signal mapping:**

Signal 260 is not a pre-defined "common peak signal" so it must be added as an arbitrary signal. Do this via an SDO write at startup (see Section 3 below), **before** going to OP state:

1. Write `260` (as u16) to SDO `0x2501:01` — this registers Signal 260 as the first arbitrary signal slot, uint16 type.
   - Or write `260` to `0x2503:01` if you want float (f32) output — use this for µm accuracy.
2. Then in the PDO mapping, add `0x2503:01` (f32 arbitrary signal 1, channel 0) to PDO 0x1A01.

After all PDO content is defined, apply with SDO write: `0x23FF = 1`.

### 2.5 Oversampling Configuration

If your sensor sample rate (SHZ) is higher than your EtherCAT bus cycle:

```
oversampling_factor = SHZ / EtherCAT_bus_freq + 3   (the +3 is for jitter)
```

**Example:** SHZ = 1000 Hz, bus = 1 kHz → factor = 1 + 3 = 4 → configure PDOs 0x1A01 through 0x1A04 with identical mapping, assign all 5 PDOs (0x1A00 + 4 data PDOs) via 0x1C13.

**Example:** SHZ = 4000 Hz, bus = 1 kHz → factor = 4 + 3 = 7 → configure 0x1A01 through 0x1A07.

For simplicity at 1 kHz sensor rate and 1 kHz bus rate: one data PDO is sufficient (factor = 1, no true oversampling needed, configure 1 spare = 0x1A01 and 0x1A02).

### 2.6 Trigger Mode

Set SDO `0x2006` (TMOD) to the desired string value:

| Mode | SDO value | Description |
|---|---|---|
| `"CTN"` | `CTN` | **Free run** — sensor acquires at its own SHZ rate. Recommended for continuous film monitoring. |
| `"ECAT"` | `ECAT` | **EtherCAT synchronized** — acquisition synced to Sync1 signal. Best timing accuracy. Requires Distributed Clocks configuration. |
| `"TRG"` | `TRG` | Trigger once — one sample per trigger. Trigger via SDO `0x2007:01 = 1`. |
| `"TRE"` | `TRE` | Trigger each — one sample per trigger event on the trigger input pin. |

**For continuous film thickness monitoring, use `"CTN"` (free run).** This is the simplest and most straightforward mode, equivalent to what the Python program does in continuous measurement mode.

**For ECAT mode:** The Sync1 period must equal or be longer than the sensor's total sample period:
```
Required Sync1 period ≥ (AVD × AVS) / SHZ  seconds
```
And SHZ must be an integer multiple of the Sync1 frequency.

---

## 3. TwinCAT PLC Structured Text Code

### 3.1 Required TwinCAT Libraries

Add to your PLC project references:
- `Tc2_EtherCAT` — for `FB_EcCoESdoRead`, `FB_EcCoESdoWrite`, `FB_EcSlaveState`
- `Tc2_Standard` — standard ST library

---

### 3.2 Global Variable List — `GVL_CHRocodile`

```pascal
// GVL_CHRocodile.TcGVL
VAR_GLOBAL
    // --- PDO inputs from sensor (linked to EtherCAT process image) ---
    // Link these to the EtherCAT slave PDO entries in TwinCAT configuration
    nFrameCounter       : BYTE;     // 0x1A00:01 — increments each frame
    nNumConfigSamples   : BYTE;     // 0x1A00:02 — PDOs configured
    nNumRecordedSamples : BYTE;     // 0x1A00:03 — samples actually received
    nFlags              : BYTE;     // 0x1A00:04 — bit 0: sample lost

    rThicknessRaw       : REAL;     // 0x2700:01 — Thickness Peak 1 (µm)
    rQuality            : REAL;     // 0x2708:01 — FFT quality
    rIntensity          : REAL;     // 0x2652:00 — Detector intensity (%)
    nSampleCounter      : UINT;     // 0x2653:00 — Sample counter
    nValidPeaks         : UINT;     // 0x2650:00 — Number of valid peaks
    rMedian1            : REAL;     // 0x2503:01 — Median of Thickness (if mapped)

    // --- Processed outputs for machine logic ---
    rThicknessOut       : REAL;     // Quality-filtered thickness (µm)
    bMeasurementValid   : BOOL;     // TRUE when nValidPeaks > 0 and quality OK
    bSampleLost         : BOOL;     // TRUE when frame flags indicate loss

    // --- Configuration parameters ---
    rQualityThreshold   : REAL := 0.0;    // Below this → output 0.0 (0 = disabled)
    bUseMedianFilter    : BOOL := FALSE;  // TRUE = use Median1, FALSE = use raw thickness

    // --- Sensor state ---
    bSensorReady        : BOOL;           // TRUE when sensor is in OP state
    bInitDone           : BOOL;           // TRUE after startup SDO config complete
END_VAR
```

---

### 3.3 Function Block — `FB_CHRocodile2_EtherCAT`

```pascal
// FB_CHRocodile2_EtherCAT.TcPOU
// Handles startup initialization via SDO and cyclic data processing.
// One instance should run in a PLC task synchronized with the EtherCAT cycle.

FUNCTION_BLOCK FB_CHRocodile2_EtherCAT
VAR_INPUT
    bEnable             : BOOL;            // Enable this FB
    nMasterNetId        : T_AmsNetId;      // TwinCAT AMS NetId of EtherCAT master
    nSlaveAddr          : UINT := 1001;    // EtherCAT slave address (from scan)
    rMeasRate_Hz        : REAL := 1000.0;  // Sample frequency [Hz]
    nDataAvg            : UINT := 1;       // Data averaging (AVD)
    nSpectrumAvg        : UINT := 1;       // Spectrum averaging (AVS)
    rLampIntensity      : REAL := 50.0;    // Lamp intensity [%]
    rRefractiveIndex    : REAL := 1.5;     // Refractive index n
    bDosDarkRef         : BOOL;            // Rising edge: perform dark reference
    bSaveSetup          : BOOL;            // Rising edge: save config to sensor flash
    nMeasMode           : USINT := 1;      // 0=Confocal, 1=Interferometric
    nNumPeaks           : USINT := 2;      // Number of peaks (NOP)
    sTriggerMode        : STRING := 'CTN'; // 'CTN', 'ECAT', 'TRG', 'TRE'
END_VAR
VAR_OUTPUT
    bInitialized        : BOOL;
    bError              : BOOL;
    sErrorMsg           : STRING(255);
    nStep               : INT;
END_VAR
VAR
    // SDO write function blocks — one per configuration step
    fbSdoWrite          : FB_EcCoESdoWrite;
    bSdoBusy            : BOOL;
    bSdoError           : BOOL;
    nSdoErrId           : UDINT;

    // SDO read for dark reference poll
    fbSdoRead           : FB_EcCoESdoRead;

    // Step sequencer
    nInitStep           : INT := 0;
    bStepDone           : BOOL;
    tStepTimer          : TON;

    // Dark reference tracking
    bDarkRefPrev        : BOOL;
    bDarkRefRequested   : BOOL;

    // Save setup tracking
    bSaveSetupPrev      : BOOL;

    // SDO write buffer (max 4 bytes)
    aWriteBuf           : ARRAY[0..3] OF BYTE;
    nWriteLen           : UDINT;

    // Temp values for SDO encoding
    rTempReal           : REAL;
    nTempUint           : UINT;
    nTempUsint          : USINT;
END_VAR

// -----------------------------------------------------------------------
// Rising edge detection
// -----------------------------------------------------------------------
IF bDosDarkRef AND NOT bDarkRefPrev THEN
    bDarkRefRequested := TRUE;
END_IF
bDarkRefPrev := bDosDarkRef;

// -----------------------------------------------------------------------
// Init state machine — runs once on enable, sequentially writes SDOs
// -----------------------------------------------------------------------
IF bEnable AND NOT bInitialized AND NOT bError THEN

    CASE nInitStep OF

    0: // Wait one cycle for slave to be accessible
        tStepTimer(IN := TRUE, PT := T#500MS);
        IF tStepTimer.Q THEN
            tStepTimer(IN := FALSE);
            nInitStep := 10;
        END_IF

    10: // Set measuring mode: MMD → 0x200B = nMeasMode (u8)
        _SdoWriteU8(16#200B, 0, nMeasMode);
        IF bStepDone THEN nInitStep := 20; END_IF

    20: // Set number of peaks: NOP → 0x2003 = nNumPeaks (u8)
        _SdoWriteU8(16#2003, 0, nNumPeaks);
        IF bStepDone THEN nInitStep := 30; END_IF

    30: // Set sample frequency: SHZ → 0x2004 = rMeasRate_Hz (f32)
        _SdoWriteF32(16#2004, 0, rMeasRate_Hz);
        IF bStepDone THEN nInitStep := 40; END_IF

    40: // Set data averaging: AVD → 0x200D = nDataAvg (u16)
        _SdoWriteU16(16#200D, 0, nDataAvg);
        IF bStepDone THEN nInitStep := 50; END_IF

    50: // Set spectrum averaging: AVS → 0x2010 = nSpectrumAvg (u16)
        _SdoWriteU16(16#2010, 0, nSpectrumAvg);
        IF bStepDone THEN nInitStep := 60; END_IF

    60: // Set lamp intensity: LAI → 0x2002 = rLampIntensity (f32)
        _SdoWriteF32(16#2002, 0, rLampIntensity);
        IF bStepDone THEN nInitStep := 70; END_IF

    70: // Set refractive index — first write value to layer 1 (subindex 02)
        // SRI → 0x2011:02 = rRefractiveIndex (f32)
        _SdoWriteF32(16#2011, 2, rRefractiveIndex);
        IF bStepDone THEN nInitStep := 80; END_IF

    80: // Then commit by writing number of layers to subindex 01
        // 0x2011:01 = 1 (u8) — executes the SRI command
        _SdoWriteU8(16#2011, 1, 1);
        IF bStepDone THEN nInitStep := 90; END_IF

    90: // Register arbitrary signal 260 (Median 1 float) at slot 0x2503:01
        // 0x2503:01 = 260 (u16) — maps Signal ID 260 as first f32 arbitrary signal
        _SdoWriteU16(16#2503, 1, 260);
        IF bStepDone THEN nInitStep := 100; END_IF

    100: // Apply PDO set: 0x23FF = 1
        _SdoWriteU8(16#23FF, 0, 1);
        IF bStepDone THEN nInitStep := 110; END_IF

    110: // Set trigger mode: TMOD → 0x2006 = sTriggerMode (variable string)
        // Use _SdoWriteStr for variable-length string SDO
        _SdoWriteStr(16#2006, 0, sTriggerMode);
        IF bStepDone THEN nInitStep := 120; END_IF

    120: // Start data output: STA → 0x2009 = 1 (u8)
        _SdoWriteU8(16#2009, 0, 1);
        IF bStepDone THEN
            nInitStep := 200;
            bInitialized := TRUE;
        END_IF

    200: // Initialized — idle, handle on-demand requests
        bInitialized := TRUE;

        // Dark reference requested
        IF bDarkRefRequested THEN
            bDarkRefRequested := FALSE;
            nInitStep := 210;
        END_IF

        // Save setup requested
        IF bSaveSetup AND NOT bSaveSetupPrev THEN
            nInitStep := 220;
        END_IF
        bSaveSetupPrev := bSaveSetup;

    210: // Initiate dark reference: DRK → 0x2001:01 = 1 (u8)
        _SdoWriteU8(16#2001, 1, 1);
        IF bStepDone THEN nInitStep := 211; END_IF

    211: // Wait until 0x2001:01 ≠ 1 (dark reference complete)
        fbSdoRead(
            sNetId    := nMasterNetId,
            nSlaveAddr:= nSlaveAddr,
            nIndex    := 16#2001,
            nSubIndex := 1,
            nLen      := 1,
            pDstBuf   := ADR(aWriteBuf),
            bExecute  := TRUE,
            tTimeout  := T#2S
        );
        IF NOT fbSdoRead.bBusy THEN
            fbSdoRead(bExecute := FALSE);
            IF aWriteBuf[0] <> 1 THEN
                nInitStep := 200; // Done
            END_IF
        END_IF

    220: // Save setup: SSU → 0x2008 = 1 (u8)
        _SdoWriteU8(16#2008, 0, 1);
        IF bStepDone THEN nInitStep := 200; END_IF

    END_CASE
END_IF

// -----------------------------------------------------------------------
// Helper: Write u8 SDO
// -----------------------------------------------------------------------
METHOD PRIVATE _SdoWriteU8
VAR_INPUT
    nIndex    : UINT;
    nSubIndex : USINT;
    nValue    : USINT;
END_VAR
    aWriteBuf[0] := nValue;
    _SdoExec(nIndex, nSubIndex, 1);
END_METHOD

// -----------------------------------------------------------------------
// Helper: Write u16 SDO
// -----------------------------------------------------------------------
METHOD PRIVATE _SdoWriteU16
VAR_INPUT
    nIndex    : UINT;
    nSubIndex : USINT;
    nValue    : UINT;
END_VAR
    MEMCPY(ADR(aWriteBuf), ADR(nValue), 2);
    _SdoExec(nIndex, nSubIndex, 2);
END_METHOD

// -----------------------------------------------------------------------
// Helper: Write f32 SDO
// -----------------------------------------------------------------------
METHOD PRIVATE _SdoWriteF32
VAR_INPUT
    nIndex    : UINT;
    nSubIndex : USINT;
    rValue    : REAL;
END_VAR
    MEMCPY(ADR(aWriteBuf), ADR(rValue), 4);
    _SdoExec(nIndex, nSubIndex, 4);
END_METHOD

// -----------------------------------------------------------------------
// Helper: Write string SDO (variable length)
// -----------------------------------------------------------------------
METHOD PRIVATE _SdoWriteStr
VAR_INPUT
    nIndex    : UINT;
    nSubIndex : USINT;
    sValue    : STRING(8);
END_VAR
VAR
    nLen : UDINT;
END_VAR
    nLen := LEN(sValue);
    MEMCPY(ADR(aWriteBuf), ADR(sValue), MIN(nLen, 4));
    _SdoExec(nIndex, nSubIndex, nLen);
END_METHOD

// -----------------------------------------------------------------------
// Execute SDO write and manage bStepDone flag
// -----------------------------------------------------------------------
METHOD PRIVATE _SdoExec
VAR_INPUT
    nIndex    : UINT;
    nSubIndex : USINT;
    nLen      : UDINT;
END_VAR
    bStepDone := FALSE;

    fbSdoWrite(
        sNetId     := nMasterNetId,
        nSlaveAddr := nSlaveAddr,
        nIndex     := nIndex,
        nSubIndex  := nSubIndex,
        nLen       := nLen,
        pSrcBuf    := ADR(aWriteBuf),
        bExecute   := TRUE,
        tTimeout   := T#2S
    );

    IF NOT fbSdoWrite.bBusy THEN
        fbSdoWrite(bExecute := FALSE);
        IF fbSdoWrite.bError THEN
            bError := TRUE;
            sErrorMsg := CONCAT('SDO write failed at index 0x', UINT_TO_STRING(nIndex));
        ELSE
            bStepDone := TRUE;
        END_IF
    END_IF
END_METHOD
```

---

### 3.4 Main PLC Task — Cyclic Data Processing

```pascal
// MAIN.TcPOU  (or your main cyclic task)
PROGRAM MAIN
VAR
    fbCHR2          : FB_CHRocodile2_EtherCAT;
    bLastFrameCount : BYTE;  // For detecting new data
    bNewSample      : BOOL;
END_VAR

// -----------------------------------------------------------------------
// Startup and SDO initialization
// -----------------------------------------------------------------------
fbCHR2(
    bEnable           := TRUE,
    nMasterNetId      := '',          // Leave empty for local TwinCAT runtime
    nSlaveAddr        := 1001,        // Match your EtherCAT slave address
    rMeasRate_Hz      := 1000.0,
    nDataAvg          := 1,
    nSpectrumAvg      := 1,
    rLampIntensity    := 50.0,
    rRefractiveIndex  := 1.5,
    bDosDarkRef       := FALSE,       // Set TRUE momentarily to trigger dark ref
    bSaveSetup        := FALSE,
    nMeasMode         := 1,           // 1 = Interferometric
    nNumPeaks         := 2,
    sTriggerMode      := 'CTN'        // Free run continuous
);

GVL_CHRocodile.bInitDone  := fbCHR2.bInitialized;
GVL_CHRocodile.bSensorReady := fbCHR2.bInitialized AND NOT fbCHR2.bError;

// -----------------------------------------------------------------------
// Detect new sample using frame counter
// The frame counter increments each time the sensor updates the PDO.
// -----------------------------------------------------------------------
bNewSample := (GVL_CHRocodile.nFrameCounter <> bLastFrameCount);
bLastFrameCount := GVL_CHRocodile.nFrameCounter;

// -----------------------------------------------------------------------
// Process measurement data (runs every PLC cycle, check bNewSample for fresh data)
// -----------------------------------------------------------------------
IF GVL_CHRocodile.bSensorReady AND bNewSample THEN

    // Check for sample loss
    GVL_CHRocodile.bSampleLost := (GVL_CHRocodile.nFlags AND 1) <> 0;

    // Measurement validity: at least one valid peak detected
    GVL_CHRocodile.bMeasurementValid :=
        (GVL_CHRocodile.nValidPeaks > 0) AND
        (NOT GVL_CHRocodile.bSampleLost);

    // Choose thickness source (raw or median filtered)
    IF GVL_CHRocodile.bUseMedianFilter THEN
        GVL_CHRocodile.rThicknessOut := GVL_CHRocodile.rMedian1;
    ELSE
        GVL_CHRocodile.rThicknessOut := GVL_CHRocodile.rThicknessRaw;
    END_IF

    // Quality threshold filter — replicate Python behavior:
    // If quality < threshold, report 0.0 (invalid measurement)
    IF GVL_CHRocodile.rQualityThreshold > 0.0 THEN
        IF GVL_CHRocodile.rQuality < GVL_CHRocodile.rQualityThreshold THEN
            GVL_CHRocodile.rThicknessOut := 0.0;
            GVL_CHRocodile.bMeasurementValid := FALSE;
        END_IF
    END_IF

    // Guard against sensor not finding any peak
    IF GVL_CHRocodile.nValidPeaks = 0 THEN
        GVL_CHRocodile.rThicknessOut := 0.0;
        GVL_CHRocodile.bMeasurementValid := FALSE;
    END_IF

END_IF

// -----------------------------------------------------------------------
// Your machine logic reads:
//   GVL_CHRocodile.rThicknessOut    → film thickness in µm
//   GVL_CHRocodile.bMeasurementValid → validity flag
//   GVL_CHRocodile.rIntensity        → detector intensity %
//   GVL_CHRocodile.rQuality          → FFT quality
//   GVL_CHRocodile.nSampleCounter    → for diagnostics
// -----------------------------------------------------------------------
```

---

### 3.5 Variable Linking in TwinCAT I/O

After writing the PLC code, **link the PDO inputs** to the `GVL_CHRocodile` variables in TwinCAT XAE:

In the **System Manager / I/O** tree, expand the CHRocodile EtherCAT slave → expand **Inputs**:

| PDO Channel | Link to PLC variable |
|---|---|
| `FrameCounter` (0x2406) | `GVL_CHRocodile.nFrameCounter` |
| `NumConfiguredSamples` (0x2400) | `GVL_CHRocodile.nNumConfigSamples` |
| `NumRecordedSamples` (0x2401) | `GVL_CHRocodile.nNumRecordedSamples` |
| `Flags` (0x2407) | `GVL_CHRocodile.nFlags` |
| `Thickness_Peak1` (0x2700:01) | `GVL_CHRocodile.rThicknessRaw` |
| `Quality_Peak1` (0x2708:01) | `GVL_CHRocodile.rQuality` |
| `InterferomIntensity` (0x2652:00) | `GVL_CHRocodile.rIntensity` |
| `SampleCounter` (0x2653:00) | `GVL_CHRocodile.nSampleCounter` |
| `NumberOfValidPeaks` (0x2650:00) | `GVL_CHRocodile.nValidPeaks` |
| `ArbitrarySignal1_f32` (0x2503:01) | `GVL_CHRocodile.rMedian1` |

Right-click each input channel → **Change Link...** → select the corresponding GVL variable.

---

## 4. Step-by-Step Commissioning Checklist

### Phase 1: Preparation (while sensor still reachable via TCP)
- [ ] Connect to sensor via browser: `http://192.168.170.2/esi` — download ESI file
- [ ] Note current sensor configuration via Python GUI "View Config" button (save as reference)
- [ ] Perform dark reference via Python GUI (confirm sensor is calibrated before handover)
- [ ] Note the refractive index, lamp intensity, and threshold values currently in use

### Phase 2: TwinCAT Hardware Configuration
- [ ] Copy ESI XML file to `C:\TwinCAT\3.1\Config\Io\EtherCAT\`
- [ ] Restart TwinCAT XAE to load ESI
- [ ] Wire EtherCAT cable to sensor EtherCAT IN port
- [ ] Scan EtherCAT network in TwinCAT (I/O → Devices → Scan)
- [ ] Verify CHRocodile 2 appears as slave with correct vendor/product ID

### Phase 3: PDO Configuration
- [ ] In TwinCAT slave Process Data tab, configure PDO 0x1A00 (Control)
- [ ] Configure PDO 0x1A01 (Data): map Thickness, Quality, Intensity, SampleCounter, ValidPeaks
- [ ] For Median 1: plan arbitrary signal setup via SDO 0x2503:01 in PLC startup code
- [ ] Calculate oversampling if SHZ > bus rate; add extra data PDOs accordingly
- [ ] Assign PDOs via 0x1C13

### Phase 4: PLC Code
- [ ] Add `Tc2_EtherCAT` library to PLC project
- [ ] Create `GVL_CHRocodile` global variable list
- [ ] Create `FB_CHRocodile2_EtherCAT` function block
- [ ] Add `MAIN` cyclic processing code
- [ ] Compile — resolve any errors

### Phase 5: Variable Linking
- [ ] Link all PDO inputs to GVL variables (right-click → Change Link)
- [ ] Verify slave address matches `nSlaveAddr` parameter in FB
- [ ] Activate TwinCAT configuration

### Phase 6: First Run and Validation
- [ ] Boot TwinCAT in Config mode → check slave state = PREOP
- [ ] Boot into Run mode → verify slave reaches OP state
- [ ] Watch `GVL_CHRocodile.nSampleCounter` — should increment continuously
- [ ] Watch `GVL_CHRocodile.rThicknessRaw` — should show measurement values
- [ ] Watch `GVL_CHRocodile.nFrameCounter` — should change every bus cycle
- [ ] Verify `GVL_CHRocodile.bSampleLost` stays FALSE (no data loss)
- [ ] Compare thickness readings with previous Python GUI readings on same target
- [ ] Adjust `rQualityThreshold` to match Python GUI setting

---

## 5. Key Differences and Things to Watch Out For

### 5.1 No More SODX
SODX is explicitly **not supported** on EtherCAT. Signal selection is done entirely through PDO mapping in TwinCAT hardware configuration plus arbitrary signal SDO writes. This is done once at startup.

### 5.2 Thickness Signal Encoding
Signal 256 (`0x2700:01`) delivers **geometrical thickness in µm as a 32-bit float**, already corrected for refractive index. This is exactly the same as what the Python program receives and displays. No conversion needed.

For reference — signal ID 256 decodes as:
- Bits 15-14: `00` = float format
- Bit 8: `1` = peak signal
- Bits 7-3: `00000` = first peak
- Bits 2-0: `000` = PeakValue (Distance/Thickness)

### 5.3 Median Signal
Signal 260 decodes as:
- Same as 256 but bits 2-0 = `100` = **PeakValue Median**
- This is the median-filtered version of the thickness, configured with the `MED`/`MEDX` commands
- The Python program uses this as an alternative output source ("Median 1" option)
- Must be mapped as an arbitrary signal since it's not in the pre-defined common signals list

### 5.4 Quality Signal Interpretation
In interferometric mode, the "quality" signal (257 / `0x2708:01`) is the **FFT peak quality**. The Python program uses a fixed-point normalization for some signal variants (dividing by 16 when > 100). With `0x2708:01` you get the native float value — no normalization needed.

### 5.5 Intensity vs. Quality
- `0x2652:00` (Signal 82, `InterferomIntensity`): Detector light intensity, in % of full well capacity. Use for monitoring light source condition.
- `0x2708:01` (Signal 257): FFT peak quality. Use for measurement validity gating.

### 5.6 EtherCAT State Machine
The sensor must be in **OP** state for PDO data to flow. If you see zeros or stale data, check the slave state. An abrupt cable disconnect puts the slave into ERROR state — TwinCAT will attempt to recover automatically if configured.

### 5.7 Dark Reference
The dark reference should be performed:
- After each power-up (sensor warm-up: ~15 min recommended)
- After significant environmental temperature changes
- Periodically during long production runs

On EtherCAT, trigger dark reference by writing 1 to SDO `0x2001:01`. Poll until the value returns to 0 (operation complete). During dark reference, measurement data is not valid — gate your machine logic on the dark reference completion.

### 5.8 Parallel Ethernet Access
The CHRocodile 2 EtherCAT unit still has its Ethernet port active in parallel. You can still connect from a PC for diagnostics, firmware updates, or to use the Python GUI alongside the EtherCAT connection. The sensor explicitly supports multi-client operation.

---

## 6. Quick Reference — SDO Index Table

| Function | SDO Index | SubIdx | Type | Example Value |
|---|---|---|---|---|
| Measuring mode (MMD) | 0x200B | 0 | u8 | `1` (interferometric) |
| Number of peaks (NOP) | 0x2003 | 0 | u8 | `2` |
| Sample frequency (SHZ) | 0x2004 | 0 | f32 | `1000.0` |
| Data averaging (AVD) | 0x200D | 0 | u16 | `1` |
| Spectrum averaging (AVS) | 0x2010 | 0 | u16 | `1` |
| Lamp intensity (LAI) | 0x2002 | 0 | f32 | `50.0` |
| Refractive index value (SRI) | 0x2011 | 2…11 | f32 | `1.5` |
| Refractive index count — executes | 0x2011 | 1 | u8 | `1` |
| Trigger mode (TMOD/CTN/TRG/TRE) | 0x2006 | 0 | vstr | `'CTN'` |
| Trigger once (STR) | 0x2007 | 1 | u8 | `1` (auto-reset) |
| Start/stop data (STA/STO) | 0x2009 | 0 | u8 | `1`=start, `0`=stop |
| Dark reference initiate (DRK) | 0x2001 | 1 | u8 | `1` |
| Dark reference status poll | 0x2001 | 1 | u8 | reads 1=in progress |
| Save setup (SSU) | 0x2008 | 0 | u8 | `1` |
| Confocal threshold (THR) | 0x2005 | 0 | u16 | (confocal mode only) |
| Arbitrary signal 1 f32 slot | 0x2503 | 1 | u16 | write signal ID here |
| Apply PDO configuration | 0x23FF | 0 | u8 | `1` |

---

## 7. Quick Reference — EtherCAT PDO Signal Table

| Object | Signal ID | Type | Description |
|---|---|---|---|
| **0x1A00:01** | 0x2406 | u8 | Frame counter |
| **0x1A00:02** | 0x2400 | u8 | Number of configured samples |
| **0x1A00:03** | 0x2401 | u8 | Number of recorded samples |
| **0x1A00:04** | 0x2407 | u8 | Flags (bit 0 = sample lost) |
| **0x2700:01** | 256 | f32 | **Thickness / Distance Peak 1 (µm)** |
| **0x2701:01** | 264 | f32 | Thickness / Distance Peak 2 (µm) |
| **0x2708:01** | 257 | f32 | Intensity / Quality Peak 1 |
| **0x2652:00** | 82 | f32 | Interferometric intensity (% full well) |
| **0x2653:00** | 83 | u16 | Sample counter |
| **0x2650:00** | 80 | u16 | Number of valid peaks |
| **0x264C:00** | 76 | u16 | Exposure flags |
| **0x2503:01** | —  | u16 | Arbitrary signal slot 1 (write desired signal ID here via SDO) |

---

*Document prepared April 2026. Based on Precitec Optronik CHRocodile 2 Command Reference R1.5.2 (01/2025) and CHRocodile Python GUI source code review.*
