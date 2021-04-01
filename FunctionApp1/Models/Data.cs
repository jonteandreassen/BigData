using System;
using System.Collections.Generic;
using System.Text;

namespace FunctionApp1.Models
{
    public class Data
    { 
        public long epochTime { get; set; }
        public float Temperature { get; set; }
        public float Humidity { get; set; }
        public int TemperatureAlert { get; set; }

    }
}
