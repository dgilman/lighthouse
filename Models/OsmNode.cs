using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class OsmNode
    {
        public OsmNode()
        {
            OsmTag = new HashSet<OsmTag>();
        }

        public long OsmNodeId { get; set; }
        public long OsmId { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }
        public long Version { get; set; }

        public virtual Link Link { get; set; }
        public virtual ICollection<OsmTag> OsmTag { get; set; }
    }
}
