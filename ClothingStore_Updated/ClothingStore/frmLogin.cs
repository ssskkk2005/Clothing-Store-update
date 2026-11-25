using System.Drawing.Drawing2D;

namespace ClothingStore
{
    public partial class frmLogin : Form
    {
        private const string ValidUser = "sara";
        private const string ValidPassword = "2005";
        private static string _rememberedUser;           // "Remember me" (kept while the app runs)
        private readonly ErrorProvider _errors = new ErrorProvider();

        public frmLogin()
        {
            InitializeComponent();
            AcceptButton = btnLogin;
            DoubleBuffered = true;

            if (!string.IsNullOrEmpty(_rememberedUser))
            {
                txtUsername.Text = _rememberedUser;
                chkRemember.Checked = true;
            }

            // ---- Event handling ----
            btnLogin.Click += BtnLogin_Click;
            txtUsername.TextChanged += (s, e) => _errors.SetError(txtUsername, "");
            txtPassword.TextChanged += (s, e) => _errors.SetError(txtPassword, "");
            chkRemember.CheckedChanged += (s, e) => { if (!chkRemember.Checked) _rememberedUser = null; };

            // ---- GDI+ ----
            Paint += FrmLogin_Paint;
            pnlTop.Paint += PnlTop_Paint;
            btnLogin.MouseEnter += (s, e) => btnLogin.BackColor = Color.FromArgb(29, 78, 216);
            btnLogin.MouseLeave += (s, e) => btnLogin.BackColor = Color.FromArgb(37, 99, 235);
        }

        private void BtnLogin_Click(object sender, EventArgs e)
        {
            _errors.SetError(txtUsername, "");
            _errors.SetError(txtPassword, "");

            if (string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                _errors.SetError(txtUsername, "Username is required");
                txtUsername.Focus();
                return;
            }
            if (string.IsNullOrEmpty(txtPassword.Text))
            {
                _errors.SetError(txtPassword, "Password is required");
                txtPassword.Focus();
                return;
            }
            if (!string.Equals(txtUsername.Text.Trim(), ValidUser, StringComparison.OrdinalIgnoreCase) ||
                txtPassword.Text != ValidPassword)
            {
                _errors.SetError(txtPassword, "Incorrect username or password");
                MessageBox.Show("Incorrect username or password.", "Login failed",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                txtPassword.SelectAll();
                txtPassword.Focus();
                return;
            }

            _rememberedUser = chkRemember.Checked ? txtUsername.Text.Trim() : null;
            txtPassword.Clear();
            new frmMain(this).Show();
            Hide();
        }

        // Gradient bar at the top of the login window.
        private void PnlTop_Paint(object sender, PaintEventArgs e)
        {
            using (var brush = new LinearGradientBrush(pnlTop.ClientRectangle,
                Color.FromArgb(37, 99, 235), Color.FromArgb(124, 58, 237), 0f))
                e.Graphics.FillRectangle(brush, pnlTop.ClientRectangle);
        }

        // Logo (T-shirt icon) + soft divider line drawn with GDI+.
        private void FrmLogin_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            UiHelper.DrawShirt(g, ClientSize.Width / 2f, 18, 1f,
                Color.FromArgb(37, 99, 235), Color.FromArgb(124, 58, 237));

            using (var pen = new Pen(Color.FromArgb(229, 231, 235), 1.5f))
                g.DrawLine(pen, 60, 160, ClientSize.Width - 60, 160);

            using (var pen = new Pen(Color.FromArgb(37, 99, 235), 3f))
                g.DrawLine(pen, ClientSize.Width / 2f - 25, 160, ClientSize.Width / 2f + 25, 160);
        }
    }
}
