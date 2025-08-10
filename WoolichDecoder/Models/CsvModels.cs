using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models
{
    public class SuzukiCsvModelDead
    {
        public string LogTime = string.Empty;
        public double WoolichTPS;
        public double ActualTPS;
        public int RPM;
        public int RPMRaw;
        public double IAP;
        public double AFR;
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;
        public double WoolichEtv; // strange override on 67-68
        public double AtmPressure; // 16
        public double ManPressure; // 14
        public double Ignition;
        public double STP;
        public int MS; // unknown
        public bool Clutch;
        public bool Pair;
        public int milliseconds;
        public string P33 = string.Empty;
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
            RPM = packet.Value.getRPM(true);
            RPMRaw = packet.Value.getRPM();
            AFR = packet.Value.getAFR(PacketFormat.Suzuki);
            WoolichTPS = packet.Value.getWoolichTPS(PacketFormat.Suzuki);
            ActualTPS = packet.Value.getTrueTPS(PacketFormat.Suzuki);
            
            CoolantTemp = packet.Value.getEngineTemperature(PacketFormat.Suzuki);
            STP = packet.Value.getWoolichSTP(PacketFormat.Suzuki);
            Ignition = packet.Value.getIgnitionOffset(PacketFormat.Suzuki);
            Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[33] & 0b00010000) != 0;
            P33 = packet.Value[33].ToBitString(0b11111111);
            AtmPressure = packet.Value.getATMPressure(PacketFormat.Suzuki);
            ManPressure = packet.Value.getMAP(PacketFormat.Suzuki);
            IAP = packet.Value.getIAP(PacketFormat.Suzuki);
            InletAirTemp = packet.Value.getInletTemperature(PacketFormat.Suzuki);
            Gear = packet.Value.getGear(PacketFormat.Suzuki);
            WoolichEtv = packet.Value.getCorrectETV(PacketFormat.Suzuki);
        }

        public void PopulateFromShortPacket(KeyValuePair<string, byte[]> shortPacket, SuzukiCsvModelDead priorPacket, SuzukiCsvModelDead nextPacket)
        {
            // copy packet header and timestamp
            for (int i = 0; i < 10; i++)
            {
                packets[i] = shortPacket.Value[i];
            }

            // default rest of packets to 0
            for (int i = 10; i < priorPacket.packets.Length; i++)
            {
                packets[i] = 0;
            }

            // Actual short packet values
            packets[64] = shortPacket.Value[26]; // AFR
            packets[73] = shortPacket.Value[35]; // Mystery
            packets[77] = shortPacket.Value[39]; // Mystery
            packets[78] = shortPacket.Value[40]; // chksum... kind of pointless really

            LogTime = shortPacket.Key + " t";
            milliseconds = packets.getMilliseconds();

            var timediff = (nextPacket.milliseconds - priorPacket.milliseconds);
            double ratio = (double)(milliseconds - priorPacket.milliseconds) / timediff;



            RPM = InterpolateValue(priorPacket.RPM, nextPacket.RPM, ratio);
            WoolichTPS = InterpolateValue(priorPacket.WoolichTPS, nextPacket.WoolichTPS, ratio, 2);
            ActualTPS = InterpolateValue(priorPacket.ActualTPS, nextPacket.ActualTPS, ratio, 2);
            CoolantTemp = InterpolateValue(priorPacket.CoolantTemp, nextPacket.CoolantTemp, ratio, 2);
            STP = InterpolateValue(priorPacket.STP, nextPacket.STP, ratio, 2);
            Ignition = InterpolateValue(priorPacket.Ignition, nextPacket.Ignition, ratio, 1);

            Clutch = priorPacket.Clutch;
            P33 = priorPacket.P33;
            Gear = priorPacket.Gear;
            Pair = priorPacket.Pair;

            AtmPressure = InterpolateValue(priorPacket.AtmPressure, nextPacket.AtmPressure, ratio, 3);
            ManPressure = InterpolateValue(priorPacket.ManPressure, nextPacket.ManPressure, ratio, 3);
            IAP = InterpolateValue(priorPacket.IAP, nextPacket.IAP, ratio, 3);

            InletAirTemp = InterpolateValue(priorPacket.InletAirTemp, nextPacket.InletAirTemp, ratio, 3);
            WoolichEtv = InterpolateValue(priorPacket.WoolichEtv, nextPacket.WoolichEtv, ratio, 3);

            AFR = packets.getAFR(PacketFormat.Suzuki);

        }


        private T InterpolateValue<T>(T priorValue, T nextValue, double ratio, int? decimalPlaces = null)
            where T : struct, IConvertible
        {
            double prior = Convert.ToDouble(priorValue);
            double next = Convert.ToDouble(nextValue);
            double interpolated = prior + (next - prior) * ratio;

            if (decimalPlaces.HasValue)
            {
                interpolated = Math.Round(interpolated, decimalPlaces.Value);
            }

            return (T)Convert.ChangeType(interpolated, typeof(T));
        }


    }

    public class KawasakiCsvModelDead
    {
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
            AFR = packet.Value.getAFR(PacketFormat.Kawasaki);
            WoolichTPS = packet.Value.getWoolichTPS(PacketFormat.Kawasaki); // TPS 65 66
            ActualTPS = packet.Value.getTrueTPS(PacketFormat.Kawasaki); // TPS 65 66
            TPSMultiplier = packet.Value.getTPSMultiplier(PacketFormat.Kawasaki);

            ActualEtv = packet.Value.getCorrectETV(PacketFormat.Kawasaki); // ETV 67 68
            WoolichEtv = packet.Value.getWoolichBadETV(PacketFormat.Kawasaki); // ETV 67 68

            Ignition = packet.Value.getIgnitionOffset(PacketFormat.Kawasaki);
            //Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[33] & 0b11111111) != 0;

            AtmPressure = packet.Value.getATMPressure(PacketFormat.Kawasaki);
            ManPressure = packet.Value.getMAP(PacketFormat.Kawasaki);
            IAP = packet.Value.getIAP(PacketFormat.Kawasaki);
            CoolantTemp = packet.Value.getEngineTemperature(PacketFormat.Kawasaki); // 25 - 26
            InletAirTemp = packet.Value.getInletTemperature(PacketFormat.Kawasaki); // 27 - 28
            Gear = packet.Value.getGear(PacketFormat.Kawasaki);


            RearWheelSpeed = packet.Value.getRearWheelSpeed(PacketFormat.Kawasaki); // Rear wheel 36-37
            FrontWheelSpeed = packet.Value.getFrontWheelSpeed(PacketFormat.Kawasaki); // Front wheel 38-39
            Speedo = packet.Value.getSpeedo(PacketFormat.Kawasaki); // Speedo 40

            P42 = packet.Value[42];
            P23 = packet.Value[23].ToBitString(0b11111111);
            P69 = packet.Value[69];
            P70 = packet.Value[70];

            TPSRaw = packet.Value.getRaw(65, 66);
            ETVRaw = packet.Value.getRaw(67, 68);


        }

    }



}
