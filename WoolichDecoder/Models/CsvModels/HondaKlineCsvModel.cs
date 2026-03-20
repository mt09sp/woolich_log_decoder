using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models.CsvModels
{
    public class HondaKlineCsvModel : BaseCsvModel
    {
        // Initialize the packet format for Kawasaki
        public HondaKlineCsvModel(PacketFormat pf)
        {
            packetFormat = pf;
        }


        // HondaKline-specific properties

        public override void PopulateFromLongPacket(KeyValuePair<string, byte[]> packet, int? minThrottle)
        {
            // Call base method to populate common properties

            PopulateBaseFromLongPacket(packet, minThrottle);

            // HondaKline-specific properties


        }

    }

    public static class HondaKlineCsvModelExtensions
    {

        /// <summary>
        /// I cannot remember why I did this. Adjusing for min throttle.
        /// </summary>
        /// <param name="priorPacket"></param>
        /// <param name="minThrottle"></param>
        /// <param name="pf"></param>
        /// <returns></returns>
        public static List<HondaKlineCsvModel> RefineSequentialDerivedPackets(this List<HondaKlineCsvModel> priorPacket, int? minThrottle, PacketFormat pf)
        {
            foreach (var packet in priorPacket)
            {


                if (pf == PacketFormat.HondaKlineCB)
                {
                    // Checked
                    // packet.ActualTPS = Math.Round((packet.ActualTPSRaw - ((minThrottle ?? 25) + 1)) / 2.00, 2);
                    //This is more correct - because the ECU zero's it.
                    packet.ActualTPS = Math.Round(packet.ActualTPS2 / 1.57, 2);


                    // checked
                    packet.WoolichTPS = Math.Round((packet.ActualTPSRaw * 0.4902) - 12.745, 3);

                    packet.WoolichTPSError = Math.Round(packet.WoolichTPS - packet.ActualTPS, 2);

                    if (packet.ActualTPS > 100) packet.ActualTPS = 100;
                    if (packet.ActualTPS < 0) packet.ActualTPS = 0;
                    if (packet.WoolichTPS > 100) packet.WoolichTPS = 100;
                    if (packet.WoolichTPS < 0) packet.WoolichTPS = 0;


                }
                else if (pf == PacketFormat.HondaKlineVFR)
                {
                    // **** Needs checking
                    // packet.ActualTPS = 100 + Math.Round((packet.ActualTPSRaw - ((minThrottle ?? 25) + 1)) / 2.04, 2);
                    // packet.WoolichTPS = 100 + Math.Round((packet.ActualTPSRaw - 30) / 1.7, 3);

                    packet.WoolichTPSError = Math.Round(packet.WoolichTPS - packet.ActualTPS, 2);

                    // if (packet.ActualTPS > 100) packet.ActualTPS = 100;
                    // if (packet.ActualTPS < 0) packet.ActualTPS = 0;
                    // if (packet.WoolichTPS > 100) packet.WoolichTPS = 100;
                    // if (packet.WoolichTPS < 0) packet.WoolichTPS = 0;



                    // packet.ActualEtv = -Math.Round((packet.ActualEtvRaw - ((minThrottle ?? 25) + 1)) / 2.04, 2);
                    //packet.WoolichEtv = -Math.Round((packet.ActualEtvRaw - 30) / 1.7, 3);

                    packet.WoolichEtvError = Math.Round(packet.WoolichEtv - packet.ActualEtv, 2);

                    // if (packet.ActualEtv > 100) packet.ActualEtv = 100;
                    // if (packet.ActualEtv < 0) packet.ActualEtv = 0;
                    // if (packet.WoolichEtv > 100) packet.WoolichEtv = 100;
                    //if (packet.WoolichEtv < 0) packet.WoolichEtv = 0;

                }


            }


            return priorPacket;
        }
    }

}