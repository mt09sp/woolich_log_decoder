using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.WoolichLog
{
    public partial class WoolichLog
    {


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
            else if (packetFormat == PacketFormat.HondaOBD)
            {
                // OBD has no unknown packets to output


                if (!includeRaws)
                {
                    return getCSV_HondaHeader(PacketFormat.HondaOBD, includeRaws);
                }
                else
                {
                    return getCSV_HondaHeader(PacketFormat.HondaOBD, includeRaws);


                    /*
                    var lengthCounts = logData.Values
                        .GroupBy(byteArray => byteArray.Length)
                        .OrderBy(group => group.Key)
                        .ToDictionary(group => group.Key, group => group.Count());

                    foreach (var group in lengthCounts)
                    {
                        Console.WriteLine($"Length: {group.Key}, Count: {group.Value}");
                    }

                    int maxCount = lengthCounts.Keys.Max();

                    // string header = "time, ms, TPS, RPM, IAP, AFR, CoolantTemperature, InletAirTemperature, Gear, AtmosphericPressure, ManifoldPressure, IgnitionAdvance, SecondaryThrottlePlates, MS,Clutch, PAIR," +
                    // "RPM, 10-11, 12,13,14,15,16,17,18,19," +
                    // "20,21,22,23,24,25,26,27,28,29," +
                    // "30,31,32,33,34,35,36,37,38,39," +
                    // "40,41,42,43,44,45,46,47,48,49," +
                    // "50,51,52,53,54,55,56,57,58,59," +
                    // "60,61,62,63,64 AFR,65,66,67,68,69," +
                    // "70,71,72,73,74,75,76,77";

                    // Header
                    string header = string.Empty;
                    header += $"time, MS, ";
                    for (int i = 0; i < 5; i++)
                    {
                        header += $"P{i},";


                    }
                    for (int i = 5; i < 10; i++)
                    {
                        header += $"T{i},";


                    }

                    header += $"Length,";

                    for (int i = 10; i < maxCount; i++)
                    {
                        header += $"B{i},";
                    }

                    return header;
                    */
                }
            }
            else if (packetFormat == PacketFormat.HondaKlineCB || packetFormat == PacketFormat.HondaKlineVFR)
            {
                return getCSV_HondaHeader(packetFormat, includeRaws);

            }

            else
            {
                var lengthCounts = logData.Values
                    .GroupBy(byteArray => byteArray.Length)
                    .OrderBy(group => group.Key)
                    .ToDictionary(group => group.Key, group => group.Count());

                foreach (var group in lengthCounts)
                {
                    Console.WriteLine($"Length: {group.Key}, Count: {group.Value}");
                }

                int maxCount = lengthCounts.Keys.Max();


                // Header
                string header = string.Empty;
                header += $"time, MS, ";
                for (int i = 0; i < 5; i++)
                {
                    header += $"P{i},";


                }
                for (int i = 5; i < 10; i++)
                {
                    header += $"T{i},";


                }

                header += $"Length,";

                for (int i = 10; i < maxCount; i++)
                {
                    header += $"B{i},";


                }
                return header;

            }

        }

    }
}
