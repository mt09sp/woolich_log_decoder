using System;
using System.Collections.Generic;

namespace WoolichDecoder.Models
{
    public class WoolichMT09Log
    {

        /// <summary>
        /// PrimaryHeaderLength I expect this identifies the source of the log data. What collected it.
        /// </summary>
        public int PrimaryHeaderLength { get; set; } = 352;

        /// <summary>
        /// SecondaryHeaderLength  should be all 0xFF
        /// </summary>
        public int SecondaryHeaderLength { get; set; } = 164;

        /// <summary>
        /// packetLength The length of the actual data packet.
        /// </summary>
        public int PacketLength { get; set; } = 96;

        public byte[] PrimaryHeaderData = { };
        public byte[] SecondaryHeaderData = { };

        Dictionary<string, byte[]> logData = new Dictionary<string, byte[]>();

        public int AddPacket(byte[] packet)
        {
            string timeString = string.Empty;
            // Timestamp
            // var ms = packet[8] << 8;
            // ms += packet[9];
            var hh = packet[5].ToString("00");
            var MM = packet[6].ToString("00");
            var ss = packet[7].ToString("00");
            var ms1 = packet[8];
            var ms2 = packet[9];
            var ms = ((ms1 << 8) + ms2).ToString("000");

            timeString += $"{hh}:{MM}:{ss}.{ms}";

            //try
            //{
            logData.Add(timeString, packet);

            byte checksum = 0x00;

            /*
            for (int i = 0; i < (packet.Length - 1); i++)
            {

                checksum ^= packet[i];

            }
            */
            checksum = (byte)(checksumCalculator(packet, 95) & 0xFF);
            return checksum;


            //}
            //catch (ArgumentException ex)
            //{
            // do nothing. Possibly a duplicate.
            //}
        }


        int checksumCalculator(byte[] data, int length)
        {
            int curr_crc = 0x0000;
            byte sum1 = (byte)curr_crc;
            byte sum2 = (byte)(curr_crc >> 8);
            int index;
            for (index = 0; index < length; index = index + 1)
            {
                sum1 = (byte)((sum1 + data[index]) % 255);
                sum2 = (byte)((sum2 + sum1) % 255);
            }
            return (sum2 << 8) | sum1;
        }

        public Dictionary<string, byte[]> GetPackets()
        {
            return logData;
        }

        public int GetPacketCount()
        {
            return logData.Count;
        }

        public void ClearPackets()
        {
            this.logData.Clear();
            Array.Clear(PrimaryHeaderData, 0, PrimaryHeaderData.Length);
            Array.Clear(SecondaryHeaderData, 0, SecondaryHeaderData.Length);
        }

    }
}
