using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class OsmTag
    {
        public long OsmTagId { get; set; }
        public long OsmNodeId { get; set; }
        public long TagKeyId { get; set; }
        public string Value { get; set; }

        public virtual OsmNode OsmNode { get; set; }
        public virtual TagKey TagKey { get; set; }
    }
}
