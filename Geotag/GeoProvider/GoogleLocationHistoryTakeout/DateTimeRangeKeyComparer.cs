using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider.GoogleLocationHistoryTakeout
{
    class DateTimeRangeKeyComparer : IComparer<DateTimeRangeKey>
    {
        public int Compare(DateTimeRangeKey x, DateTimeRangeKey y)
        {
            if (x.End < y.Start)
                return -1;
            else if (x.Start > y.End)
                return 1;
            else
                return 0;
        }
    }
}
