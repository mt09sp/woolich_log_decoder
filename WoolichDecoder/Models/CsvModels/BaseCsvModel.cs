using System;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace WoolichDecoder.Models.CsvModels
{
    public abstract class BaseCsvModel
    {
        protected PacketFormat packetFormat = PacketFormat.Unknown;

        public string LogTime = string.Empty;
        public double WoolichTPS;
        public double ActualTPS;
        public int RPM;
        public int WoolichTPSRaw;
        public double ActualTPSRaw;
        public double ActualTPS2;
        public double ActualEtvRaw;
        public double ActualEtv2;
        public int EcuETV;
        public int RPMRaw;
        public double InjectorDuration;
        public double IAP;
        public double AFR;
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;
        public double ActualEtv;
        public double WoolichEtv;
        public int ETVRaw;
        public int TPSRaw;
        public double AtmPressure;
        public double ManPressure;
        public double Ignition;
        public double O2SensorVoltage;
        public double O2SensorRaw;
        public int MS;
        public double Battery;
        public bool Clutch;
        public bool Pair;
        public int milliseconds;
        public int RearWheelSpeed;
        public int FrontWheelSpeed;
        public int Speedo;
        public double STP;
        public byte[] packets;

        public int TPSMultiplier;

        public abstract void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet);


        protected virtual void PopulateBaseFromLongPacket(KeyValuePair<string, byte[]> packet)
        {

            packets = new byte[packet.Value.Length];

            // Copy packet data
            for (int i = 0; i < packet.Value.Length && i < packets.Length; i++)
            {
                packets[i] = packet.Value[i];
            }


            // Common properties across all classes
            LogTime = packet.Key + " t";
            milliseconds = packet.Value.getMilliseconds();
            AFR = packet.Value.getAFR(packetFormat);
            WoolichTPS = packet.Value.getWoolichTPS(packetFormat);
            ActualTPS = packet.Value.getTrueTPS(packetFormat);

            RPM = packet.Value.getRPM(packetFormat, true);
            RPMRaw = packet.Value.getRPM(packetFormat, true);
            ActualTPSRaw = packet.Value.getActualTPSRaw(packetFormat);// TPS 13 13
            ActualTPS2 = packet.Value.getActualTPS2(packetFormat); // PID 4A (16-17)
            EcuETV = packet.Value.getEcuETV(packetFormat); // PID 45 (19)
            ActualEtv2 = packet.Value.getActualEtv2(packetFormat); // PID 47 (14)
            WoolichTPSRaw = packet.Value.getWoolichTPSRaw(packetFormat); // PID 0x5a (15)
            ActualEtvRaw = packet.Value.getActualEtvRaw(packetFormat); // PID 11 (18)
            O2SensorVoltage = packet.Value.getO2SensorVoltage(packetFormat);
            O2SensorRaw = packet.Value.getO2SensorRaw(packetFormat);
            InjectorDuration = packet.Value.getInjectorDuration(packetFormat);
            Battery = packet.Value.getBatteryVoltage(packetFormat);
            STP = packet.Value.getWoolichSTP(packetFormat);

            CoolantTemp = packet.Value.getEngineTemperature(packetFormat);
            InletAirTemp = packet.Value.getInletTemperature(packetFormat);
            Gear = packet.Value.getGear(packetFormat);
            AtmPressure = packet.Value.getATMPressure(packetFormat);
            ManPressure = packet.Value.getMAP(packetFormat);
            IAP = packet.Value.getIAP(packetFormat);
            Ignition = packet.Value.getIgnitionOffset(packetFormat);


            TPSMultiplier = packet.Value.getTPSMultiplier(packetFormat);

            // ETV handling - all classes have these but with different method calls
            ActualEtv = packet.Value.getCorrectETV(packetFormat);
            WoolichEtv = packet.Value.getWoolichBadETV(packetFormat); // Kawasaki and Yamaha use bad method

            RearWheelSpeed = packet.Value.getRearWheelSpeed(packetFormat);
            FrontWheelSpeed = packet.Value.getFrontWheelSpeed(packetFormat);
            Speedo = packet.Value.getSpeedo(packetFormat);

        }




        protected T InterpolateValue<T>(T priorValue, T nextValue, double ratio, int? decimalPlaces = null)
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
