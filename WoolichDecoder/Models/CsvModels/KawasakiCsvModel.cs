using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{

    public class KawasakiCsvModel
    {
        PacketFormat packetFormat = PacketFormat.Kawasaki;

        public string LogTime = string.Empty;
        public double WoolichTPS; // 65 66
        public double ActualTPS; // 65 66
        public int TPSMultiplier; // 12

        public double ActualEtv; // 67 68
        public double WoolichEtv; // 

        public int ETVRaw;
        public int TPSRaw;


        public int RPM;
        // public int RPMRaw;
        public double IAP;
        public double AFR;
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;


        public double AtmPressure; // 
        public double ManPressure; // 
        public double Ignition;
        // public double STP;
        public int MS; // unknown
        public bool Clutch;

        public bool Pair;
        public int milliseconds;

        public int RearWheelSpeed;
        public int FrontWheelSpeed;
        public int Speedo;

        public int P42;
        public int P69;
        public int P70;


        public string P23 = string.Empty;

        // These stay as raw bytes
        public byte[] packets = new byte[79];

        public void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet)
        {
            for (int i = 0; i < packet.Value.Length; i++)
            {
                packets[i] = packet.Value[i];
            }

            LogTime = packet.Key + " t";
            milliseconds = packet.Value.getMilliseconds();
            RPM = packet.Value.getRPM();
            // RPMRaw = packet.Value.getRPM();
            AFR = packet.Value.getAFR(packetFormat);
            WoolichTPS = packet.Value.getWoolichTPS(packetFormat); // TPS 65 66
            ActualTPS = packet.Value.getTrueTPS(packetFormat); // TPS 65 66
            TPSMultiplier = packet.Value.getTPSMultiplier(packetFormat);

            ActualEtv = packet.Value.getCorrectETV(packetFormat); // ETV 67 68
            WoolichEtv = packet.Value.getWoolichBadETV(packetFormat); // ETV 67 68

            Ignition = packet.Value.getIgnitionOffset(packetFormat);
            //Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[33] & 0b11111111) != 0;

            AtmPressure = packet.Value.getATMPressure(packetFormat);
            ManPressure = packet.Value.getMAP(packetFormat);
            IAP = packet.Value.getIAP(packetFormat);
            CoolantTemp = packet.Value.getEngineTemperature(packetFormat); // 25 - 26
            InletAirTemp = packet.Value.getInletTemperature(packetFormat); // 27 - 28
            Gear = packet.Value.getGear(packetFormat);


            RearWheelSpeed = packet.Value.getRearWheelSpeed(packetFormat); // Rear wheel 36-37
            FrontWheelSpeed = packet.Value.getFrontWheelSpeed(packetFormat); // Front wheel 38-39
            Speedo = packet.Value.getSpeedo(packetFormat); // Speedo 40

            P42 = packet.Value[42];
            P23 = packet.Value[23].ToBitString(0b11111111);
            P69 = packet.Value[69];
            P70 = packet.Value[70];

            TPSRaw = packet.Value.getRaw(65, 66);
            ETVRaw = packet.Value.getRaw(67, 68);


        }

    }



}
