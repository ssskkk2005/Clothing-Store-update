using System.Drawing.Drawing2D;

namespace ClothingStore
{
    public partial class ucInventory : UserControl
    {
        private const int BarMax = 50;     // quantity that fills the stock bar
        private Font _boldFont;

        public ucInventory()
        {
            InitializeComponent();

            dgvInventory.AllowUserToAddRows = false;
            dgvInventory.ReadOnly = true;
            dgvInventory.MultiSelect = false;
            dgvInventory.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvInventory.Rows.Add("Black hoodie", "M", "Black", "2", "Very low");
            dgvInventory.Rows.Add("Slim jeans", "32", "Blue", "4", "Low");
            dgvInventory.Rows.Add("Floral dress", "S", "Red", "25", "In stock");
            dgvInventory.Rows.Add("Kids tee", "6Y", "White", "40", "In stock");

            numQty.Minimum = 1;
            numQty.Maximum = 1000;
            txtItem.ReadOnly = true;

            // ---- Event handling ----
            dgvInventory.SelectionChanged += (s, e) => ShowSelected();
            dgvInventory.CellFormatting += DgvInventory_CellFormatting;
            btnFilter.Click += (s, e) => ApplyFilter();
            txtInvSearch.TextChanged += (s, e) => ApplyFilter();
            cmbSize.SelectedIndexChanged += (s, e) => ApplyFilter();
            cmbColor.SelectedIndexChanged += (s, e) => ApplyFilter();
            btnAddStock.Click += (s, e) => ChangeStock(+1);
            btnRemoveStock.Click += (s, e) => ChangeStock(-1);

            // ---- GDI+ : stock level bar inside the Quantity cells ----
            dgvInventory.CellPainting += DgvInventory_CellPainting;

            ShowSelected();
        }

        private DataGridViewRow SelectedRow =>
            dgvInventory.SelectedRows.Count > 0 ? dgvInventory.SelectedRows[0] : null;

        private static int Qty(DataGridViewRow row) =>
            int.TryParse(Convert.ToString(row.Cells[3].Value), out int q) ? q : 0;

        private static string StatusFor(int qty) => qty <= 3 ? "Very low" : qty <= 10 ? "Low" : "In stock";

        private void ShowSelected()
        {
            var row = SelectedRow;
            txtItem.Text = row == null ? "" :
                $"{row.Cells[0].Value} / {row.Cells[1].Value} / {row.Cells[2].Value}";
        }

        private void ApplyFilter()
        {
            string text = txtInvSearch.Text.Trim();
            string size = cmbSize.SelectedIndex <= 0 ? null : cmbSize.Text;
            string color = cmbColor.SelectedIndex <= 0 ? null : cmbColor.Text;

            dgvInventory.CurrentCell = null;
            foreach (DataGridViewRow row in dgvInventory.Rows)
            {
                bool ok = (text.Length == 0 ||
                           Convert.ToString(row.Cells[0].Value).Contains(text, StringComparison.OrdinalIgnoreCase))
                          && (size == null || Convert.ToString(row.Cells[1].Value) == size)
                          && (color == null || Convert.ToString(row.Cells[2].Value) == color);
                row.Visible = ok;
            }
        }

        private void ChangeStock(int direction)
        {
            var row = SelectedRow;
            if (row == null)
            {
                MessageBox.Show("Select an item from the list first.", "Stock", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            int amount = (int)numQty.Value;
            int current = Qty(row);
            if (direction < 0 && amount > current)
            {
                MessageBox.Show($"Only {current} in stock. You can't remove {amount}.", "Not enough stock",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int updated = current + direction * amount;
            row.Cells[3].Value = updated.ToString();
            row.Cells[4].Value = StatusFor(updated);
            dgvInventory.InvalidateRow(row.Index);
        }

        private void DgvInventory_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 4) return;
            switch (Convert.ToString(e.Value))
            {
                case "Very low": e.CellStyle.ForeColor = Color.FromArgb(220, 38, 38); break;
                case "Low": e.CellStyle.ForeColor = Color.FromArgb(217, 119, 6); break;
                default: e.CellStyle.ForeColor = Color.FromArgb(5, 150, 105); break;
            }
            _boldFont ??= new Font(dgvInventory.Font, FontStyle.Bold);
            e.CellStyle.Font = _boldFont;
        }

        private void DgvInventory_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex != 3) return;

            e.Paint(e.CellBounds, DataGridViewPaintParts.Background |
                                  DataGridViewPaintParts.Border |
                                  DataGridViewPaintParts.SelectionBackground);

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int qty = Qty(dgvInventory.Rows[e.RowIndex]);
            var track = new Rectangle(e.CellBounds.X + 8, e.CellBounds.Bottom - 9, e.CellBounds.Width - 16, 4);
            int fillWidth = (int)(track.Width * Math.Min(1.0, qty / (double)BarMax));
            Color level = qty <= 3 ? Color.FromArgb(239, 68, 68)
                        : qty <= 10 ? Color.FromArgb(245, 158, 11)
                        : Color.FromArgb(16, 185, 129);

            using (var trackBrush = new SolidBrush(Color.FromArgb(229, 231, 235)))
                g.FillRectangle(trackBrush, track);
            if (fillWidth > 0)
                using (var fillBrush = new SolidBrush(level))
                    g.FillRectangle(fillBrush, new Rectangle(track.X, track.Y, fillWidth, track.Height));

            bool selected = (e.State & DataGridViewElementStates.Selected) != 0;
            TextRenderer.DrawText(g, qty.ToString(), e.CellStyle.Font,
                new Rectangle(e.CellBounds.X + 8, e.CellBounds.Y, e.CellBounds.Width - 16, e.CellBounds.Height - 8),
                selected ? e.CellStyle.SelectionForeColor : e.CellStyle.ForeColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter);

            e.Handled = true;
        }
    }
}
