using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using WoolichDecoder.Models.CsvModels;


namespace WoolichDecoder.Models
{
    public class WoolichLog
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
        public PacketFormat packetFormat { get; set; } = PacketFormat.Unknown;



        public byte[] PrimaryHeaderData = { };
        public byte[] SecondaryHeaderData = { };

        SortedDictionary<string, byte[]> logData = new SortedDictionary<string, byte[]>();

        public int AddPacket(byte[] packet, int totalPacketLength)
        {
            this.PacketLength = totalPacketLength;

            var checkedDataLength = this.PacketLength - 1;

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

            // checkedDataLength = 95 for yamaha 98 bit packet. 78 for kwaka 79 bit packet
            checksum = (byte)(checksumCalculator(packet, checkedDataLength) & 0xFF);
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

        public SortedDictionary<string, byte[]> GetPackets()
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


        public void BackfillShortPackets()
        {

            int longPacketLength = 0;
            int shortPacketLength = 0;
            if (packetFormat == PacketFormat.Suzuki) {
                longPacketLength = 79;
                shortPacketLength = 41;
            }

            // First pass: Find all long record keys (avoid ToList() to save memory)
            var longRecordKeys = new List<string>();
            foreach (var kvp in logData)
            {
                if (kvp.Value.Length == longPacketLength)
                {
                    longRecordKeys.Add(kvp.Key);
                }
            }

            // Need at least 3 long records to start processing
            if (longRecordKeys.Count < 3)
            {
                // Remove only short records if we don't have enough long records
                var shortKeysToRemove = new List<string>();
                foreach (var kvp in logData)
                {
                    if (kvp.Value.Length == shortPacketLength)
                    {
                        shortKeysToRemove.Add(kvp.Key);
                    }
                }

                foreach (string key in shortKeysToRemove)
                {
                    logData.Remove(key);
                }
                return;
            }

            // Process in chunks to minimize memory usage
            var keysToRemove = new List<string>();

            // Process pairs of consecutive long records starting from the 3rd
            for (int longRecordPairIndex = 2; longRecordPairIndex < longRecordKeys.Count; longRecordPairIndex++)
            {
                string priorLongKey = longRecordKeys[longRecordPairIndex - 1];
                string currentLongKey = longRecordKeys[longRecordPairIndex];

                byte[] priorLongRecord = logData[priorLongKey];
                byte[] currentLongRecord = logData[currentLongKey];

                // Find and process short records between these two long records
                bool foundPriorLong = false;
                bool foundCurrentLong = false;
                var shortRecordsBetween = new List<string>();

                foreach (var kvp in logData)
                {
                    if (kvp.Key == priorLongKey)
                    {
                        foundPriorLong = true;
                        continue;
                    }

                    if (kvp.Key == currentLongKey)
                    {
                        foundCurrentLong = true;
                        break;
                    }

                    if (foundPriorLong && kvp.Value.Length == shortPacketLength)
                    {
                        shortRecordsBetween.Add(kvp.Key);
                    }
                }

                // Update short records between this pair
                foreach (string shortKey in shortRecordsBetween)
                {
                    var updatedShortPacket = CalculateShortRecord(
                        packetFormat,
                        logData[shortKey],
                        priorLongRecord,
                        currentLongRecord
                    );

                    logData[shortKey] = updatedShortPacket;
                }
            }

            // Second pass: Mark short records for removal
            // (those before 3rd long record and after last long record)
            string thirdLongKey = longRecordKeys[2];
            string lastLongKey = longRecordKeys[longRecordKeys.Count - 1];

            bool reachedThirdLong = false;
            bool passedLastLong = false;

            foreach (var kvp in logData)
            {
                if (kvp.Key == thirdLongKey)
                {
                    reachedThirdLong = true;
                    continue;
                }

                if (kvp.Key == lastLongKey)
                {
                    passedLastLong = true;
                    continue;
                }

                // Mark short records before 3rd long or after last long for removal
                if (kvp.Value.Length == shortPacketLength)
                {
                    if (!reachedThirdLong || passedLastLong)
                    {
                        keysToRemove.Add(kvp.Key);
                    }
                }
            }

            // Remove marked keys
            foreach (string key in keysToRemove)
            {
                logData.Remove(key);
            }
        }

        // Works but... We need it after the calculations which means we don't do anything other than the expansion.
        private byte[] CalculateShortRecord(PacketFormat packetFormat, byte[] shortPacket, byte[] priorLongPacket, byte[] currentLongPacket)
        {

            byte[] extendedPacket = new byte[currentLongPacket.Length];

            int currentMilliseconds = currentLongPacket.getMilliseconds();
            int priorMilliseconds = priorLongPacket.getMilliseconds();
            int shortMilliseconds = shortPacket.getMilliseconds();

            int[] shortPacketBytesArray = { 64, 73, 77, 78 };


            double multiplier = (shortMilliseconds - priorMilliseconds) / (currentMilliseconds - priorMilliseconds);

            if (packetFormat == PacketFormat.Suzuki) {
                // The timestamp
                for (int i = 0; i < 10; i++) {
                    extendedPacket[i] = shortPacket[i];
                }

                for (int i = 10; i < currentLongPacket.Length; i++)
                {
                    extendedPacket[i] = 0;
                }

                // Actual short packet values
                extendedPacket[64] = shortPacket[26]; // AFR
                extendedPacket[73] = shortPacket[35]; // Mystery
                extendedPacket[77] = shortPacket[39]; // Mystery
                extendedPacket[78] = shortPacket[40]; // chksum... kind of pointless really

                /* not doing. Woolich do this last after t he calculations.
                short rpm = AdjustWordPacket(priorLongPacket, currentLongPacket, 10, priorMilliseconds, currentMilliseconds, shortMilliseconds);
                extendedPacket[10] = (byte)(rpm >> 8);
                extendedPacket[11] = (byte)(rpm & 0xFF);


                for (int i = 12; i < currentLongPacket.Length; i++)
                {
                    if (shortPacketBytesArray.Contains(i)) continue;

                    extendedPacket[i] = AdjustPacket(priorLongPacket, currentLongPacket, i, priorMilliseconds, currentMilliseconds, shortMilliseconds);
                }
                */

            }

            return extendedPacket;
        }

        /*
        public static byte AdjustPacket( byte[] priorLongPacket, byte[] currentLongPacket, int index, int priorMilliseconds, int currentMilliseconds, int shortMilliseconds)
        {
            double timediff = currentMilliseconds - priorMilliseconds;
            double spread = currentLongPacket[index] - priorLongPacket[index];
            double valueChangePerMs = spread / timediff;
            double adjustment = (double)(valueChangePerMs * (shortMilliseconds - priorMilliseconds));
            int adjustedValue = priorLongPacket[index] + (int)adjustment;

            return (byte)adjustedValue;
        }

        public static short AdjustWordPacket(byte[] priorLongPacket, byte[] currentLongPacket, int index, int priorMilliseconds, int currentMilliseconds, int shortMilliseconds)
        {
            double timediff = currentMilliseconds - priorMilliseconds;

            double spread = ((currentLongPacket[index] << 8) + currentLongPacket[index + 1]) - ((priorLongPacket[index] << 8) + priorLongPacket[index + 1]);

            double valueChangePerMs = spread / timediff;
            double adjustment = (double)(valueChangePerMs * (shortMilliseconds - priorMilliseconds));
            int adjustedValue =  ((priorLongPacket[index] << 8) + priorLongPacket[index + 1]) + (int)adjustment;

            return (short)adjustedValue;
        }
        */

        public string GetHeader(List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols, bool includeRaws)
        {


            if (packetFormat == PacketFormat.Yamaha && !includeRaws)
            {
                // MT09 and R1
                return "time, ms, RPM, True TPS PID 0x49, PID 0x47, TPS (W), ETV (W), etv PID 0x11 (18), ETV (Correct), IAP,AFR,Speedo, Engine temp, PID 0x4a 16-17, EcuETV (19), ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, o2 Sensor V,Front Wheel,Rear Wheel,P37-38,P39,P40,Battery,P43,P44,P45,P46,P47,P48";
            }
            if (packetFormat == PacketFormat.Yamaha && includeRaws)
            {
                // MT09 and R1
                return "time, ms, RPM, PID 0x49 (12-13), True TPS, PID 0x47 (14), PID 0x5a(15), TPS (W), ETV (W), etv PID 0x11 (18), ETV (Correct), IAP,AFR,Speedo, Engine temp, PID 0x4a 16-17, EcuETV (19), P20, P22, ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, o2 Sensor V, PID 0x14 o2 (30),Front Wheel,Rear Wheel,P37-38,P39,P40,Battery,P43,P44,P45,P46,P47,P48";
            }


            if (packetFormat == PacketFormat.BMW)
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
            else if (packetFormat == PacketFormat.Kawasaki)
            {


                return getCSV_ZX10RHeader();
            }
            else if (packetFormat == PacketFormat.Suzuki)
            {
                return getCSV_SuzukiHeader();
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

        public static string getCSV(byte[] packet, string timeStamp, PacketFormat exportFormat, List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols, bool includeRaws)
        {
            if (exportFormat == PacketFormat.BMW)
            {
                // S1000RR
                return WoolichLog.getCSV_S1000RR(packet, timeStamp, staticPacketColumns, combinedCols);
            }
            else
            {
                // Unknown bike packet type.
                return WoolichLog.getCSV_Unknown(packet, timeStamp);

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


        public static string getCSV_SuzukiHeader()
        {
            // Note: TPS,RPM,IAP,AFR,CoolantTemperature,InletAirTemperature,Gear,AtmosphericPressure,ManifoldPressure,IgnitionAdvance,SecondaryThrottlePlates,MS,Clutch,PAIR


            // hayabusa
            /*
            string header = "time, ms, TPS(W), TPS, RPM, RPMRaw, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, IgnitionAdvance, STP, MS,Clutch, PAIR, ETV," +
                // "RPM, RPM Raw, AFR," +
                // "P10,P11, TPS 12," +
                "TPS 12, P13, P14, P15, ATM 16, P17, ign 18, gear 19," +
                "P20, STP 21, P22, P23, P24, P25, TMP 26, 27, inlt tmp 28, P29," +
                "P30, P31, P32, P33, P34,P35, P36, P37, P38, P39," +
                "40,41,42,43,44,45,46,47,48,49," +
                "50,51,52,53,54,55,56,57,58,59," +
                "60, 61, 62, 63, 64 AFR,65,66,67,68,69," +
                "70,71,72,73,74,75, 76,77, chk";
            return header;
            */

            string header = "time, ms, TPS(W), TPS, RPM, RPMRaw, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, IgnitionAdvance, STP, MS,Clutch, PAIR, ETV, B33";
            return header;

        }

        public static void getCSV_Suzuki(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns)
        {

            // string header = "time, ms, TPS(W), TPS, RPM, IAP, AFR, CoolantTemperature, InletAirTemperature, Gear, AtmosphericPressure, ManifoldPressure, IgnitionAdvance, SecondaryThrottlePlates, MS,Clutch, PAIR," +
            // "10, 11, 12,13,14,15,16,17,18,19," +
            // "20,21,22,23,24,25,26,27,28,29," +
            // "30,31,32,33,34,35,36,37,38,39," +
            // "40,41,42,43,44,45,46,47,48,49," +
            // "50,51,52,53,54,55,56,57,58,59," +
            // "60,61,62,63,64 AFR,65,66,67,68,69," +
            // "70,71,72,73,74,75,76,77,78";

            SuzukiCsvModel PriorPacket = null;
            SuzukiCsvModel NextPacket = null;
            List<KeyValuePair<string, byte[]>> shortPackets = new List<KeyValuePair<string, byte[]>>();
            int longPacketCount = 0;

            List<SuzukiCsvModel> suzukiCsvModels = new List<SuzukiCsvModel>();

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {


                var packetValue = packet.Value;

                // The hayabusa file we extracted this from outputs data at a lower rate than other bikes and lower than the zeitronix does or at least lower than the logger checks.
                // AFR only packets are shorter at 41 
                if (packetValue[3] == 0x26) // short one
                {
                    if (longPacketCount > 1)
                    {
                        // push to temporary list for processing later
                        shortPackets.Add(packet);
                    }
                }
                else
                {
                    longPacketCount++;

                    SuzukiCsvModel tmpSuzukiCsvModel = new SuzukiCsvModel();
                    tmpSuzukiCsvModel.PopulateFromLongPacket(packet);


                    // long packet.
                    if (PriorPacket == null)
                    {
                        // Very first packet
                        PriorPacket = tmpSuzukiCsvModel;
                        suzukiCsvModels.Add(tmpSuzukiCsvModel);
                    }
                    else
                    {
                        NextPacket = tmpSuzukiCsvModel;
                        if (longPacketCount > 1)
                        {

                            // convert shortPackets to SuzukiCsvModel
                            foreach (KeyValuePair<string, byte[]> shortPacket in shortPackets)
                            {
                                tmpSuzukiCsvModel = new SuzukiCsvModel();
                                tmpSuzukiCsvModel.PopulateFromShortPacket(shortPacket, PriorPacket, NextPacket);

                                suzukiCsvModels.Add(tmpSuzukiCsvModel);

                            }
                            shortPackets.Clear();

                        }

                        PriorPacket = NextPacket;
                        suzukiCsvModels.Add(NextPacket);
                    }
                }
            }


            foreach (SuzukiCsvModel csvModel in suzukiCsvModels)
            {
                // string header = "time, ms, TPS, RPM, RPMRaw, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, IgnitionAdvance, STP, MS, Clutch, PAIR," +
                // "10, 11, 12,13,14,15,16,17,18,19," +
                // "20,21,22,23,24,25,26,27,28,29," +
                // "30,31,32,33,34,35,36,37,38,39," +
                // "40,41,42,43,44,45,46,47,48,49," +
                // "50,51,52,53,54,55,56,57,58,59," +
                // "60,61,62,63,64 AFR,65,66,67,68,69," +
                // "70,71,72,73,74,75,76,77,78";

                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";
                outputString += $"{csvModel.WoolichTPS},"; // TPS
                outputString += $"{csvModel.ActualTPS},"; // TPS
                outputString += $"{csvModel.RPM},"; // RPM
                outputString += $"{csvModel.RPMRaw},"; // RPMRaw
                outputString += $"{csvModel.IAP},"; // IAP
                outputString += $"{csvModel.AFR},"; // AFR
                outputString += $"{csvModel.CoolantTemp},"; // CoolantTemp
                outputString += $"{csvModel.InletAirTemp},"; // InletAirTemp
                outputString += $"{csvModel.Gear},"; // Gear
                outputString += $"{csvModel.AtmPressure},"; // AtmPressure 16
                outputString += $"{csvModel.ManPressure},"; // ManPressure
                outputString += $"{csvModel.Ignition},"; // IgnitionAdvance 18
                outputString += $"{csvModel.STP},"; // STP
                outputString += $"{csvModel.MS},"; // MS
                outputString += $"{csvModel.Clutch},"; // Clutch
                outputString += $"{csvModel.Pair},"; // PAIR

                outputString += $"{csvModel.WoolichEtv},"; // 67 - 68 from hacking
                outputString += $"{csvModel.P33},";
                /*
                for (int i = 12; i < csvModel.packets.Length; i++)
                {

                    if (presumedStaticColumns.Where(c => c.Column == i).Any())
                    {
                        continue;
                    }

                    int[] bitwiseCols = { 33 };
                    if (bitwiseCols.Contains(i))
                    {
                        // Your special code here
                        outputString += $"{csvModel.P33},";
                        continue;
                    }

                    int[] decodedColumns = {  };
                    if (decodedColumns.Contains(i))
                    {
                        continue;
                    }

                outputString += $"{csvModel.packets[i]},";
                }
                */
                outputFile.WriteLine(outputString);
                outputFile.Flush();

            }

        }


        public static string getCSV_ZX10RHeader()
        {


            // ZX10R gen6
            // string header = "time, ms, 10-11 RPM, 12, 13 map raw, MAP, 15 atm raw, atm, IAP, 19 gear, 23, 24, 25-26 eng temp, 27-28 inlet temp, clutch 33, 36-37 Rear, 38-39 Front, 40 speedo, 42, 64 (AFR1), TPS(W), TPS True, TPS Raw, 67-68 ETV, ETV raw, 69, 70, 73 (AFR), 73(AFR2), 78 chk,";

            string headerWStatic = "time, ms, 10-11 RPM, P12, 13 map raw, MAP(13), P14, 15 atm raw, atm(15), IAP, P16, P17, P18, 19 gear, " +
                "P20, P21, P22, P23, P24, 25-26 eng temp, 27-28 inlet temp, P29, P30, P31, P32, clutch 33, 36-37 Rear, 38-39 Front, " +
                "40 speedo, P41, P42, P43, P44, P45, P46, P47, P48, P49," +
                "P50, P51, P52, P53, P54, P55, P56, P57, P58, P59," +
                "P60, P61, P62, P63," +
                "64 (AFR1), TPS(W), TPS True, TPS Raw, 67-68 ETV, ETV raw, 69, 70, 73 (AFR), 73(AFR2), 78 chk,";



            string header = "time, ms, TPS(W), TPS, TPS Mul, RPM, ETV(W), ETV, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, Speed,";
            // "P23b?, P24, 25-26 eng temp, 27-28 inlet temp, clutch 33, 36-37 Rear, 38-39 Front, " +
            // "40 speedo, P42," + 
            // STATIC "P43, P44, P45, P46, P47, P48, P49," +
            // STATIC "P50, P51, P52, P53, P54, P55, P56, P57, P58, P59," +
            // STATIC "P60, P61, P62, P63," +
            // "64 (AFR1), TPS(W), TPS True, TPS Raw, 67-68 ETV, ETV raw, P69, P70, 73 (AFR3), 73(AFR2), 78 chk,";

            string headerWithNonStatic = "time, ms, TPS(W), TPS, TPS Mul, RPM, ETV(W), ETV, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, RW Speed, FW Speed, Speed, Clutch," +
                "P42, P23, P69, P70, " +
            "P12, P13, P15, P19, P23, P25, P26, P27, P28, clutch 33, P36 RW1, P37 RW2, 38 FW1, 39 FW2, 40 speedo, P42," +
            "64 (AFR1), P65-P66, P67-68, P69, P70, 73(AFR2), 78 chk,";


            return headerWithNonStatic;

        }

        public static void getCSV_Kawasaki(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns)
        {

            List<KawasakiCsvModel> kawasakiCsvModels = new List<KawasakiCsvModel>();

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {
                var packetValue = packet.Value;
                KawasakiCsvModel tmpKawasakiCsvModel = new KawasakiCsvModel();
                tmpKawasakiCsvModel.PopulateFromLongPacket(packet);

                kawasakiCsvModels.Add(tmpKawasakiCsvModel);
            }


            foreach (KawasakiCsvModel csvModel in kawasakiCsvModels)
            {


                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";
                outputString += $"{csvModel.WoolichTPS},"; // 
                outputString += $"{csvModel.ActualTPS},"; // 
                outputString += $"{csvModel.TPSMultiplier},"; // 
                outputString += $"{csvModel.RPM},"; // RPM
                outputString += $"{csvModel.WoolichEtv},"; // 
                outputString += $"{csvModel.ActualEtv},"; // 

                outputString += $"{csvModel.IAP},"; // IAP
                outputString += $"{csvModel.AFR},"; // AFR
                outputString += $"{csvModel.CoolantTemp},"; // CoolantTemp
                outputString += $"{csvModel.InletAirTemp},"; // InletAirTemp
                outputString += $"{csvModel.Gear},"; // Gear
                outputString += $"{csvModel.AtmPressure},"; // AtmPressure
                outputString += $"{csvModel.ManPressure},"; // ManPressure

                outputString += $"{csvModel.RearWheelSpeed},"; // RearWheelSpeed
                outputString += $"{csvModel.FrontWheelSpeed},"; // FrontWheelSpeed

                outputString += $"{csvModel.Speedo},"; // FrontWheelSpeed
                outputString += $"{csvModel.Clutch},"; // Clutch


                // outputString += $"{csvModel.Ignition},"; // 

                // outputString += $"{csvModel.STP},"; // STP

                // outputString += $"{csvModel.MS},"; // MS
                // outputString += $"{csvModel.Pair},"; // PAIR

                outputString += $"{csvModel.P42},"; // Unknown
                outputString += $"{csvModel.P23},"; // Unknown
                outputString += $"{csvModel.P69},"; // Unknown
                outputString += $"{csvModel.P70},"; // Unknown

                ///*
                for (int i = 12; i < csvModel.packets.Length; i++)
                {

                    if (presumedStaticColumns.Where(c => c.Column == i).Any())
                    {
                        continue;
                    }

                    /*
                    int[] bitwiseCols = { 33 };
                    if (bitwiseCols.Contains(i))
                    {
                        // Your special code here
                        outputString += $"{csvModel.P33},";
                        continue;
                    }
                    */

                    int[] decodedColumns = { 65, 66, 67, 68, 36, 37, 38, 39 };
                    if (decodedColumns.Contains(i))
                    {
                        continue;
                    }

                outputString += $"{csvModel.packets[i]},";
                }
                // */

                outputFile.WriteLine(outputString);
                outputFile.Flush();

            }

        }


        /*

            if (packetFormat == PacketFormat.Yamaha && !includeRaws)
            {
                // MT09 and R1
                return "time, ms, RPM, True TPS PID 0x49, PID 0x47, TPS (W), ETV (W), etv PID 0x11 (18), ETV (Correct), IAP,AFR,Speedo, Engine temp, PID 0x4a 16-17, PID 0x45 19, ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, o2 Sensor V,Front Wheel,Rear Wheel,37-38,39,40,Battery,43,44,45,46,47,48";
            }
            if (packetFormat == PacketFormat.Yamaha && includeRaws)
            {
                // MT09 and R1
                return "time, ms, RPM, PID 0x49 (12-13), True TPS, PID 0x47 (14), PID 0x5a(15), TPS (W), ETV (W), etv PID 0x11 (18), ETV (Correct), IAP,AFR,Speedo, Engine temp, PID 0x4a 16-17, PID 0x45 19, 20, 22, ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, o2 Sensor V, PID 0x14 o2 (30),Front Wheel,Rear Wheel,37-38,39,40,Battery,43,44,45,46,47,48";
            }
        */


        public static void getCSV_MT09(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns, bool includeRaws)
        {


            List<YamahaCsvModel> yamahaCsvModel = new List<YamahaCsvModel>();

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {
                var packetValue = packet.Value;
                YamahaCsvModel tmpYamahaCsvModel = new YamahaCsvModel();
                tmpYamahaCsvModel.PopulateFromLongPacket(packet);

                yamahaCsvModel.Add(tmpYamahaCsvModel);
            }


            foreach (YamahaCsvModel csvModel in yamahaCsvModel)
            {

                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";
                outputString += $"{csvModel.RPM},"; // 
                if (includeRaws)
                {
                    outputString += $"{csvModel.ActualTPSRaw},"; //
                }
                outputString += $"{csvModel.ActualTPS},"; // 
                outputString += $"{csvModel.ActualEtv2},"; // 


                if (includeRaws)
                {
                    outputString += $"{csvModel.WoolichTPSRaw},"; //
                }

                outputString += $"{csvModel.WoolichTPS},"; // 

                outputString += $"{csvModel.WoolichEtv},"; // 
                outputString += $"{csvModel.ActualEtvRaw},"; // 
                outputString += $"{csvModel.ActualEtv},"; // 

                outputString += $"{csvModel.IAP},"; // IAP
                outputString += $"{csvModel.AFR},"; // AFR

                outputString += $"{csvModel.Speedo},"; // 

                outputString += $"{csvModel.CoolantTemp},"; // CoolantTemp

                outputString += $"{csvModel.ActualTPS2},"; // ActualTPS2 (16-17)

                outputString += $"{csvModel.EcuETV},"; // EcuETV (19)

                if (includeRaws)
                {
                    outputString += $"{csvModel.P20},"; //
                    outputString += $"{csvModel.P22},"; //
                }

                outputString += $"{csvModel.AtmPressure},"; // AtmPressure

                outputString += $"{csvModel.P24Pt1},"; // the first part of the gear byte 24
                outputString += $"{csvModel.Gear},"; // Gear (24)




                outputString += $"{csvModel.Clutch},"; // Clutch? (25)
                outputString += $"{csvModel.P25Pt2},"; // P25Pt2? (25)
                outputString += $"{csvModel.ThrottleOff},"; // ThrottleOff? (25)
                outputString += $"{csvModel.P25Pt4},"; // P25Pt4? (25)



                outputString += $"{csvModel.InletAirTemp},"; // InletAirTemp
                outputString += $"{csvModel.InjectorDuration},"; // InjectorDuration
                outputString += $"{csvModel.Ignition},"; // Ignition
                outputString += $"{csvModel.O2SensorVoltage},"; // O2SensorVoltage


                if (includeRaws)
                {
                    outputString += $"{csvModel.O2SensorRaw},"; //
                }

                outputString += $"{csvModel.FrontWheelSpeed},"; // FrontWheelSpeed
                outputString += $"{csvModel.RearWheelSpeed},"; // RearWheelSpeed


                outputString += $"{csvModel.P37P38},"; // P37P38
                // I could calculate this... outputString += $"{csvModel.ManPressure},"; // ManPressure


                outputString += $"{csvModel.P39},"; // P39
                outputString += $"{csvModel.P40},"; // P39
                outputString += $"{csvModel.Battery},"; // P39
                outputString += $"{csvModel.P43},"; // P39
                outputString += $"{csvModel.P44},"; // P39
                outputString += $"{csvModel.P45},"; // P39
                outputString += $"{csvModel.P46},"; // P39
                outputString += $"{csvModel.P47},"; // P39
                outputString += $"{csvModel.P48},"; // P39

                outputFile.WriteLine(outputString);
                outputFile.Flush();

            }

            /*

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

            
            // var ms1 = packet[8];
            // var ms2 = packet[9];

            // var milliseconds = (((
            //     (packet[5] * 60) // hours to minutes 
            //     + packet[6]) * 60) // minutes to seconds
            //     + packet[7]) * 1000 // seconds to miliseconds
            //     + (ms1 << 8) + ms2; // plus our miliseconds.
            
            var milliseconds = packet.getMilliseconds();

            outputString += $"{timeStamp} t,";

            // 10 RPM
            outputString += $"{packet.getRPM()},";



            // 12/13 tps related (0xc,0xd)
            // I think that this is the true TPS.
            // It reacts sooner than 15 on both rise and fall.
            // var trueTPS = getTrueTPS((packet[12] << 8) + packet[13]);
            // PID 0x49 (Accelerator Pedal Position D)
            if (includeRaws)
            {
                outputString += $"{(packet[12] << 8) + packet[13]},";
            }

            // 12/13 tps related (0xc,0xd)
            // I think that this is the true TPS.
            // outputString += $"{trueTPS},";
            // PID 0x49 (Accelerator Pedal Position D)
            outputString += $"{packet.getTrueTPS(PacketFormat.Yamaha)},";




            // 14 (0xe) possible TPS
            // PID 0x47 Absolute throttle position B
            outputString += $"{packet[14]},";

            if (includeRaws)
            {
                // 15 (0xf) confirmed woolich TPS capped at 100. 
                // PID 0x5a (Relative Accelerator Pedal Position)
                outputString += $"{packet[15]},";
            }

            // 15 (0xf) confirmed woolich TPS capped at 100. 
            // PID 0x5a (Relative Accelerator Pedal Position)
            outputString += $"{packet.getWoolichTPS(PacketFormat.Yamaha)},";


            // 18 Woolich etv confirmed. shows 2% over on ETV (18.1 instead of 17.7) It's under on the lower side too.
            outputString += $"{packet.getWoolichBadETV(PacketFormat.Yamaha)},";

            outputString += $"{packet[18]},"; // ETV raw PID 0x11

            outputString += $"{packet.getCorrectETV(PacketFormat.Yamaha)},";

            // IAP Confirmed. (packet 21)
            outputString += $"{packet.getIAP(PacketFormat.Yamaha)},";

            // AFR 42
            outputString += $"{packet.getAFR(PacketFormat.Yamaha)},";
            // outputString += $"{packet[42]},";

            // 32 Speedo No conversion ??? but that means 0 to 255 and some bikes go faster.
            // outputString += $"{packet[32]},";
            outputString += $"{packet.getSpeedo()},";

            // Engine temp
            outputString += $"{packet.getEngineTemperature(PacketFormat.Yamaha)},";

            // 0 1 when running / 0 when not running
            // outputString += $"{packet[16]},";

            // 0 to 255 when running / 20 to 145 when not running.
            // outputString += $"{packet[17]},";

            // This might be ETV also...
            // PID 0x4A Accelerator Pedal Position E
            outputString += $"{packet.get1617()},";

            // 18 im calling etv raw but I don't think it's accurate...

            // 5 to 36 not running. / 0 to 88 when running.
            // PID 0x45 Relative throttle (ETV) position 
            outputString += $"{packet[19]},";




            // 0 both running and not running
            outputString += $"{packet[20]},";

            // 0 both running and not running
            outputString += $"{packet[22]},";

            // ATM Pressure
            outputString += $"{packet.getATMPressure(PacketFormat.Yamaha)},";

            // Always 128???
            // 128 to 129 gear 0 to gear 1 last 4 bits are gears. 001 = 1, 010 = 2, 011 = 3, 100 = 4, 101 = 5, 110 = 6
            var firstPart = packet[24] & 0b11111000;
            outputString += $"{firstPart},";
            outputString += $"{packet.getGear(PacketFormat.Yamaha)},";

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
            outputString += $"b{bit4}{bit5}{bit6}{bit7}{bit8},";

            // Inlet temp
            outputString += $"{packet.getInletTemperature(PacketFormat.Yamaha)},";

            // 0 to 13 with motor running (injector duration)
            outputString += $"{packet.getInjectorDuration()},";

            // 20 to 87 with motor running
            outputString += $"{packet.getIgnitionOffset(PacketFormat.Yamaha)},";

            // 30 Captured but not mapped in either viewer. Yay
            // o2 Lambda sensor 
            outputString += $"{packet.getO2Sensor(PacketFormat.Yamaha)},";
            if (includeRaws)
            {
                // PID 0x14 - Oxygen sensor 1 (Bank 1, Sensor 1) - voltage
                outputString += $"{packet[30]},";
            }


            // jambing milliseconds in here.
            outputString += $"{milliseconds},";

            // Front Wheel Speed 33,34
            outputString += $"{packet.getFrontWheelSpeed(PacketFormat.Yamaha)},";

            // 35/36 Rear wheel speed
            // wheelSpeedSensorTicks
            outputString += $"{packet.getRearWheelSpeed(PacketFormat.Yamaha)},";


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

            */
        }



        public static void getCSV_StartingPointExample(StreamWriter outputFile, SortedDictionary<string, byte[]> logs)
        {

            KeyValuePair<string, byte[]> lastPacket = logs.First();

            // string header = "time, ms, TPS, RPM, IAP, AFR, CoolantTemperature, InletAirTemperature, Gear, AtmosphericPressure, ManifoldPressure, IgnitionAdvance, SecondaryThrottlePlates, MS,Clutch, PAIR," +
            // "RPM, 10-11, 12,13,14,15,16,17,18,19," +
            // "20,21,22,23,24,25,26,27,28,29," +
            // "30,31,32,33,34,35,36,37,38,39," +
            // "40,41,42,43,44,45,46,47,48,49," +
            // "50,51,52,53,54,55,56,57,58,59," +
            // "60,61,62,63,64 AFR,65,66,67,68,69," +
            // "70,71,72,73,74,75,76,77";

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {

                string outputString = string.Empty;

                var packetValue = packet.Value;

                outputString += $"{packet.Key} t,"; // timestamp
                var milliseconds = packetValue.getMilliseconds();
                outputString += $"{milliseconds},";

                // output for xml packets. comment out when no longer needed. do not delete because this will be a template for other files.
                for (int i = 0; i < 13; i++)
                {
                    outputString += ",";
                }

                // The hayabusa file we extracted this from outputs data at a lower rate than other bikes and lower than the zeitronix does or at least lower than the logger checks.
                // AFR only packets are shorter at 41 
                if (packetValue.Length <= 41)
                {
                    // decode parts
                    outputString += "RPM,"; // RPM
                    outputString += "RPM,"; // RPM


                    //outputString += $"{packet.getRPM()},"; // RPM = packet.getRPM() * 0.39

                    // packet 12 * 0.6 - 32.8 = TPS

                    // packet 64 * 0.1 = AFR


                    // It's a short packet 41 bytes long. We need to map it into a big packet.
                    for (int i = 10; i < 79; i++)
                    {

                        switch (i)
                        {
                            case 64: // AFR
                                // code block
                                outputString += $"{packetValue[26]},";
                                break;
                            case 73:
                                outputString += $"{packetValue[35]},";
                                break;
                            case 77:
                                outputString += $"{packetValue[39]},";
                                break;
                            default:
                                outputString += ",";
                                break;
                        }
                    }
                }
                else
                {
                    // decode parts
                    outputString += ","; // AFR

                    // It's a long full packet. 79 bytes long
                    for (int i = 10; i < packetValue.Length; i++)
                    {
                        outputString += $"{packetValue[i]},";
                    }
                }


                outputFile.WriteLine(outputString);
                outputFile.Flush();
            }
        }




    }
}

/*

using System;
using System.Collections.Generic;
using System.Linq;

public void ProcessLogsDictionary(Dictionary<string, byte[]> logs)
{
    // Convert dictionary to list of key-value pairs to maintain order
    var logList = logs.ToList();
    
    // Find indices of all long records (79 bytes)
    var longRecordIndices = new List<int>();
    for (int i = 0; i < logList.Count; i++)
    {
        if (logList[i].Value.Length == 79)
        {
            longRecordIndices.Add(i);
        }
    }
    
    // Need at least 3 long records to start processing
    if (longRecordIndices.Count < 3)
    {
        // Remove only short records if we don't have enough long records
        var keysToRemove = logs.Where(kvp => kvp.Value.Length == 41).Select(kvp => kvp.Key).ToList();
        foreach (string key in keysToRemove)
        {
            logs.Remove(key);
        }
        return;
    }
    
    // Keep track of keys to remove (only short records)
    var shortKeysToRemove = new List<string>();
    
    // Process pairs of consecutive long records starting from the 3rd long record
    for (int longRecordPairIndex = 2; longRecordPairIndex < longRecordIndices.Count; longRecordPairIndex++)
    {
        int priorLongIndex = longRecordIndices[longRecordPairIndex - 1];
        int currentLongIndex = longRecordIndices[longRecordPairIndex];
        
        byte[] priorLongRecord = logList[priorLongIndex].Value;
        byte[] currentLongRecord = logList[currentLongIndex].Value;
        
        // Process all short records between these two long records
        for (int i = priorLongIndex + 1; i < currentLongIndex; i++)
        {
            if (logList[i].Value.Length == 41) // Short record
            {
                var updatedShortPacket = CalculateShortRecord(
                    logList[i].Value, 
                    priorLongRecord, 
                    currentLongRecord
                );
                
                // Update the record in the dictionary
                logs[logList[i].Key] = updatedShortPacket;
            }
        }
    }
    
    // Collect short record keys to remove:
    // 1. All short records before the 3rd long record
    // 2. All short records after the last long record
    
    int thirdLongIndex = longRecordIndices[2];
    int lastLongIndex = longRecordIndices[longRecordIndices.Count - 1];
    
    // Remove short records before the 3rd long record
    for (int i = 0; i < thirdLongIndex; i++)
    {
        if (logList[i].Value.Length == 41) // Only short records
        {
            shortKeysToRemove.Add(logList[i].Key);
        }
    }
    
    // Remove short records after the last long record
    for (int i = lastLongIndex + 1; i < logList.Count; i++)
    {
        if (logList[i].Value.Length == 41) // Only short records
        {
            shortKeysToRemove.Add(logList[i].Key);
        }
    }
    
    // Remove only the short record keys from the dictionary
    foreach (string key in shortKeysToRemove)
    {
        logs.Remove(key);
    }
}

// Placeholder function - replace with your actual implementation
private byte[] CalculateShortRecord(byte[] shortRecord, byte[] priorLongRecord, byte[] currentLongRecord)
{
    // Your implementation here
    // This is just a placeholder that returns the original short record
    return shortRecord;
}

*/
