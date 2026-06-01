// ============================================================
//  Evadare din Castelul Bran — proiect POO (echipă de 4)
//  AUTOR PRINCIPAL: Persoana 3 — Editor (structura principală)
//  NOTĂ: blocul marcat cu ">>> PERSOANA 4" (validare + jurnal)
//  este realizat de Persoana 4. Restul fișierului este al Persoanei 3.
// ============================================================
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Story.Core.Models;

namespace EvadareBranEditor
{
    public partial class Form1 : Form
    {
        // Povestea editată în memorie
        private Poveste _povesteCurenta;

        // Folderul în care a fost extras ZIP-ul curent (pentru a reține imaginile la re-salvare). Null pentru poveste nouă.
        private string _folderResurseSursa = null;

        // Imagini noi adăugate în sesiunea curentă: "images/x.jpg" -> cale absolută sursă pe disc
        private Dictionary<string, string> _imaginiNoi = new Dictionary<string, string>();

        // Constante pentru identificarea categoriei în Tag-ul nodurilor TreeView
        private const string TAG_ROOT = "ROOT";
        private const string TAG_CAT_ATRIBUTE = "CAT_ATTR";
        private const string TAG_CAT_BLOCURI = "CAT_BLOCK";

        public Form1()
        {
            InitializeComponent();
            _povesteCurenta = CreazaPovesteGoala();
            ActualizeazaTreeView();
        }

        // ----- POVESTE NOUĂ -----

        private Poveste CreazaPovesteGoala()
        {
            _folderResurseSursa = null;
            _imaginiNoi.Clear();
            return new Poveste
            {
                Title = "Poveste nouă",
                StartBlock = "",
                Attributes = new List<AtributPoveste>(),
                Blocks = new List<BlocPoveste>()
            };
        }

        // ----- MENIU PRINCIPAL -----

        // "Nou"
        private void toolStripMenuItem1_Click_1(object sender, EventArgs e)
        {
            _povesteCurenta = CreazaPovesteGoala();
            ActualizeazaTreeView();
            pnlEditorCentral.Controls.Clear();
            MessageBox.Show("A fost creată o poveste nouă, goală! Poți începe adăugarea de atribute și blocuri.");
        }

        // "Deschide"
        private void deschideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            IncarcaPoveste();
        }

        // "Salveaza"
        private void salveazaToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Înainte de salvare facem validare
            var erori = ValideazaPoveste();
            if (erori.Count > 0)
            {
                var rezultat = MessageBox.Show(
                    "Povestea are probleme:\n\n" + string.Join("\n", erori) + "\n\nSalvezi totuși?",
                    "Validare",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);
                if (rezultat != DialogResult.Yes) return;
            }

            SalveazaPoveste();
        }

        // ----- TREEVIEW -----

        private void ActualizeazaTreeView()
        {
            tvPoveste.Nodes.Clear();

            TreeNode rootNode = new TreeNode("Povestea: " + (_povesteCurenta.Title ?? ""));
            rootNode.Tag = TAG_ROOT;

            TreeNode atributeNode = new TreeNode("Proprietăți (atribute)");
            atributeNode.Tag = TAG_CAT_ATRIBUTE;
            foreach (var attr in _povesteCurenta.Attributes)
            {
                TreeNode n = new TreeNode(attr.Key) { Tag = attr };
                atributeNode.Nodes.Add(n);
            }

            TreeNode blocuriNode = new TreeNode("Blocuri narative");
            blocuriNode.Tag = TAG_CAT_BLOCURI;
            foreach (var bloc in _povesteCurenta.Blocks)
            {
                string txt = bloc.Id + (bloc.IsFinal ? "  [FINAL]" : "");
                TreeNode n = new TreeNode(txt) { Tag = bloc };
                blocuriNode.Nodes.Add(n);
            }

            rootNode.Nodes.Add(atributeNode);
            rootNode.Nodes.Add(blocuriNode);
            tvPoveste.Nodes.Add(rootNode);
            tvPoveste.ExpandAll();

            // Jurnalul de validare se reîmprospătează „live" la fiecare schimbare de structură
            RuleazaValidareInJurnal();
        }

        private void tvPoveste_AfterSelect(object sender, TreeViewEventArgs e)
        {
            pnlEditorCentral.Controls.Clear();

            if (e.Node?.Tag is string tag)
            {
                if (tag == TAG_ROOT) AfiseazaEditorMetadate();
                else if (tag == TAG_CAT_ATRIBUTE) AfiseazaCategorieAtribute();
                else if (tag == TAG_CAT_BLOCURI) AfiseazaCategorieBlocuri();
            }
            else if (e.Node?.Tag is AtributPoveste atribut)
            {
                AfiseazaEditorAtribut(atribut, e.Node);
            }
            else if (e.Node?.Tag is BlocPoveste bloc)
            {
                AfiseazaEditorBloc(bloc, e.Node);
            }
        }

        // ----- EDITOR METADATE POVESTE -----

        private void AfiseazaEditorMetadate()
        {
            int y = 20;

            Label lblHead = new Label { Text = "Metadate Poveste", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            pnlEditorCentral.Controls.Add(lblHead);
            y += 40;

            pnlEditorCentral.Controls.Add(new Label { Text = "Titlu:", Location = new Point(20, y), AutoSize = true });
            TextBox txtTitlu = new TextBox { Text = _povesteCurenta.Title ?? "", Location = new Point(150, y), Width = 350 };
            pnlEditorCentral.Controls.Add(txtTitlu);
            y += 40;

            pnlEditorCentral.Controls.Add(new Label { Text = "Bloc de start:", Location = new Point(20, y), AutoSize = true });
            ComboBox cbStart = new ComboBox { Location = new Point(150, y), Width = 350, DropDownStyle = ComboBoxStyle.DropDownList };
            cbStart.Items.Add("(neselectat)");
            foreach (var b in _povesteCurenta.Blocks) cbStart.Items.Add(b.Id);
            cbStart.SelectedItem = string.IsNullOrEmpty(_povesteCurenta.StartBlock) ? "(neselectat)" : _povesteCurenta.StartBlock;
            pnlEditorCentral.Controls.Add(cbStart);
            y += 50;

            Button btn = new Button { Text = "Aplică modificările", Location = new Point(150, y), Width = 200, Height = 35 };
            btn.Click += (s, ev) =>
            {
                _povesteCurenta.Title = txtTitlu.Text;
                _povesteCurenta.StartBlock = (cbStart.SelectedItem?.ToString() == "(neselectat)") ? "" : (cbStart.SelectedItem?.ToString() ?? "");
                ActualizeazaTreeView();
                MessageBox.Show("Metadatele au fost actualizate.");
            };
            pnlEditorCentral.Controls.Add(btn);
        }

        // ----- CATEGORII -----

        private void AfiseazaCategorieAtribute()
        {
            int y = 20;
            Label lbl = new Label { Text = "Proprietăți (atribute)", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            pnlEditorCentral.Controls.Add(lbl);
            y += 40;

            Button btn = new Button { Text = "+ Adaugă atribut nou", Location = new Point(20, y), Width = 200, Height = 35 };
            btn.Click += (s, ev) =>
            {
                string cheie = PromptText("Cheie atribut (ex: player.viata):", "Atribut nou");
                if (string.IsNullOrWhiteSpace(cheie)) return;
                if (_povesteCurenta.Attributes.Any(a => a.Key == cheie))
                {
                    MessageBox.Show("Există deja un atribut cu cheia '" + cheie + "'.");
                    return;
                }
                var nou = new AtributPoveste
                {
                    Key = cheie,
                    HudLabel = cheie,
                    Min = 0,
                    Max = 100,
                    Initial = 0,
                    VisibleInHud = true,
                    HudOrder = _povesteCurenta.Attributes.Count + 1
                };
                _povesteCurenta.Attributes.Add(nou);
                ActualizeazaTreeView();
            };
            pnlEditorCentral.Controls.Add(btn);
            y += 50;

            Label lblList = new Label { Text = "Atribute existente:", Location = new Point(20, y), AutoSize = true };
            pnlEditorCentral.Controls.Add(lblList);
            y += 25;

            ListBox lb = new ListBox { Location = new Point(20, y), Width = 480, Height = 220 };
            foreach (var a in _povesteCurenta.Attributes.OrderBy(a => a.HudOrder))
                lb.Items.Add($"{a.Key}  ({a.HudLabel})  min={a.Min}  max={a.Max}  init={a.Initial}");
            pnlEditorCentral.Controls.Add(lb);
        }

        private void AfiseazaCategorieBlocuri()
        {
            int y = 20;
            Label lbl = new Label { Text = "Blocuri narative", Location = new Point(20, y), AutoSize = true, Font = new Font("Segoe UI", 12, FontStyle.Bold) };
            pnlEditorCentral.Controls.Add(lbl);
            y += 40;

            Button btn = new Button { Text = "+ Adaugă bloc nou", Location = new Point(20, y), Width = 200, Height = 35 };
            btn.Click += (s, ev) =>
            {
                string id = PromptText("ID bloc (ex: intro.captiv):", "Bloc nou");
                if (string.IsNullOrWhiteSpace(id)) return;
                if (_povesteCurenta.Blocks.Any(b => b.Id == id))
                {
                    MessageBox.Show("Există deja un bloc cu ID-ul '" + id + "'.");
                    return;
                }
                var nou = new BlocPoveste { Id = id, Text = "", IsFinal = false, Decisions = new List<Decizie>() };
                _povesteCurenta.Blocks.Add(nou);
                ActualizeazaTreeView();
            };
            pnlEditorCentral.Controls.Add(btn);
            y += 50;

            Label lblList = new Label { Text = "Blocuri existente:", Location = new Point(20, y), AutoSize = true };
            pnlEditorCentral.Controls.Add(lblList);
            y += 25;

            ListBox lb = new ListBox { Location = new Point(20, y), Width = 480, Height = 220 };
            foreach (var b in _povesteCurenta.Blocks)
                lb.Items.Add($"{b.Id}{(b.IsFinal ? " [FINAL]" : "")}  ({b.Decisions.Count} decizii)");
            pnlEditorCentral.Controls.Add(lb);
        }

        // ----- EDITOR ATRIBUT COMPLET -----

        private void AfiseazaEditorAtribut(AtributPoveste attr, TreeNode nodCurent)
        {
            int y = 20;

            pnlEditorCentral.Controls.Add(new Label { Text = "Cheie (key):", Location = new Point(20, y), AutoSize = true });
            TextBox txtKey = new TextBox { Text = attr.Key ?? "", Location = new Point(180, y), Width = 250 };
            pnlEditorCentral.Controls.Add(txtKey);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Etichetă HUD:", Location = new Point(20, y), AutoSize = true });
            TextBox txtLabel = new TextBox { Text = attr.HudLabel ?? "", Location = new Point(180, y), Width = 250 };
            pnlEditorCentral.Controls.Add(txtLabel);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Min:", Location = new Point(20, y), AutoSize = true });
            NumericUpDown nudMin = new NumericUpDown { Value = attr.Min, Location = new Point(180, y), Minimum = -99999, Maximum = 99999, Width = 100 };
            pnlEditorCentral.Controls.Add(nudMin);

            pnlEditorCentral.Controls.Add(new Label { Text = "Max:", Location = new Point(290, y), AutoSize = true });
            NumericUpDown nudMax = new NumericUpDown { Value = attr.Max, Location = new Point(330, y), Minimum = -99999, Maximum = 99999, Width = 100 };
            pnlEditorCentral.Controls.Add(nudMax);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Inițial:", Location = new Point(20, y), AutoSize = true });
            NumericUpDown nudInitial = new NumericUpDown { Value = attr.Initial, Location = new Point(180, y), Minimum = -99999, Maximum = 99999, Width = 100 };
            pnlEditorCentral.Controls.Add(nudInitial);
            y += 35;

            CheckBox chkVis = new CheckBox { Text = "Vizibil în HUD", Checked = attr.VisibleInHud, Location = new Point(180, y), AutoSize = true };
            pnlEditorCentral.Controls.Add(chkVis);
            y += 30;

            pnlEditorCentral.Controls.Add(new Label { Text = "Ordine HUD:", Location = new Point(20, y), AutoSize = true });
            NumericUpDown nudOrder = new NumericUpDown { Value = attr.HudOrder, Location = new Point(180, y), Minimum = 0, Maximum = 999, Width = 100 };
            pnlEditorCentral.Controls.Add(nudOrder);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Redirect Min:", Location = new Point(20, y), AutoSize = true });
            ComboBox cbMinBlock = new ComboBox { Location = new Point(180, y), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMinBlock.Items.Add("(niciunul)");
            foreach (var b in _povesteCurenta.Blocks) cbMinBlock.Items.Add(b.Id);
            cbMinBlock.SelectedItem = string.IsNullOrEmpty(attr.MinBlock) ? "(niciunul)" : attr.MinBlock;
            pnlEditorCentral.Controls.Add(cbMinBlock);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Redirect Max:", Location = new Point(20, y), AutoSize = true });
            ComboBox cbMaxBlock = new ComboBox { Location = new Point(180, y), Width = 250, DropDownStyle = ComboBoxStyle.DropDownList };
            cbMaxBlock.Items.Add("(niciunul)");
            foreach (var b in _povesteCurenta.Blocks) cbMaxBlock.Items.Add(b.Id);
            cbMaxBlock.SelectedItem = string.IsNullOrEmpty(attr.MaxBlock) ? "(niciunul)" : attr.MaxBlock;
            pnlEditorCentral.Controls.Add(cbMaxBlock);
            y += 45;

            Button btnAplica = new Button { Text = "Aplică modificările", Location = new Point(20, y), Width = 200, Height = 35 };
            btnAplica.Click += (s, ev) =>
            {
                if ((int)nudMin.Value > (int)nudMax.Value) { MessageBox.Show("Min trebuie să fie <= Max."); return; }
                if ((int)nudInitial.Value < (int)nudMin.Value || (int)nudInitial.Value > (int)nudMax.Value) { MessageBox.Show("Inițial trebuie să fie între Min și Max."); return; }

                attr.Key = txtKey.Text;
                attr.HudLabel = txtLabel.Text;
                attr.Min = (int)nudMin.Value;
                attr.Max = (int)nudMax.Value;
                attr.Initial = (int)nudInitial.Value;
                attr.VisibleInHud = chkVis.Checked;
                attr.HudOrder = (int)nudOrder.Value;
                attr.MinBlock = cbMinBlock.SelectedItem?.ToString() == "(niciunul)" ? null : cbMinBlock.SelectedItem?.ToString();
                attr.MaxBlock = cbMaxBlock.SelectedItem?.ToString() == "(niciunul)" ? null : cbMaxBlock.SelectedItem?.ToString();

                nodCurent.Text = attr.Key;
                MessageBox.Show("Atributul a fost actualizat.");
            };
            pnlEditorCentral.Controls.Add(btnAplica);

            Button btnSterge = new Button { Text = "Șterge atribut", Location = new Point(240, y), Width = 200, Height = 35, ForeColor = Color.DarkRed };
            btnSterge.Click += (s, ev) =>
            {
                if (MessageBox.Show("Ștergi atributul '" + attr.Key + "'?", "Confirmare", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _povesteCurenta.Attributes.Remove(attr);
                    ActualizeazaTreeView();
                    pnlEditorCentral.Controls.Clear();
                }
            };
            pnlEditorCentral.Controls.Add(btnSterge);
        }

        // ----- EDITOR BLOC COMPLET -----

        private void AfiseazaEditorBloc(BlocPoveste bloc, TreeNode nodCurent)
        {
            int y = 20;

            pnlEditorCentral.Controls.Add(new Label { Text = "ID bloc:", Location = new Point(20, y), AutoSize = true });
            TextBox txtId = new TextBox { Text = bloc.Id ?? "", Location = new Point(150, y), Width = 350, ReadOnly = true };
            pnlEditorCentral.Controls.Add(txtId);
            y += 35;

            pnlEditorCentral.Controls.Add(new Label { Text = "Text narativ:", Location = new Point(20, y), AutoSize = true });
            RichTextBox rtbText = new RichTextBox { Text = bloc.Text ?? "", Location = new Point(150, y), Width = 350, Height = 90 };
            pnlEditorCentral.Controls.Add(rtbText);
            y += 100;

            CheckBox chkFinal = new CheckBox { Text = "Este bloc FINAL (sfârșitul poveștii)", Checked = bloc.IsFinal, Location = new Point(150, y), AutoSize = true };
            pnlEditorCentral.Controls.Add(chkFinal);
            y += 30;

            pnlEditorCentral.Controls.Add(new Label { Text = "Imagine (URL sau cale relativă images/...):", Location = new Point(20, y), AutoSize = true });
            y += 28;
            TextBox txtImage = new TextBox { Text = bloc.Image ?? "", Location = new Point(20, y), Width = 360 };
            pnlEditorCentral.Controls.Add(txtImage);

            // Previzualizarea imaginii blocului (locală sau URL)
            PictureBox pbPreview = new PictureBox { Location = new Point(20, y + 72), Size = new Size(220, 130), BorderStyle = BorderStyle.FixedSingle, SizeMode = PictureBoxSizeMode.Zoom };
            pnlEditorCentral.Controls.Add(pbPreview);

            Button btnRasfoieste = new Button { Text = "Răsfoiește...", Location = new Point(390, y - 1), Width = 110 };
            btnRasfoieste.Click += (s, ev) =>
            {
                using (var ofd = new OpenFileDialog())
                {
                    ofd.Filter = "Imagini (*.jpg;*.png;*.bmp;*.gif)|*.jpg;*.png;*.bmp;*.gif";
                    if (ofd.ShowDialog() == DialogResult.OK)
                    {
                        // Salvăm calea absolută pentru a copia fișierul la salvare în ZIP
                        string nume = Path.GetFileName(ofd.FileName);
                        string referintaRelativa = "images/" + nume;
                        _imaginiNoi[referintaRelativa] = ofd.FileName;
                        txtImage.Text = referintaRelativa;
                        IncarcaPreviewImagine(pbPreview, txtImage.Text);
                    }
                }
            };
            pnlEditorCentral.Controls.Add(btnRasfoieste);

            Button btnPreview = new Button { Text = "Previzualizează", Location = new Point(390, y + 34), Width = 110 };
            btnPreview.Click += (s, ev) => IncarcaPreviewImagine(pbPreview, txtImage.Text);
            pnlEditorCentral.Controls.Add(btnPreview);

            IncarcaPreviewImagine(pbPreview, bloc.Image); // preview inițial
            y += 215;

            // --- Decizii ---
            pnlEditorCentral.Controls.Add(new Label { Text = "Decizii:", Location = new Point(20, y), AutoSize = true });
            ListBox lbDecizii = new ListBox { Location = new Point(150, y), Width = 230, Height = 110 };
            ReincarcaListaDecizii(lbDecizii, bloc);
            pnlEditorCentral.Controls.Add(lbDecizii);

            Button btnAdaugaDecizie = new Button { Text = "+ Decizie", Location = new Point(390, y), Width = 110, Height = 28 };
            btnAdaugaDecizie.Click += (s, ev) =>
            {
                var idBlocuri = _povesteCurenta.Blocks.Select(b => b.Id).ToList();
                var atribute = _povesteCurenta.Attributes.Select(a => a.Key).ToList();
                using (FormDecizie frm = new FormDecizie(idBlocuri, atribute))
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        bloc.Decisions.Add(frm.DecizieRezultat);
                        ReincarcaListaDecizii(lbDecizii, bloc);
                    }
                }
            };
            pnlEditorCentral.Controls.Add(btnAdaugaDecizie);

            Button btnEditDecizie = new Button { Text = "Editează", Location = new Point(390, y + 35), Width = 110, Height = 28 };
            btnEditDecizie.Click += (s, ev) =>
            {
                int idx = lbDecizii.SelectedIndex;
                if (idx < 0 || idx >= bloc.Decisions.Count) { MessageBox.Show("Selectează o decizie."); return; }
                var idBlocuri = _povesteCurenta.Blocks.Select(b => b.Id).ToList();
                var atribute = _povesteCurenta.Attributes.Select(a => a.Key).ToList();
                using (FormDecizie frm = new FormDecizie(idBlocuri, atribute) { DecizieRezultat = bloc.Decisions[idx] })
                {
                    if (frm.ShowDialog() == DialogResult.OK)
                    {
                        bloc.Decisions[idx] = frm.DecizieRezultat;
                        ReincarcaListaDecizii(lbDecizii, bloc);
                    }
                }
            };
            pnlEditorCentral.Controls.Add(btnEditDecizie);

            Button btnStergeDecizie = new Button { Text = "Șterge", Location = new Point(390, y + 70), Width = 110, Height = 28, ForeColor = Color.DarkRed };
            btnStergeDecizie.Click += (s, ev) =>
            {
                int idx = lbDecizii.SelectedIndex;
                if (idx < 0 || idx >= bloc.Decisions.Count) { MessageBox.Show("Selectează o decizie."); return; }
                if (MessageBox.Show("Ștergi decizia selectată?", "Confirmare", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    bloc.Decisions.RemoveAt(idx);
                    ReincarcaListaDecizii(lbDecizii, bloc);
                }
            };
            pnlEditorCentral.Controls.Add(btnStergeDecizie);
            y += 120;

            Button btnAplicaBloc = new Button { Text = "Aplică modificările bloc", Location = new Point(20, y), Width = 200, Height = 35 };
            btnAplicaBloc.Click += (s, ev) =>
            {
                bloc.Text = rtbText.Text;
                bloc.IsFinal = chkFinal.Checked;
                bloc.Image = txtImage.Text;
                nodCurent.Text = bloc.Id + (bloc.IsFinal ? "  [FINAL]" : "");
                MessageBox.Show("Modificările au fost salvate în memorie.");
            };
            pnlEditorCentral.Controls.Add(btnAplicaBloc);

            Button btnStergeBloc = new Button { Text = "Șterge bloc", Location = new Point(240, y), Width = 200, Height = 35, ForeColor = Color.DarkRed };
            btnStergeBloc.Click += (s, ev) =>
            {
                if (MessageBox.Show("Ștergi blocul '" + bloc.Id + "'?", "Confirmare", MessageBoxButtons.YesNo) == DialogResult.Yes)
                {
                    _povesteCurenta.Blocks.Remove(bloc);
                    ActualizeazaTreeView();
                    pnlEditorCentral.Controls.Clear();
                }
            };
            pnlEditorCentral.Controls.Add(btnStergeBloc);
        }

        private void ReincarcaListaDecizii(ListBox lb, BlocPoveste bloc)
        {
            lb.Items.Clear();
            foreach (var d in bloc.Decisions)
                lb.Items.Add($"{d.Text} → [{d.TargetBlock}]");
        }

        // ----- SALVARE ZIP -----

        private void SalveazaPoveste()
        {
            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Pachet Poveste ZIP (*.zip)|*.zip";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    string folderTemporar = Path.Combine(Path.GetTempPath(), "EvadareBranEditor_" + Guid.NewGuid().ToString("N"));
                    Directory.CreateDirectory(folderTemporar);
                    Directory.CreateDirectory(Path.Combine(folderTemporar, "images"));

                    // Copiem imaginile referite (locale, nu URL-uri)
                    CopiazaImaginiReferite(folderTemporar);

                    // story.json
                    var options = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping };
                    string jsonText = JsonSerializer.Serialize(_povesteCurenta, options);
                    File.WriteAllText(Path.Combine(folderTemporar, "story.json"), jsonText);

                    // ZIP final
                    if (File.Exists(sfd.FileName)) File.Delete(sfd.FileName);
                    ZipFile.CreateFromDirectory(folderTemporar, sfd.FileName);
                    Directory.Delete(folderTemporar, true);

                    MessageBox.Show("Povestea a fost salvată în format ZIP.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la crearea arhivei ZIP: " + ex.Message);
                }
            }
        }

        // Copiază imaginile locale referite în story (block.Image + decizie.Icon) în folderul images/
        private void CopiazaImaginiReferite(string folderDestinatie)
        {
            var refImagini = new HashSet<string>();
            foreach (var b in _povesteCurenta.Blocks)
            {
                if (!string.IsNullOrEmpty(b.Image)) refImagini.Add(b.Image);
                foreach (var d in b.Decisions)
                    if (!string.IsNullOrEmpty(d.Icon)) refImagini.Add(d.Icon);
            }

            foreach (var refImg in refImagini)
            {
                // Skip URL-uri (rămân ca link extern în JSON)
                if (refImg.StartsWith("http://") || refImg.StartsWith("https://")) continue;

                // Skip dacă nu e în images/ (referință externă/relativă neclară)
                if (!refImg.StartsWith("images/")) continue;

                string destinatie = Path.Combine(folderDestinatie, refImg.Replace('/', Path.DirectorySeparatorChar));
                Directory.CreateDirectory(Path.GetDirectoryName(destinatie));

                // 1) Imagine adăugată în sesiunea curentă din file dialog
                if (_imaginiNoi.TryGetValue(refImg, out string surseAbsolute) && File.Exists(surseAbsolute))
                {
                    File.Copy(surseAbsolute, destinatie, true);
                    continue;
                }
                // 2) Imagine existentă în folderul de la încărcare (povestea a fost încărcată dintr-un ZIP)
                if (!string.IsNullOrEmpty(_folderResurseSursa))
                {
                    string sursa = Path.Combine(_folderResurseSursa, refImg.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(sursa))
                    {
                        File.Copy(sursa, destinatie, true);
                        continue;
                    }
                }
                // 3) Altfel - lipsește, dar nu blocăm salvarea (validarea avertizează)
            }
        }

        // ----- ÎNCĂRCARE POVESTE EXISTENTĂ -----

        private void IncarcaPoveste()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fișiere Poveste (*.zip;*.json)|*.zip;*.json";
                if (ofd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    _imaginiNoi.Clear();
                    string extensie = Path.GetExtension(ofd.FileName).ToLower();

                    if (extensie == ".zip")
                    {
                        // Curățăm sursa anterioară
                        if (!string.IsNullOrEmpty(_folderResurseSursa) && _folderResurseSursa.Contains("EvadareBranEditorSrc_") && Directory.Exists(_folderResurseSursa))
                        {
                            try { Directory.Delete(_folderResurseSursa, true); } catch { }
                        }
                        _folderResurseSursa = Path.Combine(Path.GetTempPath(), "EvadareBranEditorSrc_" + Guid.NewGuid().ToString("N"));
                        Directory.CreateDirectory(_folderResurseSursa);
                        ZipFile.ExtractToDirectory(ofd.FileName, _folderResurseSursa);

                        string caleaJson = Path.Combine(_folderResurseSursa, "story.json");
                        if (!File.Exists(caleaJson))
                        {
                            MessageBox.Show("Arhiva ZIP nu conține un fișier valid story.json!");
                            return;
                        }
                        string jsonText = File.ReadAllText(caleaJson);
                        _povesteCurenta = JsonSerializer.Deserialize<Poveste>(jsonText) ?? CreazaPovesteGoala();
                    }
                    else if (extensie == ".json")
                    {
                        _folderResurseSursa = Path.GetDirectoryName(ofd.FileName);
                        string jsonText = File.ReadAllText(ofd.FileName);
                        _povesteCurenta = JsonSerializer.Deserialize<Poveste>(jsonText) ?? CreazaPovesteGoala();
                    }

                    ActualizeazaTreeView();
                    pnlEditorCentral.Controls.Clear();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Eroare la deschiderea fișierului: " + ex.Message);
                }
            }
        }

        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>
        // >>> ÎNCEPUT cod PERSOANA 4 — validarea poveștii + jurnalul de validare
        // >>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>>

        // ----- VALIDARE -----

        private List<string> ValideazaPoveste()
        {
            var erori = new List<string>();

            // Titlu
            if (string.IsNullOrWhiteSpace(_povesteCurenta.Title))
                erori.Add("• Lipsește titlul poveștii.");

            // Bloc de start
            var idBlocuri = _povesteCurenta.Blocks.Select(b => b.Id).ToList();
            if (string.IsNullOrWhiteSpace(_povesteCurenta.StartBlock))
                erori.Add("• Lipsește blocul de start.");
            else if (!idBlocuri.Contains(_povesteCurenta.StartBlock))
                erori.Add($"• Blocul de start '{_povesteCurenta.StartBlock}' nu există în listă.");

            // Unicitate ID-uri blocuri
            var dupIds = idBlocuri.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var d in dupIds) erori.Add($"• ID bloc duplicat: '{d}'.");

            // Unicitate chei atribute + min<=initial<=max + redirect blocks
            var cheiAttr = _povesteCurenta.Attributes.Select(a => a.Key).ToList();
            var dupAttr = cheiAttr.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var d in dupAttr) erori.Add($"• Cheie atribut duplicată: '{d}'.");

            foreach (var a in _povesteCurenta.Attributes)
            {
                if (a.Min > a.Max) erori.Add($"• Atribut '{a.Key}': Min ({a.Min}) > Max ({a.Max}).");
                if (a.Initial < a.Min || a.Initial > a.Max) erori.Add($"• Atribut '{a.Key}': Initial ({a.Initial}) nu este între Min și Max.");
                if (!string.IsNullOrEmpty(a.MinBlock) && !idBlocuri.Contains(a.MinBlock)) erori.Add($"• Atribut '{a.Key}': MinBlock '{a.MinBlock}' nu există.");
                if (!string.IsNullOrEmpty(a.MaxBlock) && !idBlocuri.Contains(a.MaxBlock)) erori.Add($"• Atribut '{a.Key}': MaxBlock '{a.MaxBlock}' nu există.");
            }

            // §15.2: HudOrder unic între atributele vizibile în HUD
            var ordVizibile = _povesteCurenta.Attributes.Where(a => a.VisibleInHud).Select(a => a.HudOrder).ToList();
            var dupOrd = ordVizibile.GroupBy(x => x).Where(g => g.Count() > 1).Select(g => g.Key);
            foreach (var o in dupOrd) erori.Add($"• HudOrder duplicat la atribute vizibile: {o}.");

            // Decizii: targetBlock + efecte/condiții referă atribute existente
            foreach (var b in _povesteCurenta.Blocks)
            {
                foreach (var d in b.Decisions)
                {
                    if (string.IsNullOrEmpty(d.TargetBlock) || !idBlocuri.Contains(d.TargetBlock))
                        erori.Add($"• Bloc '{b.Id}': decizia '{d.Text}' țintește bloc inexistent '{d.TargetBlock}'.");
                    foreach (var ef in d.Effects)
                    {
                        if (!cheiAttr.Contains(ef.Property))
                            erori.Add($"• Bloc '{b.Id}': efect pe atribut inexistent '{ef.Property}'.");
                        if (ef.Type != "ADD" && ef.Type != "SET")
                            erori.Add($"• Bloc '{b.Id}': efect cu tip invalid '{ef.Type}' (acceptat: ADD, SET).");
                    }
                    if (d.Condition != null)
                        ValideazaConditie(d.Condition, cheiAttr, $"Bloc '{b.Id}' decizia '{d.Text}'", erori);
                }
            }

            // §15.4: validarea resurselor (imagini)
            ValideazaImagini(erori);

            // §12.3: verificare accesibilitate (avertisment)
            VerificaAccesibilitate(erori);

            return erori;
        }

        private static readonly HashSet<string> _operatoriValizi = new HashSet<string> { "==", "!=", ">", ">=", "<", "<=" };
        private static readonly HashSet<string> _extensiiImaginiValide = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            { ".jpg", ".jpeg", ".png", ".bmp", ".gif" };

        private void ValideazaConditie(Conditie c, List<string> cheiAttr, string ctx, List<string> erori)
        {
            if (c == null) return;
            if (c.Type == "COMPARISON")
            {
                if (string.IsNullOrEmpty(c.Property) || !cheiAttr.Contains(c.Property))
                    erori.Add($"• {ctx}: condiție pe atribut inexistent '{c.Property}'.");
                if (string.IsNullOrEmpty(c.Operator) || !_operatoriValizi.Contains(c.Operator))
                    erori.Add($"• {ctx}: operator invalid '{c.Operator}' (acceptat: ==, !=, >, >=, <, <=).");
            }
            else if (c.Type == "AND" || c.Type == "OR")
            {
                if (c.Conditions == null || c.Conditions.Count == 0)
                    erori.Add($"• {ctx}: nod {c.Type} fără sub-condiții.");
                else
                    foreach (var sub in c.Conditions) ValideazaConditie(sub, cheiAttr, ctx, erori);
            }
            else
            {
                erori.Add($"• {ctx}: tip condiție necunoscut '{c.Type}' (acceptat: COMPARISON, AND, OR).");
            }
        }

        // §15.4: imaginile referite trebuie să existe și să aibă extensii compatibile
        private void ValideazaImagini(List<string> erori)
        {
            var refImagini = new List<(string Cale, string Ctx)>();
            foreach (var b in _povesteCurenta.Blocks)
            {
                if (!string.IsNullOrEmpty(b.Image)) refImagini.Add((b.Image, $"imagine bloc '{b.Id}'"));
                foreach (var d in b.Decisions)
                    if (!string.IsNullOrEmpty(d.Icon)) refImagini.Add((d.Icon, $"iconiță decizie '{d.Text}' din bloc '{b.Id}'"));
            }

            foreach (var (cale, ctx) in refImagini)
            {
                // URL-uri externe nu se validează local
                if (cale.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    cale.StartsWith("https://", StringComparison.OrdinalIgnoreCase)) continue;

                string ext = Path.GetExtension(cale);
                if (!_extensiiImaginiValide.Contains(ext))
                    erori.Add($"• {ctx}: extensie incompatibilă '{ext}' (acceptate: .jpg, .jpeg, .png, .bmp, .gif).");

                // Calea trebuie să fie sub images/
                if (!cale.Replace('\\', '/').StartsWith("images/", StringComparison.OrdinalIgnoreCase))
                {
                    erori.Add($"• {ctx}: calea '{cale}' nu este sub 'images/'.");
                    continue;
                }

                // Imaginea trebuie să existe fie ca adăugare nouă, fie în folderul sursă
                bool existaNoua = _imaginiNoi.TryGetValue(cale, out string sursaNoua) && File.Exists(sursaNoua);
                bool existaSursa = false;
                if (!existaNoua && !string.IsNullOrEmpty(_folderResurseSursa))
                {
                    string p = Path.Combine(_folderResurseSursa, cale.Replace('/', Path.DirectorySeparatorChar));
                    existaSursa = File.Exists(p);
                }
                if (!existaNoua && !existaSursa)
                    erori.Add($"• {ctx}: fișierul '{cale}' nu există (nu a fost adăugat și nu se găsește în arhiva sursă).");
            }
        }

        // §12.3: BFS din blocul de start - blocurile inaccesibile sunt avertisment
        private void VerificaAccesibilitate(List<string> erori)
        {
            if (string.IsNullOrEmpty(_povesteCurenta.StartBlock)) return;
            var index = _povesteCurenta.Blocks.ToDictionary(b => b.Id ?? "", b => b, StringComparer.Ordinal);
            if (!index.ContainsKey(_povesteCurenta.StartBlock)) return;

            var vizitate = new HashSet<string> { _povesteCurenta.StartBlock };
            var coada = new Queue<string>();
            coada.Enqueue(_povesteCurenta.StartBlock);

            while (coada.Count > 0)
            {
                var idCurent = coada.Dequeue();
                if (!index.TryGetValue(idCurent, out var bloc)) continue;
                foreach (var d in bloc.Decisions)
                {
                    if (!string.IsNullOrEmpty(d.TargetBlock) && vizitate.Add(d.TargetBlock))
                        coada.Enqueue(d.TargetBlock);
                }
            }

            // Adăugăm și redirecționările de atribute ca puncte de intrare reachable
            foreach (var a in _povesteCurenta.Attributes)
            {
                if (!string.IsNullOrEmpty(a.MinBlock) && vizitate.Add(a.MinBlock)) coada.Enqueue(a.MinBlock);
                if (!string.IsNullOrEmpty(a.MaxBlock) && vizitate.Add(a.MaxBlock)) coada.Enqueue(a.MaxBlock);
            }
            while (coada.Count > 0)
            {
                var idCurent = coada.Dequeue();
                if (!index.TryGetValue(idCurent, out var bloc)) continue;
                foreach (var d in bloc.Decisions)
                    if (!string.IsNullOrEmpty(d.TargetBlock) && vizitate.Add(d.TargetBlock))
                        coada.Enqueue(d.TargetBlock);
            }

            foreach (var b in _povesteCurenta.Blocks)
                if (!vizitate.Contains(b.Id))
                    erori.Add($"• Bloc inaccesibil din povestea curentă: '{b.Id}'.");
        }

        // ----- JURNAL DE VALIDARE -----

        private void btnValideaza_Click(object sender, EventArgs e)
        {
            RuleazaValidareInJurnal();
        }

        // Rulează validarea și afișează rezultatele în jurnalul de jos (PDF §14.1.3)
        private void RuleazaValidareInJurnal()
        {
            if (lbJurnal == null) return;
            lbJurnal.Items.Clear();
            var erori = ValideazaPoveste();
            if (erori.Count == 0)
            {
                lbJurnal.Items.Add("✓ Nicio problemă — povestea este validă.");
            }
            else
            {
                lbJurnal.Items.Add($"⚠ {erori.Count} probleme găsite:");
                foreach (var er in erori) lbJurnal.Items.Add(er);
            }
        }

        // <<< SFÂRȘIT cod PERSOANA 4
        // <<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<<

        // ----- PREVIEW IMAGINE -----  (Persoana 3)

        // Încarcă o imagine (URL sau cale locală) într-un PictureBox de previzualizare
        private void IncarcaPreviewImagine(PictureBox pb, string cale)
        {
            if (pb.Image != null) { pb.Image.Dispose(); pb.Image = null; }
            if (string.IsNullOrWhiteSpace(cale)) return;
            try
            {
                if (cale.StartsWith("http://") || cale.StartsWith("https://"))
                {
                    pb.LoadAsync(cale);
                    return;
                }
                string abs = null;
                if (_imaginiNoi.TryGetValue(cale, out var sursaNoua) && File.Exists(sursaNoua))
                    abs = sursaNoua;
                else if (!string.IsNullOrEmpty(_folderResurseSursa))
                {
                    string p = Path.Combine(_folderResurseSursa, cale.Replace('/', Path.DirectorySeparatorChar));
                    if (File.Exists(p)) abs = p;
                }
                if (abs != null)
                    using (var fs = new FileStream(abs, FileMode.Open, FileAccess.Read))
                        pb.Image = Image.FromStream(fs);
            }
            catch { pb.Image = null; }
        }

        // ----- UTILITAR DIALOG TEXT -----

        private string PromptText(string mesaj, string titlu)
        {
            using (Form dlg = new Form())
            {
                dlg.Text = titlu;
                dlg.Width = 400;
                dlg.Height = 150;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;

                Label lbl = new Label { Text = mesaj, Location = new Point(20, 15), AutoSize = true };
                TextBox txt = new TextBox { Location = new Point(20, 45), Width = 340 };
                Button ok = new Button { Text = "OK", Location = new Point(195, 75), DialogResult = DialogResult.OK };
                Button cancel = new Button { Text = "Anulează", Location = new Point(280, 75), DialogResult = DialogResult.Cancel };
                dlg.Controls.Add(lbl); dlg.Controls.Add(txt); dlg.Controls.Add(ok); dlg.Controls.Add(cancel);
                dlg.AcceptButton = ok;
                dlg.CancelButton = cancel;

                return dlg.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }
    }
}
