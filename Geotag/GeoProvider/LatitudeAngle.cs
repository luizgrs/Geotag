using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider
{
    class LatitudeAngle : Angle<LatitudeDirection>
    {
        public LatitudeAngle(decimal value, LatitudeDirection? direction = null) : base(value, direction)
        {
        }

        public LatitudeAngle(long value, byte precision, LatitudeDirection? direction = null) : base(value, precision, direction)
        {
        }

        public LatitudeAngle(uint degrees, uint minutes, decimal seconds, LatitudeDirection direction) : base(degrees, minutes, seconds, direction)
        {
        }

        protected override LatitudeDirection DirectionNegative => LatitudeDirection.South;

        protected override LatitudeDirection DirectionPositiveOrZero => LatitudeDirection.North;
    }
}
