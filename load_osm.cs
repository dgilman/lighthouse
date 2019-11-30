using System.IO;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Linq;
using System.Data;

using OsmSharp;
using OsmSharp.Streams;


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
        void load_seamark(Storage db, OsmSharp.Node seamark)
        {
            var OsmId = seamark?.Id ?? throw new Exception("OSM id is somehow null");

            var LatStr = $"{seamark?.Latitude}" ?? throw new Exception($"Node {OsmId} has null latitude");
            var LonStr = $"{seamark?.Longitude}" ?? throw new Exception($"Node {OsmId} has null longitude");
            double Lat;
            double Lon;
            if (!double.TryParse(LatStr, out Lat))
            {
                throw new Exception($"Node {OsmId} could not parse Lat {LatStr}");
            }
            if (!double.TryParse(LonStr, out Lon))
            {
                throw new Exception($"Node {OsmId} could not parse Lon {LonStr}");
            }
            var Version = seamark?.Version ?? throw new Exception($"Node {OsmId} has null version");

            var OsmNodeId = db.StoreOsmNode(OsmId, LatStr, LonStr, Lat, Lon, Version);

            foreach (var pb_tag in seamark.Tags)
            {
                var TagKeyId = db.StoreTagKey(pb_tag.Key);
                db.StoreOsmTag(OsmNodeId, TagKeyId, pb_tag.Value);
            }
        }
        public int begin(string pbfPath, string dbPath)
        {
            var StorageFactory = new StorageFactory();
            var db = StorageFactory.Create(dbPath);
            foreach (var seamark in GetSeamarks(pbfPath))
            {
                load_seamark(db, seamark);
            }
            db.DisposeConnection();
            return 0;
        }
    }
}