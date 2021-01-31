using RangPaint.Model;
using System;
using System.Linq;
using System.Text;
using System.Windows.Ink;

namespace RangPaint.Misc
{
    public static class StrokeMisc
    {
        /// <summary>
        /// Конвертирует цвет в 16-ричном формате в корректный для сохранения в svg.
        /// Меняет первые два символа после решётки и последние два символа местами.
        /// Нужен, т.к. в svg канал A цвета последний, а стандартный ToString у типа 
        /// System.Windows.Media.Color возвращает канал A в начале
        /// </summary>
        /// <param name="colorHex">Цвет в 16-ричном формате</param>
        private static string GetCorrectColorHex(string colorHex)
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

        public static string GetStyleSvgAttributes(DrawingAttributes drawingAttributes)
        {
            string backgroundColor = "";
            try
            {
                backgroundColor = $"fill=\"{GetCorrectColorHex(drawingAttributes.GetPropertyData(DrawAttributesGuid.BackgroundColor).ToString())}\"";
            }
            catch
            { }

            string strokeColor = GetCorrectColorHex(drawingAttributes.Color.ToString());
            double strokeWidth = drawingAttributes.Width;
            string styleSvgAttributes = $"stroke=\"{strokeColor}\" stroke-width=\"{strokeWidth}\" {backgroundColor}";

            return styleSvgAttributes;
        }
    }
}
