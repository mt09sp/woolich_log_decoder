using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
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
        public static int getRPM(this byte[] packet, PacketFormat packetFormat, bool isRaw = false)
        {
            if (packetFormat == PacketFormat.Suzuki && isRaw == false)
            {
                // y = 0.390667x - 0.569767

                // return (int)Math.Round(((packet[10] << 8) + packet[11]) * 0.3906, 0);
                //return (int)Math.Round(((packet[10] << 8) + packet[11]) * 0.390667 - 0.49, 0);
                // 0.390 +9
                return (int)Math.Round(((packet[10] << 8) + packet[11]) * 0.390625 - 0.44, 0);
            }

            if (packetFormat == PacketFormat.HondaKline || packetFormat == PacketFormat.HondaKlineVFR  || packetFormat == PacketFormat.HondaKlineCB )
            {
                return ((packet[10] << 8) + packet[11]);
            }

            // Same for kawasaki and yamaha and HondaKline.
            // Honda OBD doesn't use this because it doesn't have the same packet structure.
            return ((packet[10] << 8) + packet[11]);
        }


        public static double getTrueTPS(this byte[] packet, PacketFormat packetFormat)
        {


            if (packetFormat == PacketFormat.Yamaha)
            {
                // PID 0x49
                double trueTPS = Math.Round(((double)((packet[12] << 8) + packet[13]) / 7.39), 2);
                if (trueTPS > 100)
                {
                    return 100;
                }
                return trueTPS;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                // y = 0.14442455x - 29.46260918

                var trueTPS = Math.Round((((packet[65] << 8) + packet[66]) * 0.14442455) - 29.46260918, 2);
                if (trueTPS > 100)
                {
                    return 100;
                }
                return trueTPS;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {

                // Suzuki has a TPS override. Packet 43 
                if (packet[43] > 0)
                {
                    var TPS = (int)packet[43];
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;

                }
                else if (packet[66] > 0)
                {
                    var TPS = (int)packet[66];
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;

                }
                else
                {
                    var TPS = Math.Round(((double)(packet[12] - 55.0) / 1.74), 1);
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;
                }
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // P12 is the raw sensor value (relative). however Honda present a zeroed absolute value in P13
                // Needs confirming

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                // var TPS = Math.Round(((double)(packet[12] - 11.0) / 1.01), 1);
                // Alternative: var TPS = Math.Round(((double)(packet[13]) / 1.55), 1);

                // Accurate with 1.01 however using 1.00 gives 1 bit headroom at 100%
                var TPS = Math.Round(((double)(packet[12] - 11.0) / 1.00), 1);
                if (TPS > 100)
                {
                    return 100.0;
                }
                if (TPS < 0)
                {
                    return 0.0;
                }
                return TPS;
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // P12 is the raw sensor value (relative) 26 to 227. however Honda present a zeroed absolute value in P13 that's still a little bit iffy. 0 to 157
                // Correct using Riki's 21 and 63 files

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                var TPS = Math.Round(((double)(packet[12] - 26) / 2.00 ), 1);// Checked for CB - Unknown for VFR
                if (TPS > 100)
                {
                    return 100.0;
                }
                if (TPS < 0)
                {
                    return 0.0;
                }
                return TPS;
            }

            return 0.0;
        }

        public static double getWoolichTPS(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                var TPS = Math.Round(((double)packet[15] / 1.74), 2);
                if (TPS > 100)
                {
                    return 100.0;
                }
                return TPS;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                var TPS = Math.Round((((packet[65] << 8) + packet[66]) * 0.144718) - 29.6672, 2);
                if (TPS > 100)
                {
                    return 100.0;
                }
                return TPS;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                // Suzuki has a TPS override. Packet 43 
                if (packet[43] > 0)
                {
                    var TPS = (int)packet[43];
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;

                }
                else if (packet[66] > 0)
                {
                    var TPS = (int)packet[66];
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;

                }
                else
                {

                    var TPS = Math.Round(((double)(packet[12] - 55.0) / 1.60798), 1);
                    // 1.607967
                    if (TPS > 100)
                    {
                        return 100.0;
                    }
                    return TPS;
                }
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // PID 0x49
                // Needs confirming
                // var TPS = Math.Round(((double)(packet[12] * 1.1111) - 24.444), 1);

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                var TPS = Math.Round(((double)(packet[12] - 22)/ 0.9), 1);
                if (TPS > 100)
                {
                    return 100.0;
                }
                if (TPS < 0)
                {
                    return 0.0;
                }
                return TPS;
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {

                // For the CB21 they don't have ride by wire. TPS is the fuel map value.
                // *0.4902-12.745
                // Correct using Riki's 21 and 63 files
                //var TPS = Math.Round(((double)(packet[12] * 0.4902) - 12.745), 1);

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                var WTPS = Math.Round(((double)(packet[12] - 26) / 2.04), 1); // Checked for CB - Unknown for VFR
                if (WTPS > 100)
                {
                    return 100.0;
                }
                if (WTPS < 0)
                {
                    return 0.0;
                }
                return WTPS;
            }

            // ROUND(B12 *0.4902-12.745,3)

            return 0.0;
        }

        public static int getWoolichTPSRaw(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                var TPSRaw = packet[15];
                return TPSRaw;
            }
            if (packetFormat == PacketFormat.HondaKlineCB || packetFormat == PacketFormat.HondaKlineVFR)
            {
                // don't have multiple sources here.
                return packet.getActualTPSRaw(packetFormat);
            }

            return -99;
        }


        // ActualTPSRaw = True TPS raw
        // PID 0x49 (Accelerator Pedal Position D)
        public static int getActualTPSRaw(this byte[] packet, PacketFormat packetFormat)
        {


            if (packetFormat == PacketFormat.Yamaha)
            {
                // PID 0x49
                int actualTPS = (packet[12] << 8) + packet[13];
                return actualTPS;
            }
            
            if (packetFormat == PacketFormat.HondaKlineCB || packetFormat == PacketFormat.HondaKlineVFR)
            {
                // Relative TPS. raw from the sensor.
                int actualTPS = packet[12];
                return actualTPS;
            }
            return 0;
        }

        // getActualTPS2
        // PID 4A (16-17)
        public static double getActualTPS2(this byte[] packet, PacketFormat packetFormat)
        {


            if (packetFormat == PacketFormat.Yamaha)
            {
                // PID 0x4A
                double actualTPS = (packet[16] << 8) + packet[17];
                return actualTPS;
            }
            if (packetFormat == PacketFormat.HondaKlineCB || packetFormat == PacketFormat.HondaKlineVFR)
            {
                // Absolute TPS. Zeroed by the ECU. 1.55 is correct
                double actualTPS = Math.Round(packet[13]/1.55,1);
                return actualTPS;
            }
            return 0.0;
        }



        public static double getWoolichBadETV(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                // var ETV = Math.Round(((double)col18 - 38) / 1.6575, 2);
                // WTF did I get this from? var ETV = Math.Round(((double)packet[18] - 32) / 1.78, 2); <--- This is the true formula. See log 21
                // Because there were some 34's in there???
                // I really think woolich screwed up on this one. Col 19 might actually be more accurate.

                var ETV = Math.Round(((double)packet[18] - 38) / 1.66, 2);
                return ETV;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {


                var ETV = Math.Round((((packet[67] << 8) + packet[68]) * 0.12837) - 13.864, 2);
                return ETV;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                // From log file hacking

                var ETV = ((packet[67] << 8) + packet[68]);
                return ETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // TODO: work out the formula
                // B36 is the Relative
                // B37 is the zeroed value.
                double actualTPS = Math.Round((double)(packet[36] - 31)/1.66,1); // Formula is still unknown... Is it? Woolich formula is unknown as it's not in the XML
                return actualTPS;
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // Not ride by wire. Just pass back the Woolich TPS
                return packet.getWoolichTPS(packetFormat);
            }
            else
            {
                return 0.0;
            }



        }

        // Post recall. TPS (ETV) sensor was recalibrated.
        // PID 0x11
        public static double getCorrectETV(this byte[] packet, PacketFormat packetFormat, int? minThrottle)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                int zeroThrottle = 35;
                // etv raw (18) -> Example Min = 35, max = 213. Calculation = (col[18] - min) / ((max - min) / 100)
                if (minThrottle != null)
                {
                    zeroThrottle = minThrottle.Value + 1;
                }

                // Because there were some 34's in there??? <-- 34 were no rpm. throttle body???
                // shifting to 35
                var ETV = Math.Round(((double)packet[18] - (zeroThrottle)) / 1.78, 2);
                return ETV;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {


                var ETV = Math.Round((((packet[67] << 8) + packet[68]) * 0.12837) - 13.864, 2);
                return ETV;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                // From log file hacking

                var ETV = ((packet[67] << 8) + packet[68]);
                return ETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // TODO: work out the formula
                // B36 is the Relative
                // B37 is the zeroed value.

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                double actualETV = Math.Round((double)(packet[36] - 31) / 1.66,1);
                return actualETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // Not ride by wire. Just pass back the TPS
                return packet.getTrueTPS(packetFormat); // P[12]
            }
            else
            {
                return 0.0;
            }
        }

        // Currently raw.
        public static double getEcuETV(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                // PID 45 (19)
                var ecuETV = packet[19];
                return ecuETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // TODO: work out the formula
                // B36 is the Relative
                // B37 is the zeroed value. Not likely to be 0 to 100. Formula needed.

                // Base formula. See RefineSequentialDerivedPackets for adjusted
                double actualETV = Math.Round((double)packet[37] / 1.63, 1);
                return actualETV;
            }
            else
            {
                return 0;
            }
        }

        // Currently raw.
        public static double getActualEtv2(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                // 14 (0xe) possible TPS
                // PID 0x47 Absolute throttle position B
                var ecuETV = packet[14];
                return ecuETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // Base formula. See RefineSequentialDerivedPackets for adjusted
                return packet.getEcuETV(packetFormat); // P[37]
            }
            else
            {
                return 0;
            }
        }


        public static int getActualEtvRaw(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                // PID 0x11 (18)
                var ecuETV = packet[18];
                return ecuETV;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                var ecuETV = packet[36];
                return ecuETV;
            }
            else
            {
                return 0;
            }
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



        // secondary throttle plate 0 to 255
        public static double getWoolichSTP(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Suzuki)
            {
                var STP = Math.Round((double)(((packet[20] << 8) + packet[21]) * 0.3922), 1);
                /* Surprisingly not capped.
                if (STP > 100)
                {
                    return 100.0;
                }
                */
                return STP;
            }
            return 0.0;
        }

        public static int getTPSMultiplier(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Kawasaki)
            {

                return packet[12];
            }
            else
            {
                return -99;
            }

        }

        public static double getIAP(this byte[] packet, PacketFormat packetFormat)
        {

            if (packetFormat == PacketFormat.Yamaha)
            {

                return Math.Round(packet[21] / pressureScale, 2);
            }
            else if (packetFormat == PacketFormat.Kawasaki || packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round(packet.getATMPressure(packetFormat) - packet.getMAP(packetFormat), 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // confirmed same for both.

                return packet[19]; 
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
                return packet.getIAP(packetFormat);
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                // 12 is like a display TPS. Really strange making this a single packet value.
                return Math.Round((packet[13] * 0.749984), 3);
            }
            else if(packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round(((packet[14] - 29.562 ) / 2.066116), 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // confirmed same for both. copy of IAP which is output directly 

                return packet.getIAP(packetFormat);
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
                // Also 64
                return Math.Round(packet[73] / 10.0, 2);
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                    return Math.Round(packet[64] / 10.0, 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // Unknown. No data collected yet
                return -99;
            }
            else
            {
                return -99;
            }
        }


        public static double getEngineTemperature(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[26] - temperatureOffset;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round((((packet[25] << 8) + packet[26]) * 0.55555) - 17.775,2);
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                return Math.Round(((packet[25] << 8) + packet[26]) * 0.5 - 60, 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // Confirmed
                return (packet[15] - 40);
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
                // could be 14-15. Not likely given MAP isn't.
                return Math.Round((packet[15] * 0.75) - 0.25, 2);
            }
            else if(packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round((((packet[15] << 8) + packet[16]) * 0.484) - 14.308, 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // Confirmed. Not actually logged
                return -99;
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

            else if (packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round((((packet[27] << 8) + packet[28]) * 0.55555) - 17.78, 2); // maybe 17.789
            }

            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return Math.Round(((packet[27] << 8) + packet[28]) * 0.5 - 60, 2);
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // Confirmed
                return (packet[17] - 40);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // Confirmed but also P21
                return (packet[17] - 40);
            }
            else
            {
                return -99;
            }
        }

        public static double getInjectorDuration(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                return packet[28] / 2.0;
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // Confirmed
                return (packet[24] << 8) + packet[25];
            }
            else
            {
                return -99;
            }
        }


        public static double getO2SensorVoltage(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                return packet[30] * 0.0196;
            }
            return -99;
            
        }

        public static double getO2SensorRaw(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                return packet[30];
            }
            return -99;

        }


        public static double getIgnitionOffset(this byte[] packet, PacketFormat packetFormat)
        {

            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[29] - ignitionOffset;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return -99;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {

                return Math.Round((packet[18] * 0.4) - 12.5, 1);
            }
            else if (packetFormat == PacketFormat.HondaKlineCB)
            {
                // 128 - tdc
                // Confirmed
                return packet[26] * 0.5 - 64;
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR)
            {
                // 128 - tdc
                // Confirmed... Also P30
                return packet[26] * 0.5 - 64;
            }

            else
            {
                return 0.0;
            }
        }


        public static int getRearWheelSpeed(this byte[] packet, PacketFormat packetFormat, bool raw = false)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                int wheelSpeed = (packet[35] << 8) + packet[36];
                if (raw)
                {
                    return wheelSpeed;
                }
                return (int) Math.Round(wheelSpeed / wheelSpeedSensorTicks);
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                return -99;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                // outputString += $"{((packet[36] << 8) + packet[37])},"; // Rear wheel
                if (raw)
                {
                    return ((packet[36] << 8) + packet[37]);
                }
                return (int)Math.Round(((packet[36] << 8) + packet[37]) / 18.35,0);

            }
            else
            {
                return -99;
            }

        }

        public static int getFrontWheelSpeed(this byte[] packet, PacketFormat packetFormat, bool raw = false)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                int wheelSpeed = (packet[33] << 8) + packet[34];
                if (raw)
                {
                    return wheelSpeed;
                }
                return (int)Math.Round(wheelSpeed / wheelSpeedSensorTicks);
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                return -99;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                // outputString += $"{((packet[38] << 8) + packet[39])},"; // Front wheel
                if (raw)
                {
                    return ((packet[36] << 8) + packet[37]);
                }
                return (int)Math.Round(((packet[38] << 8) + packet[39]) / 18.35, 0);
            }
            else
            {
                return -99;
            }

        }


        public static int getSpeedo(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return ((packet[31] << 8) + packet[32]);
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                return -99;
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {
                // outputString += $"{packet[40]},"; // Speedo
                return (packet[40]);
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // outputString += $"{packet[40]},"; // Speedo
                return (packet[23]);
            }
            else
            {
                return -99;
            }

        }

        public static double getBatteryVoltage(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {
                return Math.Round(packet[41] / batteryVoltageDivider, 2);
            }
            else
            {
                return -99;
            }
        }

        public static int getGear(this byte[] packet, PacketFormat packetFormat)
        {
            if (packetFormat == PacketFormat.Yamaha)
            {

                return packet[24] & 0b00000111;
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                return packet[19] & 0b00000111; // Suzuki gear indicator is glitchy. So i'm keeping the mask
            }
            else if (packetFormat == PacketFormat.Kawasaki)
            {

                return packet[19];
            }
            else if (packetFormat == PacketFormat.HondaKlineVFR || packetFormat == PacketFormat.HondaKlineCB)
            {
                // Not part of Kline data set. And yeah i looked.
                return -99;
            }
            else
            {
                return -99;
            }

        }

        public static int getRaw(this byte[] packet, int byte1, int byte2)
        {
            return ((packet[byte1] << 8) + packet[byte2]);
        }


        public static string ToBitString(this byte value, byte mask)
        {
            return "b" + Convert.ToString((byte)(value & mask), 2).PadLeft(8, '0');
        }

        // PID 0x4A Accelerator Pedal Position E
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
