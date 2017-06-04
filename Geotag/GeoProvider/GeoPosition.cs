using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider
{
    struct GeoPosition
    {
        public LatitudeAngle Latitude {get; private set; }
        public LongitudeAngle Longitude { get; private set; }
        
        public GeoPosition(LatitudeAngle latitude, LongitudeAngle longitude)
        {
            if (latitude == null)
                throw new NullReferenceException("latitude cannot be null");

            if (longitude == null)
                throw new NullReferenceException("longitude cannot be null");

            Latitude = latitude;
            Longitude = longitude;
        }

        public GeoPosition(long latitude, long longitude, byte precision)
            : this(new LatitudeAngle(latitude, precision), new LongitudeAngle(longitude, precision))
        {
            
        }

        public GeoPosition(decimal latitude, decimal longitude)
            : this(new LatitudeAngle(latitude), new LongitudeAngle(longitude))
        {

        }

        public override string ToString()
        {
            return $"{Latitude} {Longitude}";
        }
    }
}
