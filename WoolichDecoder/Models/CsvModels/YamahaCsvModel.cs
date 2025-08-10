using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
    public class YamahaCsvModel : BaseCsvModel
    {
        // Initialize the packet format for Yamaha
        public YamahaCsvModel()
        {
            packetFormat = PacketFormat.Yamaha;
        }

        // Yamaha-specific properties

        public bool ThrottleOff;
        public string P23 = string.Empty;
        public int P24Pt1;
        public int P25Pt2;
        public string P25Pt4 = string.Empty;
        public int P20;
        public int P22;
        public int P37P38;
        public int P39;
        public int P40;

        public int P43;
        public int P44;
        public int P45;
        public int P46;
        public int P47;
        public int P48;

        public override void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet)
        {
            // Call base method to populate common properties
            PopulateBaseFromLongPacket(packet);

            // Yamaha-specific properties

            //Pair = (packet.Value[32] & 0b00000001) != 0;
            Clutch = (packet.Value[25] & 0b10000000) != 0; // bit1 
            ThrottleOff = (packet.Value[25] & 0b00100000) != 0; // bit3 

            // ---
            // Bike specific P-prefixed properties
            // ---
            P20 = packet.Value[20];
            P22 = packet.Value[22];
            // the first part of the gear byte 24
            P24Pt1 = packet.Value[24] & 0b11111000;

            // 25 is a whole lot of switching
            // P25 pt1 might be clutch switch
            P25Pt2 = (packet.Value[25] & 0b01000000) >> 6; // bit2
            // P25 pt3 is throttle on/off
            P25Pt4 = packet.Value[25].ToBitString(0b00011111); // bits 4, 5, 6, 7, 8

            P37P38 = packet.Value.getRaw(37, 38);
            P39 = packet.Value[39];
            P40 = packet.Value[40];
            P43 = packet.Value[43];
            P44 = packet.Value[44];
            P45 = packet.Value[45];
            P46 = packet.Value[46];
            P47 = packet.Value[47];
            P48 = packet.Value[48];
        }
    }
}