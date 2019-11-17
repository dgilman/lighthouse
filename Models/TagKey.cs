using System;
using System.Collections.Generic;

namespace lighthouse.Models
{
    public partial class TagKey
    {
        public TagKey()
        {
            LolTag = new HashSet<LolTag>();
            OsmTag = new HashSet<OsmTag>();
        }

        public long TagKeyId { get; set; }
        public string TagKey1 { get; set; }

        public virtual ICollection<LolTag> LolTag { get; set; }
        public virtual ICollection<OsmTag> OsmTag { get; set; }
    }
}
