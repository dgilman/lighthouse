using System;
using System.Collections.Generic;
using CommandLine;

namespace lighthouse
{
    [Verb("loadosm", HelpText = "Load planet osm file of lights into db")]
    class LoadOSMOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input OSM PBF file")]
        public string input_pbf {get; set;}
        [Option('d', "db", Required = true, HelpText = "Path to sqlite3 db")]
        public string db_path {get; set;}
    }
    [Verb("loadlol", HelpText = "Load list of lights files into db")]
    class LoadLOLOptions
    {
        [Option('i', "input", Required = true, HelpText = "Input LoL XML files")]
        public IEnumerable<string> input_lols {get; set;}
        [Option('d', "db", Required = true, HelpText = "Path to sqlite3 db")]
        public string db_path {get; set;}

    }

    [Verb("link", HelpText = "Populate linking table between osm and lol nodes")]
    class LinkOptions
    {

    }

    [Verb("newlights", HelpText = "Dump newly added lights to OSM file")]
    class NewLightsOptions
    {

    }

    [Verb("modifiedlights", HelpText = "Dump changes to existing lights to OSM file")]
    class ModifiedLightsOptions
    {

    }

    class Entry
    {

        static int RunLoadOSM(LoadOSMOptions opts)
        {
            var load_osm = new LoadOSM();
            return load_osm.begin(opts.input_pbf, opts.db_path);
        }

        static int RunLoadLOL(LoadLOLOptions opts)
        {
            var load_lol = new LoadLOL();
            return load_lol.begin(opts.input_lols, opts.db_path);
        }

        static int RunLink(LinkOptions opts)
        {
            return 0;
        }

        static int RunNewLights(NewLightsOptions opts)
        {
            return 0;
        }

        static int RunModifiedLights(ModifiedLightsOptions opts)
        {
            return 0;
        }
        static int Main(string[] args)
        {
            return CommandLine.Parser.Default.
            ParseArguments<LoadOSMOptions, LoadLOLOptions, LinkOptions, NewLightsOptions, ModifiedLightsOptions>(args).MapResult(
                (LoadOSMOptions opts) => RunLoadOSM(opts),
                (LoadLOLOptions opts) => RunLoadLOL(opts),
                (LinkOptions opts) => RunLink(opts),
                (NewLightsOptions opts) => RunNewLights(opts),
                (ModifiedLightsOptions opts) => RunModifiedLights(opts),
                errs => 1
            );
        }
    }
}
