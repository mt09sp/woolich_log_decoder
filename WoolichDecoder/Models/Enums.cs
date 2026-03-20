using System;

namespace WoolichDecoder.Models
{

    [Flags]
    public enum ExportItems
    {
        Yellow = 1,
        Green = 2,
        Red = 4,
        Blue = 8
    }


    public enum PacketFormat
    {
        Unknown = 0x00,
        Japanese = 0x01,
        Yamaha = 0x5d, // 00 01 02 5d
        Kawasaki = 0x4c,
        Suzuki = 0x26,
        BMW = 0x10,
        HondaOBD = 0x03, // cbr650 is variable length it seems. 
        HondaKline = 0x37, // VFR 01 02 37  .  
        HondaKlineCB = 0x38, // VFR 01 02 37  .  
        HondaKlineVFR = 0x39, // VFR 01 02 37  .  
    }




    /// <summary>
    /// OBD-II Mode 01 Parameter IDs (PIDs) for motorcycle diagnostics
    /// </summary>
    public enum ObdPid : byte
    {
        /// <summary>
        /// Supported PIDs 01-20 (bitmap)
        /// </summary>
        SupportedPids_01_20 = 0x00,

        /// <summary>
        /// Monitor status since DTCs cleared
        /// </summary>
        MonitorStatus = 0x01,

        /// <summary>
        /// Freeze DTC (Diagnostic Trouble Code that caused freeze frame)
        /// </summary>
        FreezeDTC = 0x02,

        /// <summary>
        /// Fuel system status
        /// </summary>
        FuelSystemStatus = 0x03,

        /// <summary>
        /// Calculated engine load (%)
        /// Formula: A * 100 / 255
        /// </summary>
        EngineLoad = 0x04,

        /// <summary>
        /// Engine coolant temperature (°C)
        /// Formula: A - 40
        /// </summary>
        EngineCoolantTemp = 0x05,

        /// <summary>
        /// Short term fuel trim - Bank 1 (%)
        /// Formula: (A - 128) * 100 / 128
        /// </summary>
        ShortTermFuelTrim_Bank1 = 0x06,

        /// <summary>
        /// Long term fuel trim - Bank 1 (%)
        /// Formula: (A - 128) * 100 / 128
        /// </summary>
        LongTermFuelTrim_Bank1 = 0x07,

        /// <summary>
        /// Short term fuel trim - Bank 2 (%)
        /// Formula: (A - 128) * 100 / 128
        /// </summary>
        ShortTermFuelTrim_Bank2 = 0x08,

        /// <summary>
        /// Long term fuel trim - Bank 2 (%)
        /// Formula: (A - 128) * 100 / 128
        /// </summary>
        LongTermFuelTrim_Bank2 = 0x09,

        /// <summary>
        /// Fuel pressure (gauge) (kPa)
        /// Formula: A * 3
        /// </summary>
        FuelPressure = 0x0A,

        /// <summary>
        /// Intake manifold absolute pressure (kPa)
        /// Formula: A (direct value)
        /// </summary>
        IntakeManifoldPressure = 0x0B,

        /// <summary>
        /// Engine RPM
        /// Formula: ((A * 256) + B) / 4
        /// </summary>
        EngineRPM = 0x0C,

        /// <summary>
        /// Vehicle speed (km/h)
        /// Formula: A (direct value)
        /// </summary>
        VehicleSpeed = 0x0D,

        /// <summary>
        /// Timing advance (degrees before TDC)
        /// Formula: (A - 128) / 2
        /// </summary>
        TimingAdvance = 0x0E,

        /// <summary>
        /// Intake air temperature (°C)
        /// Formula: A - 40
        /// </summary>
        IntakeAirTemp = 0x0F,

        /// <summary>
        /// MAF air flow rate (grams/sec)
        /// Formula: ((A * 256) + B) / 100
        /// </summary>
        MAFAirFlowRate = 0x10,

        /// <summary>
        /// Throttle position (%)
        /// Formula: A * 100 / 255
        /// </summary>
        ThrottlePosition = 0x11,

        /// <summary>
        /// Commanded secondary air status
        /// </summary>
        SecondaryAirStatus = 0x12,

        /// <summary>
        /// Oxygen sensors present (bitmap)
        /// </summary>
        O2SensorsPresent = 0x13,

        /// <summary>
        /// O2 Sensor 1 - Bank 1
        /// </summary>
        O2Sensor1_Bank1 = 0x14,

        /// <summary>
        /// O2 Sensor 2 - Bank 1
        /// </summary>
        O2Sensor2_Bank1 = 0x15,

        /// <summary>
        /// O2 Sensor 3 - Bank 1
        /// </summary>
        O2Sensor3_Bank1 = 0x16,

        /// <summary>
        /// O2 Sensor 4 - Bank 1
        /// </summary>
        O2Sensor4_Bank1 = 0x17,

        /// <summary>
        /// O2 Sensor 1 - Bank 2
        /// </summary>
        O2Sensor1_Bank2 = 0x18,

        /// <summary>
        /// O2 Sensor 2 - Bank 2
        /// </summary>
        O2Sensor2_Bank2 = 0x19,

        /// <summary>
        /// O2 Sensor 3 - Bank 2
        /// </summary>
        O2Sensor3_Bank2 = 0x1A,

        /// <summary>
        /// O2 Sensor 4 - Bank 2
        /// </summary>
        O2Sensor4_Bank2 = 0x1B,

        /// <summary>
        /// OBD standards this vehicle conforms to
        /// </summary>
        OBDStandard = 0x1C,

        /// <summary>
        /// Oxygen sensors present (4 banks, bitmap)
        /// </summary>
        O2SensorsPresent_4Bank = 0x1D,

        /// <summary>
        /// Auxiliary input status
        /// </summary>
        AuxiliaryInputStatus = 0x1E,

        /// <summary>
        /// Run time since engine start (seconds)
        /// Formula: (A * 256) + B
        /// </summary>
        RuntimeSinceEngineStart = 0x1F,

        /// <summary>
        /// Supported PIDs 21-40 (bitmap)
        /// </summary>
        SupportedPids_21_40 = 0x20,

        /// <summary>
        /// Distance traveled with MIL on (km)
        /// Formula: (A * 256) + B
        /// </summary>
        DistanceWithMIL = 0x21,

        /// <summary>
        /// Fuel rail pressure (relative to manifold vacuum) (kPa)
        /// Formula: ((A * 256) + B) * 0.079
        /// </summary>
        FuelRailPressure = 0x22,

        /// <summary>
        /// Fuel rail gauge pressure (diesel/gasoline direct injection) (kPa)
        /// Formula: ((A * 256) + B) * 10
        /// </summary>
        FuelRailGaugePressure = 0x23,

        /// <summary>
        /// UserSpecified 0x30
        /// </summary>
        UserSpecified_30 = 0x30,

        /// <summary>
        /// Supported PIDs 41-60 (bitmap)
        /// </summary>
        SupportedPids_41_60 = 0x40,

        /// <summary>
        /// Control module voltage (V)
        /// Formula: ((A * 256) + B) / 1000
        /// </summary>
        ControlModuleVoltage = 0x42,

        /// <summary>
        /// Absolute load value (%)
        /// Formula: ((A * 256) + B) * 100 / 255
        /// </summary>
        AbsoluteLoadValue = 0x43,

        /// <summary>
        /// Commanded air-fuel equivalence ratio
        /// Formula: ((A * 256) + B) / 32768
        /// </summary>
        CommandedAFR = 0x44,

        /// <summary>
        /// Relative throttle position (%)
        /// Formula: A * 100 / 255
        /// </summary>
        RelativeThrottlePosition = 0x45,

        /// <summary>
        /// Ambient air temperature (°C)
        /// Formula: A - 40
        /// </summary>
        AmbientAirTemp = 0x46,

        /// <summary>
        /// Absolute throttle position B (%)
        /// Formula: A * 100 / 255
        /// </summary>
        AbsoluteThrottlePositionB = 0x47,

        /// <summary>
        /// Absolute throttle position C (%)
        /// Formula: A * 100 / 255
        /// </summary>
        AbsoluteThrottlePositionC = 0x48,

        /// <summary>
        /// Accelerator pedal position D (%)
        /// Formula: A * 100 / 255
        /// </summary>
        AcceleratorPedalPositionD = 0x49,

        /// <summary>
        /// Accelerator pedal position E (%)
        /// Formula: A * 100 / 255
        /// </summary>
        AcceleratorPedalPositionE = 0x4A,

        /// <summary>
        /// Accelerator pedal position F (%)
        /// Formula: A * 100 / 255
        /// </summary>
        AcceleratorPedalPositionF = 0x4B,

        /// <summary>
        /// Commanded throttle actuator (%)
        /// Formula: A * 100 / 255
        /// </summary>
        CommandedThrottleActuator = 0x4C,

        /// <summary>
        /// UserSpecified_4D
        /// </summary>
        UserSpecified_4D = 0x4D,


        /// <summary>
        /// Supported PIDs 61-80 (bitmap)
        /// </summary>
        SupportedPids_61_80 = 0x60,

        /// <summary>
        /// Engine oil temperature (°C)
        /// Formula: A - 40
        /// </summary>
        EngineOilTemp = 0x5C
    }



}
