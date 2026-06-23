namespace RDG_Uploader_GUI
{
    partial class Form1
    {
        /// <summary>Variable du concepteur.</summary>
        private System.ComponentModel.IContainer components = null;

        // ─────────── Champs API / DOI / Serveur ───────────
        private System.Windows.Forms.Label labelApiKey;
        private System.Windows.Forms.TextBox textBoxApiKey;
        private System.Windows.Forms.Button buttonApiKeyInfo;
        private System.Windows.Forms.Label labelDoi;
        private System.Windows.Forms.TextBox textBoxDoi;
        private System.Windows.Forms.Button buttonDoiInfo;
        private System.Windows.Forms.Label labelServer;
        private System.Windows.Forms.ComboBox comboBoxServer;
        private System.Windows.Forms.Label labelLanguage;
        private System.Windows.Forms.ComboBox comboBoxLanguage;
        private System.Windows.Forms.ToolTip toolTipFieldHelp;

        // ─────────── Boutons ───────────
        private System.Windows.Forms.Button buttonSelectFiles;
        private System.Windows.Forms.Button buttonSelectFolder;
        private System.Windows.Forms.Button buttonReset;
        private System.Windows.Forms.Button buttonUpload;

        // ─────────── TreeView + icônes ───────────
        private RDG_Uploader_GUI.MultiSelectTreeView treeViewSelected;
        private System.Windows.Forms.ImageList imageListIcons;
        private System.Windows.Forms.ContextMenuStrip contextMenuTree;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemRemove;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItemFlatten;

        // ─────────── Barre de progression + labels bas ───────────
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.Label labelSpeed;

        // ─────────── GroupBox « Stats » ───────────
        private System.Windows.Forms.GroupBox groupBoxStats;
        private System.Windows.Forms.Label labelStatTotalFiles;
        private System.Windows.Forms.Label labelStatFilesValue;
        private System.Windows.Forms.Label labelStatTotalFolders;
        private System.Windows.Forms.Label labelStatFoldersValue;
        private System.Windows.Forms.Label labelStatSpeed;
        private System.Windows.Forms.Label labelStatSpeedValue;
        private System.Windows.Forms.Label labelStatETA;
        private System.Windows.Forms.Label labelStatETAValue;
        private System.Windows.Forms.Label labelStatElapsed;
        private System.Windows.Forms.Label labelStatElapsedValue;

        /// <summary>Nettoyage des ressources.</summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
            this.imageListIcons = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuTree = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItemRemove = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItemFlatten = new System.Windows.Forms.ToolStripMenuItem();
            this.labelApiKey = new System.Windows.Forms.Label();
            this.textBoxApiKey = new System.Windows.Forms.TextBox();
            this.buttonApiKeyInfo = new System.Windows.Forms.Button();
            this.labelDoi = new System.Windows.Forms.Label();
            this.textBoxDoi = new System.Windows.Forms.TextBox();
            this.buttonDoiInfo = new System.Windows.Forms.Button();
            this.labelServer = new System.Windows.Forms.Label();
            this.comboBoxServer = new System.Windows.Forms.ComboBox();
            this.labelLanguage = new System.Windows.Forms.Label();
            this.comboBoxLanguage = new System.Windows.Forms.ComboBox();
            this.toolTipFieldHelp = new System.Windows.Forms.ToolTip(this.components);
            this.buttonSelectFiles = new System.Windows.Forms.Button();
            this.buttonSelectFolder = new System.Windows.Forms.Button();
            this.buttonReset = new System.Windows.Forms.Button();
            this.buttonUpload = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.labelStatus = new System.Windows.Forms.Label();
            this.labelSpeed = new System.Windows.Forms.Label();
            this.groupBoxStats = new System.Windows.Forms.GroupBox();
            this.labelStatErrorValue = new System.Windows.Forms.Label();
            this.labelStatDoneValue = new System.Windows.Forms.Label();
            this.labelStatPendingValue = new System.Windows.Forms.Label();
            this.labelStatError = new System.Windows.Forms.Label();
            this.labelStatDone = new System.Windows.Forms.Label();
            this.labelStatPending = new System.Windows.Forms.Label();
            this.labelStatTotalFiles = new System.Windows.Forms.Label();
            this.labelStatFilesValue = new System.Windows.Forms.Label();
            this.labelStatTotalFolders = new System.Windows.Forms.Label();
            this.labelStatFoldersValue = new System.Windows.Forms.Label();
            this.labelStatSpeed = new System.Windows.Forms.Label();
            this.labelStatSpeedValue = new System.Windows.Forms.Label();
            this.labelStatETA = new System.Windows.Forms.Label();
            this.labelStatETAValue = new System.Windows.Forms.Label();
            this.labelStatElapsed = new System.Windows.Forms.Label();
            this.labelStatElapsedValue = new System.Windows.Forms.Label();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.treeViewSelected = new RDG_Uploader_GUI.MultiSelectTreeView();
            this.tabControlMain = new System.Windows.Forms.TabControl();
            this.tabPageFiles = new System.Windows.Forms.TabPage();
            this.tabPageRemote = new System.Windows.Forms.TabPage();
            this.tabPageLogs = new System.Windows.Forms.TabPage();
            this.textBoxLogs = new System.Windows.Forms.TextBox();
            this.treeViewRemote = new RDG_Uploader_GUI.MultiSelectTreeView();
            this.contextMenuTree.SuspendLayout();
            this.groupBoxStats.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageListIcons
            // 
            this.imageListIcons.ColorDepth = System.Windows.Forms.ColorDepth.Depth32Bit;
            this.imageListIcons.ImageSize = new System.Drawing.Size(16, 16);
            this.imageListIcons.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // contextMenuTree
            // 
            this.contextMenuTree.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItemRemove,
            this.toolStripMenuItemFlatten});
            this.contextMenuTree.Name = "contextMenuTree";
            this.contextMenuTree.Size = new System.Drawing.Size(167, 48);
            // 
            // toolStripMenuItemRemove
            // 
            this.toolStripMenuItemRemove.Name = "toolStripMenuItemRemove";
            this.toolStripMenuItemRemove.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItemRemove.Text = "Remove";
            this.toolStripMenuItemRemove.Click += new System.EventHandler(this.toolStripMenuItemRemove_Click);
            // 
            // toolStripMenuItemFlatten
            // 
            this.toolStripMenuItemFlatten.Name = "toolStripMenuItemFlatten";
            this.toolStripMenuItemFlatten.Size = new System.Drawing.Size(166, 22);
            this.toolStripMenuItemFlatten.Text = "Flatten this folder";
            this.toolStripMenuItemFlatten.Click += new System.EventHandler(this.toolStripMenuItemFlatten_Click);
            // 
            // labelApiKey
            // 
            this.labelApiKey.AutoSize = true;
            this.labelApiKey.Location = new System.Drawing.Point(12, 15);
            this.labelApiKey.Name = "labelApiKey";
            this.labelApiKey.Size = new System.Drawing.Size(51, 13);
            this.labelApiKey.TabIndex = 1;
            this.labelApiKey.Text = "API Key :";
            // 
            // textBoxApiKey
            // 
            this.textBoxApiKey.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxApiKey.Location = new System.Drawing.Point(80, 12);
            this.textBoxApiKey.Name = "textBoxApiKey";
            this.textBoxApiKey.Size = new System.Drawing.Size(640, 20);
            this.textBoxApiKey.TabIndex = 2;
            // 
            // buttonApiKeyInfo
            // 
            this.buttonApiKeyInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonApiKeyInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(98)))), ((int)(((byte)(163)))));
            this.buttonApiKeyInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonApiKeyInfo.FlatAppearance.BorderSize = 0;
            this.buttonApiKeyInfo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(75)))), ((int)(((byte)(125)))));
            this.buttonApiKeyInfo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(124)))), ((int)(((byte)(207)))));
            this.buttonApiKeyInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonApiKeyInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonApiKeyInfo.ForeColor = System.Drawing.Color.White;
            this.buttonApiKeyInfo.Location = new System.Drawing.Point(724, 11);
            this.buttonApiKeyInfo.Name = "buttonApiKeyInfo";
            this.buttonApiKeyInfo.Size = new System.Drawing.Size(20, 20);
            this.buttonApiKeyInfo.TabIndex = 3;
            this.buttonApiKeyInfo.TabStop = false;
            this.buttonApiKeyInfo.Text = "i";
            this.buttonApiKeyInfo.UseVisualStyleBackColor = false;
            this.buttonApiKeyInfo.Click += new System.EventHandler(this.buttonApiKeyInfo_Click);
            // 
            // labelDoi
            // 
            this.labelDoi.AutoSize = true;
            this.labelDoi.Location = new System.Drawing.Point(12, 45);
            this.labelDoi.Name = "labelDoi";
            this.labelDoi.Size = new System.Drawing.Size(32, 13);
            this.labelDoi.TabIndex = 3;
            this.labelDoi.Text = "DOI :";
            // 
            // textBoxDoi
            // 
            this.textBoxDoi.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxDoi.Location = new System.Drawing.Point(80, 42);
            this.textBoxDoi.Name = "textBoxDoi";
            this.textBoxDoi.Size = new System.Drawing.Size(640, 20);
            this.textBoxDoi.TabIndex = 4;
            // 
            // buttonDoiInfo
            // 
            this.buttonDoiInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDoiInfo.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(24)))), ((int)(((byte)(98)))), ((int)(((byte)(163)))));
            this.buttonDoiInfo.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonDoiInfo.FlatAppearance.BorderSize = 0;
            this.buttonDoiInfo.FlatAppearance.MouseDownBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(18)))), ((int)(((byte)(75)))), ((int)(((byte)(125)))));
            this.buttonDoiInfo.FlatAppearance.MouseOverBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(31)))), ((int)(((byte)(124)))), ((int)(((byte)(207)))));
            this.buttonDoiInfo.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.buttonDoiInfo.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonDoiInfo.ForeColor = System.Drawing.Color.White;
            this.buttonDoiInfo.Location = new System.Drawing.Point(724, 41);
            this.buttonDoiInfo.Name = "buttonDoiInfo";
            this.buttonDoiInfo.Size = new System.Drawing.Size(20, 20);
            this.buttonDoiInfo.TabIndex = 5;
            this.buttonDoiInfo.TabStop = false;
            this.buttonDoiInfo.Text = "i";
            this.buttonDoiInfo.UseVisualStyleBackColor = false;
            this.buttonDoiInfo.Click += new System.EventHandler(this.buttonDoiInfo_Click);
            // 
            // labelServer
            // 
            this.labelServer.AutoSize = true;
            this.labelServer.Location = new System.Drawing.Point(12, 75);
            this.labelServer.Name = "labelServer";
            this.labelServer.Size = new System.Drawing.Size(44, 13);
            this.labelServer.TabIndex = 5;
            this.labelServer.Text = "Server :";
            // 
            // comboBoxServer
            // 
            this.comboBoxServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxServer.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxServer.Location = new System.Drawing.Point(80, 72);
            this.comboBoxServer.Name = "comboBoxServer";
            this.comboBoxServer.Size = new System.Drawing.Size(650, 21);
            this.comboBoxServer.TabIndex = 6;
            // 
            // labelLanguage
            // 
            this.labelLanguage.AutoSize = true;
            this.labelLanguage.Location = new System.Drawing.Point(760, 352);
            this.labelLanguage.Name = "labelLanguage";
            this.labelLanguage.Size = new System.Drawing.Size(58, 13);
            this.labelLanguage.TabIndex = 18;
            this.labelLanguage.Text = "Language:";
            // 
            // comboBoxLanguage
            // 
            this.comboBoxLanguage.Cursor = System.Windows.Forms.Cursors.Hand;
            this.comboBoxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxLanguage.FormattingEnabled = true;
            this.comboBoxLanguage.Location = new System.Drawing.Point(760, 369);
            this.comboBoxLanguage.Name = "comboBoxLanguage";
            this.comboBoxLanguage.Size = new System.Drawing.Size(180, 21);
            this.comboBoxLanguage.TabIndex = 19;
            // 
            // toolTipFieldHelp
            // 
            this.toolTipFieldHelp.AutoPopDelay = 12000;
            this.toolTipFieldHelp.InitialDelay = 250;
            this.toolTipFieldHelp.ReshowDelay = 100;
            this.toolTipFieldHelp.ShowAlways = true;
            // 
            // buttonSelectFiles
            // 
            this.buttonSelectFiles.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSelectFiles.Location = new System.Drawing.Point(750, 10);
            this.buttonSelectFiles.Name = "buttonSelectFiles";
            this.buttonSelectFiles.Size = new System.Drawing.Size(180, 25);
            this.buttonSelectFiles.TabIndex = 7;
            this.buttonSelectFiles.Text = "Select Files";
            this.buttonSelectFiles.Click += new System.EventHandler(this.buttonSelectFiles_Click);
            // 
            // buttonSelectFolder
            // 
            this.buttonSelectFolder.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonSelectFolder.Location = new System.Drawing.Point(750, 40);
            this.buttonSelectFolder.Name = "buttonSelectFolder";
            this.buttonSelectFolder.Size = new System.Drawing.Size(180, 25);
            this.buttonSelectFolder.TabIndex = 8;
            this.buttonSelectFolder.Text = "Select Folder";
            this.buttonSelectFolder.Click += new System.EventHandler(this.buttonSelectFolder_Click);
            // 
            // buttonReset
            // 
            this.buttonReset.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonReset.Location = new System.Drawing.Point(750, 70);
            this.buttonReset.Name = "buttonReset";
            this.buttonReset.Size = new System.Drawing.Size(180, 25);
            this.buttonReset.TabIndex = 9;
            this.buttonReset.Text = "Reset";
            this.buttonReset.Click += new System.EventHandler(this.buttonReset_Click);
            // 
            // buttonUpload
            // 
            this.buttonUpload.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpload.Cursor = System.Windows.Forms.Cursors.Hand;
            this.buttonUpload.Location = new System.Drawing.Point(760, 435);
            this.buttonUpload.Name = "buttonUpload";
            this.buttonUpload.Size = new System.Drawing.Size(180, 54);
            this.buttonUpload.TabIndex = 12;
            this.buttonUpload.Text = "Upload";
            this.buttonUpload.Click += new System.EventHandler(this.buttonUpload_Click);
            // 
            // progressBar
            // 
            this.progressBar.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.progressBar.Location = new System.Drawing.Point(12, 476);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(740, 20);
            this.progressBar.TabIndex = 13;
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.Location = new System.Drawing.Point(12, 450);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(500, 20);
            this.labelStatus.TabIndex = 14;
            this.labelStatus.Text = "No files selected.";
            // 
            // labelSpeed
            // 
            this.labelSpeed.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.labelSpeed.Location = new System.Drawing.Point(620, 450);
            this.labelSpeed.Name = "labelSpeed";
            this.labelSpeed.Size = new System.Drawing.Size(130, 20);
            this.labelSpeed.TabIndex = 15;
            this.labelSpeed.Text = "0 MB/s";
            this.labelSpeed.TextAlign = System.Drawing.ContentAlignment.TopRight;
            // 
            // groupBoxStats
            // 
            this.groupBoxStats.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxStats.Controls.Add(this.labelStatErrorValue);
            this.groupBoxStats.Controls.Add(this.labelStatDoneValue);
            this.groupBoxStats.Controls.Add(this.labelStatPendingValue);
            this.groupBoxStats.Controls.Add(this.labelStatError);
            this.groupBoxStats.Controls.Add(this.labelStatDone);
            this.groupBoxStats.Controls.Add(this.labelStatPending);
            this.groupBoxStats.Controls.Add(this.labelStatTotalFiles);
            this.groupBoxStats.Controls.Add(this.labelStatFilesValue);
            this.groupBoxStats.Controls.Add(this.labelStatTotalFolders);
            this.groupBoxStats.Controls.Add(this.labelStatFoldersValue);
            this.groupBoxStats.Controls.Add(this.labelStatSpeed);
            this.groupBoxStats.Controls.Add(this.labelStatSpeedValue);
            this.groupBoxStats.Controls.Add(this.labelStatETA);
            this.groupBoxStats.Controls.Add(this.labelStatETAValue);
            this.groupBoxStats.Controls.Add(this.labelStatElapsed);
            this.groupBoxStats.Controls.Add(this.labelStatElapsedValue);
            this.groupBoxStats.Location = new System.Drawing.Point(760, 104);
            this.groupBoxStats.Name = "groupBoxStats";
            this.groupBoxStats.Size = new System.Drawing.Size(180, 184);
            this.groupBoxStats.TabIndex = 11;
            this.groupBoxStats.TabStop = false;
            this.groupBoxStats.Text = "Statistics";
            // 
            // labelStatErrorValue
            // 
            this.labelStatErrorValue.AutoSize = true;
            this.labelStatErrorValue.Location = new System.Drawing.Point(110, 162);
            this.labelStatErrorValue.Name = "labelStatErrorValue";
            this.labelStatErrorValue.Size = new System.Drawing.Size(13, 13);
            this.labelStatErrorValue.TabIndex = 15;
            this.labelStatErrorValue.Text = "0";
            // 
            // labelStatDoneValue
            // 
            this.labelStatDoneValue.AutoSize = true;
            this.labelStatDoneValue.Location = new System.Drawing.Point(110, 144);
            this.labelStatDoneValue.Name = "labelStatDoneValue";
            this.labelStatDoneValue.Size = new System.Drawing.Size(24, 13);
            this.labelStatDoneValue.TabIndex = 14;
            this.labelStatDoneValue.Text = "0/0";
            // 
            // labelStatPendingValue
            // 
            this.labelStatPendingValue.AutoSize = true;
            this.labelStatPendingValue.Location = new System.Drawing.Point(110, 124);
            this.labelStatPendingValue.Name = "labelStatPendingValue";
            this.labelStatPendingValue.Size = new System.Drawing.Size(13, 13);
            this.labelStatPendingValue.TabIndex = 13;
            this.labelStatPendingValue.Text = "0";
            // 
            // labelStatError
            // 
            this.labelStatError.AutoSize = true;
            this.labelStatError.Location = new System.Drawing.Point(12, 162);
            this.labelStatError.Name = "labelStatError";
            this.labelStatError.Size = new System.Drawing.Size(37, 13);
            this.labelStatError.TabIndex = 12;
            this.labelStatError.Text = "Errors:";
            // 
            // labelStatDone
            // 
            this.labelStatDone.AutoSize = true;
            this.labelStatDone.Location = new System.Drawing.Point(12, 144);
            this.labelStatDone.Name = "labelStatDone";
            this.labelStatDone.Size = new System.Drawing.Size(60, 13);
            this.labelStatDone.TabIndex = 11;
            this.labelStatDone.Text = "Completed:";
            // 
            // labelStatPending
            // 
            this.labelStatPending.AutoSize = true;
            this.labelStatPending.Location = new System.Drawing.Point(12, 124);
            this.labelStatPending.Name = "labelStatPending";
            this.labelStatPending.Size = new System.Drawing.Size(49, 13);
            this.labelStatPending.TabIndex = 10;
            this.labelStatPending.Text = "Pending:";
            // 
            // labelStatTotalFiles
            // 
            this.labelStatTotalFiles.AutoSize = true;
            this.labelStatTotalFiles.Location = new System.Drawing.Point(10, 22);
            this.labelStatTotalFiles.Name = "labelStatTotalFiles";
            this.labelStatTotalFiles.Size = new System.Drawing.Size(55, 13);
            this.labelStatTotalFiles.TabIndex = 0;
            this.labelStatTotalFiles.Text = "Total files:";
            // 
            // labelStatFilesValue
            // 
            this.labelStatFilesValue.AutoSize = true;
            this.labelStatFilesValue.Location = new System.Drawing.Point(110, 23);
            this.labelStatFilesValue.Name = "labelStatFilesValue";
            this.labelStatFilesValue.Size = new System.Drawing.Size(13, 13);
            this.labelStatFilesValue.TabIndex = 1;
            this.labelStatFilesValue.Text = "0";
            // 
            // labelStatTotalFolders
            // 
            this.labelStatTotalFolders.AutoSize = true;
            this.labelStatTotalFolders.Location = new System.Drawing.Point(10, 45);
            this.labelStatTotalFolders.Name = "labelStatTotalFolders";
            this.labelStatTotalFolders.Size = new System.Drawing.Size(68, 13);
            this.labelStatTotalFolders.TabIndex = 2;
            this.labelStatTotalFolders.Text = "Total folders:";
            // 
            // labelStatFoldersValue
            // 
            this.labelStatFoldersValue.AutoSize = true;
            this.labelStatFoldersValue.Location = new System.Drawing.Point(110, 45);
            this.labelStatFoldersValue.Name = "labelStatFoldersValue";
            this.labelStatFoldersValue.Size = new System.Drawing.Size(13, 13);
            this.labelStatFoldersValue.TabIndex = 3;
            this.labelStatFoldersValue.Text = "0";
            // 
            // labelStatSpeed
            // 
            this.labelStatSpeed.AutoSize = true;
            this.labelStatSpeed.Location = new System.Drawing.Point(11, 65);
            this.labelStatSpeed.Name = "labelStatSpeed";
            this.labelStatSpeed.Size = new System.Drawing.Size(76, 13);
            this.labelStatSpeed.TabIndex = 4;
            this.labelStatSpeed.Text = "Speed (MB/s):";
            // 
            // labelStatSpeedValue
            // 
            this.labelStatSpeedValue.AutoSize = true;
            this.labelStatSpeedValue.Location = new System.Drawing.Point(110, 65);
            this.labelStatSpeedValue.Name = "labelStatSpeedValue";
            this.labelStatSpeedValue.Size = new System.Drawing.Size(22, 13);
            this.labelStatSpeedValue.TabIndex = 5;
            this.labelStatSpeedValue.Text = "0.0";
            // 
            // labelStatETA
            // 
            this.labelStatETA.AutoSize = true;
            this.labelStatETA.Location = new System.Drawing.Point(11, 85);
            this.labelStatETA.Name = "labelStatETA";
            this.labelStatETA.Size = new System.Drawing.Size(50, 13);
            this.labelStatETA.TabIndex = 6;
            this.labelStatETA.Text = "Time left:";
            // 
            // labelStatETAValue
            // 
            this.labelStatETAValue.AutoSize = true;
            this.labelStatETAValue.Location = new System.Drawing.Point(110, 85);
            this.labelStatETAValue.Name = "labelStatETAValue";
            this.labelStatETAValue.Size = new System.Drawing.Size(49, 13);
            this.labelStatETAValue.TabIndex = 7;
            this.labelStatETAValue.Text = "00:00:00";
            // 
            // labelStatElapsed
            // 
            this.labelStatElapsed.AutoSize = true;
            this.labelStatElapsed.Location = new System.Drawing.Point(12, 105);
            this.labelStatElapsed.Name = "labelStatElapsed";
            this.labelStatElapsed.Size = new System.Drawing.Size(48, 13);
            this.labelStatElapsed.TabIndex = 8;
            this.labelStatElapsed.Text = "Elapsed:";
            // 
            // labelStatElapsedValue
            // 
            this.labelStatElapsedValue.AutoSize = true;
            this.labelStatElapsedValue.Location = new System.Drawing.Point(110, 105);
            this.labelStatElapsedValue.Name = "labelStatElapsedValue";
            this.labelStatElapsedValue.Size = new System.Drawing.Size(49, 13);
            this.labelStatElapsedValue.TabIndex = 9;
            this.labelStatElapsedValue.Text = "00:00:00";
            // 
            // btnAbout
            // 
            this.btnAbout.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnAbout.Location = new System.Drawing.Point(760, 405);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(180, 26);
            this.btnAbout.TabIndex = 16;
            this.btnAbout.Text = "About";
            this.btnAbout.UseVisualStyleBackColor = true;
            this.btnAbout.Click += new System.EventHandler(this.btnAbout_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.Firebrick;
            this.btnCancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancel.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnCancel.ForeColor = System.Drawing.SystemColors.Control;
            this.btnCancel.Location = new System.Drawing.Point(760, 294);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(180, 48);
            this.btnCancel.TabIndex = 17;
            this.btnCancel.Text = "CANCEL";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // treeViewSelected
            // 
            this.treeViewSelected.AllowDrop = true;
            this.treeViewSelected.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewSelected.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeViewSelected.HideSelection = false;
            this.treeViewSelected.ImageIndex = 0;
            this.treeViewSelected.ImageList = this.imageListIcons;
            this.treeViewSelected.Location = new System.Drawing.Point(3, 3);
            this.treeViewSelected.Name = "treeViewSelected";
            this.treeViewSelected.SelectedImageIndex = 0;
            this.treeViewSelected.Size = new System.Drawing.Size(726, 313);
            this.treeViewSelected.TabIndex = 0;
            this.treeViewSelected.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.treeViewSelected_ItemDrag);
            this.treeViewSelected.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeViewSelected_NodeMouseClick);
            this.treeViewSelected.DragDrop += new System.Windows.Forms.DragEventHandler(this.treeViewSelected_DragDrop);
            this.treeViewSelected.DragEnter += new System.Windows.Forms.DragEventHandler(this.treeViewSelected_DragEnter);
            // 
            // tabControlMain
            // 
            this.tabControlMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tabControlMain.Controls.Add(this.tabPageFiles);
            this.tabControlMain.Controls.Add(this.tabPageRemote);
            this.tabControlMain.Controls.Add(this.tabPageLogs);
            this.tabControlMain.Location = new System.Drawing.Point(12, 100);
            this.tabControlMain.Name = "tabControlMain";
            this.tabControlMain.SelectedIndex = 0;
            this.tabControlMain.Size = new System.Drawing.Size(740, 345);
            this.tabControlMain.TabIndex = 10;
            // 
            // tabPageFiles
            // 
            this.tabPageFiles.Controls.Add(this.treeViewSelected);
            this.tabPageFiles.Location = new System.Drawing.Point(4, 22);
            this.tabPageFiles.Name = "tabPageFiles";
            this.tabPageFiles.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageFiles.Size = new System.Drawing.Size(732, 319);
            this.tabPageFiles.TabIndex = 0;
            this.tabPageFiles.Text = "Files";
            this.tabPageFiles.UseVisualStyleBackColor = true;
            // 
            // tabPageRemote
            // 
            this.tabPageRemote.Controls.Add(this.treeViewRemote);
            this.tabPageRemote.Location = new System.Drawing.Point(4, 22);
            this.tabPageRemote.Name = "tabPageRemote";
            this.tabPageRemote.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageRemote.Size = new System.Drawing.Size(732, 319);
            this.tabPageRemote.TabIndex = 1;
            this.tabPageRemote.Text = "Server Files";
            this.tabPageRemote.UseVisualStyleBackColor = true;
            // 
            // treeViewRemote
            // 
            this.treeViewRemote.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeViewRemote.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.treeViewRemote.HideSelection = false;
            this.treeViewRemote.ImageIndex = 0;
            this.treeViewRemote.ImageList = this.imageListIcons;
            this.treeViewRemote.Location = new System.Drawing.Point(3, 3);
            this.treeViewRemote.Name = "treeViewRemote";
            this.treeViewRemote.SelectedImageIndex = 0;
            this.treeViewRemote.Size = new System.Drawing.Size(726, 313);
            this.treeViewRemote.TabIndex = 0;
            // 
            // tabPageLogs
            // 
            this.tabPageLogs.Controls.Add(this.textBoxLogs);
            this.tabPageLogs.Location = new System.Drawing.Point(4, 22);
            this.tabPageLogs.Name = "tabPageLogs";
            this.tabPageLogs.Padding = new System.Windows.Forms.Padding(3);
            this.tabPageLogs.Size = new System.Drawing.Size(732, 319);
            this.tabPageLogs.TabIndex = 2;
            this.tabPageLogs.Text = "Java Engine Logs";
            this.tabPageLogs.UseVisualStyleBackColor = true;
            // 
            // textBoxLogs
            // 
            this.textBoxLogs.BackColor = System.Drawing.Color.Black;
            this.textBoxLogs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLogs.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.textBoxLogs.ForeColor = System.Drawing.Color.LightGray;
            this.textBoxLogs.Location = new System.Drawing.Point(3, 3);
            this.textBoxLogs.Multiline = true;
            this.textBoxLogs.Name = "textBoxLogs";
            this.textBoxLogs.ReadOnly = true;
            this.textBoxLogs.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLogs.Size = new System.Drawing.Size(726, 313);
            this.textBoxLogs.TabIndex = 0;
            // 
            // Form1
            // 
            this.ClientSize = new System.Drawing.Size(944, 501);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAbout);
            this.Controls.Add(this.labelLanguage);
            this.Controls.Add(this.comboBoxLanguage);
            this.Controls.Add(this.labelApiKey);
            this.Controls.Add(this.textBoxApiKey);
            this.Controls.Add(this.buttonApiKeyInfo);
            this.Controls.Add(this.labelDoi);
            this.Controls.Add(this.textBoxDoi);
            this.Controls.Add(this.buttonDoiInfo);
            this.Controls.Add(this.labelServer);
            this.Controls.Add(this.comboBoxServer);
            this.Controls.Add(this.buttonSelectFiles);
            this.Controls.Add(this.buttonSelectFolder);
            this.Controls.Add(this.buttonReset);
            this.Controls.Add(this.tabControlMain);
            this.Controls.Add(this.groupBoxStats);
            this.Controls.Add(this.buttonUpload);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.labelSpeed);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(960, 540);
            this.Name = "Form1";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "RDG ArboDV";
            this.contextMenuTree.ResumeLayout(false);
            this.groupBoxStats.ResumeLayout(false);
            this.groupBoxStats.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Label labelStatError;
        private System.Windows.Forms.Label labelStatDone;
        private System.Windows.Forms.Label labelStatPending;
        private System.Windows.Forms.Label labelStatErrorValue;
        private System.Windows.Forms.Label labelStatDoneValue;
        private System.Windows.Forms.Label labelStatPendingValue;
        private System.Windows.Forms.TabControl tabControlMain;
        private System.Windows.Forms.TabPage tabPageFiles;
        private System.Windows.Forms.TabPage tabPageRemote;
        private System.Windows.Forms.TabPage tabPageLogs;
        private System.Windows.Forms.TextBox textBoxLogs;
        private RDG_Uploader_GUI.MultiSelectTreeView treeViewRemote;
    }
}
