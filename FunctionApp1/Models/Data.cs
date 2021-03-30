using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp1.Models
{
    public class Data
    {
        //public string DeviceName { get; set; }
        //public string MacAdress { get; set; }
        //public string Type { get; set; }
        //public string Vendor { get; set; }
        //public string Model { get; set; }
        //public string Longitude { get; set; }
        //public string TypeName { get; set; }
        public long epochTime { get; set; }
        public string Latitude { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public int TemperatureAlert { get; set; }

    }
}
