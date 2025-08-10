using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
 
    public class YamahaCsvModel
    {

        PacketFormat packetFormat = PacketFormat.Yamaha;

        public string LogTime = string.Empty;

        // 15 (0xf) confirmed woolich TPS capped at 100. 
        // PID 0x5a (Relative Accelerator Pedal Position)
        public double WoolichTPS;
        // 15 (0xf) confirmed woolich TPS capped at 100. 
        // PID 0x5a (Relative Accelerator Pedal Position)
        public int WoolichTPSRaw;

        // 12/13 tps related (0xc,0xd)
        // I think that this is the true TPS.
        // It reacts sooner than 15 on both rise and fall.
        // var trueTPS = getTrueTPS((packet[12] << 8) + packet[13]);
        // PID 0x49 (Accelerator Pedal Position D)
        public double ActualTPSRaw; // PID 49

        // 12/13 tps related (0xc,0xd)
        // I think that this is the true TPS.
        // outputString += $"{trueTPS},";
        // PID 0x49 (Accelerator Pedal Position D)
        public double ActualTPS; // PID 49

        // This might be ETV also...
        // PID 0x4A Accelerator Pedal Position E
        public double ActualTPS2; // PID 4A (16-17)

        public int TPSMultiplier; // N/A

        public double ActualEtv; // PID 11 (18)
        public double ActualEtvRaw; // PID 11 (18)
        public double ActualEtv2; // PID 47 (14)

        // 18 Woolich etv confirmed. shows 2% over on ETV (18.1 instead of 17.7) It's under on the lower side too.
        public double WoolichEtv; // WRONG (18)

        public int ETVRaw;
        public int TPSRaw;

        // 5 to 36 not running. / 0 to 88 when running.
        // PID 0x45 Relative throttle (ETV) position 
        public int EcuETV; // PID 45 (19)

        // 10 RPM
        public int RPM;
        public int RPMRaw;
        
        public double IAP; // 21
        public double AFR; // 42
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;


        public double AtmPressure;
        public double ManPressure;
        public double Ignition;
        public double InjectorDuration;
        // public double STP;
        public int MS; // unknown
        public bool Clutch;
        public bool ThrottleOff;

        public bool Pair;
        public int milliseconds;

        public int RearWheelSpeed;
        public int FrontWheelSpeed;
        public int Speedo;

        public double O2SensorVoltage;
        public double O2SensorRaw;

        public int P24Pt1;

        /*
        // Just 65 with throttle open or 97 closed. (engine off)
        // 65 = 0100 0001
        // 97 = 0110 0001
        // Engine on 1, 33, 65, 97, 129, 161, 193, 225
        // 1 = 0000 0001
        // 33 = 0010 0001
        // 65 = 0100 0001
        // 97 = 0110 0001
        // 129 = 1000 0001
        // 161 = 1010 0001
        // 193 = 1100 0001
        // 225 = 1110 0001 => Logging Start in neutral???
        var bit1 = (packet[25] & 0b10000000) >> 7;
        var bit2 = (packet[25] & 0b01000000) >> 6;
        var bit3 = (packet[25] & 0b00100000) >> 5;
        var bit4 = (packet[25] & 0b00010000) >> 4;
        var bit5 = (packet[25] & 0b00001000) >> 3;
        var bit6 = (packet[25] & 0b00000100) >> 2;
        var bit7 = (packet[25] & 0b00000010) >> 1;
        var bit8 = (packet[25] & 0b00000001);
        outputString += $"{bit1},"; // clutch maybe??? <= Nope. More complex than that.
        outputString += $"{bit2},"; // unknown
        outputString += $"{bit3},"; // throttle closed.
        outputString += $"b{bit4}{bit5}{bit6}{bit7}{bit8},";
        */
        public int P25Pt2;
        public string P25Pt4 = string.Empty;

        public int P20;
        public int P22;
        public int P37P38;
        public int P39;
        public int P40;
        public double Battery;
        public int P43;
        public int P44;
        public int P45;
        public int P46;
        public int P47;
        public int P48;


        public string P23 = string.Empty;

        // These stay as raw bytes
        public byte[] packets;

        public void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet)
        {
            packets = new byte[packet.Value.Length];
            for (int i = 0; i < packet.Value.Length; i++)
            {
                packets[i] = packet.Value[i];
            }

            LogTime = packet.Key + " t";
            milliseconds = packet.Value.getMilliseconds();
            RPM = packet.Value.getRPM(false);
            RPMRaw = packet.Value.getRPM(false);
            AFR = packet.Value.getAFR(packetFormat);

            WoolichTPS = packet.Value.getWoolichTPS(packetFormat); // PID 0x5a (15)
            ActualTPS = packet.Value.getTrueTPS(packetFormat); // PID 0x49 (12-13)

            TPSMultiplier = packet.Value.getTPSMultiplier(packetFormat); // N/A

            ActualTPSRaw = packet.Value.getActualTPSRaw(packetFormat);// TPS 13 13
            ActualTPS2 = packet.Value.getActualTPS2(packetFormat); // PID 4A (16-17)

            EcuETV = packet.Value.getEcuETV(packetFormat); // PID 45 (19)
            ActualEtv2 = packet.Value.getActualEtv2(packetFormat); // PID 47 (14)
            WoolichTPSRaw = packet.Value.getWoolichTPSRaw(packetFormat); // PID 0x5a (15)


            ActualEtv = packet.Value.getCorrectETV(packetFormat); // PID 11 (18)
            WoolichEtv = packet.Value.getWoolichBadETV(packetFormat); // ETV 67 68
            ActualEtvRaw = packet.Value.getActualEtvRaw(packetFormat); // PID 11 (18)

            O2SensorVoltage = packet.Value.getO2SensorVoltage(packetFormat);
            O2SensorRaw = packet.Value.getO2SensorRaw(packetFormat);
            InjectorDuration = packet.Value.getInjectorDuration(packetFormat);
            Ignition = packet.Value.getIgnitionOffset(packetFormat);

            Battery = packet.Value.getBatteryVoltage(packetFormat);


            //Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[25] & 0b10000000) != 0; // bit1 
            ThrottleOff = (packet.Value[25] & 0b00100000) != 0; // bit3 

            AtmPressure = packet.Value.getATMPressure(packetFormat);
            IAP = packet.Value.getIAP(packetFormat);
            ManPressure = packet.Value.getMAP(packetFormat);
            CoolantTemp = packet.Value.getEngineTemperature(packetFormat); // 25 - 26
            InletAirTemp = packet.Value.getInletTemperature(packetFormat); // 27 - 28
            Gear = packet.Value.getGear(packetFormat);


            RearWheelSpeed = packet.Value.getRearWheelSpeed(packetFormat); // Rear wheel 36-37
            FrontWheelSpeed = packet.Value.getFrontWheelSpeed(packetFormat); // Front wheel 38-39
            Speedo = packet.Value.getSpeedo(packetFormat); // Speedo 40

            // ---
            // Bike specific
            // ---
            P20 = packet.Value[20];
            P22 = packet.Value[22];
            // the first part of the gear byte 24
            P24Pt1 = packet.Value[24] & 0b11111000;

            // 25 is a whole lot of switching
            // P25 pt1 might be clutch switch
            P25Pt2 = (packet.Value[25] & 0b01000000) >> 6; // bit2
            // P25 pt3 is throttle on/off
            P25Pt4 = packet.Value[25].ToBitString(0b00011111); // bits 4, 5, 6, 7, 8



            P37P38 = packet.Value.getRaw(37, 38);
            P39 = packet.Value[39];
            P40 = packet.Value[40];
            P43 = packet.Value[43];
            P44 = packet.Value[44];
            P45 = packet.Value[45];
            P46 = packet.Value[46];
            P47 = packet.Value[47];
            P48 = packet.Value[48];


        }

    }



}
