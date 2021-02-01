using System;
using System.Windows;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;

namespace SvgPaint.Model
{
    class EllipseStroke : StrokeBase
    {
        private Point center;
        private double radiusX;
        private double radiusY;

        public EllipseStroke(StylusPointCollection pts, DrawingAttributes da)
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
            Rect r = new Rect(
                new Point(StylusPoints[0].X, StylusPoints[0].Y),
                new Point(StylusPoints[1].X, StylusPoints[1].Y));

            center = new Point(
                 (r.Left + r.Right) / 2.0,
                 (r.Top + r.Bottom) / 2.0);

            radiusX = (r.Right - r.Left) / 2.0;
            radiusY = (r.Bottom - r.Top) / 2.0;

            BrushConverter bc = new BrushConverter();
            Brush gackground = (Brush)bc.ConvertFromString(drawingAttributes.GetPropertyData(DrawAttributesGuid.BackgroundColor).ToString());

            drawingContext.DrawEllipse(
                gackground,
                pen,
                center,
                radiusX,
                radiusY);
        }

        public override string GetSvgMarkup()
        {
            return $"<ellipse cx=\"{center.X}\" cy=\"{center.Y}\" rx=\"{radiusX}\" ry=\"{radiusY}\" {StyleSvgAttributes} />";
        }
    }
}
