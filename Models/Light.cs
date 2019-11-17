using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class Light
    {
        public long Id { get; set; }
        public long OsmId { get; set; }
        public string Name { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
    }
}
