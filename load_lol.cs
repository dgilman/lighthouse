using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System;
using System.Text.RegularExpressions;
using System.Data;

namespace lighthouse
{
    class LightException : Exception
    {
        public LightException()
        {
        }

        public LightException(string message)
    : base(message)
        {
        }

        public LightException(string message, Exception inner)
    : base(message, inner)
        {
        }


    }

    class LoadLOL
    {
        private static Regex volume_matcher = new Regex(@"PUB (\d+)");
        // https://wiki.openstreetmap.org/wiki/Seamarks/Lights#Characters
        private static Regex VALID_CHARACTERISTIC = new Regex("^(F|Fl|LFl|Q|VQ|UQ|Iso|Oc|IQ|IVQ|IUQ|Mo|FFl|FlLFl|OcFl|FLFl|Al[.]Oc|Al[.]LFl|Al[.]Fl|Al[.]Gr|Q[+]LFl|VQ[+]LFl|UQ[+]LFl|Al|Al[.]FFl)");
        private static Regex VALID_RACON_GROUP = new Regex("^([A-Z]+)[(]");
        private static List<string> VALID_CHARACTER_TYPES = new List<string>() { "", "RACON" };
        private dbContext db;

        private class LightData
        {
            public int pub;
            public string volume;
            public string name;
            public string lat;
            public string lon;
            public string int_no;
            public string us_no;

            public void parse(XElement xml_light)
            {
                volume = parse_volume(xml_light);
                pub = parse_pub();
                name = parse_name(xml_light);
                lat = parse_latlon(xml_light, "Latitude");
                lon = parse_latlon(xml_light, "Longitude");
                int_no = parse_int_no(xml_light);
                us_no = parse_us_no(xml_light);
            }

            string parse_name(XElement xml_light)
            {
                return xml_light.Elements("NameLocation")?.FirstOrDefault()?.Value?.Trim() ??
                    throw new LightException("Unable to parse NameLocation");
            }

            string parse_latlon(XElement xml_light, string elem_name)
            {
                var coord_str = xml_light.Elements(elem_name)?.FirstOrDefault()?.Value ??
                    throw new LightException($"Unable to parse {elem_name} out of {name}");

                try
                {
                    // Validate the input, but let's keep their string around
                    // to retain the precision of their number.
                    double.Parse(coord_str, CultureInfo.InvariantCulture);
                }
                catch (System.FormatException)
                {
                    throw new LightException($"Volume {volume} name {name}: Unable to format {elem_name}");
                }

                coord_str = coord_str.Trim();
                if (coord_str == "")
                {
                    throw new LightException($"Light {name} lacks a {elem_name}, skipping");
                }

                return coord_str;

            }

            string parse_volume(XElement xml_light)
            {
                return xml_light.Elements("Volume")?.FirstOrDefault()?.Value?.Trim() ??
                    throw new LightException("Unable to parse volume.");
            }

            int parse_pub()
            {
                var volume_match = volume_matcher.Match(volume).Groups?[1]?.Value ??
                    throw new LightException("Unable to parse pub out of volume, good luck.");
                try
                {
                    return int.Parse(volume_match);
                }
                catch (System.FormatException)
                {
                    throw new LightException("Unable to parse pub out of volume, good luck.");
                }
            }

            string parse_int_no(XElement xml_light)
            {
                return xml_light.Elements("IntlNo")?.FirstOrDefault()?.Value?.Trim() ??
                    throw new LightException("Unable to parse intl number.");
            }

            string parse_us_no(XElement xml_light)
            {
                return xml_light.Elements("AidNo")?.FirstOrDefault()?.Value?.Trim() ??
                    throw new LightException("Unable to parse US light number.");
            }

            public string format_source()
            {
                // XXX these can be other things - cross reference osm db
                return $"US NGA Pub. {pub}.";
            }

            public string format_name()
            {
                // XXX clean up by stripping leading dashes, trailing ., RACON notification,
                // possible other stuff, changing newlines into spaces, removing duplicate spaces,
                // and finally trimming whitespace.  Oh and possibly un-urlencoding some stuff.
                return $"{name}";
            }

        }

        void load_racon(XElement xml_characteristic_props, LolNode lol_node)
        {
            var group = xml_characteristic_props.Elements("Character")?.FirstOrDefault()?.Value?.Trim() ??
                throw new LightException("Unable to extract Character from RACON");
            var period = xml_characteristic_props.Elements("Period")?.FirstOrDefault()?.Value?.Trim() ??
                throw new LightException("Unable to extract Period from RACON");

            lol_node.add_tag(db, "seamark:radar_transponder:category", "racon");
            var group_match = VALID_RACON_GROUP.Match(group).Groups[1]?.Value ??
                throw new LightException("Unable to parse group out of RACON character");
            lol_node.add_tag(db, "seamark:radar_transponder:group", group_match);

            if (period != "")
            {
                int period_int;
                try
                {
                    period_int = int.Parse(period, System.Globalization.NumberStyles.Float);
                }
                catch (System.FormatException)
                {
                    throw new LightException("Unable to parse period for RACON");
                }
                lol_node.add_tag(db, "seamark:radar_transponder:period", $"{period_int}");
            }
        }

        void load_light(LightData light_data, XElement xml_characteristic, LolNode lol_node)
        {
            var xml_characteristic_props = xml_characteristic.Elements("Characteristic")?.FirstOrDefault()
                ?? throw new LightException("Unable to extract Characteristic element from Characteristic_Element");
            var character = xml_characteristic_props.Elements("Character")?.FirstOrDefault()?.Value?.Trim()
                ?? throw new LightException($"Unable to exact Character from {light_data.name}");

            var character_match = VALID_CHARACTERISTIC.Match(character)?.Groups?[1]?.Value;
            if (character_match != null)
            {
                lol_node.add_tag(db, "seamark:light:character", character_match);
            }


        }

        string parse_character_type(LightData light_data, XElement xml_characteristic_props)
        {
            var character_type = xml_characteristic_props.Elements("Character_Type")?.FirstOrDefault()?.Value
                ?? throw new LightException($"Unable to extract Character_Type from {light_data.name}");
            character_type = character_type.Replace("-", "").Trim();
            if (!VALID_CHARACTER_TYPES.Contains(character_type))
            {
                throw new LightException($"Unknown Character_Type {character_type} in {light_data.name}");
            }
            return character_type;
        }

        void load_xml_characteristic(LightData light_data, XElement xml_characteristic)
        {
            /*
                The XML schema here has some unfortunate element names:
                <Characteristic_Element>    <--- var xml_characteristic
                    <Characteristic>        <--- var xml_characteristic_props
                        <Character>         <--- var character (scalar)
                        <Character_Type>    <--- var character_type (scalar)
            */

            /*
                Keys:
                - source (pub no.)
                - name (their name) (prefer no period, but it can be accepted.)
                    - transform newlines to spaces, then strip? and strip periods and leading dashes?
                - longname
                - locs:
                    - light:locref (USNGA only) ("int_chr int_nr.int_subnr")
                    - light:reference (alternative to locref?) ("int_chr int_nr") (only appears to be used)
                - DONE light:character
                - light:group
                - light:period
                - light:height
                - light:multiple
                - light:category ('pos' / mpos in old db)
                - light:orientation
                - light:sequence
                - light:information

                then sectors (comes from remarks.)

                - type
                    - type:colour
                    - type:colour_pattern
                    - type:height
                - racon:
                    - radar_reflector
                    - DONE radar_transponder:category
                    - DONE radar_transponder:group
                    - DONE radar_transponder:period
                - fog_signal:category
            */
            var xml_characteristic_props = xml_characteristic.Elements("Characteristic")?.FirstOrDefault()
                ?? throw new LightException("Unable to extract Characteristic element from Characteristic_Element");
            var character_type = parse_character_type(light_data, xml_characteristic_props);

            bool is_racon = character_type == "RACON" || light_data.name.Contains("RACON");

            var lol_node = new LolNode();
            lol_node.Lat = light_data.lat;
            lol_node.Lon = light_data.lon;
            db.Add(lol_node);

            lol_node.add_tag(db, "source", light_data.format_source());
            lol_node.add_tag(db, "seamark:name", light_data.format_name());

            if (is_racon)
            {
                load_racon(xml_characteristic_props, lol_node);
            }
            else
            {
                load_light(light_data, xml_characteristic, lol_node);
            }
        }

        void load_xml_light(XElement xml_light)
        {
            var light_constants = new LightData();
            light_constants.parse(xml_light);

            foreach (var characteristic_elem in xml_light.Elements("Characteristic_Element"))
            {
                load_xml_characteristic(light_constants, characteristic_elem);
            }
        }
        void load_xml_radiobeacon(XElement xml_radiobeacon)
        {
            return;
        }
        void load_xml_dgps(XElement xml_radiobeacon)
        {
            return;
        }
        void load_xml_uscg_light(XElement xml_uscg_light)
        {
            return;
        }
        void load_lol(string input_lol)
        {
            var xml_obj = XElement.Load(input_lol);
            Console.WriteLine($"Starting {input_lol}");
            foreach (var xml_light in xml_obj.Elements("LightFeature"))
            {
                try
                {
                    load_xml_light(xml_light);
                }
                catch (LightException e)
                {
                    Console.WriteLine($"Unable to load LightFeature: {e.Message}");
                }
            }
            foreach (var xml_radiobeacon in xml_obj.Elements("rbFeature"))
            {
                load_xml_radiobeacon(xml_radiobeacon);
            }
            foreach (var xml_dgps in xml_obj.Elements("dgpsFeature"))
            {
                load_xml_dgps(xml_dgps);
            }
            foreach (var xml_uscg_light in xml_obj.Elements("uscgLLEntity"))
            {
                load_xml_uscg_light(xml_uscg_light);
            }
        }
        public int begin(IEnumerable<string> input_lols, string db_path)
        {
            using (var db_obj = new dbContext(db_path))
            {
                db = db_obj;
                using (var txn = db.Database.BeginTransaction(IsolationLevel.Serializable))
                {
                    foreach (var input_lol in input_lols)
                    {
                        load_lol(input_lol);
                    }
                    db.SaveChanges();
                    throw new Exception("No.");
                    txn.Commit();
                }
            }
            return 0;
        }
    }
}