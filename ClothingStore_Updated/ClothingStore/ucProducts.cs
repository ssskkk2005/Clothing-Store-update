using System.Drawing.Drawing2D;
using System.Drawing.Text;

namespace ClothingStore
{
    public partial class ucProducts : UserControl
    {
        public ucProducts()
        {
            InitializeComponent();
            dgvProducts.AllowUserToAddRows = false;
            dgvProducts.ReadOnly = true;
            dgvProducts.MultiSelect = false;
            dgvProducts.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            dgvProducts.Rows.Add("101", "Black hoodie", "Men", "$45");
            dgvProducts.Rows.Add("102", "Slim jeans", "Men", "$55");
            dgvProducts.Rows.Add("103", "Floral dress", "Women", "$60");
            dgvProducts.Rows.Add("104", "Kids tee", "Kids", "$15");

            picProduct.SizeMode = PictureBoxSizeMode.Zoom;

            // ---- Event handling ----
            dgvProducts.SelectionChanged += (s, e) => ShowSelectedProduct();
            btnAdd.Click += BtnAdd_Click;
            btnUpdate.Click += BtnUpdate_Click;
            btnDelete.Click += BtnDelete_Click;
            btnSearch.Click += (s, e) => ApplyFilter();
            txtSearch.TextChanged += (s, e) => ApplyFilter();
            cmbFilterCategory.SelectedIndexChanged += (s, e) => ApplyFilter();
            btnChoosePicture.Click += BtnChoosePicture_Click;
            txtPrice.KeyPress += TxtPrice_KeyPress;
            txtSearch.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter) { ApplyFilter(); e.SuppressKeyPress = true; }
            };

            // ---- GDI+ ----
            picProduct.Paint += PicProduct_Paint;

            ShowSelectedProduct();
        }

        // ---------- helpers ----------
        private DataGridViewRow SelectedRow =>
            dgvProducts.SelectedRows.Count > 0 ? dgvProducts.SelectedRows[0] : null;

        private void ShowSelectedProduct()
        {
            var row = SelectedRow;
            if (row == null) return;
            txtName.Text = Convert.ToString(row.Cells[1].Value);
            cmbCategory.SelectedItem = Convert.ToString(row.Cells[2].Value);
            txtPrice.Text = Convert.ToString(row.Cells[3].Value).Replace("$", "");
        }

        private bool ValidateInput(out decimal price)
        {
            price = 0;
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter the product name.", "Missing data", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtName.Focus();
                return false;
            }
            if (!UiHelper.TryParseMoney(txtPrice.Text, out price) || price <= 0)
            {
                MessageBox.Show("Please enter a valid price greater than zero.", "Invalid price", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtPrice.Focus();
                return false;
            }
            return true;
        }

        private void ApplyFilter()
        {
            string text = txtSearch.Text.Trim();
            string cat = cmbFilterCategory.SelectedIndex <= 0 ? null : cmbFilterCategory.Text;

            dgvProducts.CurrentCell = null;   // a visible row can't be hidden while it holds the current cell
            foreach (DataGridViewRow row in dgvProducts.Rows)
            {
                bool matchName = text.Length == 0 ||
                    Convert.ToString(row.Cells[1].Value).Contains(text, StringComparison.OrdinalIgnoreCase);
                bool matchCat = cat == null || Convert.ToString(row.Cells[2].Value) == cat;
                row.Visible = matchName && matchCat;
            }
        }

        // ---------- CRUD ----------
        private void BtnAdd_Click(object sender, EventArgs e)
        {
            if (!ValidateInput(out decimal price)) return;

            int nextId = 101;
            foreach (DataGridViewRow r in dgvProducts.Rows)
                if (int.TryParse(Convert.ToString(r.Cells[0].Value), out int id) && id >= nextId)
                    nextId = id + 1;

            int idx = dgvProducts.Rows.Add(nextId.ToString(), txtName.Text.Trim(), cmbCategory.Text, UiHelper.Money(price));
            dgvProducts.ClearSelection();
            dgvProducts.Rows[idx].Selected = true;
            txtName.Clear();
            txtPrice.Clear();
            txtName.Focus();
        }

        private void BtnUpdate_Click(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null)
            {
                MessageBox.Show("Select a product from the list first.", "Update", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (!ValidateInput(out decimal price)) return;

            row.Cells[1].Value = txtName.Text.Trim();
            row.Cells[2].Value = cmbCategory.Text;
            row.Cells[3].Value = UiHelper.Money(price);
        }

        private void BtnDelete_Click(object sender, EventArgs e)
        {
            var row = SelectedRow;
            if (row == null)
            {
                MessageBox.Show("Select a product from the list first.", "Delete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            var answer = MessageBox.Show($"Delete '{row.Cells[1].Value}'?", "Confirm delete",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (answer == DialogResult.Yes)
                dgvProducts.Rows.Remove(row);
        }

        // Only digits and one decimal point in the price box.
        private void TxtPrice_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar)) return;
            if (char.IsDigit(e.KeyChar)) return;
            if (e.KeyChar == '.' && !txtPrice.Text.Contains('.')) return;
            e.Handled = true;
        }

        // ---------- picture ----------
        private void BtnChoosePicture_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Images|*.png;*.jpg;*.jpeg;*.bmp;*.gif", Title = "Choose product picture" })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                try
                {
                    using (var tmp = Image.FromFile(dlg.FileName))
                    {
                        var old = picProduct.Image;
                        picProduct.Image = new Bitmap(tmp);   // copy, so the file isn't locked
                        old?.Dispose();
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("This file could not be loaded as an image.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // Placeholder drawn with GDI+ while no picture is selected.
        private void PicProduct_Paint(object sender, PaintEventArgs e)
        {
            if (picProduct.Image != null) return;

            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
            Rectangle r = picProduct.ClientRectangle;

            using (var bg = new LinearGradientBrush(r, Color.FromArgb(243, 246, 250), Color.FromArgb(219, 234, 254), 90f))
                g.FillRectangle(bg, r);
            using (var pen = new Pen(Color.FromArgb(147, 197, 253), 2f) { DashStyle = DashStyle.Dash })
                g.DrawRectangle(pen, 3, 3, r.Width - 7, r.Height - 7);

            float scale = Math.Max(0.5f, Math.Min(r.Width, r.Height) / 140f);
            UiHelper.DrawShirt(g, r.Width / 2f, r.Height / 2f - 30 * scale, scale,
                Color.FromArgb(96, 165, 250), Color.FromArgb(37, 99, 235));

            using (var f = new Font("Segoe UI", 8.5f))
            using (var b = new SolidBrush(Color.FromArgb(107, 114, 128)))
            {
                var fmt = new StringFormat { Alignment = StringAlignment.Center };
                g.DrawString("No picture", f, b, new RectangleF(0, r.Height / 2f + 25 * scale, r.Width, 20), fmt);
            }
        }
    }
}
