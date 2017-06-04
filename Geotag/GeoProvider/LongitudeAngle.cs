using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider
{
    class LongitudeAngle : Angle<LongitudeDirection>
    {
        public LongitudeAngle(decimal value, LongitudeDirection? direction = null) : base(value, direction)
        {
        }

        public LongitudeAngle(long value, byte precision, LongitudeDirection? direction = null) : base(value, precision, direction)
        {
        }

        public LongitudeAngle(uint degrees, uint minutes, decimal seconds, LongitudeDirection direction) : base(degrees, minutes, seconds, direction)
        {
        }

        protected override LongitudeDirection DirectionNegative => LongitudeDirection.West;

        protected override LongitudeDirection DirectionPositiveOrZero => LongitudeDirection.East;
    }
}
