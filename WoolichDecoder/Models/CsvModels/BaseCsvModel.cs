using System;
using System.Collections.Generic;

namespace WoolichDecoder.Models.CsvModels
{
    public abstract class BaseCsvModel
    {
        protected PacketFormat packetFormat = PacketFormat.Unknown;

        public string LogTime = string.Empty;
        public double WoolichTPS;
        public double ActualTPS;
        public int RPM;
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
        public int MS;
        public bool Clutch;
        public bool Pair;
        public int milliseconds;
        public int RearWheelSpeed;
        public int FrontWheelSpeed;
        public int Speedo;
        public string P23 = string.Empty;
        public byte[] packets;

        public int TPSMultiplier;

        public abstract void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet);

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
