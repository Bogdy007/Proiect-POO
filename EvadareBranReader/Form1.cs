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
        // Motorul ține toată logica jocului (ce bloc suntem, atribute, istoric).
        // Reader-ul doar îl întreabă și afișează — nu calculează nimic singur.
        private MotorPoveste _motor = new MotorPoveste();

        // Unde sunt pozele poveștii curente. Pentru .zip e un folder temporar pe
        // care îl facem noi; pentru .json e folderul fișierului. Null = nimic încărcat.
        private string _folderResurse = null;

        public Form1()
        {
            InitializeComponent();
        }

        // Meniu „Deschide poveste" — toată treaba grea e în DeschidePoveste().
        private void deschideToolStripMenuItem_Click(object sender, EventArgs e)
        {
            DeschidePoveste();
        }

        // „Restart" — o luăm de la capăt, dar doar dacă chiar avem ce reseta.
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

        // „Înapoi" — motorul zice dacă se poate (are istoric). Dacă da, redesenăm.
        private void inapoiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_motor.MergiInapoi())
                AfiseazaBlocCurent();
        }

        // Salvează unde ai ajuns + atributele, ca să poți continua mai târziu.
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
                        // Motorul îmi dă starea ca text JSON, eu doar o scriu pe disc.
                        File.WriteAllText(sfd.FileName, _motor.ExportaStare());
                        MessageBox.Show("Starea a fost salvată.");
                    }
                    catch (Exception ex)
                    {
                        // Ex: fișier blocat / fără drepturi de scriere — nu vrem să crape aplicația.
                        MessageBox.Show("Eroare la salvare: " + ex.Message);
                    }
                }
            }
        }

        // Încarcă o stare salvată. Atenție: trebuie să ai deja povestea deschisă,
        // altfel starea n-are peste ce să se aplice.
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
                        _motor.ImportaStare(txt);   // motorul se ocupă să ne ducă în blocul salvat
                        AfiseazaBlocCurent();
                    }
                    catch (Exception ex)
                    {
                        // JSON stricat sau de la altă poveste — prindem și anunțăm frumos.
                        MessageBox.Show("Eroare la încărcarea stării: " + ex.Message);
                    }
                }
            }
        }

        // „Ieșire" — închiderea declanșează Form1_FormClosing, unde facem curățenia.
        private void iesireToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        // Când se închide fereastra ștergem folderul temporar, să nu lăsăm gunoi în Temp.
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            CurataFolderResurse();
        }

        // Deschide o poveste din .zip (poveste + poze la pachet) sau .json (doar textul).
        private void DeschidePoveste()
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Fișiere Poveste (*.zip;*.json)|*.zip;*.json";
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Scăpăm întâi de povestea veche (folderul temporar de dinainte).
                        CurataFolderResurse();
                        string extensie = Path.GetExtension(ofd.FileName).ToLower();
                        string jsonText = "";

                        if (extensie == ".zip")
                        {
                            // Dezarhivăm într-un folder temporar cu nume unic (Guid), ca să nu
                            // ne încurcăm cu o poveste deschisă altădată. Stă cât ține jocul.
                            _folderResurse = Path.Combine(Path.GetTempPath(), "EvadareBranReader_" + Guid.NewGuid().ToString("N"));
                            Directory.CreateDirectory(_folderResurse);
                            ZipFile.ExtractToDirectory(ofd.FileName, _folderResurse);

                            // Prin convenție povestea e mereu story.json în rădăcina arhivei.
                            string caleaJson = Path.Combine(_folderResurse, "story.json");
                            if (!File.Exists(caleaJson))
                            {
                                MessageBox.Show("Arhiva ZIP nu conține povestea (story.json)!");
                                CurataFolderResurse();   // n-are rost să ținem folderul gol
                                return;
                            }
                            jsonText = File.ReadAllText(caleaJson);
                        }
                        else if (extensie == ".json")
                        {
                            // La .json simplu, pozele (dacă există) sunt lângă fișier.
                            _folderResurse = Path.GetDirectoryName(ofd.FileName);
                            jsonText = File.ReadAllText(ofd.FileName);
                        }

                        // Dăm textul motorului și afișăm primul bloc.
                        if (!string.IsNullOrEmpty(jsonText))
                        {
                            _motor.IncarcaPovesteJson(jsonText);
                            AfiseazaBlocCurent();
                        }
                    }
                    catch (Exception ex)
                    {
                        // Orice merge prost (fișier corupt, JSON invalid) ajunge aici.
                        MessageBox.Show("Eroare la citirea poveștii: " + ex.Message);
                    }
                }
            }
        }

        // Șterge folderul temporar al poveștii. Verificăm numele („EvadareBranReader_")
        // ca să fim 100% siguri că ștergem folderul NOSTRU, nu altceva din greșeală.
        private void CurataFolderResurse()
        {
            if (!string.IsNullOrEmpty(_folderResurse)
                && _folderResurse.Contains("EvadareBranReader_")
                && Directory.Exists(_folderResurse))
            {
                // Dacă ștergerea nu reușește (vreun fișier încă blocat) nu e tragedie,
                // Windows curăță Temp oricum — așa că ignorăm liniștiți eroarea.
                try { Directory.Delete(_folderResurse, true); }
                catch { /* nu ne strică ziua */ }
            }
            _folderResurse = null;
        }

        // Inima Reader-ului: ia blocul curent de la motor și împrospătează tot ecranul.
        // O chemăm după ORICE schimbare (deschidere, decizie, restart, înapoi...).
        private void AfiseazaBlocCurent()
        {
            // Titlul poveștii (cu fallback dacă autorul n-a pus titlu).
            lblTitlu.Text = string.IsNullOrEmpty(_motor.PovesteCurenta?.Title)
                ? "(poveste fără titlu)"
                : _motor.PovesteCurenta.Title;

            // Mic indicator în ce bloc suntem — util și la debug.
            lblBlocCurent.Text = "Bloc: " + (_motor.IdBlocCurent ?? "?");

            // Activăm „Înapoi" doar dacă avem unde să ne întoarcem.
            inapoiToolStripMenuItem.Enabled = _motor.PoateMergeInapoi;

            var bloc = _motor.ObtineBlocCurent();
            if (bloc == null)
            {
                // Caz de siguranță: o decizie trimite spre un bloc care nu există.
                rtbPoveste.Text = "(blocul curent nu există în poveste)";
                pbImagineBloc.Image = null;
                flpDecizii.Controls.Clear();
                return;
            }

            // Textul propriu-zis + poza + barele cu atribute.
            rtbPoveste.Text = bloc.Text;
            AfiseazaImagineBloc(bloc);
            ActualizeazaHUD();

            // Dacă e bloc final arătăm „sfârșit", altfel desenăm butoanele de alegere.
            if (bloc.IsFinal)
                AfiseazaFinal();
            else
                GenereazaButoaneDecizii(bloc);
        }

        // Pune poza blocului în PictureBox. Suportă atât link (http) cât și poză locală.
        private void AfiseazaImagineBloc(BlocPoveste bloc)
        {
            // Eliberăm poza veche, altfel pierdem memorie de la un bloc la altul.
            if (pbImagineBloc.Image != null)
            {
                pbImagineBloc.Image.Dispose();
                pbImagineBloc.Image = null;
            }
            if (string.IsNullOrEmpty(bloc.Image)) return;   // blocul n-are poză

            try
            {
                if (bloc.Image.StartsWith("http://") || bloc.Image.StartsWith("https://"))
                {
                    // Descărcare asincronă — LoadAsync nu blochează fereastra cât se ia poza.
                    pbImagineBloc.LoadAsync(bloc.Image);
                }
                else if (!string.IsNullOrEmpty(_folderResurse))
                {
                    // Poză locală: o căutăm relativ la folderul resurselor.
                    string caleaImagine = Path.Combine(_folderResurse, bloc.Image);
                    if (File.Exists(caleaImagine))
                    {
                        // Citim prin FileStream și copiem în memorie (Image.FromStream),
                        // ca să NU rămână fișierul blocat — Image.FromFile l-ar ține ocupat
                        // și n-am mai putea șterge folderul temporar la final.
                        using (var fs = new FileStream(caleaImagine, FileMode.Open, FileAccess.Read))
                        {
                            pbImagineBloc.Image = Image.FromStream(fs);
                        }
                    }
                }
            }
            catch
            {
                // Poză lipsă / link mort / format ciudat — mai bine fără poză decât crash.
                pbImagineBloc.Image = null;
            }
        }

        // Redesenează HUD-ul (viață, energie, bani etc.) din starea curentă a atributelor.
        private void ActualizeazaHUD()
        {
            pnlHUD.Controls.Clear();   // ștergem etichetele vechi și le facem la loc
            int y = 10;

            // Arătăm doar atributele marcate „vizibile" și în ordinea cerută de autor.
            var atribVizibile = _motor.PovesteCurenta.Attributes
                .Where(a => a.VisibleInHud)
                .OrderBy(a => a.HudOrder);

            foreach (var attr in atribVizibile)
            {
                // Dacă din vreun motiv atributul n-are valoare în stare, îl sărim.
                if (!_motor.StareAtribute.ContainsKey(attr.Key)) continue;
                int valoareCurenta = _motor.StareAtribute[attr.Key];

                // Construim eticheta la runtime și o așezăm pe verticală (y += 25).
                Label lbl = new Label();
                lbl.Text = $"{attr.HudLabel}: {valoareCurenta} / {attr.Max}";
                lbl.Location = new Point(10, y);
                lbl.AutoSize = true;
                lbl.Font = new Font("Segoe UI", 10, FontStyle.Bold);
                pnlHUD.Controls.Add(lbl);
                y += 25;
            }
        }

        // Creează câte un buton pentru fiecare decizie disponibilă din bloc.
        private void GenereazaButoaneDecizii(BlocPoveste bloc)
        {
            flpDecizii.Controls.Clear();

            foreach (var decizie in bloc.Decisions)
            {
                // Unele alegeri apar doar dacă o condiție e adevărată (ex: „ai cheia").
                // Motorul evaluează condiția; dacă nu trece, nici nu arătăm butonul.
                if (!_motor.EvalueazaConditie(decizie.Condition)) continue;

                Button btn = new Button();
                btn.Text = decizie.Text;
                btn.Width = 540;
                btn.Height = 40;
                btn.TextAlign = ContentAlignment.MiddleLeft;
                btn.Tag = decizie;          // „lipim" decizia de buton ca s-o regăsim la click
                btn.Click += BtnDecizie_Click;
                flpDecizii.Controls.Add(btn);
            }

            // Dacă nicio decizie nu e disponibilă, măcar spunem ceva (fundătură fără final).
            if (flpDecizii.Controls.Count == 0)
            {
                Label lblNimic = new Label();
                lblNimic.Text = "(nu există decizii disponibile)";
                lblNimic.AutoSize = true;
                lblNimic.ForeColor = Color.Gray;
                flpDecizii.Controls.Add(lblNimic);
            }
        }

        // Ecranul de final: mesaj de sfârșit + buton de restart.
        private void AfiseazaFinal()
        {
            flpDecizii.Controls.Clear();

            Label lblSfarsit = new Label();
            lblSfarsit.Text = "*** SFÂRȘITUL POVEȘTII ***";
            lblSfarsit.Font = new Font("Segoe UI", 14, FontStyle.Bold);
            lblSfarsit.ForeColor = Color.DarkRed;
            lblSfarsit.AutoSize = true;
            flpDecizii.Controls.Add(lblSfarsit);

            // Buton de restart cu handler scris pe loc (lambda) — e simplu, n-are rost o metodă separată.
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

        // Click pe o decizie: aplicăm efectele, aflăm unde mergem și redesenăm.
        private void BtnDecizie_Click(object sender, EventArgs e)
        {
            // Scoatem înapoi decizia pe care am „lipit-o" în Tag la generarea butonului.
            Button btnApasat = sender as Button;
            Decizie decizie = btnApasat.Tag as Decizie;

            // Motorul aplică efectele (scade viață, adaugă bani...) și ne poate spune
            // că suntem redirecționați altundeva (ex: viața a ajuns la 0 → blocul de moarte).
            string blocRedirectionare = _motor.AplicaEfecteSiObtineRedirectionare(decizie);

            // Dacă există redirecționare, ea are prioritate; altfel mergem unde zice decizia.
            if (!string.IsNullOrEmpty(blocRedirectionare))
                _motor.MutaLaBloc(blocRedirectionare);
            else
                _motor.MutaLaBloc(decizie.TargetBlock);

            AfiseazaBlocCurent();
        }
    }
}
