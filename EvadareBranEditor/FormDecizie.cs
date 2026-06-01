using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
using Story.Core.Models;

namespace EvadareBranEditor
{
    public partial class FormDecizie : Form
    {
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Decizie DecizieRezultat { get => _decizie; set { _decizie = value; IncarcaDate(); } }
        private Decizie _decizie;


        private List<string> _idBlocuriDisponibile;
        private List<string> _cheiAtribute;

        private TextBox txtTextDecizie;
        private ComboBox cbTargetBlock;
        private TextBox txtIcon;
        private DataGridView dgvEfecte;
        private Button btnOk;

        public FormDecizie() : this(new List<string>(), new List<string>()) { }

        public FormDecizie(List<string> idBlocuri, List<string> cheiAtribute)
        {
            InitializeComponent();
            _idBlocuriDisponibile = idBlocuri ?? new List<string>();
            _cheiAtribute = cheiAtribute ?? new List<string>();
            ConstruiesteInterfata();
            _decizie = new Decizie();
        }

        private void ConstruiesteInterfata()
        {
            this.Text = "Editare decizie";
            this.Width = 480;
            this.Height = 520;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            int y = 15;
            this.Controls.Add(new Label { Text = "Textul deciziei:", Location = new System.Drawing.Point(20, y), AutoSize = true });
            y += 22;
            txtTextDecizie = new TextBox { Location = new System.Drawing.Point(20, y), Width = 420 };
            this.Controls.Add(txtTextDecizie);
            y += 32;

            this.Controls.Add(new Label { Text = "Blocul destinație:", Location = new System.Drawing.Point(20, y), AutoSize = true });
            y += 22;
            cbTargetBlock = new ComboBox { Location = new System.Drawing.Point(20, y), Width = 420, DropDownStyle = ComboBoxStyle.DropDownList };
            foreach (var id in _idBlocuriDisponibile) cbTargetBlock.Items.Add(id);
            this.Controls.Add(cbTargetBlock);
            y += 35;

            this.Controls.Add(new Label { Text = "Iconiță (URL sau cale):", Location = new System.Drawing.Point(20, y), AutoSize = true });
            y += 22;
            txtIcon = new TextBox { Location = new System.Drawing.Point(20, y), Width = 320 };
            this.Controls.Add(txtIcon);
            Button btnIcon = new Button { Text = "Răsfoiește...", Location = new System.Drawing.Point(345, y - 1), Width = 95 };
            btnIcon.Click += (s, ev) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Imagini (*.jpg;*.png;*.bmp;*.gif)|*.jpg;*.png;*.bmp;*.gif";
                    if (ofd.ShowDialog() == DialogResult.OK)
                        txtIcon.Text = "images/" + Path.GetFileName(ofd.FileName);
                }
            };
            this.Controls.Add(btnIcon);
            y += 32;

            Button btnEditeazaConditie = new Button { Text = "Editează condiția de afișare...", Location = new System.Drawing.Point(20, y), Width = 220, Height = 28 };
            btnEditeazaConditie.Click += (s, ev) =>
            {
                using (FormConditie frmConditie = new FormConditie(_decizie.Condition, _cheiAtribute))
                {
                    if (frmConditie.ShowDialog() == DialogResult.OK)
                    {
                        _decizie.Condition = frmConditie.ConditieRezultat;
                        MessageBox.Show("Condiția a fost atașată deciziei.");
                    }
                }
            };
            this.Controls.Add(btnEditeazaConditie);
            y += 38;

            this.Controls.Add(new Label { Text = "Efecte (tip ADD/SET, atribut, valoare):", Location = new System.Drawing.Point(20, y), AutoSize = true });
            y += 22;
            dgvEfecte = new DataGridView { Location = new System.Drawing.Point(20, y), Width = 420, Height = 140 };
            dgvEfecte.ColumnCount = 3;
            dgvEfecte.Columns[0].Name = "Tip";
            dgvEfecte.Columns[1].Name = "Atribut";
            dgvEfecte.Columns[2].Name = "Valoare";
            dgvEfecte.Columns[0].Width = 70;
            dgvEfecte.Columns[1].Width = 230;
            dgvEfecte.Columns[2].Width = 100;
            dgvEfecte.AllowUserToAddRows = true;
            this.Controls.Add(dgvEfecte);
            y += 150;

            btnOk = new Button { Text = "Salvează decizia", Location = new System.Drawing.Point(280, y), Width = 160, Height = 32 };
            btnOk.Click += BtnOk_Click;
            this.Controls.Add(btnOk);
        }

        private void IncarcaDate()
        {
            if (_decizie == null || dgvEfecte == null) return;

            txtTextDecizie.Text = _decizie.Text ?? "";
            if (!string.IsNullOrEmpty(_decizie.TargetBlock) && cbTargetBlock.Items.Contains(_decizie.TargetBlock))
                cbTargetBlock.SelectedItem = _decizie.TargetBlock;
            txtIcon.Text = _decizie.Icon ?? "";

            dgvEfecte.Rows.Clear();
            foreach (var efect in _decizie.Effects)
                dgvEfecte.Rows.Add(efect.Type, efect.Property, efect.Value.ToString());
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTextDecizie.Text))
            {
                MessageBox.Show("Textul deciziei nu poate fi gol.");
                return;
            }
            if (cbTargetBlock.SelectedItem == null)
            {
                MessageBox.Show("Alege un bloc destinație.");
                return;
            }

            _decizie.Text = txtTextDecizie.Text;
            _decizie.TargetBlock = cbTargetBlock.SelectedItem.ToString();
            _decizie.Icon = string.IsNullOrWhiteSpace(txtIcon.Text) ? null : txtIcon.Text;

            _decizie.Effects.Clear();
            foreach (DataGridViewRow row in dgvEfecte.Rows)
            {
                if (row.IsNewRow) continue;
                if (row.Cells[0].Value == null || row.Cells[1].Value == null) continue;

                int valoare = 0;
                int.TryParse(row.Cells[2].Value?.ToString(), out valoare);

                _decizie.Effects.Add(new Efect
                {
                    Type = row.Cells[0].Value.ToString().ToUpper(),
                    Property = row.Cells[1].Value.ToString(),
                    Value = valoare
                });
            }

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
