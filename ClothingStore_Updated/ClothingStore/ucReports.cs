using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Text;

namespace ClothingStore
{
    public partial class ucReports : UserControl
    {
        private record SaleDay(DateTime Date, int Orders, int Items, decimal Revenue);

        private readonly List<SaleDay> _sales = new List<SaleDay>();
        private readonly List<(string Label, double Value)> _chartData = new List<(string, double)>();
        private string _chartTitle = "";

        public ucReports()
        {
            InitializeComponent();

            int y = DateTime.Today.Year;
            _sales.Add(new SaleDay(new DateTime(y, 11, 2), 19, 34, 1180));
            _sales.Add(new SaleDay(new DateTime(y, 11, 3), 21, 40, 1410));
            _sales.Add(new SaleDay(new DateTime(y, 11, 4), 18, 31, 1240));
            _sales.Add(new SaleDay(new DateTime(y, 11, 5), 24, 45, 1620));
            _sales.Add(new SaleDay(new DateTime(y, 11, 6), 20, 36, 1310));
            _sales.Add(new SaleDay(new DateTime(y, 11, 7), 27, 52, 1890));
            _sales.Add(new SaleDay(new DateTime(y, 11, 8), 23, 41, 1540));

            dtpFrom.Value = new DateTime(y, 11, 2);
            dtpTo.Value = new DateTime(y, 11, 8);
            dgvReport.AllowUserToAddRows = false;
            dgvReport.ReadOnly = true;

            // GDI+ chart surface
            lblChartHint.Visible = false;
            UiHelper.EnableDoubleBuffer(pnlChart);
            pnlChart.Paint += PnlChart_Paint;
            pnlChart.Resize += (s, e) => pnlChart.Invalidate();

            // Event handling
            btnGenerate.Click += (s, e) => GenerateReport();
            cmbReport.SelectedIndexChanged += (s, e) => GenerateReport();
            btnExport.Click += BtnExport_Click;

            GenerateReport();
        }

        // ---------------- data ----------------
        private void GenerateReport()
        {
            bool isSales = cmbReport.SelectedIndex == 0;
            dtpFrom.Enabled = dtpTo.Enabled = isSales;

            if (isSales && dtpFrom.Value.Date > dtpTo.Value.Date)
            {
                MessageBox.Show("The 'From' date must be before the 'To' date.", "Invalid range",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            dgvReport.Rows.Clear();
            _chartData.Clear();

            switch (cmbReport.SelectedIndex)
            {
                case 0:
                    SetHeaders("Date", "Orders", "Items sold", "Revenue");
                    foreach (var d in _sales.Where(x => x.Date.Date >= dtpFrom.Value.Date && x.Date.Date <= dtpTo.Value.Date))
                        dgvReport.Rows.Add(d.Date.ToString("MMM dd", CultureInfo.InvariantCulture), d.Orders, d.Items, UiHelper.Money(d.Revenue));
                    _chartTitle = $"Daily revenue ({dtpFrom.Value:MMM dd} - {dtpTo.Value:MMM dd})";
                    break;
                case 1:
                    SetHeaders("Category", "Products", "Items in stock", "Stock value");
                    dgvReport.Rows.Add("Men", 12, 310, "$13,950");
                    dgvReport.Rows.Add("Women", 9, 240, "$14,400");
                    dgvReport.Rows.Add("Kids", 7, 180, "$2,700");
                    dgvReport.Rows.Add("Accessories", 5, 95, "$1,900");
                    _chartTitle = "Inventory value by category";
                    break;
                default:
                    SetHeaders("Product", "Orders", "Items sold", "Revenue");
                    dgvReport.Rows.Add("Black hoodie", 48, 52, "$2,340");
                    dgvReport.Rows.Add("Slim jeans", 41, 44, "$2,420");
                    dgvReport.Rows.Add("Floral dress", 33, 35, "$2,100");
                    dgvReport.Rows.Add("Jacket", 18, 19, "$1,710");
                    dgvReport.Rows.Add("Kids tee", 40, 58, "$870");
                    _chartTitle = "Top products by revenue";
                    break;
            }

            foreach (DataGridViewRow row in dgvReport.Rows)
            {
                string label = Convert.ToString(row.Cells[0].Value);
                if (UiHelper.TryParseMoney(Convert.ToString(row.Cells[3].Value), out decimal v))
                    _chartData.Add((label, (double)v));
            }
            pnlChart.Invalidate();
        }

        private void SetHeaders(string a, string b, string c, string d)
        {
            dgvReport.Columns[0].HeaderText = a;
            dgvReport.Columns[1].HeaderText = b;
            dgvReport.Columns[2].HeaderText = c;
            dgvReport.Columns[3].HeaderText = d;
        }

        // ---------------- export ----------------
        private void BtnExport_Click(object sender, EventArgs e)
        {
            if (dgvReport.Rows.Count == 0)
            {
                MessageBox.Show("There is nothing to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            using (var dlg = new SaveFileDialog { Filter = "CSV file (*.csv)|*.csv", FileName = "report.csv" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", dgvReport.Columns.Cast<DataGridViewColumn>().Select(c => Quote(c.HeaderText))));
                foreach (DataGridViewRow row in dgvReport.Rows)
                    sb.AppendLine(string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => Quote(Convert.ToString(c.Value)))));
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Report exported successfully.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static string Quote(string s) => "\"" + (s ?? "").Replace("\"", "\"\"") + "\"";

        // ---------------- GDI+ chart ----------------
        private static double NiceMax(double v)
        {
            if (v <= 0) return 1;
            double exp = Math.Pow(10, Math.Floor(Math.Log10(v)));
            double f = v / exp;
            double nf = f <= 1 ? 1 : f <= 2 ? 2 : f <= 5 ? 5 : 10;
            return nf * exp;
        }

        private void PnlChart_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;

            Rectangle r = pnlChart.ClientRectangle;
            if (r.Width < 100 || r.Height < 100) return;

            using (var bg = new LinearGradientBrush(r, Color.White, Color.FromArgb(243, 246, 250), 90f))
                g.FillRectangle(bg, r);

            using (var titleFont = new Font("Segoe UI", 11f, FontStyle.Bold))
            using (var textFont = new Font("Segoe UI", 8.5f))
            using (var titleBrush = new SolidBrush(Color.FromArgb(30, 64, 175)))
            using (var textBrush = new SolidBrush(Color.FromArgb(75, 85, 99)))
            using (var gridPen = new Pen(Color.FromArgb(229, 231, 235)))
            using (var axisPen = new Pen(Color.FromArgb(156, 163, 175), 1.5f))
            {
                g.DrawString(_chartTitle, titleFont, titleBrush, 15, 8);

                if (_chartData.Count == 0)
                {
                    var mid = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    g.DrawString("No data for the selected range", textFont, textBrush, r, mid);
                    return;
                }

                var plot = new Rectangle(75, 42, r.Width - 75 - 30, r.Height - 42 - 38);
                double max = NiceMax(_chartData.Max(d => d.Value));
                var right = new StringFormat { Alignment = StringAlignment.Far, LineAlignment = StringAlignment.Center };
                var centre = new StringFormat { Alignment = StringAlignment.Center };

                // horizontal grid + y labels
                for (int i = 0; i <= 4; i++)
                {
                    float gy = plot.Bottom - i * plot.Height / 4f;
                    g.DrawLine(gridPen, plot.Left, gy, plot.Right, gy);
                    string label = UiHelper.Money((decimal)(max * i / 4));
                    g.DrawString(label, textFont, textBrush, new RectangleF(0, gy - 10, plot.Left - 8, 20), right);
                }

                // bars
                int n = _chartData.Count;
                float slot = plot.Width / (float)n;
                float barW = Math.Min(70f, slot * 0.55f);
                var linePts = new List<PointF>();

                for (int i = 0; i < n; i++)
                {
                    float h = Math.Max(2f, (float)(_chartData[i].Value / max * plot.Height));
                    float x = plot.Left + i * slot + (slot - barW) / 2f;
                    var bar = new RectangleF(x, plot.Bottom - h, barW, h);

                    using (var brush = new LinearGradientBrush(bar, Color.FromArgb(96, 165, 250), Color.FromArgb(37, 99, 235), 90f))
                        g.FillRectangle(brush, bar);

                    g.DrawString(UiHelper.Money((decimal)_chartData[i].Value), textFont, textBrush,
                        new RectangleF(x - 15, bar.Top - 18, barW + 30, 16), centre);
                    g.DrawString(_chartData[i].Label, textFont, textBrush,
                        new RectangleF(plot.Left + i * slot, plot.Bottom + 6, slot, 18), centre);

                    linePts.Add(new PointF(x + barW / 2f, bar.Top));
                }

                // trend line over the bars
                if (linePts.Count > 1)
                    using (var linePen = new Pen(Color.FromArgb(245, 158, 11), 2.5f) { LineJoin = LineJoin.Round })
                        g.DrawLines(linePen, linePts.ToArray());
                foreach (var p in linePts)
                {
                    g.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
                    using (var pen = new Pen(Color.FromArgb(245, 158, 11), 2f))
                        g.DrawEllipse(pen, p.X - 5, p.Y - 5, 10, 10);
                }

                // axes
                g.DrawLine(axisPen, plot.Left, plot.Top, plot.Left, plot.Bottom);
                g.DrawLine(axisPen, plot.Left, plot.Bottom, plot.Right, plot.Bottom);
            }
        }
    }
}
