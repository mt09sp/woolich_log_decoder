using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models
{
    public class WoolichMT09Log
    {



        /// <summary>
        /// PrimaryHeaderLength I expect this identifies the source of the log data. What collected it.
        /// </summary>
        public int PrimaryHeaderLength { get; set; } = 352;

        /// <summary>
        /// SecondaryHeaderLength: for mt09 was all 0xFF but for R1 wasn't. We don't use it though.
        /// </summary>
        public int SecondaryHeaderLength { get; set; } = 164;

        /// <summary>
        /// packetLength The length of the actual data packet.
        /// </summary>
        public int PacketLength { get; set; } = 0;
        public int PacketPrefixLength { get; set; } = 5;

        // Don't know yet... Differentiate between R1/MT09 and S1000RR
        // I'm going to take the value in packet prefix[4] (the 5th item) 0x01 for mt09 and R1 and 0x10 for S1000RR 
        public int PacketFormat { get; set; } = 0;


        public byte[] PrimaryHeaderData = { };
        public byte[] SecondaryHeaderData = { };

        Dictionary<string, byte[]> logData = new Dictionary<string, byte[]>();

        public int AddPacket(byte[] packet, int totalPacketLength, int packetFormat)
        {
            this.PacketLength = totalPacketLength;
            this.PacketFormat = packetFormat;

            string timeString = string.Empty;
            // Timestamp
            // var ms = packet[8] << 8;
            // ms += packet[9];
            var hh = packet[5].ToString("00");
            var MM = packet[6].ToString("00");
            var ss = packet[7].ToString("00");
            var ms1 = packet[8];
            var ms2 = packet[9];
            var ms = ((ms1 << 8) + ms2).ToString("000");

            timeString += $"{hh}:{MM}:{ss}.{ms}";

            logData.Add(timeString, packet);

            byte checksum = 0x00;

            checksum = (byte)(checksumCalculator(packet, 95) & 0xFF);
            return checksum;


        }

        int checksumCalculator(byte[] data, int length)
        {
            int curr_crc = 0x0000;
            byte sum1 = (byte)curr_crc;
            byte sum2 = (byte)(curr_crc >> 8);
            int index;
            for (index = 0; index < length; index = index + 1)
            {
                sum1 = (byte)((sum1 + data[index]) % 255);
                sum2 = (byte)((sum2 + sum1) % 255);
            }
            return (sum2 << 8) | sum1;
        }

        public Dictionary<string, byte[]> GetPackets()
        {
            return logData;
        }

        public int GetPacketCount()
        {
            return logData.Count;
        }

        public void ClearPackets()
        {
            this.logData.Clear();
            Array.Clear(PrimaryHeaderData, 0, PrimaryHeaderData.Length);
            Array.Clear(SecondaryHeaderData, 0, SecondaryHeaderData.Length);
        }

        public string GetHeader(List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols)
        {
            if (this.PacketFormat == 0x01)
            {
                // MT09 and R1
                return "time, RPM, True TPS, 14, TPS (W), ETV (W), etv raw, ETV (Correct), IAP,AFR,Speedo, Engine temp, 16-17, 19, 20, 22, ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, 30,milliseconds,Front Wheel,Rear Wheel,37-38,39,40,Battery,43,44,45,46,47,48";
            }
            if (this.PacketFormat == 0x10)
            {


                // S1000RR
                string header = "time, ms, 12-13, 14-15-16a, 14-15-16b, 22-23, 32-33, 42-43, 46-47, 52-53, 56-57, 62-63, 66-67, 75-76, 83-84, 85-86,125-124-123,";
                for (int i = 10; i < this.PacketLength; i++)
                {

                    if (staticPacketColumns.Where(c => c.Column == i).Any())
                    {
                        continue;
                    }

                    if (combinedCols.Contains(i))
                    {
                        continue;
                    }

                    header += $"{i},";
                }
                return header;
            }
            else
            {
                // Unknown bike packet type.
                string header = "time,";
                for (int i = 10; i < this.PacketLength; i++)
                {
                    header += $"{i},";
                }
                return header;

            }

        }

        public static string getCSV(byte[] packet, string timeStamp, int packetFormat, List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols)
        {
            if (packetFormat == 0x01)
            {
                // MT09 and R1
                return WoolichMT09Log.getCSV_MT09(packet, timeStamp);
            }
            if (packetFormat == 0x10)
            {
                // S1000RR
                return WoolichMT09Log.getCSV_S1000RR(packet, timeStamp, staticPacketColumns, combinedCols);
            }
            else
            {
                // Unknown bike packet type.
                return WoolichMT09Log.getCSV_Unknown(packet, timeStamp);

            }
        }


        public static string getCSV_S1000RR(byte[] packet, string timeStamp, List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols)
        {
            string outputString = string.Empty;
            // Timestamp
            // var ms = packet[8] << 8;
            // ms += packet[9];

            // var hh = packet[5].ToString("00");
            // var MM = packet[6].ToString("00");
            // var ss = packet[7].ToString("00");
            // var ms1 = packet[8];
            // var ms2 = packet[9];
            // var ms = ((ms1 << 8) + ms2).ToString("000");
            // outputString += $"{hh}:{MM}:{ss}.{ms},";

            /*
            var ms1 = packet[8];
            var ms2 = packet[9];

            var milliseconds = (((
                (packet[5] * 60) // hours to minutes 
                + packet[6]) * 60) // minutes to seconds
                + packet[7]) * 1000 // seconds to miliseconds
                + (ms1 << 8) + ms2; // plus our miliseconds.
            */
            var milliseconds = packet.getMilliseconds();

            outputString += $"{timeStamp} t,";
            outputString += $"{milliseconds},";
            // "time, 12-13, 14-15-16, 22-23, 32-33, 42-43, 46-47, 52-53, 56-57, 62-63, 66-67, 75-76, 82-83, 85-86,"
            outputString += $"{((packet[13] << 8) + packet[12])},";
            outputString += $"{(((packet[16]) << 16) + (packet[15] << 8) + packet[14])},";
            outputString += $"{(((packet[16] & 0b00000001) << 16) + (packet[15] << 8) + packet[14])},";

            outputString += $"{((packet[23] << 8) + packet[22])},";
            outputString += $"{((packet[33] << 8) + packet[32])},";
            outputString += $"{((packet[43] << 8) + packet[42])},";
            outputString += $"{((packet[47] << 8) + packet[46])},";
            outputString += $"{((packet[53] << 8) + packet[52])},";
            outputString += $"{((packet[57] << 8) + packet[56])},";
            outputString += $"{((packet[63] << 8) + packet[62])},";
            outputString += $"{((packet[67] << 8) + packet[66])},";
            outputString += $"{((packet[76] << 8) + packet[75])},";
            outputString += $"{((packet[84] << 8) + packet[83])},";
            outputString += $"{((packet[86] << 8) + packet[85])},";

            int wtf = ((packet[125] << 16) + (packet[124] << 8) + packet[123]);
            outputString += $"{Convert.ToString(wtf,2)} b,";





            for (int i = 10; i < packet.Length; i++)
            {

                if (staticPacketColumns.Where(c => c.Column == i).Any())
                {
                    continue;
                }

                if (combinedCols.Contains(i))
                {
                    continue;
                }

                outputString += $"{packet[i]},";
            }

            return outputString;
        }

        public static string getCSV_Unknown(byte[] packet, string timeStamp)
        {
            string outputString = string.Empty;
            // Timestamp
            // var ms = packet[8] << 8;
            // ms += packet[9];

            // var hh = packet[5].ToString("00");
            // var MM = packet[6].ToString("00");
            // var ss = packet[7].ToString("00");
            // var ms1 = packet[8];
            // var ms2 = packet[9];
            // var ms = ((ms1 << 8) + ms2).ToString("000");
            // outputString += $"{hh}:{MM}:{ss}.{ms},";

            /*
            var ms1 = packet[8];
            var ms2 = packet[9];

            var milliseconds = (((
                (packet[5] * 60) // hours to minutes 
                + packet[6]) * 60) // minutes to seconds
                + packet[7]) * 1000 // seconds to miliseconds
                + (ms1 << 8) + ms2; // plus our miliseconds.
            */
            var milliseconds = packet.getMilliseconds();

            outputString += $"{timeStamp},";

            for (int i = 10; i < packet.Length; i++)
            {
                outputString += $"{packet[i]},";
            }
            return outputString;
        }

        public static string getCSV_MT09(byte[] packet, string timeStamp)
        {
            string outputString = string.Empty;
            // Timestamp
            // var ms = packet[8] << 8;
            // ms += packet[9];

            // var hh = packet[5].ToString("00");
            // var MM = packet[6].ToString("00");
            // var ss = packet[7].ToString("00");
            // var ms1 = packet[8];
            // var ms2 = packet[9];
            // var ms = ((ms1 << 8) + ms2).ToString("000");
            // outputString += $"{hh}:{MM}:{ss}.{ms},";

            /*
            var ms1 = packet[8];
            var ms2 = packet[9];

            var milliseconds = (((
                (packet[5] * 60) // hours to minutes 
                + packet[6]) * 60) // minutes to seconds
                + packet[7]) * 1000 // seconds to miliseconds
                + (ms1 << 8) + ms2; // plus our miliseconds.
            */
            var milliseconds = packet.getMilliseconds();

            outputString += $"{timeStamp},";

            // 10 RPM
            outputString += $"{packet.getRPM()},";

            // 12/13 tps related (0xc,0xd)
            // I think that this is the true TPS.
            // It reacts sooner than 15 on both rise and fall.
            // var trueTPS = getTrueTPS((packet[12] << 8) + packet[13]);
            // outputString += $"{trueTPS},";
            outputString += $"{packet.getTrueTPS()},";

            // 14 (0xe) possible TPS
            outputString += $"{packet[14]},";

            // 15 (0xf) confirmed woolich TPS capped at 100. 
            outputString += $"{packet.getWoolichTPS()},";


            // 18 Woolich etv confirmed. shows 2% over on ETV (18.1 instead of 17.7) It's under on the lower side too.
            outputString += $"{packet.getWoolichBadETV()},";

            outputString += $"{packet[18]},"; // ETV raw

            outputString += $"{packet.getCorrectETV()},";

            // IAP Confirmed.
            outputString += $"{packet.getIAP()},";

            // AFR 42
            outputString += $"{packet.getAFR()},";

            // 32 Speedo No conversion ??? but that means 0 to 255 and some bikes go faster.
            // outputString += $"{packet[32]},";
            outputString += $"{packet.getSpeedo()},";

            // Engine temp
            outputString += $"{packet.getEngineTemperature()},";

            // 0 1 when running / 0 when not running
            // outputString += $"{packet[16]},";

            // 0 to 255 when running / 20 to 145 when not running.
            // outputString += $"{packet[17]},";

            // This might be ETV also...
            outputString += $"{packet.get1617()},";

            // 18 im calling etv raw but I don't think it's accurate...

            // 5 to 36 not running. / 0 to 88 when running.
            outputString += $"{packet[19]},";

            // 0 both running and not running
            outputString += $"{packet[20]},";

            // 0 both running and not running
            outputString += $"{packet[22]},";

            // ATM Pressure
            outputString += $"{packet.getATMPressure()},";

            // Always 128???
            // 128 to 129 gear 0 to gear 1 last 4 bits are gears. 001 = 1, 010 = 2, 011 = 3, 100 = 4, 101 = 5, 110 = 6
            var firstPart = packet[24] & 0b11111000;
            outputString += $"{firstPart},";
            outputString += $"{packet.getGear()},";

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
            outputString += $"{bit4}{bit5}{bit6}{bit7}{bit8},";

            // Inlet temp
            outputString += $"{packet.getInletTemperature()},";

            // 0 to 13 with motor running (injector duration)
            outputString += $"{packet.getInjectorDuration()},";

            // 20 to 87 with motor running
            outputString += $"{packet.getIgnitionOffset()},";

            // 30 Captured but not mapped in either viewer.
            outputString += $"{packet[30]},";

            // jambing milliseconds in here.
            outputString += $"{milliseconds},";

            // Front Wheel Speed 33,34
            outputString += $"{packet.getFrontWheelSpeed()},";

            // 35/36 Rear wheel speed
            // wheelSpeedSensorTicks
            outputString += $"{packet.getRearWheelSpeed()},";


            // is 37/38 a 2 byte chunk?
            // 0 to 2 motor running / 0 motor not running.
            // outputString += $"{packet[37]},";
            // 0 to 255 motor running | 1, 7, 3, 5, 10 motor not running. Thought was checksum
            // outputString += $"{packet[38]},";
            outputString += $"{packet.get3738()},";

            // Nothing
            outputString += $"{packet[39]},";

            // Nothing
            outputString += $"{packet[40]},";

            // Battery Voltage 41
            outputString += $"{packet.getBatteryVoltage()},";

            // 42?

            // Nothing
            outputString += $"{packet[43]},";

            // Nothing
            outputString += $"{packet[44]},";

            // Nothing
            outputString += $"{packet[45]},";

            // Nothing
            outputString += $"{packet[46]},";

            // Nothing
            outputString += $"{packet[47]},";

            // Nothing
            outputString += $"{packet[48]},";

            // outputString += $"{packet[49]},";
            // outputString += $"{packet[50]},";
            // outputString += $"{packet[51]},";
            // outputString += $"{packet[52]},";
            // outputString += $"{packet[53]},";
            // outputString += $"{packet[54]},";
            // outputString += $"{packet[55]},";
            // outputString += $"{packet[56]},";
            // outputString += $"{packet[57]},";
            // outputString += $"{packet[58]},";
            // outputString += $"{packet[59]},";

            return outputString;
        }





    }
}
