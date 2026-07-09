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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.pictureBox4 = new System.Windows.Forms.PictureBox();
            this.pictureBox3 = new System.Windows.Forms.PictureBox();
            this.pictureBox2 = new System.Windows.Forms.PictureBox();
            this.labelBuildDateTime = new System.Windows.Forms.Label();
            this.btnCheckForUpdates = new System.Windows.Forms.Button();
            this.checkBoxIncludePrereleaseUpdates = new System.Windows.Forms.CheckBox();
            this.labelSiteWeb = new System.Windows.Forms.LinkLabel();
            this.labelLicence = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelProduit = new System.Windows.Forms.Label();
            this.TextBoxAuthors = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.pictureBoxDragon = new System.Windows.Forms.PictureBox();
            this.labelTitre = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.pictureBoxBanner = new System.Windows.Forms.PictureBox();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).BeginInit();
            this.groupBox2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDragon)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBanner)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.pictureBox4);
            this.groupBox1.Controls.Add(this.pictureBox3);
            this.groupBox1.Controls.Add(this.pictureBox2);
            this.groupBox1.Controls.Add(this.btnCheckForUpdates);
            this.groupBox1.Controls.Add(this.checkBoxIncludePrereleaseUpdates);
            this.groupBox1.Controls.Add(this.labelBuildDateTime);
            this.groupBox1.Controls.Add(this.labelSiteWeb);
            this.groupBox1.Controls.Add(this.labelLicence);
            this.groupBox1.Controls.Add(this.labelVersion);
            this.groupBox1.Controls.Add(this.labelProduit);
            this.groupBox1.Location = new System.Drawing.Point(12, 154);
            this.groupBox1.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox1.Size = new System.Drawing.Size(440, 271);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Product";
            // 
            // pictureBox4
            // 
            this.pictureBox4.Image = global::RDG_Uploader_GUI.Properties.Resources.Logo_Centre_national_de_la_recherche_scientifique__2023__;
            this.pictureBox4.Location = new System.Drawing.Point(14, 187);
            this.pictureBox4.Name = "pictureBox4";
            this.pictureBox4.Size = new System.Drawing.Size(78, 77);
            this.pictureBox4.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox4.TabIndex = 10;
            this.pictureBox4.TabStop = false;
            // 
            // pictureBox3
            // 
            this.pictureBox3.Image = global::RDG_Uploader_GUI.Properties.Resources.Logo_Université_de_Lorraine_Evo;
            this.pictureBox3.Location = new System.Drawing.Point(286, 210);
            this.pictureBox3.Name = "pictureBox3";
            this.pictureBox3.Size = new System.Drawing.Size(144, 49);
            this.pictureBox3.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox3.TabIndex = 9;
            this.pictureBox3.TabStop = false;
            // 
            // pictureBox2
            // 
            this.pictureBox2.Image = global::RDG_Uploader_GUI.Properties.Resources.logo_201533630_removebg_preview;
            this.pictureBox2.Location = new System.Drawing.Point(139, 210);
            this.pictureBox2.Name = "pictureBox2";
            this.pictureBox2.Size = new System.Drawing.Size(106, 46);
            this.pictureBox2.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox2.TabIndex = 8;
            this.pictureBox2.TabStop = false;
            // 
            // labelBuildDateTime
            // 
            this.labelBuildDateTime.AutoSize = true;
            this.labelBuildDateTime.Location = new System.Drawing.Point(136, 64);
            this.labelBuildDateTime.Name = "labelBuildDateTime";
            this.labelBuildDateTime.Size = new System.Drawing.Size(63, 13);
            this.labelBuildDateTime.TabIndex = 7;
            this.labelBuildDateTime.Text = "Build date --";
            this.labelBuildDateTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // btnCheckForUpdates
            // 
            this.btnCheckForUpdates.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCheckForUpdates.Location = new System.Drawing.Point(137, 148);
            this.btnCheckForUpdates.Name = "btnCheckForUpdates";
            this.btnCheckForUpdates.Size = new System.Drawing.Size(166, 26);
            this.btnCheckForUpdates.TabIndex = 12;
            this.btnCheckForUpdates.Text = "Check for updates";
            this.btnCheckForUpdates.UseVisualStyleBackColor = true;
            this.btnCheckForUpdates.Click += new System.EventHandler(this.btnCheckForUpdates_Click);
            // 
            // checkBoxIncludePrereleaseUpdates
            // 
            this.checkBoxIncludePrereleaseUpdates.AutoSize = true;
            this.checkBoxIncludePrereleaseUpdates.Location = new System.Drawing.Point(137, 127);
            this.checkBoxIncludePrereleaseUpdates.Name = "checkBoxIncludePrereleaseUpdates";
            this.checkBoxIncludePrereleaseUpdates.Size = new System.Drawing.Size(133, 17);
            this.checkBoxIncludePrereleaseUpdates.TabIndex = 11;
            this.checkBoxIncludePrereleaseUpdates.Text = "Include beta releases";
            this.checkBoxIncludePrereleaseUpdates.UseVisualStyleBackColor = true;
            this.checkBoxIncludePrereleaseUpdates.CheckedChanged += new System.EventHandler(this.checkBoxIncludePrereleaseUpdates_CheckedChanged);
            // 
            // labelSiteWeb
            // 
            this.labelSiteWeb.LinkBehavior = System.Windows.Forms.LinkBehavior.HoverUnderline;
            this.labelSiteWeb.LinkColor = System.Drawing.Color.Blue;
            this.labelSiteWeb.Location = new System.Drawing.Point(127, 106);
            this.labelSiteWeb.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelSiteWeb.Name = "labelSiteWeb";
            this.labelSiteWeb.Size = new System.Drawing.Size(183, 16);
            this.labelSiteWeb.TabIndex = 5;
            this.labelSiteWeb.TabStop = true;
            this.labelSiteWeb.Text = "https://recherche.data.gouv.fr/";
            this.labelSiteWeb.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.labelSiteWeb.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.labelSiteWeb_LinkClicked);
            // 
            // labelLicence
            // 
            this.labelLicence.Location = new System.Drawing.Point(145, 85);
            this.labelLicence.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelLicence.Name = "labelLicence";
            this.labelLicence.Size = new System.Drawing.Size(144, 16);
            this.labelLicence.TabIndex = 4;
            this.labelLicence.Text = "© 2026 Lucas FRENOT";
            this.labelLicence.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labelVersion
            // 
            this.labelVersion.Location = new System.Drawing.Point(188, 43);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(90, 16);
            this.labelVersion.TabIndex = 1;
            this.labelVersion.Text = "Version 1.2.0";
            // 
            // labelProduit
            // 
            this.labelProduit.Font = new System.Drawing.Font("Verdana", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelProduit.Location = new System.Drawing.Point(163, 19);
            this.labelProduit.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.labelProduit.Name = "labelProduit";
            this.labelProduit.Size = new System.Drawing.Size(119, 18);
            this.labelProduit.TabIndex = 0;
            this.labelProduit.Text = "RDG ArboDV";
            this.labelProduit.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            // 
            // TextBoxAuthors
            // 
            this.TextBoxAuthors.AutoSize = true;
            this.TextBoxAuthors.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TextBoxAuthors.Location = new System.Drawing.Point(91, 41);
            this.TextBoxAuthors.Name = "TextBoxAuthors";
            this.TextBoxAuthors.Size = new System.Drawing.Size(342, 96);
            this.TextBoxAuthors.TabIndex = 5;
            this.TextBoxAuthors.Text = resources.GetString("TextBoxAuthors.Text");
            this.TextBoxAuthors.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.TextBoxAuthors);
            this.groupBox2.Controls.Add(this.pictureBoxDragon);
            this.groupBox2.Location = new System.Drawing.Point(12, 433);
            this.groupBox2.Margin = new System.Windows.Forms.Padding(4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Padding = new System.Windows.Forms.Padding(4);
            this.groupBox2.Size = new System.Drawing.Size(440, 164);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Author";
            // 
            // pictureBoxDragon
            // 
            this.pictureBoxDragon.Image = global::RDG_Uploader_GUI.Properties.Resources.mascot;
            this.pictureBoxDragon.Location = new System.Drawing.Point(7, 21);
            this.pictureBoxDragon.Name = "pictureBoxDragon";
            this.pictureBoxDragon.Size = new System.Drawing.Size(85, 136);
            this.pictureBoxDragon.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxDragon.TabIndex = 4;
            this.pictureBoxDragon.TabStop = false;
            // 
            // labelTitre
            // 
            this.labelTitre.AutoSize = true;
            this.labelTitre.BackColor = System.Drawing.Color.Transparent;
            this.labelTitre.Font = new System.Drawing.Font("SF Pro Rounded", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelTitre.ForeColor = System.Drawing.Color.Turquoise;
            this.labelTitre.Location = new System.Drawing.Point(134, 36);
            this.labelTitre.Name = "labelTitre";
            this.labelTitre.Size = new System.Drawing.Size(302, 57);
            this.labelTitre.TabIndex = 4;
            this.labelTitre.Text = "RDG ArboDV";
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox1.Image = global::RDG_Uploader_GUI.Properties.Resources.Logo_RDG_ArboDV_Evo;
            this.pictureBox1.Location = new System.Drawing.Point(12, 9);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(116, 116);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // pictureBoxBanner
            // 
            this.pictureBoxBanner.Image = global::RDG_Uploader_GUI.Properties.Resources.bachgroundAuthor;
            this.pictureBoxBanner.Location = new System.Drawing.Point(12, 12);
            this.pictureBoxBanner.Name = "pictureBoxBanner";
            this.pictureBoxBanner.Size = new System.Drawing.Size(441, 135);
            this.pictureBoxBanner.TabIndex = 2;
            this.pictureBoxBanner.TabStop = false;
            // 
            // About
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.ClientSize = new System.Drawing.Size(465, 608);
            this.Controls.Add(this.labelTitre);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.pictureBoxBanner);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "About";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About RDG ArboDV";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox4)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox3)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox2)).EndInit();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxDragon)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxBanner)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}
