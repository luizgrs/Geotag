
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider.GoogleLocationHistoryTakeout
{
    class GeoProvider : IGeoProvider
    {
        private readonly JsonTextReader reader;
        private readonly StreamReader streamReader;
        private bool canRead = true;
        private DateTime lastTimestampRead = DateTime.MaxValue;
        private readonly JsonSerializer jsonSerializer;
        private SortedList<DateTimeRangeKey, Location> locations;

        public GeoProvider(string takeoutFilename)
        {
            streamReader = new StreamReader(takeoutFilename);
            reader = new JsonTextReader(streamReader);
            reader.CloseInput = true;
            jsonSerializer = JsonSerializer.Create();
            locations = new SortedList<DateTimeRangeKey, Location>(new DateTimeRangeKeyComparer());
            ReadUntilLocationArray();
        }

        public void Dispose()
        {
            reader.Close();
            streamReader.Dispose();
        }

        public GeoPosition? PositionIn(DateTimeOffset date)
        {
            var location = FindLocation(date);

            if (location == null)
                return null;

            return new GeoPosition(location.latitudeE7, location.longitudeE7, 7);
        }

        private Location FindLocation(DateTimeOffset pictureDate)
        {
            Location location;

            if (lastTimestampRead > pictureDate.UtcDateTime)
            {
                while (canRead)
                {
                    if (!canRead)
                        return null;

                    if (!TryReadLocation(out location))
                        return null;

                    var key = new DateTimeRangeKey(location.TimeStampUtc, lastTimestampRead.AddTicks(-1));

                    lastTimestampRead = location.TimeStampUtc;

                    locations.Add(key, location);

                    if (location.TimeStampUtc <= pictureDate.UtcDateTime)
                        return location;
                }
            }
            else if (locations.TryGetValue(new DateTimeRangeKey(pictureDate.UtcDateTime), out location))
                return location;

            return null;
        }

        private bool TryReadLocation(out Location location)
        {
            location = null;

            if (!canRead)
                return false;

            if (reader.TokenType == JsonToken.EndArray) {
                canRead = false;
                return false;
            }

            location = jsonSerializer.Deserialize<Location>(reader);
            reader.Read();
            return true;
        }


        private void ReadUntilLocationArray()
        {
            while (reader.Read())
            {
                if(reader.TokenType == JsonToken.PropertyName && reader.Value.Equals("locations"))
                {
                    if(!reader.Read() || reader.TokenType != JsonToken.StartArray
                        || !reader.Read() || reader.TokenType != JsonToken.StartObject)
                        canRead = false;

                    return;
                }
            }

            canRead = false;
        }
    }
}
