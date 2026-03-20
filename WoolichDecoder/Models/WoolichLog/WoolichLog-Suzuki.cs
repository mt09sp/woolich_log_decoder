using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WoolichDecoder.Models.CsvModels;

namespace WoolichDecoder.Models.WoolichLog
{
    public partial class WoolichLog
    {
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


        public static List<string> getCSV_Suzuki(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns)
        {
            List<string> alerts = new List<string>();

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
                    tmpSuzukiCsvModel.PopulateFromLongPacket(packet, null);

                    if (tmpSuzukiCsvModel.alerts != null)
                    {
                        alerts.AddRange(tmpSuzukiCsvModel.alerts);
                    }

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
            return alerts;
        }




    }
}
