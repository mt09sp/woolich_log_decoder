using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.WoolichLog
{
    public partial class WoolichLog
    {

        public static string getCSV_S1000RR(byte[] packet, string timeStamp, List<StaticPacketColumn> staticPacketColumns, List<int> combinedCols)
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

            outputString += $"{timeStamp} t,";
            outputString += $"{milliseconds},";
            // "time, 12-13, 14-15-16, 22-23, 32-33, 42-43, 46-47, 52-53, 56-57, 62-63, 66-67, 75-76, 82-83, 85-86,"
            outputString += $"{((packet[13] << 8) + packet[12])},";
            outputString += $"{(((packet[16]) << 16) + (packet[15] << 8) + packet[14])},";
            outputString += $"{(((packet[16] & 0b00000001) << 16) + (packet[15] << 8) + packet[14])},";

            outputString += $"{((packet[23] << 8) + packet[22])},";
            outputString += $"{((packet[33] << 8) + packet[32])},";
            outputString += $"{((packet[43] << 8) + packet[42])},";
            outputString += $"{((packet[47] << 8) + packet[46])},";
            outputString += $"{((packet[53] << 8) + packet[52])},";
            outputString += $"{((packet[57] << 8) + packet[56])},";
            outputString += $"{((packet[63] << 8) + packet[62])},";
            outputString += $"{((packet[67] << 8) + packet[66])},";
            outputString += $"{((packet[76] << 8) + packet[75])},";
            outputString += $"{((packet[84] << 8) + packet[83])},";
            outputString += $"{((packet[86] << 8) + packet[85])},";

            int wtf = ((packet[125] << 16) + (packet[124] << 8) + packet[123]);
            outputString += $"{Convert.ToString(wtf, 2)} b,";


            for (int i = 10; i < packet.Length; i++)
            {

                if (staticPacketColumns.Where(c => c.Column == i).Any())
                {
                    continue;
                }

                if (combinedCols.Contains(i))
                {
                    continue;
                }

                outputString += $"{packet[i]},";
            }

            return outputString;
        }


    }
}
