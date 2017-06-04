using Geotag.GeoProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.ImageHandler
{
    interface IImageHandler : IDisposable
    {
        DateTimeOffset? TakenIn { get; }

        GeoPosition? TakenAt { get; set; }

        void Save(string destinationPath);
    }
}
