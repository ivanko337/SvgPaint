using SvgPaint.Misc;
using System.Text;
using System.Windows.Ink;

namespace SvgPaint.Extensions
{
    public static class StrokeExtensions
    {
        public static string SvgMarkup(this Stroke stroke)
        {
            StringBuilder result = new StringBuilder("<path d=\"M");
            string styles = StrokeMisc.GetStyleSvgAttributes(stroke.DrawingAttributes, false);

            foreach (var point in stroke.StylusPoints)
            {
                result.Append($"{point.X} {point.Y} ");
            }

            result.Append($"\" {styles} />");

            return result.ToString();
        }
    }
}
