using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider.GoogleLocationHistoryTakeout
{
    class DateTimeRangeKey
    {
        public DateTime Start { get; private set; }
        public DateTime End { get; private set; }

        public DateTimeRangeKey(DateTime startAndEnd) : this(startAndEnd, startAndEnd)
        {
        }

        public DateTimeRangeKey(DateTime start, DateTime end)
        {
            Start = start;
            End = end;
        }
    }
}
