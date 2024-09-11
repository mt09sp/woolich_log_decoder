using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Eventing.Reader;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using WoolichDecoder.Models;
using WoolichDecoder.Settings;
using static System.Windows.Forms.LinkLabel;

namespace WoolichDecoder
{
    public partial class WoolichFileDecoderForm : Form
    {
        public static class LogPrefix
        {
            // Date and Time format
            private static readonly string DateTimeFormat = "yyyy-MM-dd HH:mm:ss";

            // Generate prefix with date, time and string
            public static string Prefix => $"{DateTime.Now.ToString(DateTimeFormat)} -- ";
        }

        string OpenFileName = string.Empty;

        WoolichMT09Log logs = new WoolichMT09Log();

        WoolichMT09Log exportLogs = new WoolichMT09Log();

        UserSettings userSettings;

        string outputFileNameWithoutExtension = string.Empty;
        string outputFileNameWithExtension = string.Empty;

        string logFolder = string.Empty;


        string[] autoTuneFilterOptions =
        {
            "ETV correction for MT-09",
            "Filter Out Gear 2",
            "Filter Out Idle RPM",
            "Filter Out Engine Braking in Gears 1-3",
            "Filter Out RPM range in Gear 1"
        };




        // hours: 5
        // min: 6
        // seconds: 7
        // miliseconds: 8, 9 raw
        // rpm: 10, 11 raw
        // iap: 21 (x2.0156)
        // atm pressure: 23 (x2.0156)
        // battery: 41 (x13.65)
        // engine temp: 26 (-30)
        // inlet temp: 27 (-30)
        // injector duration: 28 * 0.5 (aka half)
        // ignition btdc?: 29 (-30)

        List<int> decodedColumns = new List<int> {
            5, 6, 7, 8, 9, // Time
            10, 11, // RPM
            12, 13, // true TPS 
            // 14, unknown
            // 15, woolich tps
            // 16, 17 unknown 
            // 18 etv
            // 19 ?
            // 20 ?
            21, // iap pressure
            23, // atm pressures
            41, // battery 
            31, 32, 33, 34, 35, 36, // speeds
            // 37, 38, // combined but havent worked out what or how yet. Goes high even when just idling.
            26, 27, // temperatures
            28, // injector duration
            29, // ignition 
        };

        List<int> knownStaticColumns = new List<int> { 0, 1, 2, 3, 4 };

        List<StaticPacketColumn> presumedStaticColumns = new List<StaticPacketColumn> { };

        List<int> analysisColumn = new List<int> { };

        public WoolichFileDecoderForm()
        {
            InitializeComponent();
            cmbExportType.SelectedIndex = 0;

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 20, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 22, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 39, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 40, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 43, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 44, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 45, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 46, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 47, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 48, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 49, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 50, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 51, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 52, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 53, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 54, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 55, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 56, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 57, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 58, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 59, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 60, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 61, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 62, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 63, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 64, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 65, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 66, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 67, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 68, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 69, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 70, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 71, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 72, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 73, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 74, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 75, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 76, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 77, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 78, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 79, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 80, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 81, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 82, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 83, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 84, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 85, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 86, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 87, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 88, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 89, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 90, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 91, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 92, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 93, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 94, StaticValue = 0, File = string.Empty });

        }
        private bool IsFileLoaded()
        {
            if (string.IsNullOrEmpty(OpenFileName))
            {
                MessageBox.Show("Please open a file first.");
                return false;
            }
            return true;
        }
        private void dataGridView1_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void WoolichFileDecoder_Load(object sender, EventArgs e)
        {
            userSettings = new UserSettings();


            string logFileLocation = userSettings.LogDirectory;
            //Since there is no default value for FormText.
            if (logFileLocation != null)
            {
                this.logFolder = logFileLocation;
            }
            else
            {
                this.logFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            }

            this.aTFCheckedListBox.Items.AddRange(autoTuneFilterOptions.ToArray());


            for (int i = 0; i < this.aTFCheckedListBox.Items.Count; i++)
            {
                aTFCheckedListBox.SetItemCheckState(i, CheckState.Checked);
            }

        }

        private void btnOpenFile_Click(object sender, EventArgs e)
        {
 
            openWRLFileDialog.Title = "Select WRL file to inspect";

            if (string.IsNullOrWhiteSpace(this.openWRLFileDialog.InitialDirectory))
            {

                this.openWRLFileDialog.InitialDirectory = this.logFolder ?? Directory.GetCurrentDirectory();
            }

            this.openWRLFileDialog.Multiselect = false;
            this.openWRLFileDialog.Filter = "WRL files (*.wrl)|*.wrl|BIN files (*.bin)|*.bin|All files (*.*)|*.*";

            if (openWRLFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            };

            var filename = openWRLFileDialog.FileNames.FirstOrDefault();
            OpenFileName = filename;
            // clear any existing data
            logs.ClearPackets();
            Array.Clear(logs.PrimaryHeaderData, 0, logs.PrimaryHeaderData.Length);
            Array.Clear(logs.SecondaryHeaderData, 0, logs.SecondaryHeaderData.Length);




            string path = string.Empty;
            string binOutputFileName = string.Empty;
            string inputFileName = string.Empty;
            if (File.Exists(filename))
            {
                this.logFolder = Path.GetDirectoryName(filename);
                this.openWRLFileDialog.InitialDirectory = this.logFolder;

                outputFileNameWithoutExtension = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);
                binOutputFileName = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".bin";
                // outputFileName = Path.GetFileNameWithoutExtension(filename) + ".sql";
                inputFileName = filename;

            }
            else
            {
                this.lblFileName.Text = "Error: File Not Found";
                return;
            }
            this.lblFileName.Text = inputFileName;

            FileStream fileStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
            BinaryReader binReader = new BinaryReader(fileStream, Encoding.ASCII);

            logs.PrimaryHeaderData = binReader.ReadBytes(logs.PrimaryHeaderLength);
            logs.SecondaryHeaderData = binReader.ReadBytes(logs.SecondaryHeaderLength);

            exportLogs.PrimaryHeaderData = logs.PrimaryHeaderData;
            exportLogs.SecondaryHeaderData = logs.SecondaryHeaderData;

            bool eof = false;

            while (!eof)
            {
                byte[] packetBytes = binReader.ReadBytes(logs.PacketLength);
                if (packetBytes.Length < 96)
                {
                    eof = true;
                    continue;
                }
                logs.AddPacket(packetBytes);

            }

            this.txtLogging.AppendText($"{LogPrefix.Prefix}Data Loaded and {logs.GetPacketCount()} packets found." + Environment.NewLine);
            
            // byte[] headerBytes = binReader.ReadBytes((int)fileStream.Length);
            // byte[] fileBytes = System.IO.File.ReadAllBytes(fileNameWithPath_); // this also works

            binReader.Close();
            fileStream.Close();

            // Trial output to file...
            fileStream = File.Open(binOutputFileName, FileMode.Create);
            BinaryWriter binWriter = new BinaryWriter(fileStream);

            // push to disk
            binWriter.Flush();
            foreach (var packet in logs.GetPackets())
            {
                // byte[] outPacket = new byte[48];
                // Array.Copy(packet, outPacket, 48);

                binWriter.Write(packet.Value);
                // binWriter.Write(outPacket);
            }
            // binWriter.Write(fileBytes); // just feed it the contents verbatim
            binWriter.Flush();
            fileStream.Flush();
            binWriter.Close();

            fileStream.Close();
            this.txtLogging.AppendText($"{LogPrefix.Prefix}BIN file created and saved." + Environment.NewLine);
            
        }


        /// <summary>
        /// This is intended to analyse a specific column and filter the changes down to just where that single field changes.
        /// It's basically redundant now but may serve a purpose in the future.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAnalyse_Click(object sender, EventArgs e)
        {
            if (!IsFileLoaded())
                return;

            if (!string.IsNullOrWhiteSpace(txtBreakOnChange.Text))
            {
                try
                {
                    analysisColumn.Add(int.Parse(txtBreakOnChange.Text));
                }
                catch
                {
                }
            }

            if (logs.GetPacketCount() == 0)
            {
                MessageBox.Show("File not open");
            }

            clearLog();
            txtFeedback.Clear();
            lblExportPacketsCount.Text = string.Empty;

            exportLogs.ClearPackets();

            for (int packetColumn = 0; packetColumn < logs.PacketLength; packetColumn++)
            {
                if (decodedColumns.Contains(packetColumn))
                {
                    log($"skipping know packet item {packetColumn}");
                    continue;
                }

                if (knownStaticColumns.Contains(packetColumn))
                {
                    log($"skipping static packet item {packetColumn}");
                    continue;
                }


                int? max = 0;
                int? min = 0;
                // log($"processing packet {(packetColumn + 1)}");

                bool first = true;

                byte[] firstPacket = { };

                KeyValuePair<string, byte[]> previousPacket = new KeyValuePair<string, byte[]> { };

                foreach (KeyValuePair<string, byte[]> packet in logs.GetPackets())
                {

                    if (first)
                    {
                        if (analysisColumn.Contains(packetColumn))
                        {
                            exportLogs.AddPacket(packet.Value);
                        }
                        max = min = packet.Value[packetColumn];
                        first = false;
                    }
                    else
                    {

                        if (previousPacket.Value[packetColumn] != packet.Value[packetColumn])
                        {
                            // Our column changed.
                            if (analysisColumn.Contains(packetColumn))
                            {
                                exportLogs.AddPacket(packet.Value);
                            }
                        }

                        // These range values are for our benefit and not related to the modified WRL file.
                        if (max < packet.Value[packetColumn])
                        {
                            max = packet.Value[packetColumn];
                        }

                        if (min > packet.Value[packetColumn])
                        {
                            min = packet.Value[packetColumn];
                        }
                    }

                    // set the current packet as previous so we have it on the next loop
                    previousPacket = packet;
                }

                var presumedStaticColumn = presumedStaticColumns.Where(ps => ps.Column == packetColumn).FirstOrDefault();
                // We don't think this is static
                if (presumedStaticColumn == null)
                {
                    if (max == min)
                    {
                        log($"packet {(packetColumn)} has no variations");
                        txtFeedback.AppendText($"presumedStaticColumns.Add(new StaticPacketColumn() {{ Column = {packetColumn}, StaticValue = {min}, File = string.Empty }});" + Environment.NewLine);

                    }
                    else
                    {
                        log($"packet {(packetColumn)} varies from {min} to {max}");
                    }

                }
                else
                {
                    if (max == min)
                    {
                        if (max != presumedStaticColumn.StaticValue)
                        {
                            txtFeedback.AppendText($"presumed Static Column {packetColumn} contains an unexpected variation from {presumedStaticColumn.StaticValue} != {max});" + Environment.NewLine);
                        }
                        else
                        {
                            // It's static. We don't need to inform us of this.
                            // log($"packet {(packetColumn)} has no variations");
                        }
                    }
                    else
                    {

                        if (max != presumedStaticColumn.StaticValue)
                        {
                            txtFeedback.AppendText($"presumed Static Column {packetColumn} contains an unexpected variation from {presumedStaticColumn.StaticValue});" + Environment.NewLine);
                        }
                        log($"packet {(packetColumn)} varies from {min} to {max}");

                    }
                }
            }

            if (exportLogs.GetPacketCount() > 0)
            {
                lblExportPacketsCount.Text = $"{exportLogs.GetPacketCount()}";
                outputFileNameWithExtension = outputFileNameWithoutExtension + $"_C{txtBreakOnChange.Text.Trim()}.WRL";
                lblExportFilename.Text = outputFileNameWithExtension;
            }
            else
            {
                lblExportPacketsCount.Text = "No packets to export.";
                lblExportFilename.Text = "Export filename not defined";
            }
        }


        void log(string logData)
        {
            txtLogging.AppendText(logData + Environment.NewLine);
        }

        void clearLog()
        {
            txtLogging.Clear();
        }

        /// <summary>
        /// This is for exporting to a WRL file, the results of a column analysis. It requires a column to be analysed even though I check the export type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportTargetColumn_Click(object sender, EventArgs e)
        {
            if (!IsFileLoaded())
                return;

            WoolichMT09Log exportItem = null;
            // "Export Full File",
            // "Export Analysis Only"
            if (cmbExportType.SelectedIndex == 0)
            {
                // "Export Full File",
                exportItem = logs;
                // Error condition.
                MessageBox.Show("Only export of analysis is supported at the moment.");
            }
            else
            {
                // "Export Analysis Only"
                exportItem = exportLogs;
            }

            if (!string.IsNullOrWhiteSpace(txtBreakOnChange.Text))
            {
                try
                {
                    var columnToExport = int.Parse(txtBreakOnChange.Text.Trim());

                    // Trial output to file...
                    BinaryWriter binWriter = new BinaryWriter(File.Open(outputFileNameWithExtension, FileMode.Create));
                    // push to disk
                    // binWriter.Flush();
                    binWriter.Write(exportItem.PrimaryHeaderData);
                    binWriter.Write(exportItem.SecondaryHeaderData);
                    foreach (var packet in exportItem.GetPackets())
                    {
                        binWriter.Write(packet.Value);
                    }
                    binWriter.Close();

                }
                catch
                {
                }
            }

        }

        private void cmbExportType_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void btnExportCSV_Click(object sender, EventArgs e)
        {
            if (!IsFileLoaded())
                return;

            WoolichMT09Log exportItem = null;

            var csvFileName = outputFileNameWithoutExtension + $".csv";

            // "Export Full File",
            // "Export Analysis Only"
            if (cmbExportType.SelectedIndex == 0)
            {
                // "Export Full File",
                exportItem = logs;
            }
            else
            {
                // "Export Analysis Only"
                exportItem = exportLogs;
            }

            /*

            var csvFile = File.Open(csvFileName, FileMode.Create);

            string exportLine = string.Empty;
            foreach (var packet in exportItem.logData)
            {
                exportLine = getCSV(packet, true);
                csvFile.Write

            }

            // */
            try
            {
                using (StreamWriter outputFile = new StreamWriter(csvFileName))
                {
                    string csvHeader = "time, RPM, True TPS, 14, TPS (W), ETV (W), etv raw, ETV (Correct), IAP,AFR,Speedo, Engine temp, 16-17, 19, 20, 22, ATM?, 24(pt), gear, clutch?, 25 pt2, throttle off, 25 pt4, Inlet temp, injector dur, ignition, 30,milliseconds,Front Wheel,Rear Wheel,37-38,39,40,Battery,43,44,45,46,47,48";
                    outputFile.WriteLine(csvHeader);


                    foreach (var packet in exportItem.GetPackets())
                    {

                        var exportLine = getCSV(packet.Value, packet.Key, true);
                        outputFile.WriteLine(exportLine);
                        outputFile.Flush();

                    }
                    outputFile.Close();
                }
                log($"{LogPrefix.Prefix}File in CSV format saved");

            }
            catch
            {
                MessageBox.Show("File is open dummy");
            }

        }

        public static string getCSV(byte[] packet, string timeStamp, bool convert)
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



        private void btnAutoTuneExport_Click(object sender, EventArgs e)
        {
            if (!IsFileLoaded())
                return;

            WoolichMT09Log exportItem = logs;
            var outputFileNameWithExtension = outputFileNameWithoutExtension + $"_AT.WRL";
            try
            {

                List<string> selectedFilterOptions = new List<string>();

                var selectedCount = this.aTFCheckedListBox.CheckedItems.Count;

                for (int i = 0; i < this.aTFCheckedListBox.CheckedItems.Count; i++)
                {
                    selectedFilterOptions.Add(this.aTFCheckedListBox.CheckedItems[i].ToString());
                }

                // Trial output to file...
                BinaryWriter binWriter = new BinaryWriter(File.Open(outputFileNameWithExtension, FileMode.Create));
                // push to disk
                // binWriter.Flush();
                binWriter.Write(exportItem.PrimaryHeaderData);
                binWriter.Write(exportItem.SecondaryHeaderData);
                foreach (var packet in exportItem.GetPackets())
                {

                    // break the reference
                    byte[] exportPackets = packet.Value.ToArray();

                    int diff = 0;
                    // I'm going to change the approach to this... Rather than eliminate the record i'm going to make it one that woolich will filter out.
                    // The reason for this is that we may drop an AFR record for a prior record.
                    // Options are:
                    // Temperature
                    // gear
                    // clutch
                    // 0 RPM
                    // I'm choosing gear first. Lets make it 0
                    var outputGear = packet.Value.getGear();

                    // {
                    // 0 "MT09 ETV correction",
                    // 1 "Remove Gear 2 logs",
                    // 2 "Exclude below 1200 rpm",
                    // 3 "Remove Gear 1, 2 & 3 engine braking",
                    // 4 "Remove non launch gear 1"
                    // };



                    // "Remove Gear 2 logs"
                    if (outputGear == 2 && selectedFilterOptions.Contains(autoTuneFilterOptions[1]))
                    {
                        // 2nd gear is just for slow speed tight corners.
                        // 0 gear is neutral and is supposed to be filterable in autotune.

                        // adjust the gear packet to make woolich autotune filter it out.
                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }


                    // 4 "Remove non launch gear 1 - customizable"

                    int minRPM = int.Parse(textBox2.Text);  // Read and convert textBox2
                    int maxRPM = int.Parse(textBox3.Text);  // Read and convert textBox3

                    if (outputGear == 1 && (packet.Value.getRPM() < minRPM || packet.Value.getRPM() > maxRPM) && selectedFilterOptions.Contains(autoTuneFilterOptions[4]))

                    // 4 "Remove non launch gear 1"
                    //if (outputGear == 1 && (packet.Value.getRPM() < 1000 || packet.Value.getRPM() > 4500) && selectedFilterOptions.Contains(autoTuneFilterOptions[4]))
                    {
                        // We don't want first gear but we do want launch RPM ranges
                        // Exclude anything outside of the launch ranges.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;



                        // continue;
                    }


                    // Get rid of any RPM below defined by textBox1

                    int rpmLimit = int.Parse(textBox1.Text);

                    if (outputGear != 1 && packet.Value.getRPM() <= rpmLimit && selectedFilterOptions.Contains(autoTuneFilterOptions[2]))


                    // Get rid of anything below 1200 RPM
                    // 2 "Exclude below 1200 rpm"
                    // if (outputGear != 1 && packet.Value.getRPM() <= 1200 && selectedFilterOptions.Contains(autoTuneFilterOptions[2]))
                    {
                        // We aren't interested in below idle changes.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }

                    // This one is tricky due to wooliches error in decoding the etv packet.
                    // 3 "Remove Gear 1, 2 & 3 engine braking",
                    if (packet.Value.getCorrectETV() <= 1.2 && outputGear < 4 && selectedFilterOptions.Contains(autoTuneFilterOptions[3]))
                    {
                        // We aren't interested closed throttle engine braking.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }


                    if (selectedFilterOptions.Contains(autoTuneFilterOptions[0]))
                    {
                        // adjust the etv packet to make woolich put it in the right place.
                        double correctETV = exportPackets.getCorrectETV();
                        byte hackedETVbyte = (byte)((correctETV * 1.66) + 38);
                        diff = diff + hackedETVbyte - exportPackets[18];
                        exportPackets[18] = hackedETVbyte;
                    }
                    exportPackets[95] += (byte)diff;

                    binWriter.Write(exportPackets);
                }
                binWriter.Close();
                log($"{LogPrefix.Prefix}Autotune WRL File saved");
            }
            catch
            {
                log($"{LogPrefix.Prefix}Autotune WRL File saving error");

            }
        }

        // Utility function for testing the checksum. Not used anymore.
        private void btnExportCRCHack_Click(object sender, EventArgs e)
        {
            try
            {
                int size = 1000;
                WoolichMT09Log exportItem = logs;
                var outputFileNameWithExtension = outputFileNameWithoutExtension + $"_CRC.{size}.WRL";

                // Trial output to file...
                BinaryWriter binWriter = new BinaryWriter(File.Open(outputFileNameWithExtension, FileMode.Create));
                // push to disk
                // binWriter.Flush();
                binWriter.Write(exportItem.PrimaryHeaderData);
                binWriter.Write(exportItem.SecondaryHeaderData);

                var packets = exportItem.GetPackets().Take(size);

                foreach (var packet in packets)
                {
                    binWriter.Write(packet.Value);
                }


                binWriter.Close();

            }
            catch
            {
            }
            log($"CRC written?");
        }

        private void WoolichFileDecoderForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            userSettings.LogDirectory = this.logFolder;

            // save the user settings.
            userSettings.Save();
        }

        private void aTFCheckedListBox_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void txtBreakOnChange_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnAnalyse_Click_1(object sender, EventArgs e)
        {

        }

        private void lblFileName_Click(object sender, EventArgs e)
        {

        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void lblExportFilename_Click(object sender, EventArgs e)
        {

        }

    }
}
