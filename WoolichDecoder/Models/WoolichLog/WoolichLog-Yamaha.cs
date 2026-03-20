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

        public static List<string> getCSV_MT09(StreamWriter outputFile, WoolichLog fulLogs, List<StaticPacketColumn> presumedStaticColumns, bool includeRaws)
        {
            List<string> alerts = new List<string>();

            List<YamahaCsvModel> yamahaCsvModel = new List<YamahaCsvModel>();

            SortedDictionary<string, byte[]> logs = fulLogs.GetPackets();


            foreach (KeyValuePair<string, byte[]> packet in logs)
            {
                var packetValue = packet.Value;
                YamahaCsvModel tmpYamahaCsvModel = new YamahaCsvModel();
                tmpYamahaCsvModel.PopulateFromLongPacket(packet, fulLogs.MinThrottle);

                if (tmpYamahaCsvModel.alerts != null)
                {
                    alerts.AddRange(tmpYamahaCsvModel.alerts);
                }

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

            return alerts;

        }




    }
}
