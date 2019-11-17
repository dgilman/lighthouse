using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class Tag
    {
        public long TagId { get; set; }
        public long NodeId { get; set; }
        public long TagKeyId { get; set; }
        public string Value { get; set; }

        public virtual Node Node { get; set; }
        public virtual TagKey TagKey { get; set; }
    }
}
