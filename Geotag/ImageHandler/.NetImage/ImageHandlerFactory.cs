using Geotag.ImageHandler.Exif;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Geotag.GeoProvider;

namespace Geotag.ImageHandler.NetImage
{
    class ImageHandlerFactory : IImageHandlerFactory
    {
        private class ImageHandler : IImageHandler
        {
            private readonly Image _image;
            private readonly string _imagePath;
            private readonly HashSet<int> _properties;
            private readonly short? _defaultTimeZoneOffset;

            public ImageHandler(string imagePath, short? defaultTimeZoneOffset = null)
            {
                _imagePath = imagePath;
                _image = Image.FromFile(imagePath);
                _properties = new HashSet<int>(_image.PropertyIdList);
                _defaultTimeZoneOffset = defaultTimeZoneOffset;
            }

            public DateTimeOffset? TakenIn
            {
                get
                {
                    var date = GetExifDateTime(ExifProperties.Image_DateTimeOriginal);

                    if (date.HasValue)
                        return ApplyTimeZone(date.Value);

                    return date;
                }
            }

            public GeoPosition? TakenAt
            {
                get
                {
                    var latitude = Latitude;
                    var longitude = Longitude;

                    if (latitude != null && longitude != null)
                        return new GeoPosition(latitude, longitude);

                    return null;
                }

                set
                {
                    Latitude = value?.Latitude;
                    Longitude = value?.Longitude;
                }
            }

            public void Dispose()
            {
                _properties.Clear();
                _image.Dispose();
            }

            public void Save(string destinationPath)
            {
                _image.Save(destinationPath);
            }
            
            #region Properties Handling
            private LatitudeAngle Latitude
            {
                get
                {
                    var value = GetAngle(ExifProperties.GPSInfo_GPSLatitude);
                    var reference = GetExifString(ExifProperties.GPSInfo_GPSLatitudeRef)?[0];

                    if (value != null && (reference == 'N' || reference == 'S'))
                        return new LatitudeAngle(value.Item1, value.Item2, value.Item3, reference == 'N' ? LatitudeDirection.North : LatitudeDirection.South);

                    return null;
                }

                set
                {
                    SavePosition(ExifProperties.GPSInfo_GPSLatitude, ExifProperties.GPSInfo_GPSLatitudeRef, value);                    
                }
            }

            private LongitudeAngle Longitude
            {
                get
                {
                    var value = GetAngle(ExifProperties.GPSInfo_GPSLongitude);
                    var reference = GetExifString(ExifProperties.GPSInfo_GPSLongitudeRef)?[0];

                    if (value != null && (reference == 'E' || reference == 'W'))
                        return new LongitudeAngle(value.Item1, value.Item2, value.Item3, reference == 'E' ? LongitudeDirection.East : LongitudeDirection.West);

                    return null;
                }
                set
                {
                    SavePosition(ExifProperties.GPSInfo_GPSLongitude, ExifProperties.GPSInfo_GPSLongitudeRef, value);
                }
            }

            private void SavePosition<T>(ExifProperties positionProperty, ExifProperties referenceProperty, Angle<T> value)
                where T : struct
            {
                if (value == null)
                    RemoveProperty(positionProperty, referenceProperty);
                else
                {
                    var positionProp = GetProperty(positionProperty) ?? CreatePropertyItem(positionProperty, ExifValueTypes.Rational);
                    var referenceProp = GetProperty(referenceProperty) ?? CreatePropertyItem(referenceProperty, ExifValueTypes.ASCII);

                    positionProp.Value = GetByteValue(value);
                    referenceProp.Value = Encoding.ASCII.GetBytes(value.Direction.ToString()[0] + "\0");

                    positionProp.Len = positionProp.Value.Length;
                    referenceProp.Len = referenceProp.Value.Length;

                    _image.SetPropertyItem(positionProp);
                    _image.SetPropertyItem(referenceProp);

                    if (!_properties.Contains((int)positionProperty))
                        _properties.Add((int)positionProperty);

                    if (!_properties.Contains((int)referenceProperty))
                        _properties.Add((int)referenceProperty);
                }
            }

            private PropertyItem CreatePropertyItem(ExifProperties property, ExifValueTypes type)
            {
                var clone = GetProperty((ExifProperties)_properties.First());

                clone.Id = (int)property;
                clone.Value = null;
                clone.Len = 0;
                clone.Type = (short)type;

                return clone;
            }

            private byte[] GetByteValue<T>(Angle<T> angle)
                where T : struct
            {

                var nums = new uint[]
                {
                    angle.Degrees,
                    1,
                    angle.Minutes,
                    1,
                    (uint)Math.Truncate(angle.Seconds * 100000),
                    100000
                };

                return nums
                        .Select(BitConverter.GetBytes)
                        .SelectMany(b => b)
                        .ToArray();
            }

            private void RemoveProperty(params ExifProperties[] properties)
            {
                foreach (var property in properties)
                {
                    if (HasProperty(property))
                    {
                        _image.RemovePropertyItem((int)property);
                        _properties.Remove((int)property);
                    }
                }
            }

            private Tuple<uint, uint, decimal> GetAngle(ExifProperties property)
            {
                var prop = GetProperty(property);

                if (prop == null)
                    return null;

                var fractions = new[]
                {
                    GetExifStructure<ExifURational>(prop),
                    GetExifStructure<ExifURational>(prop, 8),
                    GetExifStructure<ExifURational>(prop, 16)
                };

                uint finalDegrees, finalMinutes;
                decimal minutes = fractions[1]?.Result ?? 0, seconds =  fractions[2]?.Result ?? 0;

                if (!fractions[0].HasValue)
                    finalDegrees = 0;
                else if (fractions[0].Value.Denominator == 1)
                    finalDegrees = fractions[0].Value.Numerator;
                else
                {
                    finalDegrees = (uint)Math.Truncate(fractions[0].Value.Result);
                    minutes += (fractions[0].Value.Result - finalDegrees) * 60;
                }

                finalMinutes = (uint)Math.Truncate(minutes);
                seconds += (minutes - finalMinutes) * 60;

                return new Tuple<uint, uint, decimal>(finalDegrees, finalMinutes, seconds);
            }

            private object GetExifValue(PropertyItem prop, int startBytePosition = 0)
            {
                switch (prop.Type)
                {
                    case 0:
                    case 8:
                        return null;

                    case (short)ExifValueTypes.Byte:
                    case (short)ExifValueTypes.Undefined:
                        return prop.Value[startBytePosition];

                    case (short)ExifValueTypes.ASCII:
                        return Encoding.ASCII.GetString(prop.Value);

                    case (short)ExifValueTypes.Short:
                        return BitConverter.ToUInt16(prop.Value, startBytePosition);

                    case (short)ExifValueTypes.Long:
                        return BitConverter.ToUInt32(prop.Value, startBytePosition);

                    case (short)ExifValueTypes.Rational:
                        return new ExifURational(prop.Value, startBytePosition);

                    case (short)ExifValueTypes.Int:
                        return BitConverter.ToInt32(prop.Value, startBytePosition);

                    case (short)ExifValueTypes.SignedRational:
                        return new ExifRational(prop.Value, startBytePosition);

                    default:
                        return prop.Value;
                }

            }

            private object GetExifValue(ExifProperties property, int startBytePosition = 0)
            {
                var prop = GetProperty(property);

                if (prop == null)
                    return null;

                return GetExifValue(prop);
            }

            private bool HasProperty(ExifProperties property)
            {
                return _properties.Contains((int)property);
            }

            private PropertyItem GetProperty(ExifProperties property)
            {
                if (HasProperty(property))
                    return _image.GetPropertyItem((int)property);

                return null;
            }

            private string GetExifString(ExifProperties property)
            {
                return GetExifValue(property) as string;
            }

            private T? GetExifStructure<T>(ExifProperties property, int startBytePosition = 0)
                where T : struct
            {
                var value = GetExifValue(property, startBytePosition);

                if (value == null)
                    return null;

                return new T?((T)value);
            }

            private T? GetExifStructure<T>(PropertyItem property, int startBytePosition = 0)
                where T : struct
            {
                var value = GetExifValue(property, startBytePosition);

                if (value == null)
                    return null;

                return new T?((T)value);
            }

            private DateTimeOffset? GetExifDateTime(ExifProperties property)
            {
                var dateString = GetExifString(property);

                if (dateString == null)
                    return null;

                DateTimeOffset date;

                if (!DateTimeOffset.TryParseExact(dateString, "yyyy:MM:dd HH:mm:ss\0", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out date))
                    throw new InvalidOperationException($"Unknown date format: {dateString}");

                return date;
            }
            #endregion

            private DateTimeOffset ApplyTimeZone(DateTimeOffset date)
            {
                var timezone = GetExifStructure<Int16>(ExifProperties.Image_TimeZoneOffset);

                if (!timezone.HasValue)
                    timezone = _defaultTimeZoneOffset;

                if (!timezone.HasValue)
                    return date;

                return new DateTimeOffset(date.DateTime, new TimeSpan(0, _defaultTimeZoneOffset.Value, 0));
            }

        }

        public IImageHandler Create(string imagePath)
        {
            return new ImageHandler(imagePath);
        }

        public void Dispose()
        {
            
        }
    }
}
