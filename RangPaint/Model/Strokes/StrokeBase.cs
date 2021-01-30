using System;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace RangPaint.Model
{
    public class StrokeBase : Stroke
    {
        protected PathGeometry pathGeomery = null;
        protected DrawingAttributes drawingAttributes;

        protected string BackgroundColor
        {
            get
            {
                try
                {
                    return $"fill=\"{GetCorrectColorHex(drawingAttributes.GetPropertyData(DrawAttributesGuid.BackgroundColor).ToString())}\"";
                }
                catch
                {
                    return "";
                }
            }
        }
        protected string StrokeColor { get => GetCorrectColorHex(drawingAttributes.Color.ToString()); }
        protected double StrokeWidth { get => drawingAttributes.Width; }
        protected string StyleSvgAttributes { get => $"stroke=\"{StrokeColor}\" stroke-width=\"{StrokeWidth}\" {BackgroundColor}"; }

        public StrokeBase(StylusPointCollection pts, DrawingAttributes da)
            : base(pts, da)
        {
            this.StylusPoints = pts;
            this.DrawingAttributes = da;
        }

        public StrokeBase()
            : base(null, null)
        {

        }

        public bool IsSelected
        {
            get
            {
                PropertyInfo property = typeof(Stroke).GetProperty("IsSelected",
                   BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);
                return (bool)property.GetValue(this, null);
            }
        }

        /// <summary>
        /// Конвертирует цвет в 16-ричном формате в корректный для сохранения в svg.
        /// Меняет первые два символа после решётки и последние два символа местами.
        /// Нужен, т.к. в svg канал A цвета последний, а стандартный ToString у типа 
        /// System.Windows.Media.Color возвращает канал A в начале
        /// </summary>
        /// <param name="colorHex">Цвет в 16-ричном формате</param>
        protected string GetCorrectColorHex(string colorHex)
        {
            // #FFF0F8FF
            if (string.IsNullOrEmpty(colorHex) || colorHex?.Length != 9 || colorHex.FirstOrDefault() != '#')
            {
                throw new ArgumentException("Incorrect color", nameof(colorHex));
            }

            StringBuilder sb = new StringBuilder(colorHex);

            // нужно переместить первые два символа после решётки в конец
            for (int i = 1; i < 3; ++i)
            {
                char temp = sb[i];
                sb.Append(temp);
            }
            sb.Remove(1, 2);

            return sb.ToString();
        }

        public virtual string GetSvgMarkup() => "";
    }
}
