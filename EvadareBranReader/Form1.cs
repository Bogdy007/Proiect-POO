using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Windows.Forms;
using Story.Core.Engine;
using Story.Core.Models;

namespace EvadareBranReader
{
    public partial class Form1 : Form
    {
        
        private MotorPoveste _motor = new MotorPoveste();

        // Folderul cu resurse (imagini) al poveștii curente
        private string _folderResurse = null;

        public Form1()
        {
            InitializeComponent();
        }

        // ----- MENIU -----

        private void deschideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeschidePoveste();
        }

        private void restartToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_motor.PovesteCurenta == null)
            {
                MessageBox.Show("Nu ai încărcat nicio poveste.");
                return;
            }
            _motor.Restart();
            AfiseazaBlocCurent();
        }

        private void inapoiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_motor.MergiInapoi())
                AfiseazaBlocCurent();
        }

        private void salveazaStareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_motor.PovesteCurenta == null)
            {
                MessageBox.Show("Nu ai nicio poveste încărcată.");
                return;
            }

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = "Stare joc (*.json)|*.json";
                sfd.FileName = "stare.json";
                if (sfd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        File.WriteAllText(sfd.FileName, _motor.ExportaStare());
                        MessageBox.Show("Starea a fost salvată.");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la salvare: " + ex.Message);
                    }
                }
            }
        }

        private void incarcaStareToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_motor.PovesteCurenta == null)
            {
                MessageBox.Show("Încarcă întâi o poveste, apoi poți importa o stare salvată.");
                return;
            }

            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Stare joc (*.json)|*.json";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        string txt = File.ReadAllText(ofd.FileName);
                        _motor.ImportaStare(txt);
                        AfiseazaBlocCurent();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la încărcarea stării: " + ex.Message);
                    }
                }
            }
        }

        private void iesireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CurataFolderResurse();
        }

        // ----- ÎNCĂRCARE POVESTE -----

        private void DeschidePoveste()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fișiere Poveste (*.zip;*.json)|*.zip;*.json";

                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        CurataFolderResurse();

                        string extensie = Path.GetExtension(ofd.FileName).ToLower();
                        string jsonText = "";

                        if (extensie == ".zip")
                        {
                            // Folder persistent cât rulează jocul (ca să avem imaginile pe disc)
                            _folderResurse = Path.Combine(Path.GetTempPath(), "EvadareBranReader_" + Guid.NewGuid().ToString("N"));
                            Directory.CreateDirectory(_folderResurse);
                            ZipFile.ExtractToDirectory(ofd.FileName, _folderResurse);

                            string caleaJson = Path.Combine(_folderResurse, "story.json");
                            if (!File.Exists(caleaJson))
                            {
                                MessageBox.Show("Arhiva ZIP nu conține povestea (story.json)!");
                                CurataFolderResurse();
                                return;
                            }
                            jsonText = File.ReadAllText(caleaJson);
                        }
                        else if (extensie == ".json")
                        {
                            // Pentru JSON, folderul de resurse e cel al fișierului
                            _folderResurse = Path.GetDirectoryName(ofd.FileName);
                            jsonText = File.ReadAllText(ofd.FileName);
                        }

                        if (!string.IsNullOrEmpty(jsonText))
                        {
                            _motor.IncarcaPovesteJson(jsonText);
                            AfiseazaBlocCurent();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Eroare la citirea poveștii: " + ex.Message);
                    }
                }
            }
        }

        // Ștergem folderul temporar doar dacă l-am creat noi
        private void CurataFolderResurse()
        {
            if (!string.IsNullOrEmpty(_folderResurse)
                && _folderResurse.Contains("EvadareBranReader_")
                && Directory.Exists(_folderResurse))
            {
                try { Directory.Delete(_folderResurse, true); }
                catch { /* ignore */ }
            }
            _folderResurse = null;
        }

        // ----- AFIȘARE -----

        private void AfiseazaBlocCurent()
        {
            lblTitlu.Text = string.IsNullOrEmpty(_motor.PovesteCurenta?.Title)
                ? "(poveste fără titlu)"
                : _motor.PovesteCurenta.Title;
            lblBlocCurent.Text = "Bloc: " + (_motor.IdBlocCurent ?? "?");
            inapoiToolStripMenuItem.Enabled = _motor.PoateMergeInapoi;

            var bloc = _motor.ObtineBlocCurent();
            if (bloc == null)
            {
                rtbPoveste.Text = "(blocul curent nu există în poveste)";
                pbImagineBloc.Image = null;
                flpDecizii.Controls.Clear();
                return;
            }

            rtbPoveste.Text = bloc.Text;
            AfiseazaImagineBloc(bloc);
            ActualizeazaHUD();

            if (bloc.IsFinal)
                AfiseazaFinal();
            else
                GenereazaButoaneDecizii(bloc);
        }

        // Imagine de fundal: URL (http/https) sau cale locală relativă la folderul resurse
        private void AfiseazaImagineBloc(BlocPoveste bloc)
        {
            if (pbImagineBloc.Image != null)
            {
                pbImagineBloc.Image.Dispose();
                pbImagineBloc.Image = null;
            }

            if (string.IsNullOrEmpty(bloc.Image)) return;

            try
            {
                if (bloc.Image.StartsWith("http://") || bloc.Image.StartsWith("https://"))
                {
                    // Asincron - PictureBox.LoadAsync e thread-safe
                    pbImagineBloc.LoadAsync(bloc.Image);
                }
                else if (!string.IsNullOrEmpty(_folderResurse))
                {
                    string caleaImagine = Path.Combine(_folderResurse, bloc.Image);
                    if (File.Exists(caleaImagine))
                    {
                        using (var fs = new FileStream(caleaImagine, FileMode.Open, FileAccess.Read))
                        {
                            pbImagineBloc.Image = Image.FromStream(fs);
                        }
                    }
                }
            }
            catch
            {
                pbImagineBloc.Image = null;
            }
        }

        private void ActualizeazaHUD()
        {
            pnlHUD.Controls.Clear();
            int y = 10;

            // Sortăm vizibile după HudOrder (PDF §4)
            var atribVizibile = _motor.PovesteCurenta.Attributes
                .Where(a => a.VisibleInHud)
                .OrderBy(a => a.HudOrder);

            foreach (var attr in atribVizibile)
            {
                if (!_motor.StareAtribute.ContainsKey(attr.Key)) continue;

                int valoareCurenta = _motor.StareAtribute[attr.Key];
                Label lbl = new Label();
                lbl.Text = $"{attr.HudLabel}: {valoareCurenta} / {attr.Max}";
                lbl.Location = new Point(10, y);
                lbl.AutoSize = true;
                lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);

                pnlHUD.Controls.Add(lbl);
                y += 25;
            }
        }

        private void GenereazaButoaneDecizii(BlocPoveste bloc)
        {
            flpDecizii.Controls.Clear();

            foreach (var decizie in bloc.Decisions)
            {
                // Filtrare decizii pe baza condițiilor AST (PDF §3.2.3)
                if (!_motor.EvalueazaConditie(decizie.Condition)) continue;

                Button btn = new Button();
                btn.Text = decizie.Text;
                btn.Width = 540;
                btn.Height = 40;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Tag = decizie;
                btn.Click += BtnDecizie_Click;

                flpDecizii.Controls.Add(btn);
            }

            if (flpDecizii.Controls.Count == 0)
            {
                Label lblNimic = new Label();
                lblNimic.Text = "(nu există decizii disponibile)";
                lblNimic.AutoSize = true;
                lblNimic.ForeColor = Color.Gray;
                flpDecizii.Controls.Add(lblNimic);
            }
        }

        private void AfiseazaFinal()
        {
            flpDecizii.Controls.Clear();

            Label lblSfarsit = new Label();
            lblSfarsit.Text = "*** SFÂRȘITUL POVEȘTII ***";
            lblSfarsit.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblSfarsit.ForeColor = Color.DarkRed;
            lblSfarsit.AutoSize = true;
            flpDecizii.Controls.Add(lblSfarsit);

            Button btnRestart = new Button();
            btnRestart.Text = "Restart poveste";
            btnRestart.Width = 200;
            btnRestart.Height = 35;
            btnRestart.Click += (s, ev) =>
            {
                _motor.Restart();
                AfiseazaBlocCurent();
            };
            flpDecizii.Controls.Add(btnRestart);
        }

        private void BtnDecizie_Click(object sender, EventArgs e)
        {
            Button btnApasat = sender as Button;
            Decizie decizie = btnApasat.Tag as Decizie;

            // Aplicăm efectele și verificăm redirecționarea pe min/max
            string blocRedirectionare = _motor.AplicaEfecteSiObtineRedirectionare(decizie);

            if (!string.IsNullOrEmpty(blocRedirectionare))
                _motor.MutaLaBloc(blocRedirectionare);
            else
                _motor.MutaLaBloc(decizie.TargetBlock);

            AfiseazaBlocCurent();
        }
    }
}
