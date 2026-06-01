// ============================================================
//  Evadare din Castelul Bran — proiect POO (echipă de 4)
//  AUTOR: Persoana 3 — Aplicația Editor (structura principală) — design fereastră
// ============================================================
namespace EvadareBranEditor
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            menuStrip1 = new MenuStrip();
            toolStripMenuItem1 = new ToolStripMenuItem();
            deschideToolStripMenuItem = new ToolStripMenuItem();
            salveazaToolStripMenuItem = new ToolStripMenuItem();
            splitContainer1 = new SplitContainer();
            tvPoveste = new TreeView();
            pnlEditorCentral = new Panel();
            pnlJurnal = new Panel();
            lblJurnal = new Label();
            btnValideaza = new Button();
            lbJurnal = new ListBox();
            menuStrip1.SuspendLayout();
            pnlJurnal.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)splitContainer1).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            SuspendLayout();
            // 
            // menuStrip1
            // 
            menuStrip1.ImageScalingSize = new Size(20, 20);
            menuStrip1.Items.AddRange(new ToolStripItem[] { toolStripMenuItem1, deschideToolStripMenuItem, salveazaToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(900, 28);
            menuStrip1.TabIndex = 0;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new Size(51, 24);
            toolStripMenuItem1.Text = "Nou";
            toolStripMenuItem1.Click += toolStripMenuItem1_Click_1;
            // 
            // deschideToolStripMenuItem
            // 
            deschideToolStripMenuItem.Name = "deschideToolStripMenuItem";
            deschideToolStripMenuItem.Size = new Size(84, 24);
            deschideToolStripMenuItem.Text = "Deschide";
            deschideToolStripMenuItem.Click += deschideToolStripMenuItem_Click;
            // 
            // salveazaToolStripMenuItem
            // 
            salveazaToolStripMenuItem.Name = "salveazaToolStripMenuItem";
            salveazaToolStripMenuItem.Size = new Size(81, 24);
            salveazaToolStripMenuItem.Text = "Salveaza";
            salveazaToolStripMenuItem.Click += salveazaToolStripMenuItem_Click;
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = DockStyle.Fill;
            splitContainer1.Location = new Point(0, 28);
            splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(tvPoveste);
            splitContainer1.Panel1MinSize = 200;
            //
            // splitContainer1.Panel2
            //
            splitContainer1.Panel2.Controls.Add(pnlEditorCentral);
            splitContainer1.Panel2MinSize = 520;
            splitContainer1.Size = new Size(900, 572);
            splitContainer1.SplitterDistance = 266;
            splitContainer1.TabIndex = 1;
            // 
            // tvPoveste
            // 
            tvPoveste.Dock = DockStyle.Fill;
            tvPoveste.Location = new Point(0, 0);
            tvPoveste.Name = "tvPoveste";
            tvPoveste.Size = new Size(266, 572);
            tvPoveste.TabIndex = 0;
            tvPoveste.AfterSelect += tvPoveste_AfterSelect;
            // 
            // pnlEditorCentral
            // 
            pnlEditorCentral.AutoScroll = true;
            pnlEditorCentral.Dock = DockStyle.Fill;
            pnlEditorCentral.Location = new Point(0, 0);
            pnlEditorCentral.Name = "pnlEditorCentral";
            pnlEditorCentral.Size = new Size(630, 572);
            pnlEditorCentral.TabIndex = 0;
            //
            // pnlJurnal
            //
            pnlJurnal.BorderStyle = BorderStyle.FixedSingle;
            pnlJurnal.Controls.Add(lbJurnal);
            pnlJurnal.Controls.Add(btnValideaza);
            pnlJurnal.Controls.Add(lblJurnal);
            pnlJurnal.Dock = DockStyle.Bottom;
            pnlJurnal.Location = new Point(0, 472);
            pnlJurnal.Name = "pnlJurnal";
            pnlJurnal.Size = new Size(900, 128);
            pnlJurnal.TabIndex = 2;
            //
            // lblJurnal
            //
            lblJurnal.AutoSize = true;
            lblJurnal.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            lblJurnal.Location = new Point(8, 5);
            lblJurnal.Name = "lblJurnal";
            lblJurnal.Text = "Jurnal de validare";
            //
            // btnValideaza
            //
            btnValideaza.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnValideaza.Location = new Point(758, 2);
            btnValideaza.Name = "btnValideaza";
            btnValideaza.Size = new Size(132, 26);
            btnValideaza.Text = "Validează acum";
            btnValideaza.Click += btnValideaza_Click;
            //
            // lbJurnal
            //
            lbJurnal.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lbJurnal.FormattingEnabled = true;
            lbJurnal.Location = new Point(8, 32);
            lbJurnal.Name = "lbJurnal";
            lbJurnal.Size = new Size(882, 88);
            lbJurnal.TabIndex = 0;
            //
            // Form1
            //
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(900, 600);
            Controls.Add(splitContainer1);
            Controls.Add(pnlJurnal);
            Controls.Add(menuStrip1);
            MainMenuStrip = menuStrip1;
            MinimumSize = new Size(820, 520);
            Name = "Form1";
            Text = "Evadare Bran - Editor";
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            pnlJurnal.ResumeLayout(false);
            pnlJurnal.PerformLayout();
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)splitContainer1).EndInit();
            splitContainer1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem deschideToolStripMenuItem;
        private ToolStripMenuItem salveazaToolStripMenuItem;
        private SplitContainer splitContainer1;
        private TreeView tvPoveste;
        private Panel pnlEditorCentral;
        private Panel pnlJurnal;
        private Label lblJurnal;
        private Button btnValideaza;
        private ListBox lbJurnal;
    }
}
