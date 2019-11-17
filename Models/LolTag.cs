using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class LolTag
    {
        public long LolTagId { get; set; }
        public long LolNodeId { get; set; }
        public long TagKeyId { get; set; }
        public string Value { get; set; }

        public virtual LolNode LolNode { get; set; }
        public virtual TagKey TagKey { get; set; }
    }
}
