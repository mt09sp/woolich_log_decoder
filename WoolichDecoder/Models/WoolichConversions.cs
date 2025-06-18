using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        // kwaka and yamaha
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

        // Post recall. TPS (ETV) sensor was recalibrated.
        public static double getCorrectETV(this byte[] packet)
        {
            // etv raw (18) -> Example Min = 35, max = 213. Calculation = (col[18] - min) / ((max - min) / 100)

            
            // Because there were some 34's in there??? <-- 34 were no rpm. throttle body???
            // shifting to 35
            var ETV = Math.Round(((double)packet[18] - 35) / 1.78, 2);
            return ETV;
        }

        // Old pre recall
        public static double getCorrectETVOld(this byte[] packet)
        {
            // var ETV = Math.Round(((double)col18 - 38) / 1.6575, 2);
            // WTF did I get this from? var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2); <--- This is the true formula. See log 21
            // Because there were some 34's in there???
            // I really think woolich screwed up on this one. Col 19 might actually be more accurate.

            var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2);
            return ETV;
        }

        public static double getKawaETV(this byte[] packet)
        {

            var ETV = Math.Round((((packet[67] << 8) + packet[68]) * 0.12837) - 13.864, 2);
            return ETV;
        }

        public static double getKawaWoolichTPS(this byte[] packet)
        {

            var ETV = Math.Round((((packet[65] << 8) + packet[66]) * 0.144718) - 29.6672, 2);
            return ETV;
        }

        public static double getKawaTrueTPS(this byte[] packet)
        {
            // y = 0.14442455x - 29.46260918

            var ETV = Math.Round((((packet[65] << 8) + packet[66]) * 0.14442455) - 29.46260918, 2);
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
        public static double getIAP(this byte[] packet, PacketFormat packetFormat)
        {

            if (packetFormat == PacketFormat.Yamaha)
            {

                return Math.Round(packet[21] / pressureScale, 2);
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round(packet.getATMPressure(packetFormat) - packet.getMAP(packetFormat) , 2);
            }
            else
            {
                return -99;
            }

        }

        public static double getMAP(this byte[] packet, PacketFormat packetFormat)
        {

            if (packetFormat == PacketFormat.Yamaha)
            {

                return -99;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round((packet[13] * 0.74998425), 2);
            }
            else
            {
                return -99;
            }

        }


        // it looks like this one was meant to be 10. I back calculated it from woolich initial decoding but now that I look at it it realy should have just been / 10
        public static double getAFR(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return Math.Round(packet[42] / 10.0, 2);
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round(packet[73] / 10.0, 2);
            }
            else
            {
                return -99;
            }
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

        public static double getEngineTemperature(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[26] - temperatureOffset;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                return Math.Round(((packet[25] << 8) + packet[26]) * 0.5 - 60, 2);
            }
            else
            {
                return -99;
            }
        }

        public static double getATMPressure(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return Math.Round(packet[23] / pressureScale, 2);
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round((packet[15] * 0.75) - 0.25, 2);
            }
            else
            {
                return -99;
            }
        }


        public static double getInletTemperature(this byte[] packet, PacketFormat packetFormat)
        {

            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[27] - temperatureOffset;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round(((packet[27] << 8) + packet[28]) * 0.5 - 60, 2);
            }
            else
            {
                return -99;
            }
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

        public static int getGear(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[24] & 0b00000111;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return packet[19];
            }
            else
            {
                return -99;
            }

        }

        public static double get1617(this byte[] packet)
        {
            return ((packet[16] << 8) + packet[17]);
        }

        public static double get3738(this byte[] packet)
        {
            return ((packet[37] << 8) + packet[38]);
        }


        public static string getListValue(this List<FilterOptions> filterOptions, int id, PacketFormat pf)
        {
            return filterOptions.Where(opt => opt.id == id && opt.type == pf).Select(opt => opt.option).FirstOrDefault() ?? "";
        }


    }
}
