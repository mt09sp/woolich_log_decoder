using System;
using System.Collections.Generic;
using static System.Net.WebRequestMethods;

namespace WoolichDecoder.Models.CsvModels
{
    public abstract class BaseCsvModel
    {
        protected PacketFormat packetFormat = PacketFormat.Unknown;

        
        
        public string LogTime = string.Empty;

        public int RPM;
        public int RPMRaw; // Yes... some are calculated. most are direct

        /// <summary>
        /// Woolich formula TPS Calculation
        /// </summary>
        public double WoolichTPS;
        public double WoolichTPSError;
        /// <summary>
        /// My formula TPS Calculation
        /// </summary>
        public double ActualTPS;
        
        /// <summary>
        /// Woolich TPS Source Data
        /// </summary>
        public int WoolichTPSRaw;
        /// <summary>
        /// ActualTPS Source Data
        /// </summary>
        public double ActualTPSRaw;

        /// <summary>
        /// My formula secondary TPS Calculation (Commonly the ecu zeroed)
        /// </summary>
        public double ActualTPS2;

        /// <summary>
        /// My formula ETV Calculation
        /// </summary>
        public double ActualEtv;
        public double WoolichEtvError;
        /// <summary>
        /// Woolich formula ETV Calculation
        /// </summary>
        public double WoolichEtv;


        /// <summary>
        /// ActualEtv Source Data (should generally be the same as ETVRaw)
        /// </summary>
        public double ActualEtvRaw;

        // public int ETVRaw;
        // public int TPSRaw;

        /// <summary>
        /// consider using EcuETV
        /// </summary>
        public double ActualEtv2;
        public double EcuETV;

        public double InjectorDuration;
        public double IAP;
        public double AFR;
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;
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

        public int LoadPct;

        public int? minThrottle;


        public int TPSMultiplier;

        public List<string> alerts = null;



        public abstract void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet, int? minThrottle);


        protected virtual void PopulateBaseFromLongPacket(KeyValuePair<string, byte[]> packet, int? minThrottle)
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

            RPM = packet.Value.getRPM(packetFormat, true);
            RPMRaw = packet.Value.getRPM(packetFormat, true);


            // ** warning... min throttle calcs may happen later
            WoolichTPS = packet.Value.getWoolichTPS(packetFormat);
            ActualTPS = packet.Value.getTrueTPS(packetFormat);

            ActualTPSRaw = packet.Value.getActualTPSRaw(packetFormat);// TPS 13 13
            ActualTPS2 = packet.Value.getActualTPS2(packetFormat); // PID 4A (16-17)

            TPSMultiplier = packet.Value.getTPSMultiplier(packetFormat);


            EcuETV = packet.Value.getEcuETV(packetFormat); // PID 45 (19)
            ActualEtv2 = packet.Value.getActualEtv2(packetFormat); // PID 47 (14)
            // ActualEtv is further down

            WoolichTPSRaw = packet.Value.getWoolichTPSRaw(packetFormat); // PID 0x5a (15)
            ActualEtvRaw = packet.Value.getActualEtvRaw(packetFormat); // PID 11 (18)


            // ETV handling - all classes have these but with different method calls
            // ** warning... min throttle calcs may happen later
            ActualEtv = packet.Value.getCorrectETV(packetFormat, minThrottle); // PID 11
            WoolichEtv = packet.Value.getWoolichBadETV(packetFormat); // Kawasaki and Yamaha use bad method // PID 11


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
