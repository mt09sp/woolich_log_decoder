using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models
{
    public static class PacketTypeIdentification
    {
        // Andrew zx10R and Jacob zx10R (same)
        public static (PacketFormat, byte[]) ZX10R     = ( PacketFormat.Kawasaki, new byte[] { 0x27, 0x02, 0x02, 0x07, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 } );

        // Mine MT09SP Gen3 
        public static (PacketFormat, byte[])[] MT09GEN3  = { 
            (PacketFormat.Yamaha, new byte[] { 0x2a, 0x02, 0x01, 0x08, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }), 
            (PacketFormat.Yamaha, new byte[] { 0x2a, 0x02, 0x01, 0x06, 0x04, 0x0a, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }) 
        };


        // R1 cannot remember who from 
        public static (PacketFormat, byte[]) R1        = (PacketFormat.Yamaha, new byte[] { 0x19, 0x02, 0x01, 0x06, 0x04, 0x0a, 0x01, 0x02, 0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        // S1000RR
        public static (PacketFormat, byte[]) S1000RR   = (PacketFormat.BMW, new byte[] { 0x2d, 0x02, 0x01, 0x00, 0x010, 0x0a, 0x01, 0x00, 0x01, 0x07, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });

        // Hayabusa
        public static (PacketFormat, byte[]) HAYABUSA  = (PacketFormat.Suzuki, new byte[] { 0x2b, 0x01, 0x01, 0x05, 0x004, 0x0a, 0x01, 0x00, 0x00, 0x00, 0xd0, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 });


        // Method to identify packet type based on first 10 bytes
        public static PacketFormat IdentifyPacketType(byte[] primaryHeaderData)
        {
            if (primaryHeaderData == null || primaryHeaderData.Length < 10)
            {
                return PacketFormat.Unknown;
            }

            // Check against all known packet types
            if (MatchesFirst10Bytes(primaryHeaderData, ZX10R.Item2))
                return ZX10R.Item1;

            foreach (var bike in MT09GEN3)
            {
                if (MatchesFirst10Bytes(primaryHeaderData, bike.Item2))
                    return bike.Item1;
            }
            if (MatchesFirst10Bytes(primaryHeaderData, R1.Item2))
                return R1.Item1;

            if (MatchesFirst10Bytes(primaryHeaderData, S1000RR.Item2))
                return S1000RR.Item1;

            if (MatchesFirst10Bytes(primaryHeaderData, HAYABUSA.Item2))
                return HAYABUSA.Item1;

            return PacketFormat.Unknown;
        }

        // Helper method to compare first 10 bytes
        private static bool MatchesFirst10Bytes(byte[] data, byte[] pattern)
        {
            for (int i = 0; i < 10; i++)
            {
                if (data[i] != pattern[i])
                    return false;
            }
            return true;
        }

    }
}
