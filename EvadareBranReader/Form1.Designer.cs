namespace EvadareBranReader
{
    partial class Form1
    {
       
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

     
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            fisierToolStripMenuItem = new ToolStripMenuItem();
            deschideToolStripMenuItem = new ToolStripMenuItem();
            restartToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            salveazaStareToolStripMenuItem = new ToolStripMenuItem();
            incarcaStareToolStripMenuItem = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            iesireToolStripMenuItem = new ToolStripMenuItem();
            inapoiToolStripMenuItem = new ToolStripMenuItem();
            lblTitlu = new Label();
            lblBlocCurent = new Label();
            rtbPoveste = new RichTextBox();
            pbImagineBloc = new PictureBox();
            pnlHUD = new Panel();
            flpDecizii = new FlowLayoutPanel();
            menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pbImagineBloc).BeginInit();
            SuspendLayout();
            //
            // menuStrip1
            //
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { fisierToolStripMenuItem, inapoiToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(900, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            //
            // fisierToolStripMenuItem
            //
            fisierToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] {
                deschideToolStripMenuItem,
                restartToolStripMenuItem,
                toolStripSeparator1,
                salveazaStareToolStripMenuItem,
                incarcaStareToolStripMenuItem,
                toolStripSeparator2,
                iesireToolStripMenuItem });
            fisierToolStripMenuItem.Name = "fisierToolStripMenuItem";
            fisierToolStripMenuItem.Size = new Size(54, 24);
            fisierToolStripMenuItem.Text = "Fișier";
            //
            // deschideToolStripMenuItem
            //
            deschideToolStripMenuItem.Name = "deschideToolStripMenuItem";
            deschideToolStripMenuItem.Size = new Size(220, 26);
            deschideToolStripMenuItem.Text = "Deschide poveste...";
            deschideToolStripMenuItem.Click += deschideToolStripMenuItem_Click;
            //
            // restartToolStripMenuItem
            //
            restartToolStripMenuItem.Name = "restartToolStripMenuItem";
            restartToolStripMenuItem.Size = new Size(220, 26);
            restartToolStripMenuItem.Text = "Restart poveste";
            restartToolStripMenuItem.Click += restartToolStripMenuItem_Click;
            //
            // toolStripSeparator1
            //
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(217, 6);
            //
            // salveazaStareToolStripMenuItem
            //
            salveazaStareToolStripMenuItem.Name = "salveazaStareToolStripMenuItem";
            salveazaStareToolStripMenuItem.Size = new Size(220, 26);
            salveazaStareToolStripMenuItem.Text = "Salvează stare...";
            salveazaStareToolStripMenuItem.Click += salveazaStareToolStripMenuItem_Click;
            //
            // incarcaStareToolStripMenuItem
            //
            incarcaStareToolStripMenuItem.Name = "incarcaStareToolStripMenuItem";
            incarcaStareToolStripMenuItem.Size = new Size(220, 26);
            incarcaStareToolStripMenuItem.Text = "Încarcă stare...";
            incarcaStareToolStripMenuItem.Click += incarcaStareToolStripMenuItem_Click;
            //
            // toolStripSeparator2
            //
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(217, 6);
            //
            // iesireToolStripMenuItem
            //
            iesireToolStripMenuItem.Name = "iesireToolStripMenuItem";
            iesireToolStripMenuItem.Size = new Size(220, 26);
            iesireToolStripMenuItem.Text = "Ieșire";
            iesireToolStripMenuItem.Click += iesireToolStripMenuItem_Click;
            //
            // inapoiToolStripMenuItem
            //
            inapoiToolStripMenuItem.Name = "inapoiToolStripMenuItem";
            inapoiToolStripMenuItem.Size = new Size(80, 24);
            inapoiToolStripMenuItem.Text = "◄ Înapoi";
            inapoiToolStripMenuItem.Enabled = false;
            inapoiToolStripMenuItem.Click += inapoiToolStripMenuItem_Click;
            //
            // lblTitlu
            //
            lblTitlu.Font = new Font("Segoe UI", 14F, FontStyle.Bold);
            lblTitlu.Location = new Point(20, 35);
            lblTitlu.Name = "lblTitlu";
            lblTitlu.Size = new Size(860, 30);
            lblTitlu.TabIndex = 1;
            lblTitlu.Text = "(nicio poveste încărcată)";
            lblTitlu.TextAlign = ContentAlignment.MiddleCenter;
            //
            // lblBlocCurent
            //
            lblBlocCurent.Font = new Font("Segoe UI", 8F, FontStyle.Italic);
            lblBlocCurent.ForeColor = Color.Gray;
            lblBlocCurent.Location = new Point(20, 65);
            lblBlocCurent.Name = "lblBlocCurent";
            lblBlocCurent.Size = new Size(860, 18);
            lblBlocCurent.TabIndex = 2;
            lblBlocCurent.Text = "";
            lblBlocCurent.TextAlign = ContentAlignment.MiddleCenter;
            //
            // rtbPoveste
            //
            rtbPoveste.BorderStyle = BorderStyle.FixedSingle;
            rtbPoveste.Font = new Font("Segoe UI", 10F);
            rtbPoveste.Location = new Point(20, 95);
            rtbPoveste.Name = "rtbPoveste";
            rtbPoveste.ReadOnly = true;
            rtbPoveste.Size = new Size(540, 210);
            rtbPoveste.TabIndex = 3;
            rtbPoveste.Text = "";
            //
            // pbImagineBloc
            //
            pbImagineBloc.BorderStyle = BorderStyle.FixedSingle;
            pbImagineBloc.Location = new Point(580, 95);
            pbImagineBloc.Name = "pbImagineBloc";
            pbImagineBloc.Size = new Size(300, 210);
            pbImagineBloc.SizeMode = PictureBoxSizeMode.Zoom;
            pbImagineBloc.TabIndex = 4;
            pbImagineBloc.TabStop = false;
            //
            // pnlHUD
            //
            pnlHUD.BorderStyle = BorderStyle.FixedSingle;
            pnlHUD.Location = new Point(20, 320);
            pnlHUD.Name = "pnlHUD";
            pnlHUD.Size = new Size(260, 220);
            pnlHUD.TabIndex = 5;
            //
            // flpDecizii
            //
            flpDecizii.AutoScroll = true;
            flpDecizii.BorderStyle = BorderStyle.FixedSingle;
            flpDecizii.FlowDirection = FlowDirection.TopDown;
            flpDecizii.Location = new Point(300, 320);
            flpDecizii.Name = "flpDecizii";
            flpDecizii.Size = new Size(580, 220);
            flpDecizii.TabIndex = 6;
            flpDecizii.WrapContents = false;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 560);
            Controls.Add(flpDecizii);
            Controls.Add(pnlHUD);
            Controls.Add(pbImagineBloc);
            Controls.Add(rtbPoveste);
            Controls.Add(lblBlocCurent);
            Controls.Add(lblTitlu);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            Name = "Form1";
            Text = "Evadare Bran - Reader";
            FormClosing += Form1_FormClosing;
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pbImagineBloc).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem fisierToolStripMenuItem;
        private ToolStripMenuItem deschideToolStripMenuItem;
        private ToolStripMenuItem restartToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem salveazaStareToolStripMenuItem;
        private ToolStripMenuItem incarcaStareToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem iesireToolStripMenuItem;
        private ToolStripMenuItem inapoiToolStripMenuItem;
        private Label lblTitlu;
        private Label lblBlocCurent;
        private RichTextBox rtbPoveste;
        private PictureBox pbImagineBloc;
        private Panel pnlHUD;
        private FlowLayoutPanel flpDecizii;
    }
}
