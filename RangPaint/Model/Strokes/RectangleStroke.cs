using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SvgPaint.Model
{
    public class RectangleStroke : StrokeBase
    {
        public RectangleStroke(StylusPointCollection pts, DrawingAttributes da)
            : base(pts, da)
        {
            this.StylusPoints = pts;
            this.DrawingAttributes = da;
        }

        protected override void DrawCore(DrawingContext drawingContext, DrawingAttributes drawingAttributes)
        {
            if (drawingContext == null)
            {
                throw new ArgumentNullException("drawingContext");
            }
            if (null == drawingAttributes)
            {
                throw new ArgumentNullException("drawingAttributes");
            }

            this.drawingAttributes = drawingAttributes;

            Pen pen = new Pen
            {
                StartLineCap = PenLineCap.Round,
                EndLineCap = PenLineCap.Round,
                Brush = new SolidColorBrush(drawingAttributes.Color),
                Thickness = drawingAttributes.Width
            };

            BrushConverter bc = new BrushConverter();
            Brush BackGround = (Brush)bc.ConvertFromString(drawingAttributes.GetPropertyData(DrawAttributesGuid.BackgroundColor).ToString());

            drawingContext.DrawRectangle(
                BackGround,
                pen,
                new Rect(new Point(StylusPoints[0].X, StylusPoints[0].Y),
                    new Point(StylusPoints[1].X, StylusPoints[1].Y)));
        }

        public override string GetSvgMarkup()
        {
            double height = Math.Abs(StylusPoints[0].Y - StylusPoints[1].Y);
            double width = Math.Abs(StylusPoints[0].X - StylusPoints[1].X);

            double x = Math.Min(StylusPoints[0].X, StylusPoints[1].X);
            double y = Math.Min(StylusPoints[0].Y, StylusPoints[1].Y);

            return $"<rect x=\"{x}\" y=\"{y}\" width=\"{width}\" height=\"{height}\" {StyleSvgAttributes} />";
        }
    }
}
