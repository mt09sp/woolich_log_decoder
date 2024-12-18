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
        Yamaha = 0x01,
        BMW = 0x10,
    }


}
