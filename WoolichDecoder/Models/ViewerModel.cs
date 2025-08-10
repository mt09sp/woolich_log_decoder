using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WoolichDecoder.Models
{
    public class LogViewerModel
    {

        public string LogTime = string.Empty;

        public int RPM;
        public int RPMRaw;
        public int milliseconds;

        public double WoolichTPS;
        public double ActualTPS;
        public int TPSMultiplier; // 12

        public double ActualEtv; // 67 68
        public double WoolichEtv;


        public double IAP;
        public double AFR;
        public double CoolantTemp;
        public double InletAirTemp;
        public int Gear;
        public double AtmPressure;
        public double ManPressure;
        public double Ignition;
        public double STP;
        public int MS; // unknown
        public bool Clutch;
        public bool Pair;

        public int RearWheelSpeed;
        public int FrontWheelSpeed;
        public int Speedo;

        public byte[] packets; // = new byte[79];


    }
}
