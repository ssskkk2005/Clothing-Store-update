using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;

namespace ClothingStore
{
    /// <summary>Small shared helpers (double buffering, money formatting, GDI+ shapes).</summary>
    internal static class UiHelper
    {
        public static readonly CultureInfo US = CultureInfo.GetCultureInfo("en-US");

        /// <summary>Turns on DoubleBuffered (protected property) so custom drawing does not flicker.</summary>
        public static void EnableDoubleBuffer(Control c)
        {
            typeof(Control).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(c, true, null);
        }

        public static string Money(decimal v)
        {
            return "$" + v.ToString(v % 1 == 0 ? "#,0" : "#,0.00", US);
        }

        public static bool TryParseMoney(string text, out decimal value)
        {
            string clean = (text ?? "").Replace("$", "").Replace(",", "").Trim();
            return decimal.TryParse(clean, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        public static GraphicsPath RoundedRect(Rectangle r, int radius)
        {
            int d = radius * 2;
            var path = new GraphicsPath();
            path.AddArc(r.X, r.Y, d, d, 180, 90);
            path.AddArc(r.Right - d, r.Y, d, d, 270, 90);
            path.AddArc(r.Right - d, r.Bottom - d, d, d, 0, 90);
            path.AddArc(r.X, r.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        /// <summary>Draws a simple T-shirt icon (used as logo / placeholder).</summary>
        public static void DrawShirt(Graphics g, float cx, float top, float scale, Color c1, Color c2)
        {
            PointF[] pts =
            {
                new PointF(cx - 10 * scale, top),
                new PointF(cx - 24 * scale, top + 6 * scale),
                new PointF(cx - 34 * scale, top + 20 * scale),
                new PointF(cx - 24 * scale, top + 26 * scale),
                new PointF(cx - 18 * scale, top + 20 * scale),
                new PointF(cx - 18 * scale, top + 44 * scale),
                new PointF(cx + 18 * scale, top + 44 * scale),
                new PointF(cx + 18 * scale, top + 20 * scale),
                new PointF(cx + 24 * scale, top + 26 * scale),
                new PointF(cx + 34 * scale, top + 20 * scale),
                new PointF(cx + 24 * scale, top + 6 * scale),
                new PointF(cx + 10 * scale, top),
                new PointF(cx, top + 9 * scale)
            };
            var bounds = new RectangleF(cx - 34 * scale, top, 68 * scale, 44 * scale);
            using (var brush = new LinearGradientBrush(bounds, c1, c2, 60f))
                g.FillPolygon(brush, pts);
            using (var pen = new Pen(Color.White, Math.Max(1.5f, 2f * scale)))
                g.DrawArc(pen, cx - 10 * scale, top - 3 * scale, 20 * scale, 18 * scale, 0, 180);
        }
    }
}
