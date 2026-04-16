using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
    public class HondaObdCsvModel : BaseCsvModel
    {
        // Initialize the packet format for Kawasaki
        public HondaObdCsvModel()
        {
            packetFormat = PacketFormat.HondaOBD;
        }



        public void PopulateFromSequentialPacket(KeyValuePair<string, byte[]> packet, HondaObdCsvModel priorPacket)
        {


            alerts = new List<string>();

            // Initialize all values from prior packet (or defaults if no prior packet)
            if (priorPacket != null)
            {
                RPM = priorPacket.RPM;
                ActualTPS = priorPacket.ActualTPS;
                ActualTPSRaw = priorPacket.ActualTPSRaw;
                WoolichTPS = priorPacket.WoolichTPS;
                WoolichTPSRaw = priorPacket.WoolichTPSRaw;
                IAP = priorPacket.IAP;
                AFR = priorPacket.AFR;
                Speedo = priorPacket.Speedo;
                CoolantTemp = priorPacket.CoolantTemp;
                AtmPressure = priorPacket.AtmPressure;
                Gear = priorPacket.Gear;
                InletAirTemp = priorPacket.InletAirTemp;
                ManPressure = priorPacket.ManPressure;
                Ignition = priorPacket.Ignition;
                Pair = priorPacket.Pair;
                LoadPct = priorPacket.LoadPct;

                minThrottle = priorPacket.minThrottle;
            }
            else
            {
                // Default values if no prior packet
                RPM = 0;
                ActualTPS = 0.0;
                ActualTPSRaw = 99;
                WoolichTPS = 0.0;
                WoolichTPSRaw = 99;
                IAP = 99;
                AFR = 99;
                Speedo = 0;
                CoolantTemp = 0.0;
                AtmPressure = 0.0;
                Gear = 0;
                InletAirTemp = 0.0;
                ManPressure = 0.0;
                Ignition = 0.0;
                Pair = false;
                LoadPct = 0;

                minThrottle = null;

            }


            // Always update LogTime and milliseconds from current packet
            LogTime = packet.Key + " t";
            milliseconds = packet.Value.getMilliseconds();

            // Parse the packet for available PIDs starting from byte 10
            byte[] data = packet.Value;
            int i = 10; // Start from byte 10

            while (i < data.Length - 2) // Need at least Mode, PID, Length
            {
                byte mode = data[i];
                byte pid = data[i + 1];

                // Process Mode 32 (woolich-specific - contains AFR)
                if (mode == 32)
                {
                    // Check if we have enough bytes for the header
                    if (i + 2 < data.Length)
                    {
                        byte woolichCustomDataLength = data[i + 2];

                        // Check for expected format: PID 1 with 4 data bytes
                        if (pid == 1 && woolichCustomDataLength == 4 && i + 6 < data.Length)
                        {
                            // First byte of 4-byte data is AFR (147 = 14.7)
                            AFR = data[i + 3] / 10.0;

                            // Check if other bytes contain unexpected data
                            if (data[i + 4] != 0 || data[i + 5] != 0 || data[i + 6] != 0)
                            {
                                string hexData = $"Mode: 0x{mode:X2}, PID: 0x{pid:X2}, Length: {woolichCustomDataLength}, Data: {data[i + 3]:X2} {data[i + 4]:X2} {data[i + 5]:X2} {data[i + 6]:X2}";
                                alerts.Add($"Mode 32 unexpected non-zero bytes at {milliseconds}ms: {hexData}");
                            }
                        }
                        else
                        {
                            // Unexpected PID or length in Mode 32
                            string hexData = $"Mode: 0x{mode:X2}, PID: 0x{pid:X2}, Length: {woolichCustomDataLength}";
                            if (i + 3 + woolichCustomDataLength <= data.Length)
                            {
                                hexData += ", Data: ";
                                for (int j = 0; j < woolichCustomDataLength && j < 10; j++) // Limit to 10 bytes for safety
                                {
                                    hexData += $"{data[i + 3 + j]:X2}";
                                    if (j < woolichCustomDataLength - 1 && j < 9) hexData += " ";
                                }
                            }
                            alerts.Add($"Mode 32 unexpected format at {milliseconds}ms: {hexData}");
                        }
                    }
                    else
                    {
                        alerts.Add($"Mode 32 incomplete header at {milliseconds}ms: insufficient bytes");
                    }

                    i += 7; // Skip: mode(1) + pid(1) + length(1) + data(4)
                    continue;
                }

                // Only process Mode 1 (current data)
                if (mode != 1)
                {
                    i++;
                    continue;
                }

                byte length = data[i + 2];

                // Ensure we have enough data
                if (i + 3 + length > data.Length)
                    break;

                // Parse based on PID
                switch ((ObdPid)pid)
                {
                    case ObdPid.EngineRPM: // 0x0C
                        if (length >= 2)
                        {
                            // divided by 4 is correct.
                            // would prefer if this bit shifted
                            RPM = ((data[i + 3] << 8) + data[i + 4]) / 4;
                        }
                        break;

                    case ObdPid.ThrottlePosition: // 0x11
                        if (length >= 1)
                        {
                            double tpsRaw = data[i + 3];
                            ActualTPSRaw = tpsRaw;
                            // calculated in RefineSequentialDerivedPackets after the initial run.
                            // ActualTPS = Math.Round((tpsRaw - 25) / 2.05,2);
                            // WoolichTPS = Math.Round((tpsRaw - 30) / 1.7,3);
                            WoolichTPSRaw = (int)tpsRaw;
                        }
                        break;

                    case ObdPid.VehicleSpeed: // 0x0D
                        if (length >= 1)
                        {
                            Speedo = data[i + 3];
                        }
                        break;

                    case ObdPid.EngineCoolantTemp: // 0x05
                        if (length >= 1)
                        {
                            CoolantTemp = data[i + 3] - 40;
                        }
                        break;

                    case ObdPid.IntakeAirTemp: // 0x0F
                        if (length >= 1)
                        {
                            InletAirTemp = data[i + 3] - 40;
                        }
                        break;

                    case ObdPid.IntakeManifoldPressure: // 0x0B
                        // This is actually IAP
                        if (length >= 1)
                        {
                            double mapValue = data[i + 3];
                            ManPressure = mapValue;
                            IAP = mapValue; // IAP is MAP Well MAP is IAP

                            // AtmPressure is MAP when engine is off (no vacuum)
                            // For simplicity, always update it (can refine later based on RPM)
                            AtmPressure = 101.3;
                        }
                        break;

                    case ObdPid.TimingAdvance: // 0x0E
                        if (length >= 1)
                        {
                            Ignition = (data[i + 3] * 0.5) - 64;
                        }
                        break;

                    case ObdPid.EngineLoad: // 0x0E
                        if (length >= 1)
                        {
                            LoadPct = data[i + 3];
                        }
                        break;

                    // PAIR valve
                    case ObdPid.SecondaryAirStatus: // 0x12
                        if (length >= 1)
                        {
                            var pair = data[i + 3];
                            // 0x01 = UPS (upstream - PAIR active)
                            // 0x04 = OFF (PAIR not operating)
                            Pair = (data[i + 3] == 0x01);
                        }
                        goto default;

                    // Scanner shows these PIDs but we haven't seen them in data yet:
                    case ObdPid.FreezeDTC: // 0x02
                    case ObdPid.FuelSystemStatus: // 0x03
                    case ObdPid.ShortTermFuelTrim_Bank1: // 0x06
                    case ObdPid.DistanceWithMIL: // 0x21
                        {
                            // Build hex string for the data bytes
                            string hexData = $"Mode: 0x{mode:X2}, PID: 0x{pid:X2}, Length: {length}";
                            if (length > 0 && i + 3 + length <= data.Length)
                            {
                                hexData += ", Data: ";
                                for (int j = 0; j < length; j++)
                                {
                                    hexData += $"{data[i + 3 + j]:X2}";
                                    if (j < length - 1) hexData += " ";
                                }
                            }
                            alerts.Add($"New data packet encountered at {milliseconds}ms: {hexData}");
                        }
                        break;

                    case ObdPid.OBDStandard: // 0x1C
                        // What OBD protocol we support
                        // alerts.Add($"Supported PIDs: {pidList}");
                        break;

                    case ObdPid.SupportedPids_01_20: // 0x00
                    case ObdPid.SupportedPids_21_40: // 0x20
                    case ObdPid.SupportedPids_41_60: // 0x40
                        // What OBD codes we support
                        if (length >= 4)
                        {
                            // Read the 4-byte bitmap
                            uint bitmap = ((uint)data[i + 3] << 24) |
                                          ((uint)data[i + 4] << 16) |
                                          ((uint)data[i + 5] << 8) |
                                          (uint)data[i + 6];

                            // Determine the starting PID based on which range we're in
                            byte startPid = (byte)(pid + 1);

                            List<string> supportedPids = new List<string>();

                            // Check each bit (bits 0-31, where bit 0 is MSB)
                            for (int bit = 0; bit < 32; bit++)
                            {
                                // Check if bit is set (MSB first)
                                if ((bitmap & (1u << (31 - bit))) != 0)
                                {
                                    byte supportedPid = (byte)(startPid + bit);
                                    supportedPids.Add($"0x{supportedPid:X2}");
                                }
                            }

                            string pidList = string.Join(", ", supportedPids);
                            alerts.Add($"Supported PIDs: {pidList}");
                        }
                        break;

                    case ObdPid.UserSpecified_30:
                    case ObdPid.UserSpecified_4D:
                        // Honda outputting VIN and serial data
                        // alerts.Add($"Supported PIDs: {pidList}");
                        break;


                    default:
                        // Any other unknown PID
                        {
                            string hexData = $"Mode: 0x{mode:X2}, PID: 0x{pid:X2}, Length: {length}";
                            if (length > 0 && i + 3 + length <= data.Length)
                            {
                                hexData += ", Data: ";
                                for (int j = 0; j < length; j++)
                                {
                                    hexData += $"{data[i + 3 + j]:X2}";
                                    if (j < length - 1) hexData += " ";
                                }
                            }
                            alerts.Add($"Unknown PID encountered at {milliseconds}ms: {hexData}");
                        }
                        break;



                }

                // Move to next PID (mode + pid + length + data bytes)
                i += 3 + length;
            }


        }





        public override void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet, int? minThrottle)
        {
            // Call base method to populate common properties
            PopulateBaseFromLongPacket(packet, minThrottle);

        }
    }

    public static class HondaObdCsvModelExtensions
    {
        public static List<HondaObdCsvModel> RefineSequentialDerivedPackets(this List<HondaObdCsvModel> priorPacket, int? minThrottle)
        {
            foreach (var packet in priorPacket)
            {
                packet.ActualTPS = Math.Round((packet.ActualTPSRaw - ((minThrottle ?? 25) + 1)) / 2.04, 2);
                packet.WoolichTPS = Math.Round((packet.ActualTPSRaw - 30) / 1.7, 3);

                packet.WoolichTPSError = Math.Round(packet.WoolichTPS - packet.ActualTPS,2);

                if (packet.ActualTPS > 100) packet.ActualTPS = 100;
                if (packet.ActualTPS < 0) packet.ActualTPS = 0;
                if (packet.WoolichTPS > 100) packet.WoolichTPS = 100;
                if (packet.WoolichTPS < 0) packet.WoolichTPS = 0;

            }


            return priorPacket;
        }
    }

}