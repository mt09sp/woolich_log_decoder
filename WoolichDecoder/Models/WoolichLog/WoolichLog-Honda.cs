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

        public static string getCSV_HondaHeader(PacketFormat pf, bool includeRaws)
        {
            string header = string.Empty;
            if (pf == PacketFormat.HondaOBD)
            {
                if (!includeRaws)
                {

                    header = "time, ms, RPM, TPS, TPS(W), W TPS ERR, IAP, AFR, Speed, CoolantTemp, AtmPressure, Gear, InletAirTemp, ManPressure, IgnitionAdvance, PAIR, LoadPct";
                }
                else
                {
                    header = "time, ms, RPM, TPS, TPS RAW, TPS(W), TPS(W) Raw, W TPS ERR, IAP, AFR, Speed, CoolantTemp, AtmPressure, Gear, InletAirTemp, ManPressure, IgnitionAdvance, PAIR, LoadPct";
                }
            }

            else if (pf == PacketFormat.HondaKlineCB)
            {
                if (!includeRaws)
                {
                
                    header = "time, ms, RPM, TPS, TPS(W), W TPS ERR, TPS A, IAP, Fueling, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                }
                else
                {
                    header = "time, ms, RPM, TPS, TPS RAW, TPS(W), TPS(W) Raw, W TPS ERR, TPS A, IAP, Fueling, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                }
            }
            else if (pf == PacketFormat.HondaKlineVFR)
            {
                if (!includeRaws)
                {

                    // header = "time, ms, RPM, ETV, ETV(W), W ETV ERR, TPS, TPS(W), W TPS ERR, IAP, Fueling, AFR, Speed, CoolantTemp,   InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                    header = "time, ms, RPM, ETV, ETV(W), W ETV ERR, ETV A, TPS, TPS(W), W TPS ERR, TPS A, IAP, Fueling?, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                }
                else
                {
                    header = "time, ms, RPM, ETV, ETV RAW, ETV(W), ETV(W) Raw, W ETV ERR, ETV A, TPS, TPS RAW, TPS(W), TPS(W) Raw, W TPS ERR, TPS A, IAP, Fueling?, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                }
            }

            return header;
        }



        public static List<string> getCSV_HondaObd(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns, bool includeRaws)
        {
            // disabled
            // includeRaws = false;

            List<HondaObdCsvModel> hondaCsvModels = new List<HondaObdCsvModel>();
            HondaObdCsvModel PriorPacket = null;

            int? minRawThrottle = null;

            List<string> alerts = new List<string>();

            

            // skip is because first record is a protocol specification.
            foreach (KeyValuePair<string, byte[]> packet in logs.Skip(1))
            {
                var packetValue = packet.Value;
                HondaObdCsvModel tmpHondaCsvModel = new HondaObdCsvModel();

                // The main conversion
                tmpHondaCsvModel.PopulateFromSequentialPacket(packet, PriorPacket);

                alerts = alerts.Union(tmpHondaCsvModel.alerts).ToList();

                if ((tmpHondaCsvModel.minThrottle ?? 255) < (minRawThrottle ?? 255))
                {
                    minRawThrottle = tmpHondaCsvModel.minThrottle;
                }

                PriorPacket = tmpHondaCsvModel;
                hondaCsvModels.Add(tmpHondaCsvModel);
            }


            hondaCsvModels = hondaCsvModels.RefineSequentialDerivedPackets(minRawThrottle);



            // string header = "time, ms, RPM, TPS, TPS(W), IAP, AFR, Speed, CoolantTemp, AtmPressure, Gear, InletAirTemp, ManPressure, IgnitionAdvance, PAIR";

            foreach (HondaObdCsvModel csvModel in hondaCsvModels)
            {

                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";
                outputString += $"{csvModel.RPM},"; // 

                outputString += $"{csvModel.ActualTPS},"; // 
                if (includeRaws) // disabled above
                {
                    outputString += $"{csvModel.ActualTPSRaw},"; //
                }


                outputString += $"{csvModel.WoolichTPS},"; // 
                if (includeRaws)
                {
                    outputString += $"{csvModel.WoolichTPSRaw},"; //
                }

                outputString += $"{csvModel.WoolichTPSError},";

                outputString += $"{csvModel.IAP},"; // IAP
                outputString += $"{csvModel.AFR},"; // AFR

                outputString += $"{csvModel.Speedo},"; // 

                outputString += $"{csvModel.CoolantTemp},"; // CoolantTemp


                outputString += $"{csvModel.AtmPressure},"; // AtmPressure

                // Not mapped
                outputString += $"{csvModel.Gear},"; // Gear 
                // outputString += $"{csvModel.Clutch},"; // Clutch

                outputString += $"{csvModel.InletAirTemp},"; // InletAirTemp
                outputString += $"{csvModel.ManPressure},"; // ManPressure

                // outputString += $"{csvModel.InjectorDuration},"; // InjectorDuration
                outputString += $"{csvModel.Ignition},"; // Ignition
                outputString += $"{csvModel.Pair},"; // Pair
                outputString += $"{csvModel.LoadPct},"; // Pair

                outputFile.WriteLine(outputString);
                outputFile.Flush();

            }

            return alerts;
        }

        public static List<string> getCSV_HondaKline(StreamWriter outputFile, SortedDictionary<string, byte[]> logs, List<StaticPacketColumn> presumedStaticColumns, bool includeRaws, PacketFormat packetFormat)
        {

            List<string> alerts = new List<string>();
            int? minRawThrottle = null;

            List<HondaKlineCsvModel> hondaKlineCsvModels = new List<HondaKlineCsvModel>();

            foreach (KeyValuePair<string, byte[]> packet in logs)
            {
                var packetValue = packet.Value;
                HondaKlineCsvModel tmpHondaKlineCsvModel = new HondaKlineCsvModel(packetFormat);


                // The actual main conversion
                tmpHondaKlineCsvModel.PopulateFromLongPacket(packet, minRawThrottle);

                if (tmpHondaKlineCsvModel.alerts != null)
                {
                    alerts.AddRange(tmpHondaKlineCsvModel.alerts);
                }


                if ((tmpHondaKlineCsvModel.minThrottle ?? 255) < (minRawThrottle ?? 255))
                {
                    minRawThrottle = tmpHondaKlineCsvModel.minThrottle;
                }


                hondaKlineCsvModels.Add(tmpHondaKlineCsvModel);
            }

            // Why am I doing this??? Adjusting for min throttle.
            hondaKlineCsvModels = hondaKlineCsvModels.RefineSequentialDerivedPackets(minRawThrottle, packetFormat);


            foreach (HondaKlineCsvModel csvModel in hondaKlineCsvModels)
            {

                string outputString = string.Empty;
                outputString += $"{csvModel.LogTime},"; // timestamp
                outputString += $"{csvModel.milliseconds},";

                // ETV is a bit messed up for VFR


                // header CB  = "time, ms, RPM, TPS, TPS RAW, TPS(W), TPS(W) Raw, W TPS ERR, TPS A, IAP, Fueling, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";
                // header VFR = "time, ms, RPM, ETV, ETV RAW, ETV(W), ETV(W) Raw, W ETV ERR, ETV A, TPS, TPS RAW, TPS(W), TPS(W) Raw, W TPS ERR, TPS A, IAP, Fueling?, AFR, Speed, CoolantTemp, InletAirTemp, IgnitionAdvance, PAIR, LoadPct";


                outputString += $"{csvModel.RPM},"; // B10, B11

                if (packetFormat == PacketFormat.HondaKlineVFR)
                {
                    outputString += $"{csvModel.ActualEtv},"; // 
                    if (includeRaws)
                    {
                        outputString += $"{csvModel.ActualEtvRaw},"; // 
                    }

                    outputString += $"{csvModel.WoolichEtv},"; // 
                    if (includeRaws)
                    {
                        outputString += $"{csvModel.ActualEtvRaw},"; // 
                    }
                    outputString += $"{csvModel.WoolichEtvError},"; // 


                    outputString += $"{csvModel.EcuETV},"; // 
                }

                outputString += $"{csvModel.ActualTPS},"; // 
                if (includeRaws)
                {
                    outputString += $"{csvModel.ActualTPSRaw},"; //
                }

                outputString += $"{csvModel.WoolichTPS},"; // 
                if (includeRaws)
                {
                    outputString += $"{csvModel.WoolichTPSRaw},"; //
                }
                outputString += $"{csvModel.WoolichTPSError},"; // 

                outputString += $"{csvModel.ActualTPS2},"; //


                outputString += $"{csvModel.IAP},"; // IAP
                outputString += $"{csvModel.InjectorDuration},"; // Fueling



                outputString += $"{csvModel.AFR},"; // AFR

                outputString += $"{csvModel.Speedo},"; // 23

                outputString += $"{csvModel.CoolantTemp},"; // CoolantTemp

                // outputString += $"{csvModel.AtmPressure},"; // AtmPressure

                // outputString += $"{csvModel.Gear},"; // Gear 

                outputString += $"{csvModel.InletAirTemp},"; // InletAirTemp
                // outputString += $"{csvModel.InjectorDuration},"; // InjectorDuration
                outputString += $"{csvModel.Ignition},"; // Ignition

                outputString += $"{csvModel.Pair},"; // PAIR

                outputString += $"{csvModel.LoadPct},"; // LoadPct


                outputFile.WriteLine(outputString);
                outputFile.Flush();

            }

            return alerts;


        }


    }
}
