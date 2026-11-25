using System.Drawing.Drawing2D;

namespace ClothingStore
{
    public partial class ucSizesColors : UserControl
    {
        private readonly Dictionary<string, Color> _colorMap =
            new Dictionary<string, Color>(StringComparer.OrdinalIgnoreCase)
            {
                { "Black", Color.Black }, { "White", Color.White }, { "Blue", Color.RoyalBlue },
                { "Red", Color.Crimson }, { "Green", Color.SeaGreen }
            };

        private Color _previewColor = Color.Black;

        public ucSizesColors()
        {
            InitializeComponent();

            // owner-drawn list so each colour shows a swatch (GDI+)
            lstColors.DrawMode = DrawMode.OwnerDrawFixed;
            lstColors.ItemHeight = 28;
            lstColors.DrawItem += LstColors_DrawItem;

            UiHelper.EnableDoubleBuffer(pnlColorPreview);
            pnlColorPreview.Paint += PnlColorPreview_Paint;

            // ---- Event handling: sizes ----
            btnAddSize.Click += (s, e) => AddSize();
            btnDeleteSize.Click += (s, e) => DeleteSelected(lstSizes, "size");
            txtSize.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { AddSize(); e.SuppressKeyPress = true; }
            };

            // ---- Event handling: colours ----
            btnAddColor.Click += (s, e) => AddColor();
            btnDeleteColor.Click += (s, e) => DeleteSelected(lstColors, "color");
            btnPickColor.Click += BtnPickColor_Click;
            txtColor.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { AddColor(); e.SuppressKeyPress = true; }
            };
            lstColors.SelectedIndexChanged += (s, e) =>
            {
                if (lstColors.SelectedItem != null)
                {
                    _previewColor = GetColor(Convert.ToString(lstColors.SelectedItem));
                    pnlColorPreview.Invalidate();
                }
            };
        }

        // ---------- sizes ----------
        private void AddSize()
        {
            string size = txtSize.Text.Trim().ToUpperInvariant();
            if (size.Length == 0)
            {
                MessageBox.Show("Enter a size first.", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtSize.Focus();
                return;
            }
            if (lstSizes.Items.Cast<object>().Any(i => string.Equals(Convert.ToString(i), size, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Size '{size}' already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            lstSizes.SelectedIndex = lstSizes.Items.Add(size);
            txtSize.Clear();
            txtSize.Focus();
        }

        // ---------- colours ----------
        private void AddColor()
        {
            string name = txtColor.Text.Trim();
            if (name.Length == 0)
            {
                MessageBox.Show("Enter a color name first (or pick one).", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtColor.Focus();
                return;
            }
            if (lstColors.Items.Cast<object>().Any(i => string.Equals(Convert.ToString(i), name, StringComparison.OrdinalIgnoreCase)))
            {
                MessageBox.Show($"Color '{name}' already exists.", "Duplicate", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // if the name isn't a known colour, use the colour chosen in the picker
            if (!_colorMap.ContainsKey(name))
            {
                Color known = Color.FromName(name);
                _colorMap[name] = known.IsKnownColor ? known : _previewColor;
            }

            lstColors.SelectedIndex = lstColors.Items.Add(name);
            txtColor.Clear();
            txtColor.Focus();
        }

        private void BtnPickColor_Click(object sender, EventArgs e)
        {
            using (var dlg = new ColorDialog { Color = _previewColor, FullOpen = true })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                _previewColor = dlg.Color;
                txtColor.Text = dlg.Color.IsNamedColor
                    ? dlg.Color.Name
                    : $"#{dlg.Color.R:X2}{dlg.Color.G:X2}{dlg.Color.B:X2}";
                pnlColorPreview.Invalidate();
            }
        }

        private Color GetColor(string name)
        {
            if (_colorMap.TryGetValue(name, out Color c)) return c;
            Color named = Color.FromName(name);
            if (named.IsKnownColor) return named;
            try { return ColorTranslator.FromHtml(name); }
            catch (Exception) { return Color.Gray; }
        }

        private void DeleteSelected(ListBox list, string what)
        {
            if (list.SelectedIndex < 0)
            {
                MessageBox.Show($"Select a {what} from the list first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var answer = MessageBox.Show($"Delete {what} '{list.SelectedItem}'?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
                list.Items.RemoveAt(list.SelectedIndex);
        }

        // ---------- GDI+ drawing ----------
        private void LstColors_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            if (e.Index < 0) return;

            string name = Convert.ToString(lstColors.Items[e.Index]);
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var swatch = new Rectangle(e.Bounds.X + 8, e.Bounds.Y + 4, 20, e.Bounds.Height - 8);
            using (var brush = new SolidBrush(GetColor(name)))
                g.FillEllipse(brush, swatch);
            using (var pen = new Pen(Color.FromArgb(156, 163, 175)))
                g.DrawEllipse(pen, swatch);

            TextRenderer.DrawText(g, name, e.Font,
                new Rectangle(e.Bounds.X + 38, e.Bounds.Y, e.Bounds.Width - 38, e.Bounds.Height),
                e.ForeColor, TextFormatFlags.VerticalCenter | TextFormatFlags.Left);
            e.DrawFocusRectangle();
        }

        private void PnlColorPreview_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            Rectangle r = pnlColorPreview.ClientRectangle;
            int d = Math.Max(10, Math.Min(r.Width, r.Height) - 8);
            var circle = new Rectangle(r.X + (r.Width - d) / 2, r.Y + (r.Height - d) / 2, d, d);

            using (var shadow = new SolidBrush(Color.FromArgb(40, 0, 0, 0)))
                g.FillEllipse(shadow, circle.X + 2, circle.Y + 3, d, d);
            using (var fill = new SolidBrush(_previewColor))
                g.FillEllipse(fill, circle);
            using (var pen = new Pen(Color.FromArgb(156, 163, 175), 2f))
                g.DrawEllipse(pen, circle);

            // glossy highlight
            var gloss = new Rectangle(circle.X + d / 6, circle.Y + d / 8, d / 2, d / 3);
            using (var path = new GraphicsPath())
            {
                path.AddEllipse(gloss);
                using (var shine = new LinearGradientBrush(gloss, Color.FromArgb(120, 255, 255, 255), Color.FromArgb(0, 255, 255, 255), 90f))
                    g.FillPath(shine, path);
            }
        }
    }
}
