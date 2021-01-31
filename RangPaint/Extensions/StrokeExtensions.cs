using RangPaint.Misc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Ink;

namespace RangPaint.Extensions
{
    public static class StrokeExtensions
    {
        public static string SvgMarkup(this Stroke stroke)
        {
            StringBuilder result = new StringBuilder("<path d=\"");
            string styles = StrokeMisc.GetStyleSvgAttributes(stroke.DrawingAttributes);

            foreach (var point in stroke.StylusPoints)
            {

            }

            return "";
        }
    }
}
