using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using System.Windows.Shapes;

namespace SvgPaint.Model
{
    class TriangleStroke : StrokeBase
    {
        private string geometeryPath = "";

        public TriangleStroke(StylusPointCollection pts, DrawingAttributes da)
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
            GeometryConverter gc = new GeometryConverter();
            geometeryPath = string.Format("M{0},{1} {2},{3} {4},{5}Z", StylusPoints[0].X, StylusPoints[1].Y, (Math.Abs(StylusPoints[1].X - StylusPoints[0].X)) / 2 + StylusPoints[0].X, StylusPoints[0].Y, StylusPoints[1].X, StylusPoints[1].Y);
            Geometry geometry = (Geometry)gc.ConvertFromString(geometeryPath);
            GeometryDrawing gd = new GeometryDrawing(BackGround, pen, geometry);

            drawingContext.DrawDrawing(gd);
        }

        public override string GetSvgMarkup()
        {
            if (string.IsNullOrEmpty(geometeryPath))
            {
                return "";
            }

            return $"<path d=\"{geometeryPath}\" {StyleSvgAttributes} />";
        }
    }
}
