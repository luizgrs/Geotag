using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider
{
    interface IGeoProvider : IDisposable
    {
        GeoPosition? PositionIn(DateTimeOffset date);
    }
}
