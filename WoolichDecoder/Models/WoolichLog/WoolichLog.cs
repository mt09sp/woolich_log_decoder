using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Runtime.Remoting.Messaging;
using WoolichDecoder.Models.CsvModels;


namespace WoolichDecoder.Models.WoolichLog
{
    public partial class WoolichLog
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

        public int? MinThrottle { get; set; } = null;
        public int? MaxThrottle { get; set; } = null;
        


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
            var hh = packet[5].ToString("00"); // NOT PART OF CHKSUM
            var MM = packet[6].ToString("00");
            var ss = packet[7].ToString("00");
            var ms1 = packet[8];
            var ms2 = packet[9];
            var ms = ((ms1 << 8) + ms2).ToString("000");

            timeString += $"{hh}:{MM}:{ss}.{ms}";

            logData.Add(timeString, packet);

            byte checksum = 0x00;

            // checkedDataLength = 95 for yamaha 98 bit packet. 78 for kwaka 79 bit packet
            // Honda has variable length packets.
            checksum = (byte)(checksumCalculator(packet, checkedDataLength) & 0xFF);
            return checksum;

        }

        

        public void SetMinThrottle(int minThrottle)
        {
            MinThrottle = minThrottle;
        }
        public void SetMaxThrottle(int maxThrottle)
        {
            MaxThrottle = maxThrottle;
        }
        public void CorrectHondaKlinePacketFormat()
        {
            if (logData.First().Value != null && packetFormat == PacketFormat.HondaKline)
            {
                if (logData.First().Value[34] == 0)
                {
                    packetFormat = PacketFormat.HondaKlineCB;
                }
                else
                {
                    packetFormat = PacketFormat.HondaKlineVFR;
                }
            }

        }


        int checksumCalculator(byte[] data, int length)
        {
            int curr_crc = 0x0000;
            byte sum1 = (byte)curr_crc;
            byte sum2 = (byte)(curr_crc >> 8);

            int index;
            // CHKSUM starts from minutes. Hours is hours but isn't part of the chksum.
            for (index = 6; index < length; index = index + 1)
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



            string header = "time, ms, TPS(W), TPS, TPS Mul, TPS Err, RPM, ETV(W), ETV, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, Speed,";
            // "P23b?, P24, 25-26 eng temp, 27-28 inlet temp, clutch 33, 36-37 Rear, 38-39 Front, " +
            // "40 speedo, P42," + 
            // STATIC "P43, P44, P45, P46, P47, P48, P49," +
            // STATIC "P50, P51, P52, P53, P54, P55, P56, P57, P58, P59," +
            // STATIC "P60, P61, P62, P63," +
            // "64 (AFR1), TPS(W), TPS True, TPS Raw, 67-68 ETV, ETV raw, P69, P70, 73 (AFR3), 73(AFR2), 78 chk,";

            string headerWithNonStatic = "time, ms, TPS(W), TPS, TPS Mul, TPS Err, RPM, ETV(W), ETV, IAP, AFR, CoolantTemp, InletAirTemp, Gear, AtmPressure, ManPressure, RW Speed, FW Speed, Speed, Clutch," +
                "P42, P23, P69, P70, " +
            "P12, P13, P15, P19, P23, P25, P26, P27, P28, clutch 33, P36 RW1, P37 RW2, 38 FW1, 39 FW2, 40 speedo, P42," +
            "64 (AFR1), P65-P66, P67-68, P69, P70, 73(AFR2), 78 chk,";


            return headerWithNonStatic;

        }

        public static List<string> getCSV_Kawasaki(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns)
        {
            List<string> alerts = new List<string>();
            List<KawasakiCsvModel> kawasakiCsvModels = new List<KawasakiCsvModel>();

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {
                var packetValue = packet.Value;
                KawasakiCsvModel tmpKawasakiCsvModel = new KawasakiCsvModel();
                tmpKawasakiCsvModel.PopulateFromLongPacket(packet, null);

                if (tmpKawasakiCsvModel.alerts != null)
                {
                    alerts.AddRange(tmpKawasakiCsvModel.alerts);
                }

                kawasakiCsvModels.Add(tmpKawasakiCsvModel);
            }


            foreach (KawasakiCsvModel csvModel in kawasakiCsvModels)
            {


                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";
                outputString += $"{csvModel.WoolichTPS},"; // 
                outputString += $"{csvModel.ActualTPS},"; // 
                
                outputString += $"{csvModel.WoolichTPSError},";

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
            return alerts;
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


 


        public static List<string> getCSV_StartingPointExample(StreamWriter outputFile, SortedDictionary<string, byte[]> logs)
        {

            List<string> alerts = new List<string>();
            // KeyValuePair<string, byte[]> lastPacket = logs.First();

            var lengthCounts = logs.Values
                .GroupBy(byteArray => byteArray.Length)
                .OrderBy(group => group.Key)
                .ToDictionary(group => group.Key, group => group.Count());

            foreach (var group in lengthCounts)
            {
                Console.WriteLine($"Length: {group.Key}, Count: {group.Value}");
            }

            int maxCount = lengthCounts.Keys.Max();


            // Header
            string outputString = string.Empty;




            foreach (KeyValuePair<string, byte[]> packet in logs)
            {

                outputString = string.Empty;

                var packetValue = packet.Value;

                outputString += $"{packet.Key} t,"; // timestamp
                var milliseconds = packetValue.getMilliseconds();
                outputString += $"{milliseconds},";

                for (int i = 0; i < 10; i++)
                {
                    // outputString += ",";
                    outputString += $"{packetValue[i]},";
                }

                // Packet length
                outputString += $"{packet.Value.Length},";

                // output for xml packets. comment out when no longer needed. do not delete because this will be a template for other files.
                for (int i = 10; i < packet.Value.Length; i++)
                {
                    // outputString += ",";
                    outputString += $"{packetValue[i]},";
                }



                outputFile.WriteLine(outputString);
                outputFile.Flush();
            }

            return alerts;

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
