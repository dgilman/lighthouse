using System;
using System.Collections.Generic;
using System.Linq;

using lighthouse.DBContext;

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

        public static TagKey upsert_tag_key(dbContext db, string tag_name)
        {
            var db_tag_key = db.TagKey.Where(tk => tk.TagKey1 == tag_name).FirstOrDefault();
            if (db_tag_key is null)
            {
                db_tag_key = new lighthouse.Models.TagKey();
                db_tag_key.TagKey1 = tag_name;
                db.Add(db_tag_key);
                db.SaveChanges();
            }
            return db_tag_key;
        }
    }
}
