using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
    public class SuzukiCsvModel
    {

        PacketFormat packetFormat = PacketFormat.Suzuki;

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
            AFR = packet.Value.getAFR(packetFormat);
            WoolichTPS = packet.Value.getWoolichTPS(packetFormat);
            ActualTPS = packet.Value.getTrueTPS(packetFormat);
            
            CoolantTemp = packet.Value.getEngineTemperature(packetFormat);
            STP = packet.Value.getWoolichSTP(packetFormat);
            Ignition = packet.Value.getIgnitionOffset(packetFormat);
            Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[33] & 0b00010000) != 0;
            P33 = packet.Value[33].ToBitString(0b11111111);
            AtmPressure = packet.Value.getATMPressure(packetFormat);
            ManPressure = packet.Value.getMAP(packetFormat);
            IAP = packet.Value.getIAP(packetFormat);
            InletAirTemp = packet.Value.getInletTemperature(packetFormat);
            Gear = packet.Value.getGear(packetFormat);
            WoolichEtv = packet.Value.getCorrectETV(packetFormat);
        }

        public void PopulateFromShortPacket(KeyValuePair<string, byte[]> shortPacket, SuzukiCsvModel priorPacket, SuzukiCsvModel nextPacket)
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

            AFR = packets.getAFR(packetFormat);

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




}
