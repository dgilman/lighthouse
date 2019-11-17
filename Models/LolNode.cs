using System;
using System.Collections.Generic;

using lighthouse.DBContext;
using lighthouse.Models;

namespace lighthouse.Models
{
    public partial class LolNode
    {
        public LolNode()
        {
            LolTag = new HashSet<LolTag>();
        }

        public long LolNodeId { get; set; }
        public string Lat { get; set; }
        public string Lon { get; set; }

        public virtual Link Link { get; set; }
        public virtual ICollection<LolTag> LolTag { get; set; }

        public void add_tag(dbContext db, string key, string value)
        {
            var source_tag = new LolTag();
            source_tag.TagKey = TagKey.upsert_tag_key(db, key);
            source_tag.Value = value;
            db.Add(source_tag);
            LolTag.Add(source_tag);
        }
    }
}
