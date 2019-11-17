using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class Link
    {
        public long LinkId { get; set; }
        public long OsmNodeId { get; set; }
        public long LolNodeId { get; set; }

        public virtual LolNode LolNode { get; set; }
        public virtual OsmNode OsmNode { get; set; }
    }
}
