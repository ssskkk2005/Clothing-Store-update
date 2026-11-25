using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ClothingStore
{
    public partial class ucDashboard : UserControl
    {
        private readonly Panel _pnlDonut = new Panel();
        private Panel[] _cards;

        private static readonly Color[] Accents =
        {
            Color.FromArgb(37, 99, 235),   // blue
            Color.FromArgb(16, 185, 129),  // green
            Color.FromArgb(245, 158, 11),  // amber
            Color.FromArgb(239, 68, 68)    // red
        };

        // Tiny trend lines drawn inside each KPI card.
        private static readonly float[][] Sparks =
        {
            new float[] { 3, 5, 4, 7, 6, 9, 8 },
            new float[] { 2, 3, 3, 5, 4, 6, 7 },
            new float[] { 6, 5, 6, 4, 5, 3, 4 },
            new float[] { 2, 2, 3, 3, 4, 3, 5 }
        };

        // Stock distribution used by the donut chart.
        private static readonly (string Name, int Value, Color Color)[] StockByCategory =
        {
            ("Men", 45, Color.FromArgb(37, 99, 235)),
            ("Women", 30, Color.FromArgb(236, 72, 153)),
            ("Kids", 15, Color.FromArgb(245, 158, 11)),
            ("Accessories", 10, Color.FromArgb(16, 185, 129))
        };

        public ucDashboard()
        {
            InitializeComponent();
            dgvRecent.Rows.Add("#1042", "Sara A.", "$85", "Nov 04");
            dgvRecent.Rows.Add("#1041", "Omar K.", "$120", "Nov 04");
            dgvRecent.Rows.Add("#1040", "Walk-in", "$42", "Nov 03");
            dgvLow.Rows.Add("Black hoodie M", "2");
            dgvLow.Rows.Add("Slim jeans 32", "4");
            dgvLow.Rows.Add("Floral dress S", "5");

            SetupCards();
            SetupDonut();

            // Event handling: clicking a recent sale shows its details.
            dgvRecent.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var r = dgvRecent.Rows[e.RowIndex];
                MessageBox.Show($"Order {r.Cells[0].Value}\nCustomer: {r.Cells[1].Value}\nTotal: {r.Cells[2].Value}\nDate: {r.Cells[3].Value}",
                    "Order details", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
        }

        // ---------- KPI cards (GDI+) ----------
        private void SetupCards()
        {
            _cards = new[] { pnlCard1, pnlCard2, pnlCard3, pnlCard4 };
            foreach (var card in _cards)
            {
                UiHelper.EnableDoubleBuffer(card);
                card.Paint += Card_Paint;
                foreach (Control c in card.Controls)
                    if (c is Label l) l.BackColor = Color.Transparent;   // let the GDI+ drawing show through
            }
        }

        private void Card_Paint(object sender, PaintEventArgs e)
        {
            var card = (Panel)sender;
            int idx = Array.IndexOf(_cards, card);
            if (idx < 0) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            // accent strip on the left
            using (var brush = new SolidBrush(Accents[idx]))
                g.FillRectangle(brush, 0, 0, 5, card.Height);

            // sparkline in the bottom-right corner
            float[] data = Sparks[idx];
            var area = new RectangleF(card.Width - 92, card.Height - 36, 80, 24);
            float max = data.Max(), min = data.Min();
            float range = Math.Max(1f, max - min);
            var pts = new PointF[data.Length];
            for (int i = 0; i < data.Length; i++)
            {
                float x = area.Left + i * area.Width / (data.Length - 1);
                float y = area.Bottom - (data[i] - min) / range * area.Height;
                pts[i] = new PointF(x, y);
            }
            using (var pen = new Pen(Accents[idx], 2f) { LineJoin = LineJoin.Round })
                g.DrawLines(pen, pts);
            using (var brush = new SolidBrush(Accents[idx]))
                g.FillEllipse(brush, pts[pts.Length - 1].X - 3, pts[pts.Length - 1].Y - 3, 6, 6);
        }

        // ---------- Donut chart (GDI+) ----------
        private void SetupDonut()
        {
            dgvLow.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            dgvLow.Height = 170;

            _pnlDonut.Location = new Point(dgvLow.Left, dgvLow.Bottom + 12);
            _pnlDonut.Size = new Size(dgvLow.Width, 210);
            _pnlDonut.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            _pnlDonut.BackColor = Color.FromArgb(243, 246, 250);
            UiHelper.EnableDoubleBuffer(_pnlDonut);
            _pnlDonut.Paint += Donut_Paint;
            Controls.Add(_pnlDonut);
        }

        private void Donut_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            using (var titleFont = new Font("Segoe UI", 10f, FontStyle.Bold))
            using (var textFont = new Font("Segoe UI", 9f))
            using (var bigFont = new Font("Segoe UI", 14f, FontStyle.Bold))
            using (var dark = new SolidBrush(Color.FromArgb(30, 64, 175)))
            using (var grey = new SolidBrush(Color.FromArgb(75, 85, 99)))
            {
                g.DrawString("Stock by category", titleFont, dark, 12, 8);

                const int thickness = 26;
                var ring = new Rectangle(28, 48, 104, 104);   // arc is centred on the pen line
                int total = StockByCategory.Sum(s => s.Value);
                float start = -90f;

                foreach (var slice in StockByCategory)
                {
                    float sweep = slice.Value * 360f / total;
                    using (var pen = new Pen(slice.Color, thickness))
                        g.DrawArc(pen, ring, start, Math.Max(1f, sweep - 1.5f));
                    start += sweep;
                }

                var centre = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.DrawString("100%", bigFont, dark, new RectangleF(ring.X, ring.Y, ring.Width, ring.Height), centre);

                // legend
                int y = 52;
                foreach (var slice in StockByCategory)
                {
                    using (var b = new SolidBrush(slice.Color))
                        g.FillRectangle(b, 160, y + 3, 12, 12);
                    g.DrawString($"{slice.Name}  {slice.Value * 100 / total}%", textFont, grey, 178, y);
                    y += 26;
                }
            }
        }
    }
}
