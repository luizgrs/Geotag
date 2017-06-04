using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.GeoProvider
{
    abstract class Angle<TDirection>
        where TDirection : struct
    {
        public uint Degrees { get; private set; }
        public uint Minutes { get; private set; }
        public decimal Seconds { get; private set; }
        public TDirection Direction { get; private set; }

        protected abstract TDirection DirectionPositiveOrZero { get; }

        protected abstract TDirection DirectionNegative { get; }

        private TDirection DirectionFromSign(int sign)
        {
            return sign == -1 ? DirectionNegative : DirectionPositiveOrZero;
        }

        public Angle(long value, byte precision, TDirection? direction = null)
            : this(value / (decimal)(Math.Pow(10, precision)), direction)
        {

        }

        public Angle(decimal value, TDirection? direction = null)
        {
            Direction = direction ?? DirectionFromSign(Math.Sign(value));
            value = Math.Abs(value);
            Degrees = (uint)Math.Truncate(value);

            var result = (value - Degrees) * 60;
            Minutes = (uint)Math.Truncate(result);

            Seconds = (result - Minutes) * 60;
        }

        public Angle(uint degrees, uint minutes, decimal seconds, TDirection direction)
        {
            if (seconds < 0)
                throw new InvalidOperationException("seconds cannot be less than 0. Use direction parameter");

            Degrees = degrees;
            Minutes = minutes;
            Seconds = seconds;
            Direction = direction;
        }

        public long ToLong(byte precision)
        {
            return (long)Math.Truncate(ToDecimal() * (long)Math.Pow(10, precision));
        }

        public decimal ToDecimal()
        {
            var value = Degrees + (decimal)Minutes / 60 + Seconds / 3600;
            if (Direction.Equals(DirectionNegative))
                value *= -1;

            return value;
        }

        public override string ToString()
        {
            return $"{Degrees}°{Minutes}'{Seconds}''{Direction.ToString()[0]}";
        }
    }
}