using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.ImageHandler.Exif
{
    struct ExifRational
    {
        public long Numerator { get; set; }
        public long Denominator { get; set; }
        public double Result
        {
            get
            {
                return Numerator / (double)Denominator;
            }
        }

        public override string ToString()
        {
            return $"{Numerator} / {Denominator}";
        }

        public ExifRational(byte[] value, int posStart = 0)
        {
            Numerator = BitConverter.ToInt32(value, posStart);
            Denominator = BitConverter.ToInt32(value, posStart + 4);
        }
    }
}
