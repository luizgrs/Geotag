using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.ImageHandler.Exif
{
    struct ExifURational
    {
        public uint Numerator { get; set; }
        public uint Denominator { get; set; }
        public decimal Result
        {
            get
            {
                return Numerator / (decimal)Denominator;
            }
        }

        public override string ToString()
        {
            return $"{Numerator} / {Denominator}";
        }

        public ExifURational(byte[] value, int posStart = 0)
        {
            Numerator = BitConverter.ToUInt32(value, posStart);
            Denominator = BitConverter.ToUInt32(value, posStart+4);
        }
    }
}
