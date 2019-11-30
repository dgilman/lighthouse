using System;
using System.Data;
using System.Collections.Generic;

using System.Data.SQLite;


namespace lighthouse
{
    class StorageFactory
    {
        public Storage Create(string dbPath)
        {
            var ConnStringBuilder = new SQLiteConnectionStringBuilder();
            ConnStringBuilder.DataSource = dbPath;
            ConnStringBuilder.ForeignKeys = true;
            var Conn = new SQLiteConnection(ConnStringBuilder.ToString());
            Conn.Open();
            return new Storage(Conn);
        }
    }

    class Storage
    {
        private SQLiteConnection Conn;
        private SQLiteTransaction Transaction;
        private Dictionary<string, long> TagKeyCache;

        private SQLiteCommand StoreTagKeySearchQuery;
        private SQLiteParameter StoreTagKeySearchParam;

        private SQLiteCommand StoreTagKeyInsertQuery;
        private SQLiteParameter StoreTagKeyInsertParam;

        private SQLiteCommand StoreOsmNodeInsertQuery;
        private SQLiteParameter StoreOsmNodeInsertOsmId;
        private SQLiteParameter StoreOsmNodeInsertLatStr;
        private SQLiteParameter StoreOsmNodeInsertLonStr;
        private SQLiteParameter StoreOsmNodeInsertLat;
        private SQLiteParameter StoreOsmNodeInsertLon;
        private SQLiteParameter StoreOsmNodeInsertVersion;

        private SQLiteCommand StoreOsmTagInsertQuery;
        private SQLiteParameter StoreOsmTagInsertOsmNode;
        private SQLiteParameter StoreOsmTagInsertTagKey;
        private SQLiteParameter StoreOsmTagInsertValue;

        public Storage(SQLiteConnection conn)
        {
            TagKeyCache = new Dictionary<string, long>();
            Conn = conn;
            Transaction = conn.BeginTransaction(IsolationLevel.Serializable);

            StoreTagKeySearchQuery = new SQLiteCommand("select tag_key_id from tag_key where tag_key = :tag_key_name;", Conn, Transaction);
            StoreTagKeySearchParam = new SQLiteParameter("tag_key_name", null);
            StoreTagKeySearchQuery.Parameters.Add(StoreTagKeySearchParam);

            StoreTagKeyInsertQuery = new SQLiteCommand("insert into tag_key (tag_key) values (:tag_key_value);", Conn, Transaction);
            StoreTagKeyInsertParam = new SQLiteParameter("tag_key_value", null);
            StoreTagKeyInsertQuery.Parameters.Add(StoreTagKeyInsertParam);

            StoreOsmNodeInsertQuery = new SQLiteCommand(@"
            insert into osm_node
            (osm_id, lat_str, lon_str, lat, lon, version)
            values
            (:osm_id, :lat_str, :lon_str, :lat, :lon, :version)
            ", Conn, Transaction);
            StoreOsmNodeInsertOsmId = new SQLiteParameter("osm_id", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertOsmId);
            StoreOsmNodeInsertLatStr = new SQLiteParameter("lat_str", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertLatStr);
            StoreOsmNodeInsertLonStr = new SQLiteParameter("lon_str", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertLonStr);
            StoreOsmNodeInsertLat = new SQLiteParameter("lat", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertLat);
            StoreOsmNodeInsertLon = new SQLiteParameter("lon", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertLon);
            StoreOsmNodeInsertVersion = new SQLiteParameter("version", null);
            StoreOsmNodeInsertQuery.Parameters.Add(StoreOsmNodeInsertVersion);

            StoreOsmTagInsertQuery = new SQLiteCommand(@"
            insert into osm_tag
            (osm_node_id, tag_key_id, value)
            values
            (:osm_node_id, :tag_key_id, :value)
            ", Conn, Transaction);
            StoreOsmTagInsertOsmNode = new SQLiteParameter("osm_node_id", null);
            StoreOsmTagInsertQuery.Parameters.Add(StoreOsmTagInsertOsmNode);
            StoreOsmTagInsertTagKey = new SQLiteParameter("tag_key_id", null);
            StoreOsmTagInsertQuery.Parameters.Add(StoreOsmTagInsertTagKey);
            StoreOsmTagInsertValue = new SQLiteParameter("value", null);
            StoreOsmTagInsertQuery.Parameters.Add(StoreOsmTagInsertValue);
        }

        public void DisposeConnection()
        {
            // I'm not sure if i want to implement IDisposable here
            // or how to do it right, so we have this instead.
            Transaction.Commit();
            Conn.Close();
        }

        public long StoreOsmNode(long OsmId, string LatStr, string LonStr, double Lat, double Lon, int Version)
        {
            StoreOsmNodeInsertOsmId.Value = OsmId;
            StoreOsmNodeInsertLatStr.Value = LatStr;
            StoreOsmNodeInsertLonStr.Value = LonStr;
            StoreOsmNodeInsertLat.Value = Lat;
            StoreOsmNodeInsertLon.Value = Lon;
            StoreOsmNodeInsertVersion.Value = Version;

            StoreOsmNodeInsertQuery.ExecuteNonQuery();
            return Conn.LastInsertRowId;
        }

        public long StoreTagKey(string tagKey)
        {
            if (TagKeyCache.ContainsKey(tagKey))
            {
                return TagKeyCache[tagKey];
            }
            StoreTagKeySearchParam.Value = tagKey;
            using (var reader = StoreTagKeySearchQuery.ExecuteReader())
            {
                if (reader.HasRows)
                {
                    long TagKeyId = reader.GetInt64(0);
                    TagKeyCache[tagKey] = TagKeyId;
                    return TagKeyId;
                }
            }
            StoreTagKeyInsertParam.Value = tagKey;
            StoreTagKeyInsertQuery.ExecuteNonQuery();
            long NewTagKeyId = Conn.LastInsertRowId;
            TagKeyCache[tagKey] = NewTagKeyId;
            return NewTagKeyId;
        }

        public long StoreOsmTag(long osmNodeId, long tagKeyId, string value)
        {
            StoreOsmTagInsertOsmNode.Value = osmNodeId;
            StoreOsmTagInsertTagKey.Value = tagKeyId;
            StoreOsmTagInsertValue.Value = value;

            StoreOsmTagInsertQuery.ExecuteNonQuery();
            return Conn.LastInsertRowId;
        }
    }
}