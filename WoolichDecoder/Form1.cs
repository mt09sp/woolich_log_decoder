using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using WoolichDecoder.Models;
using WoolichDecoder.Settings;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace WoolichDecoder
{
    public partial class WoolichFileDecoderForm : Form
    {

        WoolichLog logs = new WoolichLog();

        /** logs for change point detection of specific bytes */
        WoolichLog analysisLogs = new WoolichLog();

        UserSettings userSettings;

        string outputFileNameWithoutExtension = string.Empty;
        string outputFileNameWithExtension = string.Empty;

        string logFolder = string.Empty;

        bool keepSmallPackets = true;

        List<FilterOptions> autoTuneFilterOptions = new List<FilterOptions>()
        {
            new FilterOptions { id = 1, type = PacketFormat.Yamaha, option = "MT09 Gen3/Gen4 ETV correction" },
            new FilterOptions { id = 2, type = PacketFormat.Yamaha, option = "Remove Gear 2 logs" },
            new FilterOptions { id = 3, type = PacketFormat.Yamaha, option = "Exclude below 1200 rpm" },
            new FilterOptions { id = 4, type = PacketFormat.Yamaha, option = "Remove Gear 1, 2 & 3 engine braking" },
            new FilterOptions { id = 5, type = PacketFormat.Yamaha, option = "Remove non launch gear 1" },
            new FilterOptions { id = 6, type = PacketFormat.Suzuki, option = "Hyabusa TPS correction" },

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

        List<int> decodedColumns = new List<int>();

        // The first columns of each packet. 0, 1, 2 => 00, 01, 02 The 3 is the number of data items (93 or 253 so far) and 4 varies but is unknown
        List<int> knownPacketDefinitionColumns = new List<int> { 0, 1, 2, 3, 4 };

        // Used by the analysis.
        List<StaticPacketColumn> presumedStaticColumns = new List<StaticPacketColumn> { };

        List<int> analysisColumn = new List<int> { };

        // Constructor for form???
        public WoolichFileDecoderForm()
        {
            InitializeComponent();
            cmbExportType.SelectedIndex = 0;
            lblExportFilename.Text = "";

            cmbBikeType.Items.Clear();
            foreach (PacketFormat format in Enum.GetValues(typeof(PacketFormat)))
            {
                cmbBikeType.Items.Add(new KeyValuePair<int, string>((int)format, format.ToString()));
            }
            cmbBikeType.DisplayMember = "Value";
            cmbBikeType.ValueMember = "Key";
        }

        public void SetMT09_StaticColumns()
        {
            decodedColumns = new List<int> {
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


            presumedStaticColumns.Clear();
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

        public void SetZX10R_StaticColumns()
        {
            decodedColumns = new List<int> {
                5, 6, 7, 8, 9, // Time
                10, 11,
                12, // unknown TPS Multiplier
                13, // // MAP RAW
                15, // // ATM raw
                19, // gear
                23, // unknown
                24, // unknown
                25, 26, // CoolantTemperature
                27, 28, // InletAirTemperature
                33, // unknown
                36, 37, // unknown
                38, 39, // unknown
                40, // unknown
                42, // unknown
                64, // unknown
                65, 66, // TPS Main
                67, 68, // ETV
                69, // unknown
                70, // unknown
                73, // unknown
                78, // checksum
            };


            presumedStaticColumns.Clear();
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 14, StaticValue = 255, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 16, StaticValue = 255, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 17, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 18, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 20, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 21, StaticValue = 255, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 22, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 24, StaticValue = 0, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 29, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 30, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 31, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 32, StaticValue = 0, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 34, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 35, StaticValue = 0, File = string.Empty });


            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 41, StaticValue = 255, File = string.Empty });

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

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 71, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 72, StaticValue = 0, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 74, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 75, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 76, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 77, StaticValue = 0, File = string.Empty });
        }


        // Suzuki has long and short packets.
        public void SetSuzuki_StaticColumns()
        {
            decodedColumns = new List<int> {
                5, 6, 7, 8, 9, // Time
                10, 11, // RPM calculated
                12, // TPS
                // 13, // Unknown
                14, // MAP
                15, 16, // ATM Pressure
                // 17, // Unknown
                18, // Ignition Advance
                19, // Gear
                20, 21, // STP
                // 22, // Unknown
                // 23, // Unknown
                // 24, // Unknown
                25, 26, // Engine Temp
                27, 28, // Inlet Temperature
                // 29, // Unknown
                // 30, // Unknown
                // 31, // Unknown
                32, // Pair valve
                33, // Clutch. Binary flag
                // 34, // Unknown
                // 35, // Unknown

                43, // TPS1



                64, // AFR

                // 65,
                66, // TPS2
                67, // ETV

                77, // HAS A VARIATION
                78, // checksum

            };


            presumedStaticColumns.Clear();
            
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 13, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 17, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 22, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 23, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 24, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 29, StaticValue = 255, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 30, StaticValue = 255, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 31, StaticValue = 255, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 34, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 35, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 36, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 37, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 38, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 39, StaticValue = 0, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 40, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 41, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 42, StaticValue = 0, File = string.Empty });

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


            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 65, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 68, StaticValue = 0, File = string.Empty });


            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 69, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 70, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 71, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 72, StaticValue = 0, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 73, StaticValue = 255, File = string.Empty });

            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 74, StaticValue = 128, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 75, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 76, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 77, StaticValue = 128, File = string.Empty });
            // 
        }



        public void SetS1000RR_StaticColumns()
        {

            decodedColumns = new List<int>();

            presumedStaticColumns.Clear();


            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 150, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 151, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 152, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 153, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 154, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 155, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 156, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 157, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 158, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 159, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 160, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 161, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 162, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 163, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 164, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 165, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 166, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 167, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 168, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 169, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 170, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 171, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 172, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 173, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 174, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 175, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 176, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 177, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 178, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 179, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 180, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 181, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 182, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 183, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 184, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 185, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 186, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 187, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 188, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 189, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 190, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 191, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 192, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 193, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 194, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 195, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 196, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 197, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 198, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 199, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 200, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 201, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 202, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 203, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 204, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 205, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 206, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 207, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 208, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 209, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 210, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 211, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 212, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 213, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 214, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 215, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 216, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 217, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 218, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 219, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 220, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 221, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 222, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 223, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 224, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 225, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 226, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 227, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 228, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 229, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 230, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 231, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 232, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 233, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 234, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 235, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 236, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 237, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 238, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 239, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 240, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 241, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 242, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 243, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 244, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 245, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 246, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 247, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 248, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 249, StaticValue = 0, File = string.Empty });
            presumedStaticColumns.Add(new StaticPacketColumn() { Column = 250, StaticValue = 0, File = string.Empty });

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

            this.aTFCheckedListBox.Items.Clear();

        }



        // Open and decode the log file
        private void btnOpenFile_Click(object sender, EventArgs e)
        {

            openWRLFileDialog.Title = "Select WRL file to inspect";

            if (string.IsNullOrWhiteSpace(this.openWRLFileDialog.InitialDirectory))
            {

                this.openWRLFileDialog.InitialDirectory = this.logFolder ?? Directory.GetCurrentDirectory();
            }

            this.openWRLFileDialog.Multiselect = false;
            openWRLFileDialog.Filter = "Woolich Log File (*.wrl)|*.wrl|All files (*.*)|*.*";
            openWRLFileDialog.DefaultExt = "wrl";

            // show open file dialog
            if (openWRLFileDialog.ShowDialog() != DialogResult.OK)
            {
                return;
            };


            // Only grab the first file. I don't believe that I allow multi select
            string filename = openWRLFileDialog.FileNames.FirstOrDefault();

            // clear any existing data and initialise the generic logs object
            logs.ClearPackets();
            Array.Clear(logs.PrimaryHeaderData, 0, logs.PrimaryHeaderData.Length);
            Array.Clear(logs.SecondaryHeaderData, 0, logs.SecondaryHeaderData.Length);


            // Set up filenames for later???
            string path = string.Empty;
            string binOutputFileName = string.Empty;
            string inputFileName = string.Empty;
            if (File.Exists(filename))
            {
                this.logFolder = Path.GetDirectoryName(filename);
                this.openWRLFileDialog.InitialDirectory = this.logFolder;

                outputFileNameWithoutExtension = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename);
                binOutputFileName = Path.GetDirectoryName(filename) + "\\" + Path.GetFileNameWithoutExtension(filename) + ".bin";

                inputFileName = filename;
            }
            else
            {
                this.lblFileName.Text = "Error: File Not Found";
                return;
            }
            this.lblFileName.Text = inputFileName;

            // Lets get ready to READ!!!!!
            FileStream fileStream = new FileStream(inputFileName, FileMode.Open, FileAccess.Read);
            BinaryReader wrlLogReader = new BinaryReader(fileStream, Encoding.ASCII);

            // start reading the log file. There is at least one identifiable heaser that seems consistent for all bikes.
            // some bikes have a secondary header though. What's in it is unclear.
            logs.PrimaryHeaderData = wrlLogReader.ReadBytes(logs.PrimaryHeaderLength);

            // this is a hack? A hack that seems to work.
            // hayabusa has just the primary packet but second byte is 01. Yamaha, kawasaki and S1000RR are 02 for second byte of file and they have the second header.
            int headerCount = logs.PrimaryHeaderData[1];
            if (headerCount > 1)
            {
                logs.SecondaryHeaderData = wrlLogReader.ReadBytes(logs.SecondaryHeaderLength);
            }

            bool eof = false;

            // The first 10 bytes of the header appear to be unique for each bike. Try and identify what bike we're dealing with.
            PacketFormat pf = PacketTypeIdentification.IdentifyPacketType(logs.PrimaryHeaderData);

            if (pf != PacketFormat.Unknown)
            {
                // we have an exact match. Yamaha, Kawasaki, Suzuki, etc
                cmbBikeType.SelectedItem = new KeyValuePair<int, string>((int)pf, pf.ToString());
            }

            // Set the packet format in the logs object
            logs.packetFormat = pf;

            // Every row has a row packet prefix despite it being identical for every row.
            while (!eof)
            {

                byte[] packetPrefixBytes = wrlLogReader.ReadBytes(logs.PacketPrefixLength);
                if (packetPrefixBytes.Length < 5)
                {
                    eof = true;
                    continue;
                }

                // It's wierd that I have to do - 2 but it works... I hope.
                int calculatedRemainingPacketBytes = (int)packetPrefixBytes[3] - 2;

                byte[] packetBytes = wrlLogReader.ReadBytes(calculatedRemainingPacketBytes);
                if (packetBytes.Length < calculatedRemainingPacketBytes)
                {
                    eof = true;
                    continue;
                }

                int totalPacketLength = (int)packetPrefixBytes[3] + 3;


                logs.AddPacket(packetPrefixBytes.Concat(packetBytes).ToArray(), totalPacketLength);
            }

            aTFCheckedListBox.Items.Clear();

            // populate the filter options with the options relevant for the packet type
            this.aTFCheckedListBox.Items.AddRange(autoTuneFilterOptions.Where(opt => opt.type == pf).Select(opt => opt.option).ToArray());

            for (int i = 0; i < this.aTFCheckedListBox.Items.Count; i++)
            {
                aTFCheckedListBox.SetItemCheckState(i, CheckState.Checked);
            }


            lblExportPacketsCount.Text = $"{logs.GetPacketCount()}";
            this.txtLogging.AppendText($"Load complete. {logs.GetPacketCount()} packets found." + Environment.NewLine);

            // All finished. Time to close the files.
            wrlLogReader.Close();
            fileStream.Close();

        }


        /// <summary>
        /// This is intended to analyse a specific column and filter the changes down to just where that single field changes.
        /// It's basically redundant now but may serve a purpose in the future.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnAnalyse_Click(object sender, EventArgs e)
        {

            analysisLogs.packetFormat = logs.packetFormat;
            analysisLogs.PrimaryHeaderData = logs.PrimaryHeaderData;
            analysisLogs.SecondaryHeaderData = logs.SecondaryHeaderData;
            // clear prior analysis
            analysisLogs.ClearPackets();

            if (analysisLogs.packetFormat == PacketFormat.Yamaha)
            {
                SetMT09_StaticColumns();
            }
            else if (analysisLogs.packetFormat == PacketFormat.BMW)
            {
                SetS1000RR_StaticColumns();
                /*
                combinedCols = new List<int> { 12, 13,
                    14, 15, 16,
                    22, 23,
                    32, 33,
                    // nice pattern here.
                    42, 43, 46, 47,
                    52, 53, 56, 57,
                    62, 63, 66, 67,

                    75, 76,
                    83, 84,
                    85, 86 };
                */
            }
            else if (analysisLogs.packetFormat == PacketFormat.Suzuki)
            {
                SetSuzuki_StaticColumns();

            }
            else
            {
                this.presumedStaticColumns.Clear();
            }

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



            for (int packetColumn = 0; packetColumn < logs.PacketLength; packetColumn++)
            {
                if (decodedColumns.Contains(packetColumn))
                {
                    log($"skipping know packet item {packetColumn}");
                    continue;
                }

                if (knownPacketDefinitionColumns.Contains(packetColumn))
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
                            analysisLogs.AddPacket(packet.Value, logs.PacketLength);
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
                                analysisLogs.AddPacket(packet.Value, logs.PacketLength);
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

            if (analysisLogs.GetPacketCount() > 0)
            {
                lblExportPacketsCount.Text = $"{analysisLogs.GetPacketCount()}";
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
        /// This is for exporting to a WRL file for loading into the data viewer and includes the results of a column analysis.
        /// It requires a column to be analysed even though I check the export type.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnExportTargetColumn_Click(object sender, EventArgs e)
        {

            // Set export source data

            WoolichLog exportItem = null;
            // "Export Full File",
            // "Export Analysis Only"
            if (cmbExportType.SelectedIndex == 0 && logs.packetFormat != PacketFormat.Suzuki)
            {
                // "Export Full File",
                exportItem = logs;
                // Error condition.
                MessageBox.Show("Only export of analysis is supported at the moment.");
                return;
            }
            else if (cmbExportType.SelectedIndex == 1)
            {
                // "Export Analysis Only"
                exportItem = analysisLogs;
            }
            else
            {
                exportItem = logs;
            }

            // if an analysis has been done then txtBreakOnChange will have a value. That's currently our check
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
            else if (logs.packetFormat == PacketFormat.Suzuki)
            {

                var outputFileNameWithExtension = outputFileNameWithoutExtension + $"_SUZ_LONG.WRL";
                // Export long packets only
                try
                {
                    // Trial output to file...
                    BinaryWriter binWriter = new BinaryWriter(File.Open(outputFileNameWithExtension, FileMode.Create));
                    // push to disk
                    // binWriter.Flush();
                    binWriter.Write(exportItem.PrimaryHeaderData);
                    binWriter.Write(exportItem.SecondaryHeaderData);
                    foreach (var packet in exportItem.GetPackets())
                    {
                        if (packet.Value[3] == 0x26)
                        {
                            continue;
                        }
                        binWriter.Write(packet.Value);
                        binWriter.Flush();
                    }
                    binWriter.Close();

                    MessageBox.Show($"Export to {outputFileNameWithExtension} complete ");
                    log($"Export to {outputFileNameWithExtension} complete ");
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

            PacketFormat exportFormat = PacketFormat.Unknown;

            if (cmbBikeType.SelectedItem != null)
            {
                var selectedPair = (KeyValuePair<int, string>)cmbBikeType.SelectedItem;
                exportFormat = (PacketFormat)selectedPair.Key;
            }
            else
            {
                MessageBox.Show("Bike Type not selected");
                return;
            }

            WoolichLog exportItem = null;

            string csvFileName = outputFileNameWithoutExtension + $".csv";

            lblExportFilename.Text = csvFileName;
            bool includeRaws = false;

            // Set export source data
            // "Export Full File",
            // "Export Analysis Only"
            if (cmbExportType.SelectedIndex == 0)
            {
                // "Export Full File",
                exportItem = logs;
                
            }
            else if(cmbExportType.SelectedIndex == 1)
            {
                // "Export Analysis Only"
                exportItem = analysisLogs;
            }
            else
            {
                exportItem = logs;
                includeRaws = true;
                csvFileName = outputFileNameWithoutExtension + $"_r.csv";
            }

            int packetCount = exportItem.GetPacketCount();

            if (packetCount == 0) {
                MessageBox.Show("Nothing to export");
                return;
            }


            int count = 0;
            List<int> combinedCols = new List<int>();



            if (exportFormat == PacketFormat.Yamaha)
            {
                log($"Exporting for Yamaha");
                SetMT09_StaticColumns();
            }
            else if (exportFormat == PacketFormat.Kawasaki)
            {
                log($"Exporting for Kawasaki");
                SetZX10R_StaticColumns();
            }
            else if (exportFormat == PacketFormat.Suzuki)
            {
                log($"Exporting for Suzuki");
                // this.presumedStaticColumns.Clear();
                SetSuzuki_StaticColumns();
            }
            else if (exportFormat == PacketFormat.BMW)
            {
                log($"Exporting for BMW");

                SetS1000RR_StaticColumns();
                /*
                combinedCols = new List<int> { 12, 13,
                    14, 15, 16,
                    22, 23,
                    32, 33,
                    // nice pattern here.
                    42, 43, 46, 47,
                    52, 53, 56, 57,
                    62, 63, 66, 67,

                    75, 76,
                    83, 84,
                    85, 86 };
                */
            }
            else
            {
                log($"Unknown export type");
                this.presumedStaticColumns.Clear();
            }


            try
            {

                using (StreamWriter outputFile = new StreamWriter(csvFileName))
                {
                    string csvHeader = exportItem.GetHeader(this.presumedStaticColumns, combinedCols, includeRaws);
                    outputFile.WriteLine(csvHeader);

                    if (exportItem.packetFormat == PacketFormat.Suzuki)
                    {

                        // suzuki looks like it has a data rate issue. it has large and small packets. small packets only seem to have AFR 
                        // exportItem.BackfillShortPackets();

                        WoolichLog.getCSV_Suzuki(outputFile, exportItem.GetPackets(), this.presumedStaticColumns);
                    }
                    else if(exportItem.packetFormat == PacketFormat.Kawasaki)
                    {

                        // suzuki looks like it has a data rate issue. it has large and small packets. small packets only seem to have AFR 
                        // exportItem.BackfillShortPackets();

                        WoolichLog.getCSV_Kawasaki(outputFile, exportItem.GetPackets(), this.presumedStaticColumns);
                    }
                    else if (exportItem.packetFormat == PacketFormat.Yamaha)
                    {

                        // suzuki looks like it has a data rate issue. it has large and small packets. small packets only seem to have AFR 
                        // exportItem.BackfillShortPackets();

                        WoolichLog.getCSV_MT09(outputFile, exportItem.GetPackets(), this.presumedStaticColumns, includeRaws);
                    }
                    else
                    {

                        foreach (var packet in exportItem.GetPackets())
                        {

                            var exportLine = WoolichLog.getCSV(packet.Value, packet.Key, exportFormat, this.presumedStaticColumns, combinedCols, includeRaws);
                            outputFile.WriteLine(exportLine);
                            outputFile.Flush();
                            count++;

                            if (count > 100000 && exportItem.packetFormat == PacketFormat.Unknown)
                            {
                                // if the file format is unknown then limit the output to make excel easier to use.
                                // break;
                            }

                        }
                    }
                    outputFile.Close();
                }
                var result = MessageBox.Show(
                    $"CSV Export Complete\n\nFile saved to: {csvFileName}\n\nWould you like to open the folder?",
                    "Export Complete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);

                if (result == DialogResult.Yes)
                {
                    OpenFileLocation(csvFileName);
                }
                log($"CSV written to disk");
            }
            catch(Exception ex)
            {
                var error = ex.ToString();
                MessageBox.Show("File is open dummy");
            }

        }


        private void OpenFileLocation(string filePath)
        {
            try
            {
                // This opens Windows Explorer and selects the file
                Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Unable to open file location: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }


        private void btnAutoTuneExport_Click(object sender, EventArgs e)
        {
            WoolichLog exportItem = logs;

            if (exportItem.PacketLength != (int)PacketFormat.Yamaha)
            {
                MessageBox.Show("This bikes file cannot be adjusted by this software yet.");
                return;
            }

            string outputFileNameWithExtension = outputFileNameWithoutExtension + $"_AT.WRL";

            lblExportFilename.Text = outputFileNameWithExtension;

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
                    var outputGear = packet.Value.getGear(PacketFormat.Yamaha);


                    // { id = 2, type = PacketFormat.Yamaha, option = "Remove Gear 2 logs" },
                    if (outputGear == 2 && selectedFilterOptions.Contains(autoTuneFilterOptions.getListValue(2, PacketFormat.Yamaha)))
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

                    // { id = 5, type = PacketFormat.Yamaha, option = "Remove non launch gear 1" },
                    if (outputGear == 1 && (packet.Value.getRPM(PacketFormat.Yamaha) < 1000 || packet.Value.getRPM(PacketFormat.Yamaha) > 4500) && selectedFilterOptions.Contains(autoTuneFilterOptions.getListValue(5, PacketFormat.Yamaha)))
                    {
                        // We don't want first gear but we do want launch RPM ranges
                        // Exclude anything outside of the launch ranges.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }


                    // Get rid of anything below 1200 RPM
                    // { id = 3, type = PacketFormat.Yamaha, option = "Exclude below 1200 rpm" },
                    if (outputGear != 1 && packet.Value.getRPM(PacketFormat.Yamaha) <= 1200 && selectedFilterOptions.Contains(autoTuneFilterOptions.getListValue(3, PacketFormat.Yamaha)))
                    {
                        // We aren't interested in below idle changes.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }

                    // This one is tricky due to wooliches error in decoding the etv packet.
                    // { id = 4, type = PacketFormat.Yamaha, option = "Remove Gear 1, 2 & 3 engine braking" },
                    if (packet.Value.getCorrectETV(PacketFormat.Yamaha) <= 1.2 && outputGear < 4 && selectedFilterOptions.Contains(autoTuneFilterOptions.getListValue(4, PacketFormat.Yamaha)))
                    {
                        // We aren't interested closed throttle engine braking.

                        byte newOutputGearByte = (byte)(exportPackets[24] & (~0b00000111));
                        diff = diff + newOutputGearByte - outputGear;
                        outputGear = newOutputGearByte;
                        exportPackets[24] = newOutputGearByte;

                        // continue;
                    }

                    // { id = 1, type = PacketFormat.Yamaha, option = "MT09 ETV correction" },
                    // Caution: This value is TPS calibration dependant.
                    if (selectedFilterOptions.Contains(autoTuneFilterOptions.getListValue(1, PacketFormat.Yamaha)))
                    {

                        // adjust the etv packet to make woolich put it in the right place.
                        // We know what the correct value should be so get that first.
                        double correctETV = exportPackets.getCorrectETV(PacketFormat.Yamaha);
                        // Then use the wollich formula (reversed) to create the binary value. 
                        byte hackedETVbyte = (byte)((correctETV * 1.66) + 38);
                        diff = diff + hackedETVbyte - exportPackets[18];
                        exportPackets[18] = hackedETVbyte;
                    }
                    exportPackets[95] += (byte)diff;

                    binWriter.Write(exportPackets);
                }
                binWriter.Close();
                


                MessageBox.Show("Autotune Filtered Export Complete", "Export Complete");
                log($"Autotune Filtered Export Complete");
            }
            catch
            {
                log($"Autotune log write error");

            }
        }

        // Utility function for testing the checksum. Not used anymore.
        private void btnExportCRCHack_Click(object sender, EventArgs e)
        {
            try
            {
                int size = 1000;
                WoolichLog exportItem = logs;
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


    }
}
