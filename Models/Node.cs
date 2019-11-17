using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class Node
    {
        public Node()
        {
            Tag = new HashSet<Tag>();
        }

        public long NodeId { get; set; }
        public long OsmId { get; set; }
        public double Lat { get; set; }
        public double Lon { get; set; }
        public long Version { get; set; }

        public virtual ICollection<Tag> Tag { get; set; }
    }
}
