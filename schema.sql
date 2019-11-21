CREATE TABLE osm_node (
    osm_node_id INTEGER PRIMARY KEY,
    osm_id INTEGER NOT NULL UNIQUE,
    lat TEXT NOT NULL,
    lon TEXT NOT NULL,
    version INTEGER NOT NULL
);

-- todo: indexing strategy
-- solo column indexes on (lat) and (lon) are
-- probably going to be decent replacements for a spatial index
-- as a 1-dimensional bounding strip of the earth probably doesn't
-- have many lighthouses in it.

-- we probably want to keep both float
-- and text versions of lat and lon to preserve floating point precision.

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
    lat TEXT NOT NULL,
    lon TEXT NOT NULL
);

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
