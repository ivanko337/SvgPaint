using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Ink;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows;
using System.Windows.Shapes;
using System.Text;

namespace SvgPaint.Model
{
    public class ArrowLineStroke : StrokeBase
    {
        private List<Tuple<Point, Point>> lines;

        public ArrowLineStroke(StylusPointCollection pts, DrawingAttributes da)
            : base(pts, da)
        {
            this.StylusPoints = pts;
            this.DrawingAttributes = da;

            lines = new List<Tuple<Point, Point>>();
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

            drawingContext.DrawLine(pen,
                new Point(StylusPoints[0].X, StylusPoints[0].Y),
                new Point(StylusPoints[1].X, StylusPoints[1].Y));

            double headWidth = drawingAttributes.Width * 2;
            double headHeight = drawingAttributes.Width * 2;
            double theta = Math.Atan2(StylusPoints[0].Y - StylusPoints[1].Y, StylusPoints[0].X - StylusPoints[1].X);
            double sint = Math.Sin(theta);
            double cost = Math.Cos(theta);

            Point pt1 = new Point(
                StylusPoints[1].X + (headWidth * cost - headHeight * sint),
                StylusPoints[1].Y + (headWidth * sint + headHeight * cost));

            drawingContext.DrawLine(pen,
                new Point(StylusPoints[1].X, StylusPoints[1].Y),
                new Point(pt1.X, pt1.Y));

            Point pt2 = new Point(
                StylusPoints[1].X + (headWidth * cost + headHeight * sint),
                StylusPoints[1].Y - (headHeight * cost - headWidth * sint));

            drawingContext.DrawLine(pen,
                new Point(StylusPoints[1].X, StylusPoints[1].Y),
                new Point(pt2.X, pt2.Y));

            lines.Add(new Tuple<Point, Point>(new Point(StylusPoints[0].X, StylusPoints[0].Y),
                                              new Point(StylusPoints[1].X, StylusPoints[1].Y)));
            lines.Add(new Tuple<Point, Point>(new Point(StylusPoints[1].X, StylusPoints[1].Y), pt1));
            lines.Add(new Tuple<Point, Point>(new Point(StylusPoints[1].X, StylusPoints[1].Y), pt2));
        }

        public override string GetSvgMarkup()
        {
            StringBuilder builder = new StringBuilder("<g fill=\"none\">");

            foreach (var line in lines)
            {
                builder.Append($"<line x1=\"{line.Item1.X}\" y1=\"{line.Item1.Y}\" x2=\"{line.Item2.X}\" y2=\"{line.Item2.Y}\"" +
                    $" {StyleSvgAttributes} />");
            }

            builder.Append("</g>");

            return builder.ToString();
        }
    }
}
