using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Story.Core.Models;

namespace EvadareBranEditor
{
    public partial class FormConditie : Form
    {
        public Conditie ConditieRezultat { get; private set; }

        private List<string> _cheiAtribute;

        private TreeView tvArbore;
        private Panel pnlEditare;
        private ComboBox cbProprietate, cbOperator;
        private NumericUpDown nudValoare;
        private Button btnSalveazaConditia;

        public FormConditie(Conditie conditieExistenta = null, List<string> cheiAtribute = null)
        {
            InitializeComponent();
            _cheiAtribute = cheiAtribute ?? new List<string>();
            ConstruiesteInterfata();

            if (conditieExistenta != null)
            {
                IncarcaConditieInArbore(conditieExistenta, null);
            }
            else
            {
                using (var dlg = new Form
                {
                    Text = "Tip condiție rădăcină",
                    Width = 320,
                    Height = 170,
                    FormBorderStyle = FormBorderStyle.FixedDialog,
                    StartPosition = FormStartPosition.CenterParent,
                    MaximizeBox = false,
                    MinimizeBox = false
                })
                {
                    Label lbl = new Label { Text = "Alege tipul nodului rădăcină:", Location = new System.Drawing.Point(15, 15), AutoSize = true };
                    Button bAnd = new Button { Text = "AND", Location = new System.Drawing.Point(15, 50), Width = 80, DialogResult = DialogResult.Yes };
                    Button bOr = new Button { Text = "OR", Location = new System.Drawing.Point(105, 50), Width = 80, DialogResult = DialogResult.No };
                    Button bComp = new Button { Text = "comparație", Location = new System.Drawing.Point(195, 50), Width = 95, DialogResult = DialogResult.OK };
                    dlg.Controls.Add(lbl); dlg.Controls.Add(bAnd); dlg.Controls.Add(bOr); dlg.Controls.Add(bComp);

                    var rez = dlg.ShowDialog();
                    Conditie cond;
                    string text;
                    if (rez == DialogResult.OK) { cond = new Conditie { Type = "COMPARISON", Operator = "==" }; text = " ==  0"; }
                    else if (rez == DialogResult.No) { cond = new Conditie { Type = "OR" }; text = "OR (cel puțin una)"; }
                    else { cond = new Conditie { Type = "AND" }; text = "AND (toate adevărate)"; }

                    TreeNode root = new TreeNode(text) { Tag = cond };
                    tvArbore.Nodes.Add(root);
                }
            }
            tvArbore.ExpandAll();
        }

        private void ConstruiesteInterfata()
        {
            this.Text = "Editor de condiții logice (AST)";
            this.Width = 650;
            this.Height = 450;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;

            tvArbore = new TreeView { Location = new System.Drawing.Point(10, 10), Width = 300, Height = 300 };
            tvArbore.AfterSelect += TvArbore_AfterSelect;
            this.Controls.Add(tvArbore);

            Button btnAddAnd = new Button { Text = "+ AND", Location = new System.Drawing.Point(10, 320), Width = 90 };
            btnAddAnd.Click += (s, e) => AdaugaNodCopil("AND", "AND (toate adevărate)");

            Button btnAddOr = new Button { Text = "+ OR", Location = new System.Drawing.Point(110, 320), Width = 90 };
            btnAddOr.Click += (s, e) => AdaugaNodCopil("OR", "OR (cel puțin una)");

            Button btnAddComp = new Button { Text = "+ comparație", Location = new System.Drawing.Point(210, 320), Width = 100 };
            btnAddComp.Click += (s, e) => AdaugaNodCopil("COMPARISON", "comparație");

            Button btnSterge = new Button { Text = "Șterge nod", Location = new System.Drawing.Point(10, 355), Width = 300, ForeColor = System.Drawing.Color.DarkRed };
            btnSterge.Click += BtnStergeNod_Click;

            this.Controls.Add(btnAddAnd); this.Controls.Add(btnAddOr); this.Controls.Add(btnAddComp); this.Controls.Add(btnSterge);

            pnlEditare = new Panel { Location = new System.Drawing.Point(330, 10), Width = 280, Height = 300, BorderStyle = BorderStyle.FixedSingle };

            Label lblProp = new Label { Text = "Proprietate:", Location = new System.Drawing.Point(10, 10), AutoSize = true };
            cbProprietate = new ComboBox { Location = new System.Drawing.Point(10, 30), Width = 250 };
            // Populare dinamică din atributele poveștii
            foreach (var k in _cheiAtribute) cbProprietate.Items.Add(k);

            Label lblOp = new Label { Text = "Operator:", Location = new System.Drawing.Point(10, 70), AutoSize = true };
            cbOperator = new ComboBox { Location = new System.Drawing.Point(10, 90), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cbOperator.Items.AddRange(new[] { "==", "!=", ">", ">=", "<", "<=" });

            Label lblVal = new Label { Text = "Valoare:", Location = new System.Drawing.Point(10, 130), AutoSize = true };
            nudValoare = new NumericUpDown { Location = new System.Drawing.Point(10, 150), Width = 250, Minimum = -99999, Maximum = 99999 };

            Button btnAplica = new Button { Text = "Aplică la nod", Location = new System.Drawing.Point(10, 200), Width = 250 };
            btnAplica.Click += BtnAplicaModificareNod_Click;

            pnlEditare.Controls.Add(lblProp); pnlEditare.Controls.Add(cbProprietate);
            pnlEditare.Controls.Add(lblOp); pnlEditare.Controls.Add(cbOperator);
            pnlEditare.Controls.Add(lblVal); pnlEditare.Controls.Add(nudValoare);
            pnlEditare.Controls.Add(btnAplica);

            pnlEditare.Visible = false;
            this.Controls.Add(pnlEditare);

            btnSalveazaConditia = new Button { Text = "Salvează condiția", Location = new System.Drawing.Point(330, 320), Width = 280, Height = 40 };
            btnSalveazaConditia.Click += BtnSalveazaConditia_Click;
            this.Controls.Add(btnSalveazaConditia);
        }

        private void AdaugaNodCopil(string type, string text)
        {
            if (tvArbore.SelectedNode == null) { MessageBox.Show("Selectează un nod părinte."); return; }

            Conditie condParinte = tvArbore.SelectedNode.Tag as Conditie;
            if (condParinte != null && condParinte.Type == "COMPARISON")
            {
                MessageBox.Show("Un nod COMPARISON nu poate avea sub-condiții.");
                return;
            }

            TreeNode nodNou = new TreeNode(text);
            nodNou.Tag = new Conditie { Type = type };
            tvArbore.SelectedNode.Nodes.Add(nodNou);
            tvArbore.SelectedNode.Expand();
        }

        private void BtnStergeNod_Click(object sender, EventArgs e)
        {
            if (tvArbore.SelectedNode == null) return;
            if (tvArbore.SelectedNode.Parent == null)
            {
                MessageBox.Show("Nu poți șterge nodul rădăcină.");
                return;
            }
            tvArbore.SelectedNode.Remove();
            pnlEditare.Visible = false;
        }

        private void TvArbore_AfterSelect(object sender, TreeViewEventArgs e)
        {
            Conditie cond = e.Node?.Tag as Conditie;
            if (cond == null) { pnlEditare.Visible = false; return; }

            if (cond.Type == "COMPARISON")
            {
                pnlEditare.Visible = true;
                cbProprietate.Text = cond.Property ?? "";
                cbOperator.Text = cond.Operator ?? "==";
                int v = cond.Value;
                if (v < nudValoare.Minimum) v = (int)nudValoare.Minimum;
                if (v > nudValoare.Maximum) v = (int)nudValoare.Maximum;
                nudValoare.Value = v;
            }
            else
            {
                pnlEditare.Visible = false;
            }
        }

        private void BtnAplicaModificareNod_Click(object sender, EventArgs e)
        {
            if (tvArbore.SelectedNode == null) return;
            Conditie cond = tvArbore.SelectedNode.Tag as Conditie;
            if (cond == null) return;

            cond.Property = cbProprietate.Text;
            cond.Operator = cbOperator.Text;
            cond.Value = (int)nudValoare.Value;

            tvArbore.SelectedNode.Text = $"{cond.Property} {cond.Operator} {cond.Value}";
        }

        private void BtnSalveazaConditia_Click(object sender, EventArgs e)
        {
            if (tvArbore.Nodes.Count > 0)
            {
                ConditieRezultat = ConstruiesteConditieDinArbore(tvArbore.Nodes[0]);
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private Conditie ConstruiesteConditieDinArbore(TreeNode nod)
        {
            Conditie cond = nod.Tag as Conditie;
            if (cond == null) cond = new Conditie { Type = "AND" };
            cond.Conditions.Clear();
            foreach (TreeNode copil in nod.Nodes)
                cond.Conditions.Add(ConstruiesteConditieDinArbore(copil));
            return cond;
        }

        private void IncarcaConditieInArbore(Conditie cond, TreeNode parinte)
        {
            string textNod = cond.Type;
            if (cond.Type == "COMPARISON") textNod = $"{cond.Property} {cond.Operator} {cond.Value}";
            else if (cond.Type == "AND") textNod = "AND (toate adevărate)";
            else if (cond.Type == "OR") textNod = "OR (cel puțin una)";

            TreeNode nodNou = new TreeNode(textNod);
            nodNou.Tag = cond;
            if (parinte == null) tvArbore.Nodes.Add(nodNou);
            else parinte.Nodes.Add(nodNou);

            if (cond.Conditions != null)
                foreach (var copil in cond.Conditions)
                    IncarcaConditieInArbore(copil, nodNou);
        }
    }
}
