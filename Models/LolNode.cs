using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class LolNode
    {
        public LolNode()
        {
            LolTag = new HashSet<LolTag>();
        }

        public long LolNodeId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }

        public virtual Link Link { get; set; }
        public virtual ICollection<LolTag> LolTag { get; set; }
    }
}
