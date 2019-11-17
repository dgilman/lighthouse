using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Globalization;
using System;
using System.Text.RegularExpressions;

using lighthouse.DBContext;
using lighthouse.Models;

namespace lighthouse
{
    class LoadLOL
    {
        private static Regex volume_matcher = new Regex(@"PUB (\d+)");
        // https://wiki.openstreetmap.org/wiki/Seamarks/Lights#Characters
        private static Regex characteristic_matcher = new Regex("^(F|Fl|LFl|Q|VQ|UQ|Iso|Oc|IQ|IVQ|IUQ|Mo|FFl|FlLFl|OcFl|FLFl|Al[.]Oc|Al[.]LFl|Al[.]Fl|Al[.]Gr|Q[+]LFl|VQ[+]LFl|UQ[+]LFl|Al|Al[.]FFl)");

        string get_source(XElement xml_light)
        {
            int pub;

            var volume = xml_light.Elements("Volume").First().Value;
            var volume_match = volume_matcher.Match(volume).Groups?[1]?.Value;
            if (volume_match is null)
            {
                Console.WriteLine("Unable to parse volume, good luck");
                return null;
            }
            try
            {
                pub = int.Parse(volume_match);
            }
            catch (System.FormatException)
            {
                Console.WriteLine("Unable to parse volume, good luck");
                return null;
            }

            // XXX these can be other things - cross reference osm db
            return $"US NGA Pub. {pub}.";
        }

        double? get_latlon(XElement xml_light, string elem_name, string volume, string name)
        {
            var lat_str = xml_light.Elements(elem_name).First().Value;

            try
            {
                return double.Parse(lat_str, CultureInfo.InvariantCulture);
            }
            catch (System.FormatException)
            {
                Console.WriteLine($"Volume {volume} name {name}: Unable to format {elem_name}");
                return null;
            }
        }

        void load_xml_light(dbContext db, XElement xml_light)
        {
            var name = xml_light.Elements("NameLocation").First().Value.Trim();
            var source_value = get_source(xml_light);
            if (source_value == null)
            {
                return;
            }

            var lat = get_latlon(xml_light, "Latitude", source_value, name);
            var lon = get_latlon(xml_light, "Longitude", source_value, name);
            if (!lon.HasValue || !lat.HasValue)
            {
                return;
            }

            // XXX do characteristic

            /*
                Keys:
                - source (pub no.)
                - name (their name)
                - longname
                - locs:
                    - light:locref (USNGA only) ("int_chr int_nr.int_subnr")
                    - light:reference (alternative to locref?) ("int_chr int_nr")
                - light:character
                - light:group
                - light:period
                - light:height
                - light:multiple
                - light:category ('pos' / mpos in old db)
                - light:orientation
                - light:sequence
                - light:information

                then sectors

                - type
                    - type:colour
                    - type:colour_pattern
                    - type:height
                - racon:
                    - radar_reflector
                    - radar_transponder:category
                    - radar_transponder:group
                    - radar_transponder:period
                - fog_signal:category
            */

            foreach (var characteristic_elem in xml_light.Elements("Characteristic_Element"))
            {
                string type_;
                var characteristic = characteristic_elem.Elements("Characteristic").First();
                var characteristic_type = characteristic.Elements("Character_type").First().Value.Trim();
                if (characteristic_type == "")
                {
                    type_ = "default"; // ???

                } else if (characteristic_type == "RACON" )
                {
                    type_ = "RACON"; // ??
                } else
                {
                    Console.WriteLine($"Volume {source_value} name {name}: unknown type {characteristic_type}");
                    continue;
                }


                var lol_node = new LolNode();
                lol_node.Lat = lat.Value;
                lol_node.Lon = lon.Value;

                var source_tag = new LolTag();
                source_tag.TagKey = TagKey.upsert_tag_key(db, "source");
                source_tag.Value = source_value;
                db.Add(source_tag);
                lol_node.LolTag.Append(source_tag);


                var name_tag = new LolTag();
                name_tag.TagKey = TagKey.upsert_tag_key(db, "name");
                name_tag.Value = name;
                db.Add(name_tag);
                lol_node.LolTag.Append(name_tag);


                db.Add(lol_node);
            }
        }
        void load_lol(dbContext db, string input_lol)
        {
            var xml_obj = XElement.Load(input_lol);
            foreach (var xml_light in xml_obj.Elements())
            {
                // XXX filter on AidType
                load_xml_light(db, xml_light);
            }
        }
        public int begin(IEnumerable<string> input_lols, string db_path)
        {
            using (var db = new dbContext(db_path))
            {
                foreach (var input_lol in input_lols)
                {
                    load_lol(db, input_lol);
                }
                db.SaveChanges();
            }
            return 0;
        }
    }
}