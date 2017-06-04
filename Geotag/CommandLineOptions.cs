using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag
{
    class CommandLineOptions
    {
        [Option('l', "locationFile", Required = true, HelpText = "Path of the locations file which will be used")]
        public string LocationHistoryFile { get; set; }

        [Option('i', "inputFolder", Required = true, HelpText = "Path of a folder or a specific file to be processed. Only JPG files are valid")]
        public string InputPhotosFolder { get; set; }

        [Option('r', "readOnly", HelpText="If set no files will be modified or saved")]
        public bool ReadOnly { get; set; }

        [Option("overwrite", HelpText = "If set allows Geotag to overwrite GPS data in photos which already contains such data")]
        public bool Overwrite { get; set; }

        [Option('o', "output", HelpText="Destination folder where modified files will be saved. If null, files will be saved at same folder but with _modified_{time} appended.")]
        public string DestinationFolder { get; internal set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText
            {
                Heading = new HeadingInfo("Geotag"),
                AdditionalNewLineAfterOption = true,
                AddDashesToOption = true
            };

            help.AddOptions(this);

            return help;
        }
    }
}
