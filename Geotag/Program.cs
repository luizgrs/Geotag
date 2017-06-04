using Geotag.GeoProvider;
using Geotag.ImageHandler;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag
{
    class Program
    {
        static int Main(string[] args)
        {
            var commandLineArgs = new CommandLineOptions();

            if(CommandLine.Parser.Default.ParseArguments(args, commandLineArgs))
            {
                var filesList = GetFilesPath(commandLineArgs.InputPhotosFolder);

                if (commandLineArgs.ReadOnly)
                    Debug($"Read Only Mode. No change will be saved.");

                using (IImageHandlerFactory imageHandlerFactory = new ImageHandler.NetImage.ImageHandlerFactory())
                {
                    using (IGeoProvider geoProvider = new GeoProvider.GoogleLocationHistoryTakeout.GeoProvider(commandLineArgs.LocationHistoryFile))
                    {
                        foreach (var filePath in filesList)
                        {
                            Debug($"Processing {filePath}");

                            using (var handler = imageHandlerFactory.Create(filePath))
                            {
                                var takenIn = handler.TakenIn;

                                if (takenIn.HasValue)
                                {
                                    Debug("\tTaken at: {0}", takenIn);

                                    var currentPhotoPosition = handler.TakenAt;

                                    if (currentPhotoPosition.HasValue)
                                        Debug("\tCurrent Photo Position: {0}", currentPhotoPosition.Value);
                                    else
                                        Debug("\tNo current position in Photo");
                                    
                                    if(currentPhotoPosition.HasValue && !commandLineArgs.Overwrite && !commandLineArgs.ReadOnly)
                                        Debug("\tSkiping because overwrite is not enabled.");
                                    else
                                    {
                                        var position = geoProvider.PositionIn(takenIn.Value);

                                        if (position.HasValue)
                                        {
                                            Debug("\tPosition at that time: {0}", position);

                                            if (!commandLineArgs.ReadOnly)
                                            {
                                                handler.TakenAt = position;
                                                handler.Save(BuildDestination(filePath, commandLineArgs));
                                            }

                                        }
                                        else
                                        {
                                            Debug("\tNo position available from Provider. Skiping photo.");
                                        }                                        
                                    }                                    
                                }
                                else
                                    Debug("\tNo \"Taken At\" date in picture. Skipping file...");
                            }
                        }
                    }
                }

                return 0;
            }
            else
            {
                return -1;
            }
        }

        static IEnumerable<string> GetFilesPath(string searchPath)
        {
            if (File.Exists(searchPath))
                return new[] { searchPath };
            else
                return Directory.EnumerateFiles(searchPath, "*.jpg")
                        .Concat(Directory.EnumerateFiles(searchPath, "*.jpeg"));
        }

        static string BuildDestination(string filePath, CommandLineOptions options)
        {
            var originalFolder = Path.GetDirectoryName(filePath);

            var fileName = options.DestinationFolder == null ?
                                Path.GetFileNameWithoutExtension(filePath) + $"_modified_{DateTime.Now.Ticks}" + Path.GetExtension(filePath) :
                                Path.GetFileName(filePath);

            return Path.Combine(options.DestinationFolder ?? originalFolder, fileName);
        }

        static void Debug(string message, params object[] args)
        {
            Console.WriteLine(String.Format(message, args));
        }
    }
}
