namespace ClothingStore
{
    public partial class ucSales : UserControl
    {
        private readonly List<(Panel Panel, Label Name, Label Price, Button Add)> _tiles =
            new List<(Panel, Label, Label, Button)>();

        public ucSales()
        {
            InitializeComponent();

            dgvCart.AllowUserToAddRows = false;
            dgvCart.ReadOnly = true;
            dgvCart.MultiSelect = false;
            dgvCart.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            _tiles.Add((pnlProduct1, lblProductName1, lblProductPrice1, btnAddToOrder1));
            _tiles.Add((pnlProduct2, lblProductName2, lblProductPrice2, btnAddToOrder2));
            _tiles.Add((pnlProduct3, lblProductName3, lblProductPrice3, btnAddToOrder3));
            _tiles.Add((pnlProduct4, lblProductName4, lblProductPrice4, btnAddToOrder4));
            _tiles.Add((pnlProduct5, lblProductName5, lblProductPrice5, btnAddToOrder5));
            _tiles.Add((pnlProduct6, lblProductName6, lblProductPrice6, btnAddToOrder6));

            // ---- Event handling ----
            foreach (var tile in _tiles)
            {
                var t = tile;
                t.Add.Click += (s, e) => AddToCart(t.Name.Text, t.Price.Text);
            }
            txtProductSearch.TextChanged += (s, e) => FilterTiles();
            btnRemoveItem.Click += BtnRemoveItem_Click;
            btnCheckout.Click += BtnCheckout_Click;

            UpdateTotal();
        }

        private void FilterTiles()
        {
            string text = txtProductSearch.Text.Trim();
            foreach (var t in _tiles)
                t.Panel.Visible = text.Length == 0 ||
                    t.Name.Text.Contains(text, StringComparison.OrdinalIgnoreCase);
        }

        private void AddToCart(string name, string priceText)
        {
            if (!UiHelper.TryParseMoney(priceText, out decimal unit)) return;

            foreach (DataGridViewRow row in dgvCart.Rows)
            {
                if (Convert.ToString(row.Cells[0].Value) == name)
                {
                    int qty = Convert.ToInt32(row.Cells[1].Value) + 1;
                    row.Cells[1].Value = qty;
                    row.Cells[2].Value = UiHelper.Money(unit * qty);
                    UpdateTotal();
                    return;
                }
            }

            int idx = dgvCart.Rows.Add(name, 1, UiHelper.Money(unit));
            dgvCart.Rows[idx].Tag = unit;     // unit price kept on the row
            UpdateTotal();
        }

        private void UpdateTotal()
        {
            decimal total = 0;
            foreach (DataGridViewRow row in dgvCart.Rows)
                total += Convert.ToInt32(row.Cells[1].Value) * (decimal)row.Tag;
            lblTotal.Text = total.ToString("C2", UiHelper.US);
        }

        private void BtnRemoveItem_Click(object sender, EventArgs e)
        {
            if (dgvCart.SelectedRows.Count == 0)
            {
                MessageBox.Show("Select an item in the order first.", "Remove item", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            dgvCart.Rows.Remove(dgvCart.SelectedRows[0]);
            UpdateTotal();
        }

        private void BtnCheckout_Click(object sender, EventArgs e)
        {
            if (dgvCart.Rows.Count == 0)
            {
                MessageBox.Show("The order is empty. Add at least one product.", "Checkout", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var lines = new List<string>();
            foreach (DataGridViewRow row in dgvCart.Rows)
                lines.Add($"{row.Cells[1].Value} x {row.Cells[0].Value}   {row.Cells[2].Value}");

            MessageBox.Show(
                "Order completed!\n\n" + string.Join("\n", lines) +
                $"\n\nPayment: {cmbPayment.Text}\nTotal: {lblTotal.Text}",
                "Receipt", MessageBoxButtons.OK, MessageBoxIcon.Information);

            dgvCart.Rows.Clear();
            UpdateTotal();
        }
    }
}
