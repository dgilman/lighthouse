using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;

using OsmSharp;
using OsmSharp.Streams;

using lighthouse.DBContext;

namespace lighthouse
{
    class LoadOSM
    {
        IEnumerable<OsmSharp.Node> GetSeamarks(string planet_path)
        {
            using (var fileStream = new FileInfo(planet_path).OpenRead())
            {
                var source = new PBFOsmStreamSource(fileStream);
                foreach (var obj in source)
                {
                    if (obj.Type != OsmGeoType.Node)
                    {
                        continue;
                    }
                    var node = (OsmSharp.Node)obj;
                    foreach (var tag in node.Tags)
                    {
                        if (tag.Key.StartsWith("seamark"))
                        {
                            yield return node;
                            break;
                        }
                    }
                }
            }
        }
        void load_seamark(dbContext db, OsmSharp.Node seamark)
        {
            var db_node = new lighthouse.Models.Node();
            db_node.OsmId = seamark.Id ?? throw new SystemException("OSM id is somehow null");
            db_node.Lat = seamark.Latitude ?? throw new SystemException(String.Format("Node {0} has null latitude", db_node.OsmId));
            db_node.Lon = seamark.Longitude ?? throw new SystemException(String.Format("Node {0} has null longitude", db_node.OsmId));

            foreach (var pb_tag in seamark.Tags)
            {
                var db_tag_key = db.TagKey.Where(tk => tk.TagKey1 == pb_tag.Key).FirstOrDefault();
                if (db_tag_key is null)
                {
                    db_tag_key = new lighthouse.Models.TagKey();
                    db_tag_key.TagKey1 = pb_tag.Key;
                    db.Add(db_tag_key);
                }

                var db_tag = new lighthouse.Models.Tag();
                db_tag.Node = db_node;
                db_tag.TagKey = db_tag_key;
                db_tag.Value = pb_tag.Value;
                db.Add(db_tag);
            }
            db.Add(db_node);
        }
        public int begin(string input_pbf, string db_path)
        {
            using (var db = new dbContext(db_path))
            {
                var seamark_idx = 0;
                foreach (var seamark in GetSeamarks(input_pbf))
                {
                    if (seamark_idx != 0 && seamark_idx % 10000 == 0)
                    {
                        Console.WriteLine(String.Format("Inserting seamark {0}", seamark_idx));
                    }
                    load_seamark(db, seamark);
                    seamark_idx++;
                }
                db.SaveChanges();
            }
            return 0;
        }
    }
}