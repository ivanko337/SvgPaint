using RangPaint.Misc;
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

        protected string StyleSvgAttributes { get => StrokeMisc.GetStyleSvgAttributes(drawingAttributes); }

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

        public virtual string GetSvgMarkup() => "";
    }
}
