using System.IO;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Data;
using Microsoft.EntityFrameworkCore;

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
                    yield return node;
                }
            }
        }
        void load_seamark(dbContext db, OsmSharp.Node seamark)
        {
            var db_node = new lighthouse.Models.OsmNode();
            db_node.OsmId = seamark?.Id ?? throw new Exception("OSM id is somehow null");
            db_node.Lat = $"{seamark?.Latitude}" ?? throw new Exception($"Node {db_node.OsmId} has null latitude");
            db_node.Lon = $"{seamark?.Longitude}" ?? throw new Exception($"Node {db_node.OsmId} has null longitude");
            db_node.Version = seamark?.Version ?? throw new Exception($"Node {db_node.OsmId} has null version");

            db.Add(db_node);

            foreach (var pb_tag in seamark.Tags)
            {
                var db_tag_key = lighthouse.Models.TagKey.upsert_tag_key(db, pb_tag.Key);
                var db_tag = new lighthouse.Models.OsmTag();
                db_tag.OsmNode = db_node;
                db_tag.TagKey = db_tag_key;
                db_tag.Value = pb_tag.Value;
                db.Add(db_tag);
            }
        }
        public int begin(string input_pbf, string db_path)
        {
            using (var db = new dbContext(db_path))
            {
                using (var txn = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    // XXX truncate osm tag and node tables?
                    foreach (var seamark in GetSeamarks(input_pbf))
                    {
                        load_seamark(db, seamark);
                    }
                    db.SaveChanges();
                    txn.Commit();
                }
            }
            return 0;
        }
    }
}