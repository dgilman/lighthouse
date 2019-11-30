CREATE TABLE osm_node (
    osm_node_id INTEGER PRIMARY KEY,
    osm_id INTEGER NOT NULL UNIQUE,
    lat_str TEXT NOT NULL,
    lon_str TEXT NOT NULL,
    lat REAL NOT NULL,
    lon REAL NOT NULL,
    version INTEGER NOT NULL
);

-- Still need to figure out which is more selective
CREATE INDEX osm_node_lat_lon ON osm_node(lat, lon);

CREATE TABLE tag_key (
    tag_key_id INTEGER PRIMARY KEY,
    tag_key TEXT NOT NULL UNIQUE
);

CREATE TABLE osm_tag (
    osm_tag_id INTEGER PRIMARY KEY,
    osm_node_id INTEGER NOT NULL,
    tag_key_id INTEGER NOT NULL,
    value TEXT NOT NULL,

    FOREIGN KEY (tag_key_id) REFERENCES tag_key (tag_key_id),
    FOREIGN KEY (osm_node_id) REFERENCES osm_node (osm_node_id),

    UNIQUE (osm_node_id, tag_key_id)
);

CREATE TABLE lol_node (
    lol_node_id INTEGER PRIMARY KEY,
    lat_str TEXT NOT NULL,
    lon_str TEXT NOT NULL,
    lat REAL NOT NULL,
    lon REAL NOT NULL
);

CREATE INDEX lol_node_lat_lon ON lol_node(lat, lon);

CREATE TABLE lol_tag (
    lol_tag_id INTEGER PRIMARY KEY,
    lol_node_id INTEGER NOT NULL,
    tag_key_id INTEGER NOT NULL,
    value TEXT NOT NULL,

    FOREIGN KEY (tag_key_id) REFERENCES tag_key (tag_key_id),
    FOREIGN KEY (lol_node_id) REFERENCES lol_node (lol_node_id),

    UNIQUE (lol_node_id, tag_key_id)
);

CREATE TABLE link (
    link_id INTEGER PRIMARY KEY,
    osm_node_id INTEGER NOT NULL UNIQUE,
    lol_node_id INTEGER NOT NULL UNIQUE,

    FOREIGN KEY (osm_node_id) REFERENCES osm_node (osm_node_id),
    FOREIGN KEY (lol_node_id) REFERENCES lol_node (lol_node_id)
);
