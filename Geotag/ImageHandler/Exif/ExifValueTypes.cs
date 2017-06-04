using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Geotag.ImageHandler.Exif
{
    enum ExifValueTypes : short
    {
        Byte = 1,
        Undefined = 7,
        ASCII = 2,
        Short = 3,
        Long = 4,
        Rational = 5,
        Int = 9,
        SignedRational = 10
    }
}
