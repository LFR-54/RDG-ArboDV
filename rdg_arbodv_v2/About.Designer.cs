namespace RDG_Uploader_GUI
{
    partial class About
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">
        ///   true if managed resources should be disposed; otherwise, false.
        /// </param>
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
        ///   Méthode requise pour la prise en charge du Designer :
        ///   Ne pas modifier le contenu de cette méthode avec l’éditeur de code.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(About));
            groupBox1 = new GroupBox();
            pictureBox4 = new PictureBox();
            pictureBox3 = new PictureBox();
            pictureBox2 = new PictureBox();
            btnCheckForUpdates = new Button();
            checkBoxIncludePrereleaseUpdates = new CheckBox();
            pictureBoxGitHub = new PictureBox();
            labelBuildDateTime = new Label();
            labelSiteWeb = new LinkLabel();
            labelLicence = new Label();
            labelVersion = new Label();
            labelProduit = new Label();
            labelGitHubRepository = new LinkLabel();
            TextBoxAuthors = new Label();
            groupBox2 = new GroupBox();
            pictureBoxDragon = new PictureBox();
            labelTitre = new Label();
            pictureBox1 = new PictureBox();
            pictureBoxBanner = new PictureBox();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGitHub).BeginInit();
            groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBoxDragon).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxBanner).BeginInit();
            SuspendLayout();
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(pictureBox4);
            groupBox1.Controls.Add(pictureBox3);
            groupBox1.Controls.Add(pictureBox2);
            groupBox1.Controls.Add(btnCheckForUpdates);
            groupBox1.Controls.Add(checkBoxIncludePrereleaseUpdates);
            groupBox1.Controls.Add(pictureBoxGitHub);
            groupBox1.Controls.Add(labelBuildDateTime);
            groupBox1.Controls.Add(labelSiteWeb);
            groupBox1.Controls.Add(labelLicence);
            groupBox1.Controls.Add(labelVersion);
            groupBox1.Controls.Add(labelProduit);
            groupBox1.Controls.Add(labelGitHubRepository);
            groupBox1.Location = new Point(12, 154);
            groupBox1.Margin = new Padding(4);
            groupBox1.Name = "groupBox1";
            groupBox1.Padding = new Padding(4);
            groupBox1.Size = new Size(440, 271);
            groupBox1.TabIndex = 0;
            groupBox1.TabStop = false;
            groupBox1.Text = "Product";
            // 
            // pictureBox4
            // 
            pictureBox4.Image = Properties.Resources.Logo_Centre_national_de_la_recherche_scientifique__2023__;
            pictureBox4.Location = new Point(44, 197);
            pictureBox4.Name = "pictureBox4";
            pictureBox4.Size = new Size(74, 58);
            pictureBox4.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox4.TabIndex = 10;
            pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            pictureBox3.Image = Properties.Resources.Logo_Université_de_Lorraine_Evo;
            pictureBox3.Location = new Point(264, 197);
            pictureBox3.Name = "pictureBox3";
            pictureBox3.Size = new Size(132, 58);
            pictureBox3.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox3.TabIndex = 9;
            pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            pictureBox2.Image = Properties.Resources.logo_201533630_removebg_preview;
            pictureBox2.Location = new Point(125, 197);
            pictureBox2.Name = "pictureBox2";
            pictureBox2.Size = new Size(132, 58);
            pictureBox2.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox2.TabIndex = 8;
            pictureBox2.TabStop = false;
            // 
            // btnCheckForUpdates
            // 
            btnCheckForUpdates.Cursor = Cursors.Hand;
            btnCheckForUpdates.Location = new Point(131, 166);
            btnCheckForUpdates.Name = "btnCheckForUpdates";
            btnCheckForUpdates.Size = new Size(178, 27);
            btnCheckForUpdates.TabIndex = 12;
            btnCheckForUpdates.Text = "Check for updates";
            btnCheckForUpdates.UseVisualStyleBackColor = true;
            btnCheckForUpdates.Click += btnCheckForUpdates_Click;
            // 
            // checkBoxIncludePrereleaseUpdates
            // 
            checkBoxIncludePrereleaseUpdates.AutoSize = true;
            checkBoxIncludePrereleaseUpdates.Location = new Point(153, 148);
            checkBoxIncludePrereleaseUpdates.Name = "checkBoxIncludePrereleaseUpdates";
            checkBoxIncludePrereleaseUpdates.Size = new Size(135, 19);
            checkBoxIncludePrereleaseUpdates.TabIndex = 11;
            checkBoxIncludePrereleaseUpdates.Text = "Include beta releases";
            checkBoxIncludePrereleaseUpdates.UseVisualStyleBackColor = true;
            checkBoxIncludePrereleaseUpdates.CheckedChanged += checkBoxIncludePrereleaseUpdates_CheckedChanged;
            // 
            // pictureBoxGitHub
            // 
            pictureBoxGitHub.BackColor = Color.Transparent;
            pictureBoxGitHub.BackgroundImage = Properties.Resources.github24x24;
            pictureBoxGitHub.BackgroundImageLayout = ImageLayout.Zoom;
            pictureBoxGitHub.Cursor = Cursors.Hand;
            pictureBoxGitHub.Location = new Point(149, 99);
            pictureBoxGitHub.Name = "pictureBoxGitHub";
            pictureBoxGitHub.Size = new Size(25, 25);
            pictureBoxGitHub.TabIndex = 13;
            pictureBoxGitHub.TabStop = false;
            pictureBoxGitHub.Click += pictureBoxGitHub_Click;
            // 
            // labelBuildDateTime
            // 
            labelBuildDateTime.Location = new Point(18, 59);
            labelBuildDateTime.Name = "labelBuildDateTime";
            labelBuildDateTime.Size = new Size(404, 16);
            labelBuildDateTime.TabIndex = 7;
            labelBuildDateTime.Text = "Build date --";
            labelBuildDateTime.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelSiteWeb
            // 
            labelSiteWeb.LinkBehavior = LinkBehavior.HoverUnderline;
            labelSiteWeb.LinkColor = Color.Blue;
            labelSiteWeb.Location = new Point(18, 77);
            labelSiteWeb.Margin = new Padding(4, 0, 4, 0);
            labelSiteWeb.Name = "labelSiteWeb";
            labelSiteWeb.Size = new Size(404, 17);
            labelSiteWeb.TabIndex = 5;
            labelSiteWeb.TabStop = true;
            labelSiteWeb.Text = "https://recherche.data.gouv.fr/";
            labelSiteWeb.TextAlign = ContentAlignment.MiddleCenter;
            labelSiteWeb.LinkClicked += labelSiteWeb_LinkClicked;
            // 
            // labelLicence
            // 
            labelLicence.Location = new Point(18, 127);
            labelLicence.Margin = new Padding(4, 0, 4, 0);
            labelLicence.Name = "labelLicence";
            labelLicence.Size = new Size(404, 16);
            labelLicence.TabIndex = 4;
            labelLicence.Text = "© 2026 Lucas FRENOT";
            labelLicence.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelVersion
            // 
            labelVersion.Location = new Point(18, 40);
            labelVersion.Margin = new Padding(4, 0, 4, 0);
            labelVersion.Name = "labelVersion";
            labelVersion.Size = new Size(404, 16);
            labelVersion.TabIndex = 1;
            labelVersion.Text = "Version 1.2.0";
            labelVersion.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // labelProduit
            // 
            labelProduit.Font = new Font("Verdana", 12F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelProduit.Location = new Point(18, 20);
            labelProduit.Margin = new Padding(4, 0, 4, 0);
            labelProduit.Name = "labelProduit";
            labelProduit.Size = new Size(404, 20);
            labelProduit.TabIndex = 0;
            labelProduit.Text = "RDG ArboDV";
            labelProduit.TextAlign = ContentAlignment.TopCenter;
            // 
            // labelGitHubRepository
            // 
            labelGitHubRepository.LinkBehavior = LinkBehavior.HoverUnderline;
            labelGitHubRepository.LinkColor = Color.FromArgb(36, 41, 46);
            labelGitHubRepository.Location = new Point(125, 102);
            labelGitHubRepository.Name = "labelGitHubRepository";
            labelGitHubRepository.Size = new Size(216, 17);
            labelGitHubRepository.TabIndex = 14;
            labelGitHubRepository.TabStop = true;
            labelGitHubRepository.Text = "LFR-54/RDG-ArboDV";
            labelGitHubRepository.TextAlign = ContentAlignment.MiddleCenter;
            labelGitHubRepository.LinkClicked += labelGitHubRepository_LinkClicked;
            // 
            // TextBoxAuthors
            // 
            TextBoxAuthors.Font = new Font("Microsoft Sans Serif", 9.5F, FontStyle.Bold, GraphicsUnit.Point, 0);
            TextBoxAuthors.Location = new Point(101, 29);
            TextBoxAuthors.Name = "TextBoxAuthors";
            TextBoxAuthors.Size = new Size(322, 112);
            TextBoxAuthors.TabIndex = 5;
            TextBoxAuthors.Text = resources.GetString("TextBoxAuthors.Text");
            TextBoxAuthors.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(TextBoxAuthors);
            groupBox2.Controls.Add(pictureBoxDragon);
            groupBox2.Location = new Point(12, 433);
            groupBox2.Margin = new Padding(4);
            groupBox2.Name = "groupBox2";
            groupBox2.Padding = new Padding(4);
            groupBox2.Size = new Size(440, 164);
            groupBox2.TabIndex = 1;
            groupBox2.TabStop = false;
            groupBox2.Text = "Author";
            // 
            // pictureBoxDragon
            // 
            pictureBoxDragon.Image = Properties.Resources.mascot;
            pictureBoxDragon.Location = new Point(12, 20);
            pictureBoxDragon.Name = "pictureBoxDragon";
            pictureBoxDragon.Size = new Size(80, 138);
            pictureBoxDragon.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBoxDragon.TabIndex = 4;
            pictureBoxDragon.TabStop = false;
            // 
            // labelTitre
            // 
            labelTitre.BackColor = Color.Transparent;
            labelTitre.Font = new Font("Segoe UI", 30F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelTitre.ForeColor = Color.Turquoise;
            labelTitre.Location = new Point(135, 38);
            labelTitre.Name = "labelTitre";
            labelTitre.Size = new Size(290, 56);
            labelTitre.TabIndex = 4;
            labelTitre.Text = "RDG ArboDV";
            labelTitre.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // pictureBox1
            // 
            pictureBox1.BackColor = Color.Transparent;
            pictureBox1.Image = Properties.Resources.Logo_RDG_ArboDV_Evo;
            pictureBox1.Location = new Point(14, 16);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(100, 100);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // pictureBoxBanner
            // 
            pictureBoxBanner.Image = Properties.Resources.bachgroundAuthor;
            pictureBoxBanner.Location = new Point(12, 12);
            pictureBoxBanner.Name = "pictureBoxBanner";
            pictureBoxBanner.Size = new Size(441, 135);
            pictureBoxBanner.TabIndex = 2;
            pictureBoxBanner.TabStop = false;
            // 
            // About
            // 
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(465, 608);
            Controls.Add(labelTitre);
            Controls.Add(pictureBox1);
            Controls.Add(pictureBoxBanner);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "About";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About RDG ArboDV";
            groupBox1.ResumeLayout(false);
            groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)pictureBox4).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox3).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox2).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxGitHub).EndInit();
            groupBox2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)pictureBoxDragon).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ((System.ComponentModel.ISupportInitialize)pictureBoxBanner).EndInit();
            ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelProduit;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelLicence;
        private System.Windows.Forms.LinkLabel labelSiteWeb;

        private System.Windows.Forms.GroupBox groupBox2;

        private System.Windows.Forms.PictureBox pictureBoxBanner;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label TextBoxAuthors;
        private System.Windows.Forms.PictureBox pictureBoxDragon;
        private System.Windows.Forms.Label labelBuildDateTime;
        private System.Windows.Forms.PictureBox pictureBox2;
        private System.Windows.Forms.Label labelTitre;
        private System.Windows.Forms.PictureBox pictureBox3;
        private System.Windows.Forms.PictureBox pictureBox4;
        private System.Windows.Forms.Button btnCheckForUpdates;
        private System.Windows.Forms.CheckBox checkBoxIncludePrereleaseUpdates;
        private System.Windows.Forms.LinkLabel labelGitHubRepository;
        private System.Windows.Forms.PictureBox pictureBoxGitHub;
    }
}
