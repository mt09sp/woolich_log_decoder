using System;

namespace WoolichDecoder.Models
{
    public static class WoolichConversions
    {
        static readonly int temperatureOffset = 30;
        static readonly int ignitionOffset = 30;
        static readonly double pressureScale = 2.0156;
        static readonly double batteryVoltageDivider = 13.65;
        static readonly double wheelSpeedSensorTicks = 100;

        public static int getMilliseconds(this byte[] packet)
        {
            var ms1 = packet[8];
            var ms2 = packet[9];
            var milliseconds = (((
                (packet[5] * 60) // hours to minutes 
                + packet[6]) * 60) // minutes to seconds
                + packet[7]) * 1000 // seconds to miliseconds
                + (ms1 << 8) + ms2; // plus our miliseconds.

            return milliseconds;
        }

        public static int getRPM(this byte[] packet)
        {
            return ((packet[10] << 8) + packet[11]);
        }

        public static double getTrueTPS(this byte[] packet)
        {
            double trueTPS = Math.Round(((double)((packet[12] << 8) + packet[13]) / 7.39), 2);
            if (trueTPS > 100)
            {
                return 100;
            }
            return trueTPS;
        }


        public static double getWoolichBadETV(this byte[] packet)
        {
            // var ETV = Math.Round(((double)col18 - 38) / 1.6575, 2);
            // WTF did I get this from? var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2); <--- This is the true formula. See log 21
            // Because there were some 34's in there???
            // I really think woolich screwed up on this one. Col 19 might actually be more accurate.

            var ETV = Math.Round(((double)packet[18] - 38) / 1.66, 2);
            return ETV;
        }

        public static double getCorrectETV(this byte[] packet)
        {
            // var ETV = Math.Round(((double)col18 - 38) / 1.6575, 2);
            // WTF did I get this from? var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2); <--- This is the true formula. See log 21
            // Because there were some 34's in there???
            // I really think woolich screwed up on this one. Col 19 might actually be more accurate.

            var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2);
            return ETV;
        }

        public static double getWoolichTPS(this byte[] packet)
        {
            var TPS = Math.Round(((double)packet[15] / 1.74), 2);
            if (TPS > 100)
            {
                return 100.0;
            }
            return TPS;
        }
        public static double getIAP(this byte[] packet)
        {
            return Math.Round(packet[21] / pressureScale, 2);
        }

        public static double getAFR(this byte[] packet)
        {
            return Math.Round(packet[42] / 10.2, 2);
        }

        public static double getSpeedo(this byte[] packet)
        {
            return ((packet[31] << 8) + packet[32]);
        }

        public static double getFrontWheelSpeed(this byte[] packet)
        {
            int wheelSpeed = (packet[33] << 8) + packet[34];
            return Math.Round(wheelSpeed / wheelSpeedSensorTicks);
        }

        public static double getRearWheelSpeed(this byte[] packet)
        {
            int wheelSpeed = (packet[35] << 8) + packet[36];
            return Math.Round(wheelSpeed / wheelSpeedSensorTicks);
        }

        public static double getEngineTemperature(this byte[] packet)
        {
            return packet[26] - temperatureOffset;
        }

        public static double getATMPressure(this byte[] packet)
        {
            return Math.Round(packet[23] / pressureScale, 2);
        }

        public static double getInletTemperature(this byte[] packet)
        {
            return packet[27] - temperatureOffset;
        }

        public static double getInjectorDuration(this byte[] packet)
        {
            return packet[28] / 2.0;
        }

        public static double getIgnitionOffset(this byte[] packet)
        {
            return packet[29] - ignitionOffset;
        }



        public static double getBatteryVoltage(this byte[] packet)
        {

            return Math.Round(packet[41] / batteryVoltageDivider, 2);
        }

        public static int getGear(this byte[] packet)
        {

            return packet[24] & 0b00000111;
        }

        public static double get1617(this byte[] packet)
        {
            return ((packet[16] << 8) + packet[17]);
        }

        public static double get3738(this byte[] packet)
        {
            return ((packet[37] << 8) + packet[38]);
        }

    }
}
