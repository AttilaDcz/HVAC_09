using System.Collections.Generic;

namespace HVACDesigner.Data.Models.Duct
{
    public class RectangularDuctSize
    {
        public int Width { get; set; }

        public int Height { get; set; }


        public override string ToString()
        {
            return $"{Width}x{Height}";
        }
    }
}