using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
    public class KawasakiCsvModel : BaseCsvModel
    {
        // Initialize the packet format for Kawasaki
        public KawasakiCsvModel()
        {
            packetFormat = PacketFormat.Kawasaki;
        }

        // Kawasaki-specific properties
        public string P23 = string.Empty;
        public int P42;
        public int P69;
        public int P70;

        public override void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet)
        {
            // Call base method to populate common properties
            PopulateBaseFromLongPacket(packet);

            // Kawasaki-specific properties
            Clutch = (packet.Value[33] & 0b11111111) != 0;
            P42 = packet.Value[42];
            P23 = packet.Value[23].ToBitString(0b11111111);
            P69 = packet.Value[69];
            P70 = packet.Value[70];
        }
    }
}