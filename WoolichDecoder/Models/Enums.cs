using System;

namespace WoolichDecoder.Models
{

    [Flags]
    public enum ExportItems
    {
        Yellow = 1,
        Green = 2,
        Red = 4,
        Blue = 8
    }


    public enum PacketFormat
    {
        Unknown = 0x00,
        Japanese = 0x01,
        Yamaha = 0x5d,
        Kawasaki = 0x4c,
        Suzuki = 0x26,
        BMW = 0x10,
    }


}
