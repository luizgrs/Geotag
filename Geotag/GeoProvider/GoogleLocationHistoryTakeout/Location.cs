using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider.GoogleLocationHistoryTakeout
{
    class Location
    {
        private long _timestampMs;

        public long timestampMs {
            get { return _timestampMs; }
            set
            {
                _timestampMs = value;
                TimeStampUtc = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(timestampMs);
            }
        }
        public long latitudeE7 { get; set; }
        public long longitudeE7 { get; set; }

        public DateTime TimeStampUtc { get; private set; }
    }
}
