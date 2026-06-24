using Newtonsoft.Json;                                     // Pour la sérialisation et désérialisation JSON
using System.Diagnostics;                                  // Pour Process, ProcessStartInfo
using System;                                             // Types de base (String, DateTime, etc.)
using System.Collections.Generic;                         // List<T>, Dictionary<TKey, TValue>
using System.Drawing;                                     // Pour Color, Bitmap, etc.
using System.IO;                                          // File, Directory, FileStream
using System.Net;
using System.Net.Http;                                    // HttpClient, HttpRequestMessage, HttpContent
using System.Net.Http.Headers;                            // MediaTypeHeaderValue
using System.Runtime.InteropServices;                     // DllImport pour SendMessage
using System.Threading;                                   // CancellationTokenSource, CancellationToken
using System.Threading.Tasks;                             // Task, async/await
using System.Windows.Forms;                               // WinForms (Form, Button, TreeView, etc.)
using System.Net.NetworkInformation;                      // Pour détécter une compure réseau
using WinFormsTimer = System.Windows.Forms.Timer;

namespace RDG_Uploader_GUI                                  // Espace de noms du projet
{
    public enum AppLanguage
    {
        English,
        French
    }

    public partial class Form1 : Form                      // Déclaration de la Form principale
    {
        // Champs & constantes pour la ProgressBar

        private const uint PBM_SETSTATE = 0x0400 + 16;      // Code de message Windows pour changer l'état de la ProgressBar
        private const int PBST_NORMAL = 1;                 // Valeur pour état normal (soit le vert)
        private const int PBST_ERROR = 2;                 // Valeur pour état erreur (rouge)

        private const uint WM_VSCROLL = 0x0115;            // Code de message Windows pour faire défiler la barre verticale
        private const int SB_BOTTOM = 7;                   // Position de défilement (bas)

        [DllImport("user32.dll", CharSet = CharSet.Auto)] // Import de la fonction SendMessage depuis user32.dll
        private static extern IntPtr SendMessage(
            IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);  // Signature de SendMessage

        private readonly List<string> _filesToUpload = new List<string>();          // Liste des chemins complets des fichiers à uploader
        private readonly Dictionary<string, string> _fileRelativePaths = new Dictionary<string, string>();  // Dictionnaire mapping fichier => chemin relatif Dataverse
        private readonly HashSet<string> _remotePaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase); // Liste des chemins de fichiers distants sur le serveur
        // OLD : private readonly List<string> _foldersSelected = new List<string>();          // Liste des dossiers racine sélectionnés
        // HashSet = plus rapide et évite les doublons
        private readonly HashSet<string> _foldersSelected = new HashSet<string>();
        private readonly HttpClient _httpClient = new HttpClient();            // Instance partagée d'HttpClient

        // Compteurs de statut
        private int _doneCount = 0;   // fichiers terminés avec succès
        private int _errorCount = 0;   // fichiers en échec
        private int PendingCount => _filesToUpload.Count - _doneCount - _errorCount;

        // Pour suivre quel fichier on télécharge et sa progression
        private string _currentFilePath;
        private int _currentFileIndex;
        private long _currentFileTotalBytes;
        private long _currentFileBytesSent;


        private long _totalBytesToSend;               // Taille totale à envoyer (en octets du coup)
        private DateTime _uploadStartTime;                // "Horaire" du début de l'upload
        private CancellationTokenSource _cts;               // Source de token pour annulation
        private string _lastStartedFile;
        private long _accumulatedBytesSent;

        private readonly WinFormsTimer _timerElapsed = new WinFormsTimer { Interval = 1000 };

        private bool _networkDisconnected = false;
        private int _uploadSessionCounter = 0;
        private int _activeUploadSessionId = 0;
        private AppLanguage _currentLanguage = AppLanguage.English;
        private bool _isApplyingLanguageSelection = false;

        // Custom UI fields
        private Label labelTargetDir;
        private Button btnResetTargetDir;
        private Button btnRefreshRemote;
        private ToolStripMenuItem menuRefresh;
        private ToolStripMenuItem menuRename;
        private ToolStripMenuItem menuMove;
        private ToolStripMenuItem menuDownload;
        private ToolStripMenuItem menuDelete;
        private ToolStripMenuItem menuSetDest;
        private ToolStripMenuItem menuFlattenRemote;
        private TreeNode _clickedLocalNode;
        private List<DataverseFileItem> _remoteFiles = new List<DataverseFileItem>();

        // Constructeur
        public Form1()                                        // Constructeur de la Form1
        {
            InitializeComponent();                           // Initialisation générée par le Designer
            InitializeLanguageSelector();
            SetLanguage(LoadSavedLanguage(), false);

            // Chargement des icônes depuis les ressources du projet
            imageListIcons.Images.Clear();
            imageListIcons.Images.Add("folder", Properties.Resources.FolderIcon16x16);
            imageListIcons.Images.Add("file", Properties.Resources.FileIcon16x16);

            progressBar.Style = ProgressBarStyle.Continuous; // Mise en style continu de la ProgressBar pour prise en compte de PBM_SETSTATE

            if (Properties.Settings.Default.ShowWarning)
            {
                ShowStartupWarning();                            // Affiche un avertissement au démarrage
                Properties.Settings.Default.ShowWarning = false;
                SaveLanguagePreference();
            }
            InitializeServerComboBox();                      // Initialise la liste des serveurs disponibles
            textBoxDoi.Leave += textBoxDoi_Leave;
            textBoxApiKey.Leave += textBoxApiKey_Leave;
            comboBoxServer.SelectedIndexChanged += comboBoxServer_SelectedIndexChanged;

            btnCancel.Visible = false;                       // Cache le bouton Cancel tant qu'aucun upload n'est lancé

            treeViewSelected.AllowDrop = true;

            treeViewSelected.DragEnter += treeViewSelected_DragEnter;
            treeViewSelected.DragDrop += treeViewSelected_DragDrop;

            // Gestion du timer pour le temps écoulé
            _timerElapsed.Tick += TimerElapsed_Tick;
            // Initialisation du label
            labelStatElapsedValue.Text = "00:00:00";
            _httpClient.Timeout = Timeout.InfiniteTimeSpan;   // ou TimeSpan.FromMinutes(30);
            tabControlMain.SelectedIndexChanged += tabControlMain_SelectedIndexChanged;



            // Écouteur réseau
            NetworkChange.NetworkAvailabilityChanged += (s, e) =>
            {
                if (!e.IsAvailable && _cts != null && !_cts.IsCancellationRequested)
                {
                    _networkDisconnected = true;
                    _cts.Cancel();  // stoppe vraiment l'upload en cours
                    SafeSetLabel(labelStatus, Localize("Network disconnected", "Réseau déconnecté"));
                }
            };

            // Initialize Refresh Panel on tabPageRemote
            treeViewRemote.Dock = DockStyle.Fill;
            Panel panelRemoteHeader = new Panel { Dock = DockStyle.Top, Height = 35 };
            btnRefreshRemote = new Button
            {
                Text = Localize("Refresh", "Actualiser"),
                Location = new Point(5, 5),
                Size = new Size(120, 25),
                Cursor = Cursors.Hand
            };
            btnRefreshRemote.Click += async (s, ev) => await LoadRemoteFilesAsync();
            panelRemoteHeader.Controls.Add(btnRefreshRemote);

            tabPageRemote.Controls.Clear();
            tabPageRemote.Controls.Add(treeViewRemote);
            tabPageRemote.Controls.Add(panelRemoteHeader);

            // Initialize Target Folder Panel on tabPageFiles
            treeViewSelected.Dock = DockStyle.Fill;
            Panel panelFilesHeader = new Panel { Dock = DockStyle.Top, Height = 40 };

            labelTargetDir = new Label
            {
                Location = new Point(5, 12),
                AutoSize = true,
                Font = new Font(treeViewSelected.Font, FontStyle.Bold),
                ForeColor = Color.DarkSlateGray,
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };

            btnResetTargetDir = new Button
            {
                Location = new Point(550, 7),
                Size = new Size(170, 25),
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnResetTargetDir.Click += (s, ev) =>
            {
                treeViewRemote.SelectedNode = null;
                UpdateTargetDirLabel();
                CompareLocalWithRemote(_remotePaths);
            };

            panelFilesHeader.Controls.Add(labelTargetDir);
            panelFilesHeader.Controls.Add(btnResetTargetDir);

            tabPageFiles.Controls.Clear();
            tabPageFiles.Controls.Add(treeViewSelected);
            tabPageFiles.Controls.Add(panelFilesHeader);

            UpdateTargetDirLabel();

            // Initialize ContextMenu for Remote TreeView
            var contextMenuRemote = new ContextMenuStrip();
            
            menuRefresh = new ToolStripMenuItem();
            menuRefresh.Text = Localize("Refresh", "Actualiser");
            menuRefresh.Click += async (s, ev) => await LoadRemoteFilesAsync();
            contextMenuRemote.Items.Add(menuRefresh);

            menuRename = new ToolStripMenuItem();
            menuRename.Text = Localize("Rename on server", "Renommer sur le serveur");
            menuRename.Click += async (s, ev) => await RenameRemoteFileAsync();
            contextMenuRemote.Items.Add(menuRename);

            menuMove = new ToolStripMenuItem();
            menuMove.Text = Localize("Move on server", "Déplacer sur le serveur");
            menuMove.Click += async (s, ev) => await MoveRemoteFileAsync();
            contextMenuRemote.Items.Add(menuMove);

            menuDownload = new ToolStripMenuItem();
            menuDownload.Text = Localize("Download", "Télécharger");
            menuDownload.Click += async (s, ev) => await DownloadRemoteItemAsync();
            contextMenuRemote.Items.Add(menuDownload);

            menuDelete = new ToolStripMenuItem();
            menuDelete.Text = Localize("Delete from server", "Supprimer du serveur");
            menuDelete.Click += async (s, ev) => await DeleteRemoteFileAsync();
            contextMenuRemote.Items.Add(menuDelete);

            menuFlattenRemote = new ToolStripMenuItem();
            menuFlattenRemote.Text = Localize("Flatten this folder", "Aplatir ce dossier");
            menuFlattenRemote.Click += async (s, ev) => await FlattenRemoteFolderAsync();
            contextMenuRemote.Items.Add(menuFlattenRemote);

            menuSetDest = new ToolStripMenuItem();
            menuSetDest.Text = Localize("Upload to this folder", "Téléverser dans ce dossier");
            menuSetDest.Click += (s, ev) => SetTargetFolderFromRemote();
            contextMenuRemote.Items.Add(menuSetDest);

            contextMenuRemote.Opening += (s, ev) =>
            {
                TreeNode node = treeViewRemote.SelectedNode;
                bool isFile = node != null && node.ImageKey == "file";
                bool isFolder = node != null && node.ImageKey == "folder" && node.Parent != null;
                bool isRoot = node != null && node.ImageKey == "folder" && node.Parent == null;
                
                var selectedNodes = treeViewRemote.SelectedNodes;
                if (selectedNodes == null || selectedNodes.Count == 0)
                {
                    ev.Cancel = true;
                    return;
                }

                bool hasFile = false;
                bool hasFolder = false;
                bool hasRoot = false;

                foreach (var n in selectedNodes)
                {
                    if (n.ImageKey == "file") hasFile = true;
                    else if (n.Parent == null) hasRoot = true;
                    else hasFolder = true;
                }

                // Rename only for single selection (file or folder, excluding root)
                menuRename.Enabled = selectedNodes.Count == 1 && !hasRoot;

                // Move for any selection (excluding root)
                menuMove.Enabled = !hasRoot;

                // Download for any selection
                menuDownload.Enabled = true;

                // Delete for any selection (excluding root)
                menuDelete.Enabled = !hasRoot;

                // Flatten only if folders (excluding root) are selected
                menuFlattenRemote.Enabled = hasFolder && !hasFile && !hasRoot;

                // Upload destination only for single folder or root selection
                menuSetDest.Enabled = selectedNodes.Count == 1 && (hasFolder || hasRoot);

                if (isFile)
                {
                    menuDelete.Text = Localize("Delete from server", "Supprimer du serveur");
                }
                else if (isFolder)
                {
                    menuDelete.Text = Localize("Delete this folder and its contents", "Supprimer ce dossier et son contenu");
                }
            };

            treeViewRemote.NodeMouseClick += (s, ev) =>
            {
                if (ev.Button == MouseButtons.Right)
                {
                    if (!treeViewRemote.SelectedNodes.Contains(ev.Node))
                    {
                        treeViewRemote.SelectedNodes.Clear();
                        treeViewRemote.SelectedNodes.Add(ev.Node);
                        treeViewRemote.SelectedNode = ev.Node;
                    }
                    contextMenuRemote.Show(treeViewRemote, ev.Location);
                }
            };

            treeViewRemote.AfterSelect += (s, ev) =>
            {
                UpdateTargetDirLabel();
                CompareLocalWithRemote(_remotePaths);
            };

            // Extract the JAR engine from resources on startup
            ExtractJarFromResources();
        }

        private bool IsFrench => _currentLanguage == AppLanguage.French;

        private string Localize(string english, string french)
        {
            return IsFrench ? french : english;
        }

        private void InitializeLanguageSelector()
        {
            comboBoxLanguage.Items.Clear();
            comboBoxLanguage.Items.Add("English");
            comboBoxLanguage.Items.Add("Français");
            comboBoxLanguage.SelectedIndexChanged += comboBoxLanguage_SelectedIndexChanged;
        }

        private AppLanguage LoadSavedLanguage()
        {
            string savedValue = Properties.Settings.Default.UiLanguage;
            if (string.IsNullOrEmpty(savedValue))
            {
                string sysLang = System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;
                if (string.Equals(sysLang, "fr", StringComparison.OrdinalIgnoreCase))
                {
                    return AppLanguage.French;
                }
                return AppLanguage.English;
            }
            return string.Equals(savedValue, "fr", StringComparison.OrdinalIgnoreCase)
                ? AppLanguage.French
                : AppLanguage.English;
        }

        private void SaveLanguagePreference()
        {
            Properties.Settings.Default.UiLanguage = IsFrench ? "fr" : "en";
            Properties.Settings.Default.Save();
        }

        private void SetLanguage(AppLanguage language, bool savePreference)
        {
            _currentLanguage = language;

            _isApplyingLanguageSelection = true;
            comboBoxLanguage.SelectedIndex = IsFrench ? 1 : 0;
            _isApplyingLanguageSelection = false;

            if (savePreference)
                SaveLanguagePreference();

            ApplyLanguage();
        }

        private void comboBoxLanguage_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isApplyingLanguageSelection || comboBoxLanguage.SelectedIndex < 0)
                return;

            SetLanguage(comboBoxLanguage.SelectedIndex == 1 ? AppLanguage.French : AppLanguage.English, true);
        }

        private void ApplyLanguage()
        {
            toolStripMenuItemRemove.Text = Localize("Remove", "Supprimer");
            toolStripMenuItemFlatten.Text = Localize("Flatten this folder", "Aplatir ce dossier");

            labelApiKey.Text = "API Key :";
            labelDoi.Text = "DOI :";
            labelServer.Text = Localize("Server :", "Serveur :");
            labelLanguage.Text = Localize("Language:", "Langue :");

            buttonSelectFiles.Text = Localize("Select Files", "Sélectionner des fichiers");
            buttonSelectFolder.Text = Localize("Select Folder", "Sélectionner un dossier");
            buttonReset.Text = Localize("Reset", "Réinitialiser");
            buttonUpload.Text = Localize("Upload", "Téléverser");
            btnAbout.Text = Localize("About", "À propos");
            btnCancel.Text = Localize("CANCEL", "ANNULER");

            groupBoxStats.Text = Localize("Statistics", "Statistiques");
            tabPageFiles.Text = Localize("Files", "Fichiers");
            tabPageRemote.Text = Localize("Server Files", "Fichiers sur le serveur");
            tabPageLogs.Text = Localize("Java Engine Logs", "Logs du moteur Java");

            if (btnResetTargetDir != null)
                btnResetTargetDir.Text = Localize("Reset to Root", "Déposer à la racine");
            if (btnRefreshRemote != null)
                btnRefreshRemote.Text = Localize("Refresh", "Actualiser");
            if (menuRefresh != null)
                menuRefresh.Text = Localize("Refresh", "Actualiser");
            if (menuRename != null)
                menuRename.Text = Localize("Rename on server", "Renommer sur le serveur");
            if (menuMove != null)
                menuMove.Text = Localize("Move on server", "Déplacer sur le serveur");
            if (menuDownload != null)
                menuDownload.Text = Localize("Download", "Télécharger");
            if (menuDelete != null)
                menuDelete.Text = Localize("Delete from server", "Supprimer du serveur");
            if (menuFlattenRemote != null)
                menuFlattenRemote.Text = Localize("Flatten this folder", "Aplatir ce dossier");
            if (menuSetDest != null)
                menuSetDest.Text = Localize("Upload to this folder", "Téléverser dans ce dossier");
            UpdateTargetDirLabel();
            labelStatTotalFiles.Text = Localize("Total files:", "Total fichiers :");
            labelStatTotalFolders.Text = Localize("Total folders:", "Total dossiers :");
            labelStatSpeed.Text = Localize("Speed (MB/s):", "Vitesse (Mo/s) :");
            labelStatETA.Text = Localize("Time left:", "Temps restant :");
            labelStatElapsed.Text = Localize("Elapsed:", "Temps écoulé :");
            labelStatPending.Text = Localize("Pending:", "En attente :");
            labelStatDone.Text = Localize("Completed:", "Terminés :");
            labelStatError.Text = Localize("Errors:", "Erreurs :");

            InitializeFieldHelp();

            if (_cts == null && labelSpeed.Text.StartsWith("0", StringComparison.Ordinal))
                labelSpeed.Text = $"0 {GetSpeedUnit()}";

            if (_cts == null && _doneCount == 0 && _errorCount == 0)
                labelStatus.Text = GetSelectionStatusText();

            RefreshTreeNodeStatusesForCurrentLanguage();

            if (_remoteFiles != null && _remoteFiles.Count > 0)
            {
                var expanded = GetExpandedPaths(treeViewRemote);
                string selectedPath = GetSelectedNodePath(treeViewRemote);
                BuildRemoteTreeView(_remoteFiles, expanded, selectedPath);
            }
        }

        private string GetSpeedUnit()
        {
            return IsFrench ? "Mo/s" : "MB/s";
        }

        private string GetSelectionStatusText()
        {
            if (_filesToUpload.Count == 0)
                return Localize("No files selected.", "Aucun fichier sélectionné.");

            return IsFrench
                ? $"{_filesToUpload.Count} fichier(s) sélectionné(s)."
                : $"{_filesToUpload.Count} file(s) selected.";
        }

        private void RefreshTreeNodeStatusesForCurrentLanguage()
        {
            foreach (TreeNode root in treeViewSelected.Nodes)
                RefreshTreeNodeStatusesRecursive(root);
        }

        private void RefreshTreeNodeStatusesRecursive(TreeNode node)
        {
            if (node.Tag is string path && File.Exists(path))
            {
                string fileName = Path.GetFileName(path);

                if (node.ForeColor == Color.Green)
                {
                    if (node.Text.Contains("sur le serveur") || node.Text.Contains("on server"))
                    {
                        node.Text = IsFrench
                            ? $"{fileName} — Déjà sur le serveur"
                            : $"{fileName} — Already on server";
                    }
                    else
                    {
                        node.Text = BuildDoneLabel(fileName);
                    }
                }
                else if (node.ForeColor == Color.Red)
                {
                    node.Text = BuildRetryErrorLabel(fileName);
                }
                else if (node.ForeColor == Color.Chocolate)
                {
                    string text = node.Text;
                    string pathsStr = "";
                    int idx = text.IndexOf(" — Existe déjà dans : ");
                    if (idx >= 0)
                    {
                        pathsStr = text.Substring(idx + " — Existe déjà dans : ".Length);
                    }
                    else
                    {
                        idx = text.IndexOf(" — Already exists in: ");
                        if (idx >= 0)
                        {
                            pathsStr = text.Substring(idx + " — Already exists in: ".Length);
                        }
                    }
                    if (!string.IsNullOrEmpty(pathsStr))
                    {
                        node.Text = IsFrench
                            ? $"{fileName} — Existe déjà dans : {pathsStr}"
                            : $"{fileName} — Already exists in: {pathsStr}";
                    }
                }
            }

            foreach (TreeNode child in node.Nodes)
                RefreshTreeNodeStatusesRecursive(child);
        }

        private void FlashProgressBarError(int delayMs = 5_000)
        {
            // 1) Passe immédiatement la barre en rouge
            progressBar.Invoke((Action)(() =>
            {
                SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_ERROR, IntPtr.Zero);
            }));

            // 2) Après delayMs, on revient au vert et on remet la valeur à 0
            var t = new WinFormsTimer { Interval = delayMs };
            t.Tick += (s, _) =>
            {
                t.Stop();

                progressBar.Invoke((Action)(() =>
                {
                    //   → couleur normale
                    SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);
                    //   → position remise à zéro
                    progressBar.Value = 0;
                }));
            };
            t.Start();
        }


        // Thread-safe setter pour un Label avec couleur optionnelle
        private void SafeSetLabel(Label lbl, string text, Color? color = null)
        {
            Color targetColor = color ?? SystemColors.ControlText;
            if (lbl.InvokeRequired)
            {
                lbl.Invoke((Action)(() =>
                {
                    lbl.Text = text;
                    lbl.ForeColor = targetColor;
                }));
            }
            else
            {
                lbl.Text = text;
                lbl.ForeColor = targetColor;
            }
        }

        // Thread-safe append pour le TextBox de logs
        private void AppendLog(string text)
        {
            if (textBoxLogs.InvokeRequired)
            {
                textBoxLogs.BeginInvoke((Action)(() => AppendLog(text)));
            }
            else
            {
                textBoxLogs.AppendText(text + Environment.NewLine);
                textBoxLogs.SelectionStart = textBoxLogs.TextLength;
                textBoxLogs.ScrollToCaret();
                SendMessage(textBoxLogs.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            }
        }

        // Met à jour Terminés / Erreurs / En attente
        private void RefreshStatLabels()
        {
            SafeSetLabel(labelStatDoneValue, $"{_doneCount}/{_filesToUpload.Count}");
            SafeSetLabel(labelStatErrorValue, _errorCount.ToString());
            SafeSetLabel(labelStatPendingValue, PendingCount.ToString());
        }


        private void TimerElapsed_Tick(object sender, EventArgs e)
        {
            if (_uploadStartTime == default) return;
            var elapsed = DateTime.Now - _uploadStartTime;
            labelStatElapsedValue.Text =
                $"{elapsed.Hours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
        }


        private void ShowStartupWarning()                    // Affiche un message d'avertissement au lancement
        {
            string message = Localize(
                "This software is designed for large datasets with many subfolders.\n" +
                "For small datasets, please use the web interface to save server resources.",
                "Ce logiciel est prévu pour de gros volumes comportant de nombreux sous-dossiers.\n" +
                "Pour de petits jeux de données, préférez l’interface web afin d’économiser les ressources serveur.");

            MessageBox.Show(message,
                            Localize("Recommended use", "Usage recommandé"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning);
        }

        private void InitializeServerComboBox()               // Remplit et configure le ComboBox des serveurs
        {
            comboBoxServer.Items.Clear();                    // Vide d'abord la liste (pour éviter des erreurs ?)
            comboBoxServer.Items.Add("https://demo.recherche.data.gouv.fr");    // Serveur de demo de RDG
            comboBoxServer.Items.Add("https://entrepot.recherche.data.gouv.fr"); // Entrepôt officiel de RDG
            comboBoxServer.DropDownStyle = ComboBoxStyle.DropDownList;          // Non éditable
            if (comboBoxServer.Items.Count > 0)
                comboBoxServer.SelectedIndex = 0;             // Sélectionne le premier élément par défaut
        }

        private void InitializeFieldHelp()
        {
            toolTipFieldHelp.SetToolTip(
                buttonApiKeyInfo,
                Localize("Understand what the API key is and where to find it.",
                         "Comprendre la clé API et savoir où la récupérer."));
            toolTipFieldHelp.SetToolTip(
                buttonDoiInfo,
                Localize("Understand what the DOI is, which format is expected, and where to find it.",
                         "Comprendre le DOI, son format attendu et où le trouver."));
            toolTipFieldHelp.SetToolTip(textBoxApiKey, GetApiKeyHelpText());
            toolTipFieldHelp.SetToolTip(textBoxDoi, GetDoiHelpText());
        }

        private void buttonApiKeyInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                GetApiKeyHelpText(),
                Localize("About the API key", "À propos de la clé API"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void buttonDoiInfo_Click(object sender, EventArgs e)
        {
            MessageBox.Show(
                GetDoiHelpText(),
                Localize("About the DOI", "À propos du DOI"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private string GetApiKeyHelpText()
        {
            return Localize(
                "Your API key is your personal access token. It allows the application " +
                "to deposit files in Dataverse without using your password.\n\n" +
                "Where to find it: open your profile on the RDG / Dataverse platform, " +
                "then go to the API Token section to generate or copy your token.",
                "La clé API est votre jeton personnel d'accès. Elle permet à l'application " +
                "de déposer les fichiers dans Dataverse sans utiliser votre mot de passe.\n\n" +
                "Où la trouver : ouvrez votre profil sur la plateforme RDG / Dataverse, puis " +
                "la section API Token ou Clé API pour générer ou copier votre jeton.");
        }

        private string GetDoiHelpText()
        {
            return Localize(
                "The DOI is the persistent identifier of the dataset that will receive the files.\n\n" +
                "Where to find it: open the target dataset page and copy the DOI shown by the platform.\n\n" +
                "Example shown on the site: https://doi.org/10.57745/IQ91KC\n" +
                "Expected format here: doi:10.57745/IQ91KC\n\n" +
                "You can paste the full DOI URL: it will be converted automatically.",
                "Le DOI identifie de manière pérenne le jeu de données qui recevra les fichiers.\n\n" +
                "Où le trouver : ouvrez la page du dataset cible et copiez le DOI affiché par la plateforme.\n\n" +
                "Exemple affiché sur le site : https://doi.org/10.57745/IQ91KC\n" +
                "Format attendu ici : doi:10.57745/IQ91KC\n\n" +
                "Vous pouvez coller l'URL complète : elle sera convertie automatiquement.");
        }

        private async void textBoxDoi_Leave(object sender, EventArgs e)
        {
            textBoxDoi.Text = NormalizeDoiInput(textBoxDoi.Text);
            await AutoCheckRemoteFilesAsync();
        }

        private async void textBoxApiKey_Leave(object sender, EventArgs e)
        {
            await AutoCheckRemoteFilesAsync();
        }

        private async void comboBoxServer_SelectedIndexChanged(object sender, EventArgs e)
        {
            await AutoCheckRemoteFilesAsync();
        }

        private string NormalizeDoiInput(string rawValue)
        {
            if (string.IsNullOrWhiteSpace(rawValue))
                return string.Empty;

            string value = rawValue.Trim();

            if (value.StartsWith("https://doi.org/", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("https://doi.org/".Length);
            else if (value.StartsWith("http://doi.org/", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("http://doi.org/".Length);
            else if (value.StartsWith("https://dx.doi.org/", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("https://dx.doi.org/".Length);
            else if (value.StartsWith("http://dx.doi.org/", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("http://dx.doi.org/".Length);
            else if (value.StartsWith("doi:", StringComparison.OrdinalIgnoreCase))
                value = value.Substring("doi:".Length);

            value = value.Trim().TrimStart('/');
            return string.IsNullOrWhiteSpace(value) ? string.Empty : $"doi:{value}";
        }

        private bool IsNormalizedDoi(string value)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.StartsWith("doi:", StringComparison.OrdinalIgnoreCase))
                return false;

            string suffix = value.Substring("doi:".Length).Trim();
            return suffix.Length > 0 && suffix.Contains("/");
        }

        // Annuler = barre rouge

        private void btnCancel_Click(object sender, EventArgs e) // Handler (surveuille un événement précis) soit le clic du bouton Cancel
        {
            btnCancel.Enabled = false;                         // Empêche le double clic
            SendMessage(progressBar.Handle,                    // Envoie un message Windows
                        PBM_SETSTATE,                          // Pour changer l'état
                        (IntPtr)PBST_ERROR,                    // Vers l'état ERROR (rouge)
                        IntPtr.Zero);                           // lParam inutile
            _cts?.Cancel();                                    // Demande l'annulation via le CancellationTokenSource
        }

        // Sélection de fichiers

        private void buttonSelectFiles_Click(object sender, EventArgs e) // Handler du clic sur Sélectionner Fichiers
        {
            using (var ofd = new OpenFileDialog                  // Crée un OpenFileDialog
            {
                Title = Localize("Select one or more files", "Sélectionner un ou plusieurs fichiers"),
                Filter = Localize("All files (*.*)|*.*", "Tous les fichiers (*.*)|*.*"),
                Multiselect = true                                   // Autorise la sélection multiple de fichiers
            })
            {
                if (ofd.ShowDialog() != DialogResult.OK)            // Si l'utilisateur annule
                    return;                                         // Sort de la méthode

                foreach (string file in ofd.FileNames)               // Pour chaque fichier sélectionné
                {
                    if (_filesToUpload.Contains(file))              // Si déjà dans la liste
                        continue;                                   // Passe au suivant

                    _filesToUpload.Add(file);                       // Ajoute le chemin complet
                    _fileRelativePaths[file] = "";                  // Chemin relatif vide (racine)
                    AddFileNode(file);                              // Ajoute un nœud dans l'arborescence
                }

                UpdateStatus();                                     // Met à jour les statistiques à l'écran
            }
        }

        // Sélection de dossier

        private void buttonSelectFolder_Click(object sender, EventArgs e) // Handler du clic sur Sélectionner Dossier
        {
            using (var fbd = new FolderBrowserDialog               // Crée un FolderBrowserDialog
            {
                Description = Localize(
                    "Select a folder to upload, including all subfolders.",
                    "Sélectionner un dossier à uploader (tous les sous-dossiers inclus)")
            })
            {
                if (fbd.ShowDialog() != DialogResult.OK             // Si annulation
                    || string.IsNullOrWhiteSpace(fbd.SelectedPath)) // Ou chemin vide
                    return;                                         // Sort

                string root = fbd.SelectedPath;                     // Chemin du dossier sélectionné

                if (_foldersSelected.Contains(root))                // Si déjà ajouté
                {
                    MessageBox.Show(
                                    Localize("This folder has already been added.", "Ce dossier a déjà été ajouté."),
                                    Localize("Information", "Information"),
                                    MessageBoxButtons.OK,
                                    MessageBoxIcon.Information);
                    return;
                }

                _foldersSelected.Add(root);                         // Ajoute au lot de dossiers
                var rootNode = new TreeNode(Path.GetFileName(root)) // Crée un nœud racine
                {
                    Tag = root,                        // Stocke le chemin
                    ImageKey = "folder",                    // Clé icône folder
                    SelectedImageKey = "folder"                     // Icône selection
                };
                treeViewSelected.Nodes.Add(rootNode);               // Ajoute à l'arbre
                AddDirectoryNodes(rootNode, root, root);            // Reconstruit récursivement
                rootNode.ExpandAll();                               // Déplie tous les nœuds enfants
                UpdateStatus();                                     // Met à jour les stats
            }
        }

        private void buttonReset_Click(object sender, EventArgs e) // Handler du clic sur Réinitialiser
        {
            // Ignore toute ligne de sortie retardataire d'un moteur Java déjà terminé.
            Interlocked.Exchange(ref _activeUploadSessionId, 0);
            _filesToUpload.Clear();                                // Vide la liste de fichiers
            _fileRelativePaths.Clear();                            // Vide le mapping relatifs
            _foldersSelected.Clear();                              // Vide la liste de dossiers
            treeViewSelected.ClearAllSelections();                 // Supprime aussi la sélection visuelle personnalisée
            treeViewSelected.SelectedNode = null;
            treeViewSelected.Nodes.Clear();                        // Vide l'arborescence affichée
            
            // Réinitialisation des statistiques et de la barre de progression
            SafeSetLabel(labelStatSpeedValue, "0.0");
            SafeSetLabel(labelStatETAValue, "00:00:00");
            SafeSetLabel(labelStatElapsedValue, "00:00:00");
            SafeSetLabel(labelSpeed, $"0.0 {GetSpeedUnit()}");
            
            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;
            SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);
            
            UpdateStatus();                                        // Met à jour l'affichage
        }

        /*
         * =========================
         * Pour AddDirectoryNodes
         * =========================
         * Notre but : explorer un dossier sur le disque, construire un arbre visuel
         *      (le TreeView) qui reflète sa structure, et préparer la liste des
         *      fichiers à envoyer avec leur chemin relatif pour Dataverse.
         *
         * Principe :
         * 1. Pour chaque sous-dossier de currentDir :
         *    - On crée un nœud « dossier » dans l’arborescence (TreeNode).
         *    - On appelle récursivement la même méthode pour ses sous-dossiers.
         *
         * 2. Pour chaque fichier de currentDir :
         *    - On vérifie qu’il n’a pas déjà été ajouté (évite les doublons).
         *    - On ajoute son chemin complet à _filesToUpload.
         *    - On calcule son chemin relatif par rapport à baseDir pour Dataverse :
         *        • Supprimer la partie commune baseDir
         *        • Remplacer les séparateurs Windows par des slashs « / »
         *        • Préfixer par le nom de dossier racine
         *    - On stocke ce « directoryLabel » dans _fileRelativePaths[file].
         *    - On crée un nœud « fichier » dans l’arbre pour l’afficher.
         *
         * Résultat :
         * - TreeView construit : chaque dossier et chaque fichier apparaît à la
         *   bonne position, exactement comme sur notre disque dur, avec cela on garantit
         *   que la structure sera strictement conservée.
         * =========================================================================
         */

        private void AddDirectoryNodes(TreeNode parentNode, string currentDir, string baseDir)
        {
            // 1. On commence par lister tous les sous-dossiers du dossier courant
            foreach (string subDir in Directory.GetDirectories(currentDir))
            {
                _foldersSelected.Add(subDir);
                // 1.1. Pour chaque sous-dossier, on crée un nœud « dossier » dans l’arbre
                var dirNode = new TreeNode(Path.GetFileName(subDir)) // Le nom affiché est juste le dernier segment du chemin
                {
                    Tag = subDir,      // On stocke le chemin complet dans Tag pour y revenir plus tard
                    ImageKey = "folder",    // On utilise l’icône « dossier »
                    SelectedImageKey = "folder"     // Même icône quand c’est sélectionné
                };
                parentNode.Nodes.Add(dirNode);      // On accroche ce nœud au nœud parent
                AddDirectoryNodes(dirNode, subDir, baseDir); // Puis on répète la même opération à l’intérieur de ce sous-dossier (donc récursion)
            }

            // 2. Ensuite, on ajoute tous les fichiers du dossier courant
            foreach (string file in Directory.GetFiles(currentDir))
            {
                // 2.1. Si ce fichier est déjà dans notre liste, on passe au suivant
                if (_filesToUpload.Contains(file))
                    continue;

                // 2.2. On ajoute le chemin complet du fichier à la liste des fichiers à uploader
                _filesToUpload.Add(file);

                // 2.3. On calcule ensuite son chemin relatif pour Dataverse
                //      (c’est-à-dire la partie de l’arborescence qui apparaît dans l’interface Dataverse)
                string parentPath = Path.GetDirectoryName(file) ?? ""; // Dossier parent du fichier
                string rel = "";                                       // Variable pour construire le chemin relatif
                if (parentPath.Length > baseDir.Length)
                {
                    // On retire la partie « baseDir » et on nettoie les séparateurs
                    rel = parentPath
                          .Substring(baseDir.Length)                               // Tout ce qui vient après baseDir
                          .TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) // Enlève les slashs au début
                          .Replace(Path.DirectorySeparatorChar, '/')               // Remplace les backslashs par slashs
                          .Replace(Path.AltDirectorySeparatorChar, '/');
                }
                string rootName = Path.GetFileName(baseDir);                      // Le nom de la racine (dossier sélectionné)
                string fullRel = string.IsNullOrEmpty(rel)
                                 ? rootName                                      // Si pas de sous-dossier, on garde juste la racine
                                 : $"{rootName}/{rel}";                         // Sinon on ajoute la sous-arborescence
                _fileRelativePaths[file] = fullRel;                                // On enregistre ce chemin relatif

                // 2.4. Enfin on crée un nœud « fichier » dans l’arbre pour ce fichier
                var fileNode = new TreeNode(Path.GetFileName(file)) // Affiche seulement le nom du fichier, pas tout le chemin
                {
                    Tag = file,      // On stocke le chemin complet pour l’upload
                    ImageKey = "file",    // Icône « fichier »
                    SelectedImageKey = "file"     // Même icône si sélectionné
                };
                parentNode.Nodes.Add(fileNode);   // On accroche ce fichier sous son dossier parent dans le TreeView
            }
        }


        private void AddFileNode(string filePath)               // Ajoute un fichier isolé (pas de dossier)
        {
            var node = new TreeNode(Path.GetFileName(filePath))
            {
                Tag = filePath,
                ImageKey = "file",
                SelectedImageKey = "file"
            };
            treeViewSelected.Nodes.Add(node);                   // Insère à la racine
        }


        // Clic droit => supprimer
        private void treeViewSelected_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            // On ne traite que le clic droit
            if (e.Button != MouseButtons.Right)
                return;

            // Si on n'a rien de sélectionné, on n'affiche rien
            var msTree = treeViewSelected as MultiSelectTreeView;
            if (msTree == null || msTree.SelectedNodes.Count == 0)
                return;

            int selectedFolderCount = 0;
            foreach (TreeNode selectedNode in msTree.SelectedNodes)
            {
                if (selectedNode.Tag is string selectedPath && Directory.Exists(selectedPath))
                    selectedFolderCount++;
            }

            toolStripMenuItemFlatten.Visible = selectedFolderCount > 0;
            toolStripMenuItemFlatten.Text = selectedFolderCount > 1
                ? Localize("Flatten selected folders", "Aplatir les dossiers sélectionnés")
                : Localize("Flatten this folder", "Aplatir ce dossier");

            // Si le nœud cliqué fait partie de la sélection multiple, on affiche le menu
            if (msTree.SelectedNodes.Contains(e.Node))
            {
                _clickedLocalNode = e.Node;
                contextMenuTree.Show(treeViewSelected, e.Location);
            }
            // Sinon, on n'affiche rien (cliquer droit en dehors d'une sélection multiple est ignoré)
        }

        // Clic droit => "Aplatir ce dossier"
        private void toolStripMenuItemFlatten_Click(object sender, EventArgs e)
        {
            var foldersToFlatten = new List<TreeNode>();
            foreach (TreeNode selectedNode in treeViewSelected.SelectedNodes)
            {
                if (selectedNode.Tag is string selectedPath && Directory.Exists(selectedPath))
                    foldersToFlatten.Add(selectedNode);
            }

            // Repli pour le cas où le menu a été ouvert sur un seul nœud sans sélection multiple.
            if (foldersToFlatten.Count == 0 &&
                _clickedLocalNode?.Tag is string clickedPath && Directory.Exists(clickedPath))
            {
                foldersToFlatten.Add(_clickedLocalNode);
            }

            if (foldersToFlatten.Count == 0) return;

            // Les descendants sont traités avant leurs parents. Ainsi, sélectionner toute
            // l'arborescence produit bien un lot entièrement aplati à la racine.
            foldersToFlatten.Sort((a, b) => GetNodeDepth(b).CompareTo(GetNodeDepth(a)));

            foreach (TreeNode folderNode in foldersToFlatten)
            {
                if (folderNode.TreeView != treeViewSelected ||
                    !(folderNode.Tag is string folderPath) || !Directory.Exists(folderPath))
                {
                    continue;
                }

                TreeNode parent = folderNode.Parent;
                var children = new List<TreeNode>();
                foreach (TreeNode child in folderNode.Nodes) children.Add(child);

                foreach (TreeNode child in children)
                {
                    child.Remove();
                    if (parent != null) parent.Nodes.Add(child);
                    else treeViewSelected.Nodes.Add(child);
                }

                _foldersSelected.Remove(folderPath);
                folderNode.Remove();
            }

            // L'arbre visible devient l'unique source de vérité pour le manifeste d'upload.
            RebuildRelativePathsFromTree();
            treeViewSelected.ClearAllSelections();
            _clickedLocalNode = null;

            UpdateStatus();
        }

        private int GetNodeDepth(TreeNode node)
        {
            int depth = 0;
            while (node?.Parent != null)
            {
                depth++;
                node = node.Parent;
            }
            return depth;
        }


        private void toolStripMenuItemRemove_Click(object sender, EventArgs e)
        {
            var msTree = treeViewSelected as MultiSelectTreeView;
            List<TreeNode> toRemove;

            if (msTree != null && msTree.SelectedNodes.Count > 0)
            {
                // Copie la liste, car on va la modifier en enlevant des nœuds
                toRemove = new List<TreeNode>(msTree.SelectedNodes);
            }
            else
            {
                // Fallback : suppression du nœud unique sélectionné
                var single = treeViewSelected.SelectedNode;
                if (single == null) return;
                toRemove = new List<TreeNode> { single };
            }

            foreach (var node in toRemove)
            {
                // Gestion dossiers vs fichiers
                if (node.Tag is string dir && Directory.Exists(dir))
                {
                    _foldersSelected.Remove(dir);
                    var filesUnder = new List<string>();
                    CollectFilePaths(node, filesUnder);
                    foreach (var f in filesUnder)
                    {
                        _filesToUpload.Remove(f);
                        _fileRelativePaths.Remove(f);
                    }
                }
                else if (node.Tag is string file && File.Exists(file))
                {
                    _filesToUpload.Remove(file);
                    _fileRelativePaths.Remove(file);
                }

                // Retire le nœud de l'arbre
                node.Remove();
            }

            // Enfin, vide la sélection multiple
            if (msTree != null)
                msTree.SelectedNodes.Clear();

            UpdateStatus();
        }


        private void CollectFilePaths(TreeNode parentNode, List<string> collector) // Collecte récursivement fichiers
        {
            foreach (TreeNode child in parentNode.Nodes)
            {
                if (child.Tag is string p && File.Exists(p))
                    collector.Add(p);                            // Ajoute fichier
                else if (child.Tag is string d && Directory.Exists(d))
                    CollectFilePaths(child, collector);          // Appel récursif pour dossier
            }
        }

        private string ExtractJarFromResources()
        {
            try
            {
                string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                string jarName = "DVUploader-v1.3.0-RDGengine.jar";
                string jarPath = Path.Combine(targetDir, jarName);
                string resourceName = "RDG_Uploader_GUI." + jarName;

                var assembly = typeof(Form1).Assembly;
                using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                {
                    if (stream == null)
                    {
                        File.WriteAllText(Path.Combine(targetDir, "extraction_error.txt"), "Resource stream is null: " + resourceName);
                        return null;
                    }

                    if (File.Exists(jarPath))
                    {
                        FileInfo fi = new FileInfo(jarPath);
                        if (fi.Length == stream.Length)
                        {
                            return jarPath;
                        }
                    }

                    using (FileStream fs = new FileStream(jarPath, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        stream.CopyTo(fs);
                    }
                    return jarPath;
                }
            }
            catch (Exception ex1)
            {
                try
                {
                    string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                    File.WriteAllText(Path.Combine(targetDir, "extraction_error.txt"), "First try failed: " + ex1.ToString());
                }
                catch {}
                
                try
                {
                    string jarName = "DVUploader-v1.3.0-RDGengine.jar";
                    string fallbackDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "RDG_ArboDV");
                    if (!Directory.Exists(fallbackDir))
                    {
                        Directory.CreateDirectory(fallbackDir);
                    }
                    string jarPath = Path.Combine(fallbackDir, jarName);
                    string resourceName = "RDG_Uploader_GUI." + jarName;
                    var assembly = typeof(Form1).Assembly;
                    using (Stream stream = assembly.GetManifestResourceStream(resourceName))
                    {
                        if (stream == null) return null;

                        if (File.Exists(jarPath))
                        {
                            FileInfo fi = new FileInfo(jarPath);
                            if (fi.Length == stream.Length)
                            {
                                return jarPath;
                            }
                        }

                        using (FileStream fs = new FileStream(jarPath, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            stream.CopyTo(fs);
                        }
                        return jarPath;
                    }
                }
                catch (Exception ex2)
                {
                    try
                    {
                        string targetDir = AppDomain.CurrentDomain.BaseDirectory;
                        File.AppendAllText(Path.Combine(targetDir, "extraction_error.txt"), "\nFallback failed: " + ex2.ToString());
                    }
                    catch {}
                    return null;
                }
            }
        }

        // Upload principal

        private async void buttonUpload_Click(object sender, EventArgs e)
        {
            /* -- validations rapides -- */
            _networkDisconnected = false;

            string api = textBoxApiKey.Text.Trim();
            string doi = NormalizeDoiInput(textBoxDoi.Text);
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            textBoxDoi.Text = doi;

            if (string.IsNullOrWhiteSpace(api))
            {
                MessageBox.Show(
                    Localize(
                        "Enter the API key associated with your account before starting the upload.",
                        "Renseignez la clé API associée à votre compte avant de lancer l'upload."),
                    Localize("API Key required", "Clé API requise"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                textBoxApiKey.Focus();
                return;
            }

            if (!IsNormalizedDoi(doi))
            {
                MessageBox.Show(
                    Localize(
                        "The DOI must be provided in the following format:\n\ndoi:10.57745/IQ91KC\n\n" +
                        "You can also paste the full DOI URL; it will be converted automatically.",
                        "Le DOI doit être renseigné au format suivant :\n\ndoi:10.57745/IQ91KC\n\n" +
                        "Vous pouvez aussi coller l'URL complète du DOI ; elle sera convertie automatiquement."),
                    Localize("Invalid DOI format", "Format du DOI invalide"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                textBoxDoi.Focus();
                textBoxDoi.SelectAll();
                return;
            }

            if (string.IsNullOrWhiteSpace(srv))
            {
                MessageBox.Show(
                    Localize(
                        "Select the target server before starting the upload.",
                        "Sélectionnez le serveur cible avant de lancer l'upload."),
                    Localize("Server required", "Serveur requis"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                comboBoxServer.Focus();
                return;
            }

            if (_filesToUpload.Count == 0)
            {
                MessageBox.Show(
                    Localize(
                        "Add at least one file or folder before starting the upload.",
                        "Ajoutez au moins un fichier ou un dossier à uploader."),
                    Localize("No files selected", "Aucun fichier sélectionné"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // Find DVUploader jar
            string jarPath = ExtractJarFromResources();
            if (string.IsNullOrEmpty(jarPath) || !File.Exists(jarPath))
            {
                jarPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DVUploader-v1.3.0-RDGengine.jar");
                if (!File.Exists(jarPath))
                {
                    // check one level up (useful during development)
                    string parentDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.FullName;
                    if (parentDir != null)
                    {
                        jarPath = Path.Combine(parentDir, "DVUploader-v1.3.0-RDGengine.jar");
                    }
                }
                if (!File.Exists(jarPath))
                {
                    // Check in rdg_arbodv_v2 folder
                    string rootDir = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)?.Parent?.Parent?.Parent?.FullName;
                    if (rootDir != null)
                    {
                        jarPath = Path.Combine(rootDir, "DVUploader-v1.3.0-RDGengine.jar");
                    }
                }
                if (!File.Exists(jarPath))
                {
                    // Fallback to searching the whole rdg_arbodv_v2 directory structure
                    string searchDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..");
                    if (Directory.Exists(searchDir))
                    {
                        var files = Directory.GetFiles(searchDir, "DVUploader-*.jar", SearchOption.AllDirectories);
                        if (files.Length > 0)
                        {
                            jarPath = files[0];
                        }
                    }
                }
            }

            if (!File.Exists(jarPath))
            {
                MessageBox.Show(
                    Localize(
                        "The upload engine (DVUploader-v1.3.0-RDGengine.jar) was not found.\n" +
                        "Please compile it and place the jar in the application directory.",
                        "Le moteur d'envoi (DVUploader-v1.3.0-RDGengine.jar) est introuvable.\n" +
                        "Veuillez le compiler et placer le fichier jar dans le répertoire de l'application."),
                    Localize("Engine not found", "Moteur introuvable"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            // L'arbre visible est la source de vérité des chemins virtuels à envoyer.
            RebuildRelativePathsFromTree();

            // Check for duplicate files on server before starting upload
            try
            {
                await AutoCheckRemoteFilesAsync();
            }
            catch { }

            int duplicateCount = 0;
            int elsewhereCount = 0;
            string targetPrefixForDup = GetActiveTargetFolder();
            foreach (var file in _filesToUpload)
            {
                _fileRelativePaths.TryGetValue(file, out string relPath);
                string filename = Path.GetFileName(file);
                
                string finalRelPath = relPath ?? "";
                if (!string.IsNullOrEmpty(targetPrefixForDup))
                {
                    finalRelPath = string.IsNullOrEmpty(finalRelPath) 
                        ? targetPrefixForDup 
                        : $"{targetPrefixForDup}/{finalRelPath.Trim('/')}";
                }

                string destPath = string.IsNullOrEmpty(finalRelPath) 
                    ? filename 
                    : $"{finalRelPath.Replace('\\', '/').Trim('/')}/{filename}";

                if (_remotePaths.Contains(destPath))
                {
                    duplicateCount++;
                }
                else
                {
                    var existingPaths = FindAllRemotePathsWithFilename(filename);
                    if (existingPaths.Count > 0)
                    {
                        elsewhereCount++;
                    }
                }
            }

            if (duplicateCount > 0 || elsewhereCount > 0)
            {
                if (duplicateCount == _filesToUpload.Count)
                {
                    string msg = IsFrench
                        ? "Tous les fichiers sélectionnés sont déjà présents sur le serveur à l'emplacement choisi. Rien à téléverser.\n\nVoulez-vous quand même lancer le moteur de vérification ?"
                        : "All selected files already exist on the server at the target location. Nothing to upload.\n\nDo you still want to run the verification engine?";
                    
                    var dr = MessageBox.Show(msg, 
                        Localize("All files already on server", "Tous les fichiers déjà sur le serveur"), 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Information);
                    
                    if (dr != DialogResult.Yes)
                    {
                        return; // cancel upload
                    }
                }
                else
                {
                    string msg = "";
                    if (duplicateCount > 0)
                    {
                        msg += IsFrench
                            ? $"• {duplicateCount} fichier(s) existe(nt) déjà à l'emplacement choisi et ne seron(t) pas ré-téléversé(s).\n"
                            : $"• {duplicateCount} file(s) already exist at the chosen location and will not be re-uploaded.\n";
                    }
                    if (elsewhereCount > 0)
                    {
                        msg += IsFrench
                            ? $"• {elsewhereCount} fichier(s) existe(nt) déjà dans d'autres répertoires sur le serveur.\n"
                            : $"• {elsewhereCount} file(s) already exist in other directories on the server.\n";
                    }
                    msg += "\n" + (IsFrench ? "Voulez-vous continuer ?" : "Do you want to continue?");

                    var dr = MessageBox.Show(msg, 
                        Localize("Duplicates detected", "Doublons détectés"), 
                        MessageBoxButtons.YesNo, 
                        MessageBoxIcon.Question);
                    
                    if (dr != DialogResult.Yes)
                    {
                        return; // cancel upload
                    }
                }
            }

            /* -- préparation UI + token -- */
            ToggleControls(false);
            btnCancel.Visible = btnCancel.Enabled = true;
            SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);

            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            int uploadSessionId = Interlocked.Increment(ref _uploadSessionCounter);
            Interlocked.Exchange(ref _activeUploadSessionId, uploadSessionId);

            _doneCount = _errorCount = 0;
            _accumulatedBytesSent = 0;
            _lastStartedFile = null;
            RefreshStatLabels();
            textBoxLogs.Clear();

            // Clear old log files
            try { File.Delete("dvuploader_stdout.log"); } catch { }
            try { File.Delete("dvuploader_stderr.log"); } catch { }

            string tempManifestPath = Path.Combine(Path.GetTempPath(), $"rdg_arbodv_manifest_{Guid.NewGuid()}.json");

            try
            {
                /* Taille totale */
                _totalBytesToSend = 0;
                foreach (var f in _filesToUpload) _totalBytesToSend += new FileInfo(f).Length;

                progressBar.Invoke((Action)(() =>
                {
                    progressBar.Value = 0;
                    progressBar.Maximum = (int)(_totalBytesToSend / 1024);
                }));

                _uploadStartTime = DateTime.Now;
                labelStatElapsedValue.Text = "00:00:00";
                _timerElapsed.Start();

                // Write manifest JSON
                string targetPrefix = GetActiveTargetFolder();
                var manifestList = new List<object>();
                foreach (var file in _filesToUpload)
                {
                    _fileRelativePaths.TryGetValue(file, out string relPath);
                    string finalRelPath = relPath ?? "";
                    
                    if (!string.IsNullOrEmpty(targetPrefix))
                    {
                        finalRelPath = string.IsNullOrEmpty(finalRelPath) 
                            ? targetPrefix 
                            : $"{targetPrefix}/{finalRelPath.Trim('/')}";
                    }

                    manifestList.Add(new
                    {
                        source = file,
                        directoryLabel = finalRelPath
                    });
                }
                File.WriteAllText(tempManifestPath, JsonConvert.SerializeObject(manifestList, Formatting.Indented));

                // Start process
                var startInfo = new ProcessStartInfo
                {
                    FileName = "java",
                    Arguments = $"-jar \"{jarPath}\" -server=\"{srv}\" -key=\"{api}\" -did=\"{doi}\" -manifest=\"{tempManifestPath}\" -recurse",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = System.Text.Encoding.UTF8,
                    StandardErrorEncoding = System.Text.Encoding.UTF8
                };

                using (var process = new Process { StartInfo = startInfo })
                {
                    process.OutputDataReceived += (senderProcess, args) =>
                    {
                        if (args.Data != null)
                        {
                            ParseStdoutLine(args.Data, uploadSessionId);
                        }
                    };
                    process.ErrorDataReceived += (senderProcess, args) =>
                    {
                        if (args.Data != null)
                        {
                            ParseStderrLine(args.Data);
                        }
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    process.BeginErrorReadLine();

                    while (!process.HasExited)
                    {
                        if (token.IsCancellationRequested)
                        {
                            try { process.Kill(); } catch { }
                            break;
                        }
                        await Task.Delay(100);
                    }

                    // Attend aussi la fin des callbacks OutputDataReceived. Sans cette étape,
                    // une ancienne ligne "Dataset locked" peut recolorer l'UI après la fin.
                    await Task.Run(() => process.WaitForExit());
                }

                // If cancelled
                token.ThrowIfCancellationRequested();

                /* Fin de batch */
                _timerElapsed.Stop();
                progressBar.Style = ProgressBarStyle.Continuous;
                progressBar.Value = progressBar.Maximum;
                if (_errorCount == 0)
                {
                    SafeSetLabel(
                        labelStatus,
                        IsFrench
                            ? $"Téléversement terminé : {_doneCount}/{_filesToUpload.Count} fichier(s) traité(s)."
                            : $"Upload complete: {_doneCount}/{_filesToUpload.Count} file(s) processed.",
                        Color.DarkGreen);
                    MessageBox.Show(
                        Localize("All files were uploaded successfully.", "Tous les fichiers ont été uploadés !"),
                        Localize("Success", "Succès"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    SafeSetLabel(
                        labelStatus,
                        IsFrench
                            ? $"Téléversement terminé avec {_errorCount} erreur(s)."
                            : $"Upload completed with {_errorCount} error(s).",
                        Color.DarkRed);
                    MessageBox.Show(
                        IsFrench
                            ? $"{_errorCount} fichier(s) en échec. Voir dvuploader_stderr.log."
                            : $"{_errorCount} file(s) failed. See dvuploader_stderr.log.",
                        Localize("Completed with errors", "Terminé avec erreurs"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                // Refresh remote tree view automatically
                await LoadRemoteFilesAsync();
            }
            catch (OperationCanceledException)
            {
                _timerElapsed.Stop();
                if (_networkDisconnected)
                {
                    FlashProgressBarError(); // rouge 5 s
                    MessageBox.Show(
                        Localize("Upload stopped: network disconnected.", "Upload stoppé : réseau déconnecté."),
                        Localize("Network error", "Erreur réseau"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
                else
                {
                    MessageBox.Show(
                        Localize("Upload cancelled by the user.", "Upload annulé par l’utilisateur."),
                        Localize("Cancelled", "Annulé"),
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                try { await LoadRemoteFilesAsync(); } catch { }
            }
            catch (Exception ex)
            {
                _timerElapsed.Stop();
                MessageBox.Show(
                    Localize($"An error occurred: {ex.Message}", $"Une erreur est survenue : {ex.Message}"),
                    Localize("Error", "Erreur"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                try { await LoadRemoteFilesAsync(); } catch { }
            }
            finally
            {
                Interlocked.CompareExchange(ref _activeUploadSessionId, 0, uploadSessionId);
                try { if (File.Exists(tempManifestPath)) File.Delete(tempManifestPath); } catch { }

                SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);
                progressBar.Invoke((Action)(() => progressBar.Style = ProgressBarStyle.Continuous));
                btnCancel.Visible = false;
                ToggleControls(true);
                _timerElapsed.Stop();
                _cts?.Dispose();
                _cts = null;

                // Reset status label text/color and tree view highlights
                UpdateStatus();
            }
        }

        private void ParseStdoutLine(string line, int uploadSessionId)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            // Log stdout
            try { File.AppendAllText("dvuploader_stdout.log", $"{DateTime.Now:u} | {line}\n"); } catch { }
            AppendLog(line);

            // Les événements de sortie sont asynchrones : une ligne d'une session terminée
            // reste dans les logs, mais ne doit plus modifier l'interface courante.
            if (uploadSessionId != Volatile.Read(ref _activeUploadSessionId))
                return;

            // 1. PROCESSING(F): D:\path\to\file.txt
            if (line.Contains("PROCESSING(F): "))
            {
                int index = line.IndexOf("PROCESSING(F): ");
                string filePath = line.Substring(index + "PROCESSING(F): ".Length).Trim();
                
                _lastStartedFile = filePath;
                _currentFilePath = filePath;
                _currentFileIndex = _filesToUpload.IndexOf(filePath);
                _currentFileBytesSent = 0;
                _currentFileTotalBytes = File.Exists(filePath) ? new FileInfo(filePath).Length : 0;

                SafeSetLabel(labelStatus, BuildUploadProgressLabel(Path.GetFileName(filePath), 0));
                
                var n = FindNodeByTag(filePath);
                if (n != null)
                {
                    treeViewSelected.Invoke((Action)(() =>
                    {
                        n.ForeColor = Color.Orange;
                        n.Text = $"{Path.GetFileName(filePath)} — En cours...";
                    }));
                }

                progressBar.Invoke((Action)(() =>
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                }));
            }
            // 2. Progress: 50.00%
            else if (line.Contains("Progress: "))
            {
                int index = line.IndexOf("Progress: ");
                string pctStr = line.Substring(index + "Progress: ".Length).Replace("%", "").Replace(',', '.').Trim();
                if (double.TryParse(pctStr, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out double pct))
                {
                    int percentFile = (int)pct;
                    _currentFileBytesSent = (long)(_currentFileTotalBytes * (percentFile / 100.0));
                    
                    SafeSetLabel(labelStatus, BuildUploadProgressLabel(Path.GetFileName(_currentFilePath), percentFile));
                    
                    var n = FindNodeByTag(_currentFilePath);
                    if (n != null)
                    {
                        string suffix = GetParallelUploadSuffix(percentFile);
                        treeViewSelected.Invoke((Action)(() =>
                        {
                            n.Text = $"{Path.GetFileName(_currentFilePath)} — {percentFile}%{suffix}";
                        }));
                    }

                    long totalSentNow = _accumulatedBytesSent + _currentFileBytesSent;
                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = (int)Math.Min(totalSentNow / 1024, progressBar.Maximum);
                    }));

                    UpdateSpeedAndETA(totalSentNow);
                }
            }
            // 3. UPLOADED as: or Found as:
            else if (line.Contains("UPLOADED as:") || line.Contains("Found as:"))
            {
                if (!string.IsNullOrEmpty(_currentFilePath))
                {
                    if (line.Contains("Found as:"))
                    {
                        MarkAsAlreadyOnServer(_currentFilePath);
                    }
                    else
                    {
                        MarkAsDone(_currentFilePath);
                    }
                    
                    _accumulatedBytesSent += _currentFileTotalBytes;
                    _currentFileBytesSent = _currentFileTotalBytes;

                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = (int)Math.Min(_accumulatedBytesSent / 1024, progressBar.Maximum);
                    }));

                    UpdateSpeedAndETA(_accumulatedBytesSent);
                    RefreshStatLabels();
                }
            }
            // 4. Error response when processing or failed with error:
            else if (line.Contains("Error response when processing") || line.Contains("failed with error:"))
            {
                if (!string.IsNullOrEmpty(_currentFilePath))
                {
                    MarkAsError(_currentFilePath);
                    
                    _accumulatedBytesSent += _currentFileTotalBytes;

                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Style = ProgressBarStyle.Continuous;
                        progressBar.Value = (int)Math.Min(_accumulatedBytesSent / 1024, progressBar.Maximum);
                    }));

                    RefreshStatLabels();
                }
            }
            // 5. Dataset locked
            else if (line.Contains("Dataset locked"))
            {
                string statusMsg = IsFrench
                    ? "Serveur temporairement occupé : finalisation en cours, aucune action requise..."
                    : "Server temporarily busy: finalizing, no action required...";
                SafeSetLabel(labelStatus, statusMsg, Color.FromArgb(220, 100, 0)); // Orange foncé lisible

                progressBar.Invoke((Action)(() =>
                {
                    progressBar.Style = ProgressBarStyle.Marquee;
                    progressBar.MarqueeAnimationSpeed = 30;
                }));
            }
        }

        private void ParseStderrLine(string line)
        {
            if (string.IsNullOrWhiteSpace(line)) return;

            try { File.AppendAllText("dvuploader_stderr.log", $"{DateTime.Now:u} | {line}\n"); } catch { }
            AppendLog("[ERROR] " + line);
        }

        private void UpdateSpeedAndETA(long totalSentNow)
        {
            TimeSpan elapsed = DateTime.Now - _uploadStartTime;
            double secs = Math.Max(elapsed.TotalSeconds, 0.1);
            double moPerSec = (totalSentNow / 1024.0 / 1024.0) / secs;
            
            labelSpeed.Invoke((Action)(() =>
                labelSpeed.Text = $"{moPerSec:F1} {GetSpeedUnit()}"));
            labelStatSpeedValue.Invoke((Action)(() =>
                labelStatSpeedValue.Text = $"{moPerSec:F1}"));

            long remainingBytes = _totalBytesToSend - totalSentNow;
            TimeSpan eta = TimeSpan.FromSeconds(
                secs > 0 && totalSentNow > 0 ? remainingBytes / (totalSentNow / secs) : 0);
            string etaStr = $"{eta.Hours:00}:{eta.Minutes:00}:{eta.Seconds:00}";
            labelStatETAValue.Invoke((Action)(() =>
                labelStatETAValue.Text = etaStr));
        }

        private void MarkAsDone(string path)
        {
            SafeSetLabel(labelStatus, BuildDoneLabel(Path.GetFileName(path)));
            var n = FindNodeByTag(path);
            if (n != null)
            {
                treeViewSelected.Invoke((Action)(() =>
                {
                    n.Text = BuildDoneLabel(Path.GetFileName(path));
                    n.ForeColor = Color.Green;
                }));
            }
            _doneCount++;
        }

        private void MarkAsAlreadyOnServer(string path)
        {
            string filename = Path.GetFileName(path);
            string msg = IsFrench
                ? $"{filename} — Déjà sur le serveur"
                : $"{filename} — Already on server";
            SafeSetLabel(labelStatus, msg);
            var n = FindNodeByTag(path);
            if (n != null)
            {
                treeViewSelected.Invoke((Action)(() =>
                {
                    n.Text = msg;
                    n.ForeColor = Color.Green;
                }));
            }
            _doneCount++;
        }

        private void MarkAsError(string path)
        {
            SafeSetLabel(labelStatus, BuildRetryErrorLabel(Path.GetFileName(path)));
            var n = FindNodeByTag(path);
            if (n != null)
            {
                treeViewSelected.Invoke((Action)(() =>
                {
                    n.Text = BuildRetryErrorLabel(Path.GetFileName(path));
                    n.ForeColor = Color.Red;
                }));
            }
            progressBar.Invoke((Action)(() => progressBar.Style = ProgressBarStyle.Continuous));
            _errorCount++;
        }

        private string BuildDoneLabel(string fileName)
        {
            return IsFrench
                ? $"{fileName} — Terminé"
                : $"{fileName} — Done";
        }

        private string BuildRetryErrorLabel(string fileName)
        {
            return IsFrench
                ? $"{fileName} — Erreur, réessayez plus tard"
                : $"{fileName} — Error! Retry later";
        }

        private string GetParallelUploadSuffix(int percentFile)
        {
            if (_currentFileTotalBytes > 100 * 1024 * 1024 && percentFile > 0 && percentFile < 100)
            {
                return IsFrench 
                    ? " (Envoi parallèle en cours...)" 
                    : " (Parallel upload in progress...)";
            }
            return "";
        }

        private string BuildUploadProgressLabel(string fileName, int percentFile)
        {
            string suffix = GetParallelUploadSuffix(percentFile);
            if (percentFile == 0)
            {
                return IsFrench
                    ? $"Envoi {_currentFileIndex + 1}/{_filesToUpload.Count} : {fileName} — Téléversement en cours..."
                    : $"Upload {_currentFileIndex + 1}/{_filesToUpload.Count} : {fileName} — Uploading...";
            }
            return IsFrench
                ? $"Envoi {_currentFileIndex + 1}/{_filesToUpload.Count} : {fileName} — {percentFile}%{suffix}"
                : $"Upload {_currentFileIndex + 1}/{_filesToUpload.Count} : {fileName} — {percentFile}%{suffix}";
        }

        private TreeNode FindNodeByTag(string tag)
        {
            foreach (TreeNode root in treeViewSelected.Nodes)
            {
                var found = FindNodeByTagRecursive(root, tag);
                if (found != null) return found;
            }
            return null;
        }

        private TreeNode FindNodeByTagRecursive(TreeNode node, string tag)
        {
            if (node.Tag is string t && t == tag)
                return node;
            foreach (TreeNode c in node.Nodes)
            {
                var f = FindNodeByTagRecursive(c, tag);
                if (f != null) return f;
            }
            return null;
        }


        // UI : activer/désactiver + mise à jour statut

        private void ToggleControls(bool enabled)                // Active/désactive les contrôles
        {
            textBoxApiKey.Enabled = enabled;
            textBoxDoi.Enabled = enabled;
            comboBoxServer.Enabled = enabled;
            comboBoxLanguage.Enabled = enabled;
            buttonSelectFiles.Enabled = enabled;
            buttonSelectFolder.Enabled = enabled;
            buttonReset.Enabled = enabled;
            buttonUpload.Enabled = enabled;
            buttonApiKeyInfo.Enabled = enabled;
            buttonDoiInfo.Enabled = enabled;
        }

        private void UpdateStatus()
        {
            SafeSetLabel(labelStatus, GetSelectionStatusText());

            // ► nouveau : comptage fiable via HashSet
            labelStatFoldersValue.Text = _foldersSelected.Count.ToString();
            labelStatFilesValue.Text = _filesToUpload.Count.ToString();
            // On remet les compteurs à zéro dès que la sélection change
            _doneCount = 0;
            _errorCount = 0;
            RefreshStatLabels();

            CompareLocalWithRemote(_remotePaths);
        }


        private void btnAbout_Click(object sender, EventArgs e) // Ouvre la fenêtre About
        {
            new About(_currentLanguage).Show();                                  // Affiche ma fenêtre About
        }

        /// Démarre un drag interne quand on commence à glisser un TreeNode.
        private void treeViewSelected_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Item is TreeNode node)
                DoDragDrop(node, DragDropEffects.Move);
        }


        // Détermine l’effet de drop : copie pour les fichiers externes, move pour les noeuds internes.
        private void treeViewSelected_DragEnter(object sender, DragEventArgs e)
        {
            // Fichiers ou dossiers glissés depuis l’explorateur Windows ?
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effect = DragDropEffects.Copy;
                return;
            }
            // Nœud interne (repositionnement dans le TreeView) ?
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                e.Effect = DragDropEffects.Move;
                return;
            }
            e.Effect = DragDropEffects.None;
        }


        // GESTION DU GLISSER-DÉPOSER DANS LE TREEVIEW
        // Ce morceau de code permet à l'utilisateur de déposer des fichiers ou dossiers
        // depuis l'Explorateur Windows (drop externe) ou de déplacer des nœuds à l'intérieur
        // de l'arborescence (drop interne). L'idée est de toujours mettre à jour
        // _filesToUpload et l'affichage du TreeView sans toucher au disque.
        private void treeViewSelected_DragDrop(object sender, DragEventArgs e)
        {
            // ───────────── CAS 1 : DROP EXTERNE (depuis l'Explorateur Windows) ─────────────
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // 1. Récupère tous les chemins (fichiers et dossiers) déposés
                var paths = (string[])e.Data.GetData(DataFormats.FileDrop);

                // 2. Identifie le nœud TreeNode sous la souris (pour déposer dedans)
                Point pt = treeViewSelected.PointToClient(new Point(e.X, e.Y));
                TreeNode targetNode = treeViewSelected.GetNodeAt(pt);

                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                    {
                        // Si c'est un dossier, on l'ajoute récursivement
                        TreeNode parent = targetNode;
                        if (parent == null)
                        {
                            // Pas de dossier cible => création d'un nœud racine
                            parent = new TreeNode(Path.GetFileName(path))
                            {
                                Tag = path,            // Stocke le chemin complet
                                ImageKey = "folder",
                                SelectedImageKey = "folder"
                            };
                            treeViewSelected.Nodes.Add(parent);
                        }
                        // Parcours recursef du dossier pour ajouter tous ses sous-éléments
                        AddDirectoryNodes(parent, path, path);
                        parent.Expand();  // Déplie pour que l'utilisateur voit immédiatement le contenu
                    }
                    else if (File.Exists(path))
                    {
                        //  Si c'est un fichier, on vérifie les doublons
                        if (_filesToUpload.Contains(path))
                            continue;

                        // 1) Ajoute en mémoire pour l'upload
                        _filesToUpload.Add(path);
                        _fileRelativePaths[path] = "";

                        // 2) Détermine si on accroche sous un dossier cible ou à la racine
                        if (targetNode != null
                            && targetNode.Tag is string tdir
                            && Directory.Exists(tdir))
                        {
                            // Sous un dossier existant
                            var fileNode = new TreeNode(Path.GetFileName(path))
                            {
                                Tag = path,
                                ImageKey = "file",
                                SelectedImageKey = "file"
                            };
                            targetNode.Nodes.Add(fileNode);
                            targetNode.Expand();
                        }
                        else
                        {
                            // À la racine du TreeView
                            AddFileNode(path);
                        }
                    }
                }

                // 3) Mise à jour des compteurs (fichiers / dossiers)
                UpdateStatus();
                return;
            }

            // ───────────── CAS 2 : déplacement d'un TreeNode ─────────────
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                // 1) Récupère le nœud déplacé
                TreeNode draggedNode = (TreeNode)e.Data.GetData(typeof(TreeNode));
                // 2) Trouve la cible sous la souris
                Point pt = treeViewSelected.PointToClient(new Point(e.X, e.Y));
                TreeNode targetNode = treeViewSelected.GetNodeAt(pt);

                if (targetNode == null)
                {
                    // Cible absente → on remet le nœud à la racine
                    draggedNode.Remove();
                    treeViewSelected.Nodes.Add(draggedNode);
                    UpdateRelativePathsRecursive(draggedNode);
                    UpdateStatus();
                    return;
                }

                // 3) Si la cible correspond à un dossier réel sur disque
                if (targetNode.Tag is string targetPath && Directory.Exists(targetPath))
                {
                    draggedNode.Remove();
                    targetNode.Nodes.Add(draggedNode);
                    targetNode.Expand();
                }
                else
                {
                    // 4) Sinon, on remonte jusqu'au parent-dossier valide
                    var parent = targetNode.Parent;
                    while (parent != null && !(parent.Tag is string p && Directory.Exists(p)))
                        parent = parent.Parent;

                    if (parent != null)
                    {
                        draggedNode.Remove();
                        parent.Nodes.Add(draggedNode);
                        parent.Expand();
                    }
                    else
                    {
                        // Pas de parent valide → retour à la racine
                        draggedNode.Remove();
                        treeViewSelected.Nodes.Add(draggedNode);
                    }
                }
                UpdateRelativePathsRecursive(draggedNode);
                UpdateStatus();
            }
        }

        private async void tabControlMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControlMain.SelectedIndex == 1) // Remote Files tab selected
            {
                if (treeViewRemote.Nodes.Count == 0 || 
                    (treeViewRemote.Nodes.Count == 1 && (treeViewRemote.Nodes[0].ForeColor == Color.Red || treeViewRemote.Nodes[0].ForeColor == Color.Gray)))
                {
                    await LoadRemoteFilesAsync();
                }
            }
            else if (tabControlMain.SelectedIndex == 2) // Logs tab selected
            {
                textBoxLogs.SelectionStart = textBoxLogs.TextLength;
                textBoxLogs.ScrollToCaret();
                SendMessage(textBoxLogs.Handle, WM_VSCROLL, (IntPtr)SB_BOTTOM, IntPtr.Zero);
            }
        }

        private async Task LoadRemoteFilesAsync()
        {
            string api = textBoxApiKey.Text.Trim();
            string doi = NormalizeDoiInput(textBoxDoi.Text);
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            if (string.IsNullOrWhiteSpace(api) || !IsNormalizedDoi(doi) || string.IsNullOrWhiteSpace(srv))
            {
                treeViewRemote.Nodes.Clear();
                string msg = IsFrench 
                    ? "Veuillez renseigner la clé API, le DOI et sélectionner un serveur valide pour charger les fichiers du serveur."
                    : "Please fill in the API Key, DOI, and select a valid server to load server files.";
                treeViewRemote.Nodes.Add(new TreeNode(msg) { ForeColor = Color.Red });
                return;
            }

            // Save current expanded state
            var expandedPaths = GetExpandedPaths(treeViewRemote);
            string selectedPath = GetSelectedNodePath(treeViewRemote);

            treeViewRemote.Nodes.Clear();
            string loadingMsg = IsFrench ? "Chargement des fichiers depuis le serveur..." : "Loading files from server...";
            treeViewRemote.Nodes.Add(new TreeNode(loadingMsg) { ForeColor = Color.Gray });

            try
            {
                string url = $"{srv}/api/datasets/:persistentId/versions/:latest/files?persistentId={doi}";
                
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("X-Dataverse-key", api);
                    
                    var response = await _httpClient.SendAsync(request);
                    if (!response.IsSuccessStatusCode)
                    {
                        string errMsg = IsFrench 
                            ? $"Erreur de chargement ({response.StatusCode}) : {(int)response.StatusCode}"
                            : $"Load error ({response.StatusCode}) : {(int)response.StatusCode}";
                        treeViewRemote.Nodes.Clear();
                        treeViewRemote.Nodes.Add(new TreeNode(errMsg) { ForeColor = Color.Red });
                        return;
                    }

                    string jsonStr = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonConvert.DeserializeObject<DataverseFilesResponse>(jsonStr);

                    if (apiResponse == null || !"OK".Equals(apiResponse.Status, StringComparison.OrdinalIgnoreCase) || apiResponse.Data == null)
                    {
                        string errMsg = IsFrench ? "Format de réponse du serveur invalide." : "Invalid server response format.";
                        treeViewRemote.Nodes.Clear();
                        treeViewRemote.Nodes.Add(new TreeNode(errMsg) { ForeColor = Color.Red });
                        return;
                    }

                    _remoteFiles.Clear();
                    _remotePaths.Clear();
                    CompareLocalWithRemote(_remotePaths);

                    if (apiResponse.Data.Count == 0)
                    {
                        treeViewRemote.Nodes.Clear();
                        string noFilesMsg = IsFrench ? "Aucun fichier sur le serveur." : "No files on the server.";
                        treeViewRemote.Nodes.Add(new TreeNode(noFilesMsg) { ForeColor = Color.Gray });
                        return;
                    }

                    _remoteFiles = apiResponse.Data;
                    BuildRemoteTreeView(apiResponse.Data, expandedPaths, selectedPath);

                    // Build path set to identify duplicates locally
                    _remotePaths.Clear();
                    foreach (var item in apiResponse.Data)
                    {
                        string filename = item.Label ?? "";
                        string dirLabel = item.DirectoryLabel ?? "";
                        string destPath = string.IsNullOrEmpty(dirLabel) 
                            ? filename 
                            : $"{dirLabel.Replace('\\', '/').Trim('/')}/{filename}";
                        _remotePaths.Add(destPath);
                    }

                    CompareLocalWithRemote(_remotePaths);
                }
            }
            catch (Exception ex)
            {
                string errMsg = IsFrench 
                    ? $"Une erreur est survenue lors du chargement : {ex.Message}" 
                    : $"An error occurred during load: {ex.Message}";
                treeViewRemote.Nodes.Clear();
                treeViewRemote.Nodes.Add(new TreeNode(errMsg) { ForeColor = Color.Red });
            }
        }

        private async Task AutoCheckRemoteFilesAsync()
        {
            string api = textBoxApiKey.Text.Trim();
            string doi = NormalizeDoiInput(textBoxDoi.Text);
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            if (string.IsNullOrWhiteSpace(api) || !IsNormalizedDoi(doi) || string.IsNullOrWhiteSpace(srv))
            {
                return;
            }

            try
            {
                string url = $"{srv}/api/datasets/:persistentId/versions/:latest/files?persistentId={doi}";
                
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.Add("X-Dataverse-key", api);
                    
                    var response = await _httpClient.SendAsync(request);
                    if (response.IsSuccessStatusCode)
                    {
                        string jsonStr = await response.Content.ReadAsStringAsync();
                        var apiResponse = JsonConvert.DeserializeObject<DataverseFilesResponse>(jsonStr);

                        if (apiResponse != null && "OK".Equals(apiResponse.Status, StringComparison.OrdinalIgnoreCase) && apiResponse.Data != null)
                        {
                            _remoteFiles = apiResponse.Data;
                            _remotePaths.Clear();
                            foreach (var item in apiResponse.Data)
                            {
                                string filename = item.Label ?? "";
                                string dirLabel = item.DirectoryLabel ?? "";
                                string destPath = string.IsNullOrEmpty(dirLabel) 
                                    ? filename 
                                    : $"{dirLabel.Replace('\\', '/').Trim('/')}/{filename}";
                                _remotePaths.Add(destPath);
                            }

                            var expandedPaths = GetExpandedPaths(treeViewRemote);
                            string selectedPath = GetSelectedNodePath(treeViewRemote);
                            BuildRemoteTreeView(apiResponse.Data, expandedPaths, selectedPath);
                            CompareLocalWithRemote(_remotePaths);
                        }
                    }
                }
            }
            catch
            {
                // Silent catch for background auto-check
            }
        }

        private string FormatFileSize(long bytes)
        {
            double size = bytes;
            string unit = IsFrench ? "octets" : "bytes";
            if (bytes >= 1024 * 1024 * 1024)
            {
                size = bytes / (1024.0 * 1024.0 * 1024.0);
                unit = IsFrench ? "Go" : "GB";
            }
            else if (bytes >= 1024 * 1024)
            {
                size = bytes / (1024.0 * 1024.0);
                unit = IsFrench ? "Mo" : "MB";
            }
            else if (bytes >= 1024)
            {
                size = bytes / 1024.0;
                unit = IsFrench ? "Ko" : "KB";
            }
            return $"{size:F1} {unit}";
        }

        private string FormatFileDate(string creationDateStr)
        {
            if (DateTime.TryParse(creationDateStr, out DateTime dt))
            {
                return IsFrench 
                    ? dt.ToString("d MMMM yyyy", new System.Globalization.CultureInfo("fr-FR"))
                    : dt.ToString("MMMM d, yyyy", new System.Globalization.CultureInfo("en-US"));
            }
            return creationDateStr;
        }

        private string GetCleanNodeText(TreeNode node)
        {
            if (node == null) return "";
            if (node.ImageKey == "file" && node.Tag is long fileId && fileId > 0)
            {
                var item = _remoteFiles.Find(f => f.DataFile != null && f.DataFile.Id == fileId);
                if (item != null) return item.Label;
            }
            return node.Text;
        }

        private void BuildRemoteTreeView(List<DataverseFileItem> files, HashSet<string> expandedPaths = null, string selectedPath = null)
        {
            treeViewRemote.BeginUpdate();
            treeViewRemote.Nodes.Clear();

            TreeNode rootNode = new TreeNode(Localize("Root (/)", "Racine (/)"))
            {
                ImageKey = "folder",
                SelectedImageKey = "folder"
            };
            treeViewRemote.Nodes.Add(rootNode);

            var folders = new Dictionary<string, TreeNode>();

            foreach (var item in files)
            {
                string filename = item.Label ?? "unknown";
                string dirLabel = item.DirectoryLabel ?? "";

                TreeNode parentNode = null;
                if (!string.IsNullOrEmpty(dirLabel))
                {
                    string normDir = dirLabel.Replace('\\', '/').Trim('/');
                    string[] parts = normDir.Split('/');

                    string currentKey = "";
                    for (int i = 0; i < parts.Length; i++)
                    {
                        string part = parts[i];
                        currentKey = string.IsNullOrEmpty(currentKey) ? part : $"{currentKey}/{part}";

                        if (!folders.TryGetValue(currentKey, out TreeNode folderNode))
                        {
                            folderNode = new TreeNode(part)
                            {
                                ImageKey = "folder",
                                SelectedImageKey = "folder"
                            };

                            if (parentNode == null)
                            {
                                rootNode.Nodes.Add(folderNode);
                            }
                            else
                            {
                                parentNode.Nodes.Add(folderNode);
                            }
                            folders[currentKey] = folderNode;
                        }
                        parentNode = folderNode;
                    }
                }

                string displayText = filename;
                if (item.DataFile != null)
                {
                    string sizeStr = FormatFileSize(item.DataFile.Filesize);
                    string dateStr = FormatFileDate(item.DataFile.CreationDate);
                    string datePrefix = IsFrench ? "Déposé le " : "Uploaded on ";
                    displayText = $"{filename} ({sizeStr} — {datePrefix}{dateStr})";
                }

                var fileNode = new TreeNode(displayText)
                {
                    ImageKey = "file",
                    SelectedImageKey = "file",
                    ForeColor = Color.DarkBlue,
                    Tag = item.DataFile?.Id ?? 0L
                };

                if (parentNode == null)
                {
                    rootNode.Nodes.Add(fileNode);
                }
                else
                {
                    parentNode.Nodes.Add(fileNode);
                }
            }

            if (expandedPaths == null || expandedPaths.Count == 0)
            {
                rootNode.Expand();
            }
            else
            {
                RestoreExpandedPaths(treeViewRemote, expandedPaths);
            }

            if (!string.IsNullOrEmpty(selectedPath))
            {
                RestoreSelectedNodePath(treeViewRemote, selectedPath);
            }
            treeViewRemote.EndUpdate();
        }

        private void CompareLocalWithRemote(HashSet<string> remotePaths)
        {
            treeViewSelected.Invoke((Action)(() =>
            {
                treeViewSelected.BeginUpdate();
                foreach (TreeNode root in treeViewSelected.Nodes)
                {
                    MarkRemoteDuplicatesRecursive(root, remotePaths);
                }
                treeViewSelected.EndUpdate();
            }));
        }

        private void MarkRemoteDuplicatesRecursive(TreeNode node, HashSet<string> remotePaths)
        {
            if (node.Tag is string file && File.Exists(file))
            {
                _fileRelativePaths.TryGetValue(file, out string relPath);
                string filename = Path.GetFileName(file);
                
                string targetPrefix = GetActiveTargetFolder();
                string finalRelPath = relPath ?? "";
                if (!string.IsNullOrEmpty(targetPrefix))
                {
                    finalRelPath = string.IsNullOrEmpty(finalRelPath) 
                        ? targetPrefix 
                        : $"{targetPrefix}/{finalRelPath.Trim('/')}";
                }

                string destPath = string.IsNullOrEmpty(finalRelPath) 
                    ? filename 
                    : $"{finalRelPath.Replace('\\', '/').Trim('/')}/{filename}";
                if (remotePaths != null && remotePaths.Contains(destPath))
                {
                    node.ForeColor = Color.Green;
                    node.Text = IsFrench 
                        ? $"{filename} — Déjà sur le serveur" 
                        : $"{filename} — Already on server";
                }
                else
                {
                    var existingPaths = FindAllRemotePathsWithFilename(filename);
                    if (existingPaths.Count > 0)
                    {
                        string pathsStr;
                        if (existingPaths.Count <= 2)
                        {
                            pathsStr = string.Join(", ", existingPaths);
                        }
                        else
                        {
                            string othersText = IsFrench 
                                ? $"+ {existingPaths.Count - 2} autre(s)" 
                                : $"+ {existingPaths.Count - 2} other(s)";
                            pathsStr = $"{existingPaths[0]}, {existingPaths[1]} ({othersText})";
                        }

                        node.ForeColor = Color.Chocolate;
                        node.Text = IsFrench
                            ? $"{filename} — Existe déjà dans : {pathsStr}"
                            : $"{filename} — Already exists in: {pathsStr}";
                    }
                    else
                    {
                        if ((node.ForeColor == Color.Green && (node.Text.Contains("sur le serveur") || node.Text.Contains("on server"))) ||
                            (node.ForeColor == Color.Chocolate && (node.Text.Contains("Existe déjà dans") || node.Text.Contains("Already exists in"))))
                        {
                            node.ForeColor = SystemColors.WindowText;
                            node.Text = filename;
                        }
                    }
                }
            }

            foreach (TreeNode child in node.Nodes)
            {
                MarkRemoteDuplicatesRecursive(child, remotePaths);
            }
        }

        private string GetTreeRelativePath(TreeNode node)
        {
            var parts = new List<string>();
            TreeNode parent = node.Parent;
            while (parent != null)
            {
                parts.Insert(0, parent.Text);
                parent = parent.Parent;
            }
            return string.Join("/", parts);
        }

        private void UpdateRelativePathsRecursive(TreeNode node)
        {
            if (node.Tag is string path && File.Exists(path))
            {
                string relPath = GetTreeRelativePath(node);
                _fileRelativePaths[path] = relPath;
            }

            foreach (TreeNode child in node.Nodes)
            {
                UpdateRelativePathsRecursive(child);
            }
        }

        private void RebuildRelativePathsFromTree()
        {
            _fileRelativePaths.Clear();
            foreach (TreeNode root in treeViewSelected.Nodes)
            {
                UpdateRelativePathsRecursive(root);
            }
        }

        private string GetRemoteNodeDirectoryLabel(TreeNode node)
        {
            if (node == null) return "";
            TreeNode parent = node.Parent;
            var parts = new List<string>();
            while (parent != null)
            {
                parts.Insert(0, parent.Text);
                parent = parent.Parent;
            }
            if (parts.Count > 0)
            {
                parts.RemoveAt(0); // Remove "Racine (/)" / "Root (/)"
            }
            return string.Join("/", parts);
        }

        private async Task RenameRemoteFileAsync()
        {
            var selectedNodes = treeViewRemote.SelectedNodes;
            if (selectedNodes == null || selectedNodes.Count != 1) return;
            TreeNode node = selectedNodes[0];
            if (node == null || !(node.Tag is long fileId)) return;

            var item = _remoteFiles.Find(f => f.DataFile != null && f.DataFile.Id == fileId);
            if (item == null) return;
            string currentFilename = item.Label;
            string currentDir = GetRemoteNodeDirectoryLabel(node);

            string msgPrompt = IsFrench
                ? "Entrez le nouveau nom du fichier :"
                : "Enter the new filename:";
            
            string captionPrompt = IsFrench ? "Renommer sur le serveur" : "Rename on server";

            string userInput = Prompt.ShowDialog(msgPrompt, captionPrompt, currentFilename);
            if (string.IsNullOrWhiteSpace(userInput) || userInput == currentFilename) return;

            userInput = userInput.Replace('\\', '/');
            if (userInput.Contains("/"))
            {
                string errMsg = IsFrench 
                    ? "Le nom de fichier ne doit pas contenir de dossiers. Utilisez l'option 'Déplacer' pour changer de dossier."
                    : "The filename must not contain directory paths. Use the 'Move' option to change folders.";
                MessageBox.Show(errMsg, Localize("Error", "Erreur"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            string api = textBoxApiKey.Text.Trim();
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');
            bool success = await UpdateRemoteFileMetadataAsync(fileId, userInput, currentDir, api, srv);
            if (success)
            {
                await LoadRemoteFilesAsync();
            }
        }

        private async Task MoveRemoteFileAsync()
        {
            var selectedNodes = treeViewRemote.SelectedNodes;
            if (selectedNodes == null || selectedNodes.Count == 0) return;

            using (var dialog = new FolderPickerDialog(treeViewRemote, IsFrench))
            {
                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    string targetDir = dialog.SelectedFolderPath;

                    // Collect all files to move with their new directoryLabels
                    var moves = new List<Tuple<DataverseFileItem, string>>();

                    foreach (var node in selectedNodes)
                    {
                        if (node.ImageKey == "file")
                        {
                            if (node.Tag is long fileId && fileId > 0)
                            {
                                var item = _remoteFiles.Find(f => f.DataFile != null && f.DataFile.Id == fileId);
                                if (item != null)
                                {
                                    moves.Add(Tuple.Create(item, targetDir));
                                }
                            }
                        }
                        else // folder
                        {
                            string folderPath = GetRemoteFolderNodePath(node);
                            string folderName = node.Text;

                            // New path of the folder: targetDir/folderName
                            string newFolderPath = string.IsNullOrEmpty(targetDir) ? folderName : $"{targetDir}/{folderName}";

                            foreach (var item in _remoteFiles)
                            {
                                string dir = item.DirectoryLabel ?? "";
                                dir = dir.Replace('\\', '/').Trim('/');

                                bool isMatch = false;
                                string newDir = "";

                                if (string.Equals(dir, folderPath, StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    newDir = newFolderPath;
                                }
                                else if (dir.StartsWith(folderPath + "/", StringComparison.OrdinalIgnoreCase))
                                {
                                    isMatch = true;
                                    string suffix = dir.Substring(folderPath.Length + 1);
                                    newDir = $"{newFolderPath}/{suffix}";
                                }

                                if (isMatch)
                                {
                                    moves.Add(Tuple.Create(item, newDir));
                                }
                            }
                        }
                    }

                    if (moves.Count == 0) return;

                    string api = textBoxApiKey.Text.Trim();
                    string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

                    // Prepare UI
                    ToggleControls(false);
                    _cts = new CancellationTokenSource();
                    var token = _cts.Token;

                    progressBar.Value = 0;
                    progressBar.Maximum = moves.Count;
                    progressBar.Style = ProgressBarStyle.Continuous;
                    SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);

                    int successCount = 0;
                    int errorCount = 0;
                    int completedCount = 0;

                    try
                    {
                        for (int i = 0; i < moves.Count; i++)
                        {
                            token.ThrowIfCancellationRequested();
                            var move = moves[i];
                            var item = move.Item1;
                            string newDir = move.Item2;
                            long fileId = item.DataFile?.Id ?? 0;
                            string filename = item.Label;

                            if (i > 0)
                            {
                                // Petit délai pour éviter les bannissements IP (rate limiting) par le pare-feu du serveur
                                await Task.Delay(350, token);
                            }

                            completedCount++;
                            int currentCompleted = completedCount;

                            string statusMsg = IsFrench
                                ? $"Déplacement {currentCompleted}/{moves.Count} : {filename}..."
                                : $"Moving {currentCompleted}/{moves.Count} : {filename}...";
                            SafeSetLabel(labelStatus, statusMsg);

                            bool success = await UpdateRemoteFileMetadataAsync(fileId, filename, newDir, api, srv, silent: true, token: token);

                            if (success) successCount++;
                            else errorCount++;

                            progressBar.Invoke((Action)(() =>
                            {
                                progressBar.Value = currentCompleted;
                            }));
                        }

                        string completeMsg = IsFrench
                            ? $"Déplacement terminé ! {successCount} réussi(s), {errorCount} échoué(s)."
                            : $"Move complete! {successCount} succeeded, {errorCount} failed.";
                        MessageBox.Show(completeMsg, Localize("Completed", "Terminé"), MessageBoxButtons.OK,
                            errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
                    }
                    catch (OperationCanceledException)
                    {
                        MessageBox.Show(
                            IsFrench ? "Opération annulée." : "Operation cancelled.",
                            Localize("Cancelled", "Annulé"),
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information);
                    }
                    finally
                    {
                        ToggleControls(true);
                        _cts?.Dispose();
                        _cts = null;
                        SafeSetLabel(labelStatus, IsFrench ? "Prêt" : "Ready");
                        progressBar.Value = 0;
                        await LoadRemoteFilesAsync();
                    }
                }
            }
        }

        private async Task FlattenRemoteFolderAsync()
        {
            var selectedNodes = treeViewRemote.SelectedNodes;
            if (selectedNodes == null || selectedNodes.Count == 0) return;

            // Collect only folders that can be flattened (excluding root and files)
            var selectedFolders = new List<TreeNode>();
            foreach (var node in selectedNodes)
            {
                if (node.ImageKey == "folder" && node.Parent != null)
                {
                    selectedFolders.Add(node);
                }
            }

            if (selectedFolders.Count == 0) return;

            // To handle nested selected folders properly (e.g. flattening both "A" and "A/B"),
            // we resolve destinations iteratively starting with the deepest folders.
            // We can sort selectedFolders by path depth descending:
            selectedFolders.Sort((a, b) =>
            {
                string pathA = GetRemoteFolderNodePath(a);
                string pathB = GetRemoteFolderNodePath(b);
                return pathB.Length.CompareTo(pathA.Length);
            });

            // Map each remote file ID to its final resolved DirectoryLabel
            var fileDestinations = new Dictionary<long, string>();
            foreach (var item in _remoteFiles)
            {
                if (item.DataFile != null)
                {
                    fileDestinations[item.DataFile.Id] = (item.DirectoryLabel ?? "").Replace('\\', '/').Trim('/');
                }
            }

            // Apply each selected folder's flattening to the resolved paths
            foreach (var folderNode in selectedFolders)
            {
                string folderPath = GetRemoteFolderNodePath(folderNode);
                TreeNode parentNode = folderNode.Parent;
                string parentPath = parentNode.Parent != null ? GetRemoteFolderNodePath(parentNode) : "";

                // Find files under folderPath and update their destination
                var keys = new List<long>(fileDestinations.Keys);
                foreach (var fileId in keys)
                {
                    string currentDest = fileDestinations[fileId];

                    if (string.Equals(currentDest, folderPath, StringComparison.OrdinalIgnoreCase))
                    {
                        fileDestinations[fileId] = parentPath;
                    }
                    else if (currentDest.StartsWith(folderPath + "/", StringComparison.OrdinalIgnoreCase))
                    {
                        string suffix = currentDest.Substring(folderPath.Length + 1);
                        fileDestinations[fileId] = string.IsNullOrEmpty(parentPath) ? suffix : $"{parentPath}/{suffix}";
                    }
                }
            }

            // Identify moves to perform: files whose destination directory changed
            var moves = new List<Tuple<DataverseFileItem, string>>();
            foreach (var item in _remoteFiles)
            {
                if (item.DataFile != null)
                {
                    long fileId = item.DataFile.Id;
                    string originalDir = (item.DirectoryLabel ?? "").Replace('\\', '/').Trim('/');
                    string finalDir = fileDestinations[fileId];
                    if (!string.Equals(originalDir, finalDir, StringComparison.OrdinalIgnoreCase))
                    {
                        moves.Add(Tuple.Create(item, finalDir));
                    }
                }
            }

            if (moves.Count == 0)
            {
                string emptyMsg = IsFrench
                    ? "Aucun fichier à déplacer pour aplatir la sélection."
                    : "No files to move to flatten the selection.";
                MessageBox.Show(emptyMsg, Localize("Information", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // Prepare confirmation message
            string confirmMsg;
            if (selectedFolders.Count == 1)
            {
                confirmMsg = IsFrench
                    ? $"Voulez-vous vraiment aplatir le dossier '{selectedFolders[0].Text}' ?\n\nTous les fichiers et sous-dossiers qu'il contient ({moves.Count} fichier(s)) seront remontés."
                    : $"Do you really want to flatten the folder '{selectedFolders[0].Text}'?\n\nAll files and subfolders inside it ({moves.Count} file(s)) will be moved up.";
            }
            else
            {
                confirmMsg = IsFrench
                    ? $"Voulez-vous vraiment aplatir les {selectedFolders.Count} dossiers sélectionnés ?\n\nTous les fichiers et sous-dossiers qu'ils contiennent ({moves.Count} fichier(s)) seront remontés."
                    : $"Do you really want to flatten the {selectedFolders.Count} selected folders?\n\nAll files and subfolders inside them ({moves.Count} file(s)) will be moved up.";
            }

            string caption = IsFrench ? "Aplatir dossier(s)" : "Flatten folder(s)";
            var dr = MessageBox.Show(confirmMsg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) return;

            string api = textBoxApiKey.Text.Trim();
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            // Prepare UI
            ToggleControls(false);
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            progressBar.Value = 0;
            progressBar.Maximum = moves.Count;
            progressBar.Style = ProgressBarStyle.Continuous;
            SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);

            int successCount = 0;
            int errorCount = 0;
            int completedCount = 0;

                        try
            {
                for (int i = 0; i < moves.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var move = moves[i];
                    var item = move.Item1;
                    long fileId = item.DataFile?.Id ?? 0;
                    string filename = item.Label;
                    string newDir = move.Item2;

                    if (i > 0)
                    {
                        // Petit délai pour éviter les bannissements IP (rate limiting) par le pare-feu du serveur
                        await Task.Delay(350, token);
                    }

                    completedCount++;
                    int currentCompleted = completedCount;

                    // Set label status
                    string statusMsg = IsFrench
                        ? $"Aplatissement {currentCompleted}/{moves.Count} : {filename}..."
                        : $"Flattening {currentCompleted}/{moves.Count} : {filename}...";
                    SafeSetLabel(labelStatus, statusMsg);

                    bool success = await UpdateRemoteFileMetadataAsync(fileId, filename, newDir, api, srv, silent: true, token: token);

                    if (success) successCount++;
                    else errorCount++;

                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Value = currentCompleted;
                    }));
                }

                string completeMsg = IsFrench
                    ? $"Aplatissement terminé ! {successCount} réussi(s), {errorCount} échoué(s)."
                    : $"Flattening complete! {successCount} succeeded, {errorCount} failed.";
                MessageBox.Show(completeMsg, Localize("Completed", "Terminé"), MessageBoxButtons.OK,
                    errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    IsFrench ? "Opération annulée." : "Operation cancelled.",
                    Localize("Cancelled", "Annulé"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                ToggleControls(true);
                _cts?.Dispose();
                _cts = null;
                SafeSetLabel(labelStatus, IsFrench ? "Prêt" : "Ready");
                progressBar.Value = 0;
                await LoadRemoteFilesAsync();
            }
        }

        private void CollectRemoteFileIds(TreeNode node, List<long> fileIds)
        {
            if (node == null) return;
            if (node.Tag is long fileId && fileId > 0)
            {
                fileIds.Add(fileId);
            }
            foreach (TreeNode child in node.Nodes)
            {
                CollectRemoteFileIds(child, fileIds);
            }
        }

        private async Task DeleteRemoteFileAsync()
        {
            var selectedNodes = treeViewRemote.SelectedNodes;
            if (selectedNodes == null || selectedNodes.Count == 0) return;

            // Collect all file IDs recursively from all selected nodes
            var fileIdsToDelete = new List<long>();

            foreach (var node in selectedNodes)
            {
                CollectRemoteFileIds(node, fileIdsToDelete);
            }

            if (fileIdsToDelete.Count == 0)
            {
                string emptyMsg = IsFrench
                    ? "Aucun fichier à supprimer dans la sélection."
                    : "No files to delete in the selection.";
                MessageBox.Show(emptyMsg, Localize("Information", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string confirmMsg;
            if (selectedNodes.Count == 1 && selectedNodes[0].ImageKey == "file")
            {
                string filename = selectedNodes[0].Text;
                if (selectedNodes[0].Tag is long fileId)
                {
                    var item = _remoteFiles.Find(f => f.DataFile != null && f.DataFile.Id == fileId);
                    if (item != null) filename = item.Label;
                }
                confirmMsg = IsFrench
                    ? $"Voulez-vous vraiment supprimer définitivement le fichier '{filename}' du serveur ?"
                    : $"Do you really want to permanently delete the file '{filename}' from the server?";
            }
            else
            {
                confirmMsg = IsFrench
                    ? $"Voulez-vous vraiment supprimer définitivement ces {selectedNodes.Count} élément(s) sélectionnés et tout leur contenu ({fileIdsToDelete.Count} fichier(s)) du serveur ?"
                    : $"Do you really want to permanently delete these {selectedNodes.Count} selected item(s) and all their contents ({fileIdsToDelete.Count} file(s)) from the server?";
            }

            string caption = IsFrench ? "Suppression du serveur" : "Delete from server";
            var dr = MessageBox.Show(confirmMsg, caption, MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (dr != DialogResult.Yes) return;

            string api = textBoxApiKey.Text.Trim();
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            // Prepare UI
            ToggleControls(false);
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            progressBar.Value = 0;
            progressBar.Maximum = fileIdsToDelete.Count;
            progressBar.Style = ProgressBarStyle.Continuous;
            SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);

            int successCount = 0;
            int errorCount = 0;
            int completedCount = 0;

            try
            {
                for (int i = 0; i < fileIdsToDelete.Count; i++)
                {
                    token.ThrowIfCancellationRequested();
                    long id = fileIdsToDelete[i];

                    if (i > 0)
                    {
                        // Petit délai pour éviter les bannissements IP (rate limiting) par le pare-feu du serveur
                        await Task.Delay(350, token);
                    }

                    bool success = await DeleteRemoteFileHttpAsync(id, api, srv, silent: true, token: token);
                    
                    if (success) successCount++;
                    else errorCount++;
                    completedCount++;

                    int currentCompleted = completedCount;
                    progressBar.Invoke((Action)(() =>
                    {
                        progressBar.Value = currentCompleted;
                    }));

                    string statusMsg = IsFrench
                        ? $"Suppression {currentCompleted}/{fileIdsToDelete.Count}..."
                        : $"Deleting {currentCompleted}/{fileIdsToDelete.Count}...";
                    SafeSetLabel(labelStatus, statusMsg);
                }

                if (errorCount > 0)
                {
                    string resultMsg = IsFrench
                        ? $"Suppression terminée avec des erreurs.\nFichiers supprimés : {successCount}\nÉchecs : {errorCount}"
                        : $"Deletion completed with errors.\nFiles deleted: {successCount}\nFailures: {errorCount}";
                    MessageBox.Show(resultMsg, Localize("Warning", "Avertissement"), MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    IsFrench ? "Opération annulée." : "Operation cancelled.",
                    Localize("Cancelled", "Annulé"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                ToggleControls(true);
                _cts?.Dispose();
                _cts = null;
                SafeSetLabel(labelStatus, IsFrench ? "Prêt" : "Ready");
                progressBar.Value = 0;
                await LoadRemoteFilesAsync();
            }
        }

        private async Task<bool> DeleteRemoteFileHttpAsync(long fileId, string api, string srv, bool silent = false, CancellationToken token = default)
        {
            string url = $"{srv}/api/files/{fileId}";

            try
            {
                using (var request = new HttpRequestMessage(HttpMethod.Delete, url))
                {
                    request.Headers.Add("X-Dataverse-key", api);

                    var response = await _httpClient.SendAsync(request, token);
                    if (response.IsSuccessStatusCode)
                    {
                        return true;
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                    {
                        try { File.AppendAllText("remote_errors.log", $"{DateTime.Now:u} | DELETE file {fileId} already deleted (returned NotFound, treated as success).\n"); } catch {}
                        return true;
                    }
                    else
                    {
                        string err = await response.Content.ReadAsStringAsync();
                        try { File.AppendAllText("remote_errors.log", $"{DateTime.Now:u} | DELETE file {fileId} failed: {response.StatusCode} - {err}\n"); } catch {}
                        if (!silent)
                        {
                            MessageBox.Show(IsFrench 
                                ? $"Erreur serveur lors de la suppression : {response.StatusCode}\n{err}" 
                                : $"Server error during deletion: {response.StatusCode}\n{err}",
                                Localize("Error", "Erreur"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("remote_errors.log", $"{DateTime.Now:u} | DELETE file {fileId} exception: {ex.Message}\n"); } catch {}
                if (!silent)
                {
                    MessageBox.Show(IsFrench 
                        ? $"Erreur lors de la suppression : {ex.Message}" 
                        : $"Error during deletion: {ex.Message}",
                        Localize("Error", "Erreur"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }

        private void SetTargetFolderFromRemote()
        {
            UpdateTargetDirLabel();
            CompareLocalWithRemote(_remotePaths);
            tabControlMain.SelectedIndex = 0;
        }

        private string GetActiveTargetFolder()
        {
            TreeNode node = treeViewRemote.SelectedNode;
            if (node == null) return "";

            if (node.Parent == null)
            {
                return "";
            }

            if (node.ImageKey == "folder")
            {
                var parts = new List<string>();
                TreeNode curr = node;
                while (curr != null)
                {
                    parts.Insert(0, curr.Text);
                    curr = curr.Parent;
                }
                if (parts.Count > 0)
                {
                    parts.RemoveAt(0); // Remove "Racine (/)" / "Root (/)"
                }
                return string.Join("/", parts);
            }
            else if (node.ImageKey == "file")
            {
                if (node.Parent != null && node.Parent.ImageKey == "folder")
                {
                    if (node.Parent.Parent == null)
                    {
                        return "";
                    }
                    var parts = new List<string>();
                    TreeNode curr = node.Parent;
                    while (curr != null)
                    {
                        parts.Insert(0, curr.Text);
                        curr = curr.Parent;
                    }
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(0); // Remove "Racine (/)" / "Root (/)"
                    }
                    return string.Join("/", parts);
                }
            }
            return "";
        }

        private List<string> FindAllRemotePathsWithFilename(string filename)
        {
            var paths = new List<string>();
            if (_remotePaths == null) return paths;

            foreach (string path in _remotePaths)
            {
                if (string.Equals(path, filename, StringComparison.OrdinalIgnoreCase) ||
                    path.EndsWith("/" + filename, StringComparison.OrdinalIgnoreCase))
                {
                    paths.Add(path);
                }
            }
            return paths;
        }

        private void UpdateTargetDirLabel()
        {
            if (labelTargetDir == null) return;

            string target = GetActiveTargetFolder();
            if (string.IsNullOrEmpty(target))
            {
                labelTargetDir.Text = IsFrench
                    ? "Dossier de destination sur le serveur : Racine (/)"
                    : "Destination folder on server: Root (/)";
            }
            else
            {
                labelTargetDir.Text = IsFrench
                    ? $"Dossier de destination sur le serveur : {target}"
                    : $"Destination folder on server: {target}";
            }
        }

        private async Task<bool> UpdateRemoteFileMetadataAsync(long fileId, string newLabel, string newDirectoryLabel, string api, string srv, bool silent = false, CancellationToken token = default)
        {
            string url = $"{srv}/api/files/{fileId}/metadata";

            try
            {
                using (var content = new MultipartFormDataContent())
                {
                    string finalDir = newDirectoryLabel;
                    if (string.IsNullOrWhiteSpace(finalDir))
                    {
                        finalDir = "/";
                    }
                    else
                    {
                        finalDir = finalDir.Replace('\\', '/').Trim('/');
                        if (string.IsNullOrWhiteSpace(finalDir))
                        {
                            finalDir = "/";
                        }
                    }

                    var metadata = new
                    {
                        label = newLabel,
                        directoryLabel = finalDir
                    };

                    string jsonStr = JsonConvert.SerializeObject(metadata);
                    var stringContent = new StringContent(jsonStr);
                    stringContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
                    content.Add(stringContent, "jsonData");

                    using (var request = new HttpRequestMessage(HttpMethod.Post, url))
                    {
                        request.Headers.Add("X-Dataverse-key", api);
                        request.Content = content;

                        var response = await _httpClient.SendAsync(request, token);
                        if (response.IsSuccessStatusCode)
                        {
                            return true;
                        }
                        else
                        {
                            string err = await response.Content.ReadAsStringAsync();
                            try { File.AppendAllText("remote_errors.log", $"{DateTime.Now:u} | POST metadata file {fileId} (label={newLabel}, dir={newDirectoryLabel}) failed: {response.StatusCode} - {err}\n"); } catch {}
                            if (!silent)
                            {
                                MessageBox.Show(IsFrench 
                                    ? $"Erreur serveur lors de la modification : {response.StatusCode}\n{err}" 
                                    : $"Server error during modification: {response.StatusCode}\n{err}",
                                    Localize("Error", "Erreur"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                try { File.AppendAllText("remote_errors.log", $"{DateTime.Now:u} | POST metadata file {fileId} exception: {ex.Message}\n"); } catch {}
                if (!silent)
                {
                    MessageBox.Show(IsFrench 
                        ? $"Erreur lors de la modification : {ex.Message}" 
                        : $"Error during modification: {ex.Message}",
                        Localize("Error", "Erreur"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            return false;
        }

        private HashSet<string> GetExpandedPaths(TreeView tv)
        {
            var expandedPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (TreeNode root in tv.Nodes)
            {
                GetExpandedPathsRecursive(root, "", expandedPaths);
            }
            return expandedPaths;
        }

        private void GetExpandedPathsRecursive(TreeNode node, string currentPath, HashSet<string> paths)
        {
            string cleanText = GetCleanNodeText(node);
            string nodePath = string.IsNullOrEmpty(currentPath) ? cleanText : $"{currentPath}/{cleanText}";
            if (node.IsExpanded)
            {
                paths.Add(nodePath);
            }
            foreach (TreeNode child in node.Nodes)
            {
                GetExpandedPathsRecursive(child, nodePath, paths);
            }
        }

        private void RestoreExpandedPaths(TreeView tv, HashSet<string> expandedPaths)
        {
            if (expandedPaths == null || expandedPaths.Count == 0)
            {
                tv.ExpandAll();
                return;
            }

            foreach (TreeNode root in tv.Nodes)
            {
                RestoreExpandedPathsRecursive(root, "", expandedPaths);
            }
        }

        private void RestoreExpandedPathsRecursive(TreeNode node, string currentPath, HashSet<string> paths)
        {
            string cleanText = GetCleanNodeText(node);
            string nodePath = string.IsNullOrEmpty(currentPath) ? cleanText : $"{currentPath}/{cleanText}";
            if (paths.Contains(nodePath))
            {
                node.Expand();
            }
            else
            {
                node.Collapse();
            }
            foreach (TreeNode child in node.Nodes)
            {
                RestoreExpandedPathsRecursive(child, nodePath, paths);
            }
        }

        private string GetSelectedNodePath(TreeView tv)
        {
            TreeNode selected = tv.SelectedNode;
            if (selected == null) return null;

            var parts = new List<string>();
            TreeNode curr = selected;
            while (curr != null)
            {
                parts.Insert(0, GetCleanNodeText(curr));
                curr = curr.Parent;
            }
            return string.Join("/", parts);
        }

        private void RestoreSelectedNodePath(TreeView tv, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            foreach (TreeNode root in tv.Nodes)
            {
                var found = FindNodeByPathRecursive(root, "", path);
                if (found != null)
                {
                    tv.SelectedNode = found;
                    break;
                }
            }
        }

        private TreeNode FindNodeByPathRecursive(TreeNode node, string currentPath, string targetPath)
        {
            string cleanText = GetCleanNodeText(node);
            string nodePath = string.IsNullOrEmpty(currentPath) ? cleanText : $"{currentPath}/{cleanText}";
            if (string.Equals(nodePath, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                return node;
            }
            foreach (TreeNode child in node.Nodes)
            {
                var found = FindNodeByPathRecursive(child, nodePath, targetPath);
                if (found != null) return found;
            }
            return null;
        }

        private string GetRemoteFolderNodePath(TreeNode folderNode)
        {
            if (folderNode == null) return "";
            var parts = new List<string>();
            TreeNode curr = folderNode;
            while (curr != null)
            {
                parts.Insert(0, curr.Text);
                curr = curr.Parent;
            }
            if (parts.Count > 0)
            {
                parts.RemoveAt(0); // Remove "Racine (/)" / "Root (/)"
            }
            return string.Join("/", parts);
        }

        private async Task DownloadRemoteItemAsync()
        {
            var selectedNodes = treeViewRemote.SelectedNodes;
            if (selectedNodes == null || selectedNodes.Count == 0) return;

            // 1. Collect files to download
            var downloads = new List<Tuple<DataverseFileItem, string>>();
            var addedFileIds = new HashSet<long>();

            foreach (var node in selectedNodes)
            {
                if (node.ImageKey == "file")
                {
                    if (node.Tag is long fileId && fileId > 0)
                    {
                        if (addedFileIds.Contains(fileId)) continue;

                        var item = _remoteFiles.Find(f => f.DataFile != null && f.DataFile.Id == fileId);
                        if (item != null)
                        {
                            downloads.Add(Tuple.Create(item, "")); // Saves directly to targetLocalDir
                            addedFileIds.Add(fileId);
                        }
                    }
                }
                else // folder or root
                {
                    string selectedFolderPath = GetRemoteFolderNodePath(node);
                    string folderName = node.Text;
                    bool isVirtualRoot = node.Parent == null;

                    foreach (var item in _remoteFiles)
                    {
                        if (item.DataFile == null) continue;
                        long fileId = item.DataFile.Id;
                        if (addedFileIds.Contains(fileId)) continue;

                        string dir = item.DirectoryLabel ?? "";
                        dir = dir.Replace('\\', '/').Trim('/');

                        bool isMatch = false;
                        string relativeDir = "";

                        if (string.IsNullOrEmpty(selectedFolderPath))
                        {
                            isMatch = true;
                            relativeDir = dir;
                        }
                        else if (string.Equals(dir, selectedFolderPath, StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;
                            relativeDir = isVirtualRoot ? "" : folderName;
                        }
                        else if (dir.StartsWith(selectedFolderPath + "/", StringComparison.OrdinalIgnoreCase))
                        {
                            isMatch = true;
                            string sub = dir.Substring(selectedFolderPath.Length + 1);
                            relativeDir = isVirtualRoot ? sub : $"{folderName}/{sub}";
                        }

                        if (isMatch)
                        {
                            downloads.Add(Tuple.Create(item, relativeDir));
                            addedFileIds.Add(fileId);
                        }
                    }
                }
            }

            if (downloads.Count == 0)
            {
                string msg = IsFrench ? "Aucun fichier à télécharger." : "No files to download.";
                MessageBox.Show(msg, Localize("Information", "Information"), MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. Ask user for target directory
            string targetLocalDir = "";
            using (var fbd = new FolderBrowserDialog())
            {
                fbd.Description = IsFrench 
                    ? "Sélectionnez le dossier local de destination pour le téléchargement :" 
                    : "Select local destination folder for download:";
                if (fbd.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }
                targetLocalDir = fbd.SelectedPath;
            }

            // 3. Prepare UI
            ToggleControls(false);
            btnCancel.Visible = btnCancel.Enabled = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            progressBar.Value = 0;
            progressBar.Style = ProgressBarStyle.Continuous;
            SendMessage(progressBar.Handle, PBM_SETSTATE, (IntPtr)PBST_NORMAL, IntPtr.Zero);

            long totalBytesToDownload = 0;
            foreach (var d in downloads)
            {
                totalBytesToDownload += d.Item1.DataFile?.Filesize ?? 0;
            }
            progressBar.Maximum = (int)(Math.Max(totalBytesToDownload, 1024) / 1024);

            long totalBytesDownloadedSoFar = 0;
            _uploadStartTime = DateTime.Now; // reuse for elapsed time
            labelStatElapsedValue.Text = "00:00:00";
            _timerElapsed.Start();

            string api = textBoxApiKey.Text.Trim();
            string srv = comboBoxServer.SelectedItem?.ToString().Trim().TrimEnd('/');

            int successCount = 0;
            int errorCount = 0;

            try
            {
                for (int i = 0; i < downloads.Count; i++)
                {
                    token.ThrowIfCancellationRequested();

                    var item = downloads[i].Item1;
                    string relativeDir = downloads[i].Item2;
                    string filename = item.Label;

                    string localFileDir = string.IsNullOrEmpty(relativeDir) 
                        ? targetLocalDir 
                        : Path.Combine(targetLocalDir, relativeDir.Replace('/', Path.DirectorySeparatorChar));

                    if (!Directory.Exists(localFileDir))
                    {
                        Directory.CreateDirectory(localFileDir);
                    }

                    string localFilePath = Path.Combine(localFileDir, filename);
                    long fileId = item.DataFile?.Id ?? 0;
                    long fileSize = item.DataFile?.Filesize ?? 0;

                    string downloadUrl = $"{srv}/api/access/datafile/{fileId}";

                    // Set label
                    string statusMsg = IsFrench
                        ? $"Téléchargement {i + 1}/{downloads.Count} : {filename} — 0%"
                        : $"Downloading {i + 1}/{downloads.Count} : {filename} — 0%";
                    SafeSetLabel(labelStatus, statusMsg);

                    try
                    {
                        using (var request = new HttpRequestMessage(HttpMethod.Get, downloadUrl))
                        {
                            if (!string.IsNullOrEmpty(api))
                            {
                                request.Headers.Add("X-Dataverse-key", api);
                            }

                            using (var response = await _httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, token))
                            {
                                response.EnsureSuccessStatusCode();

                                using (var downloadStream = await response.Content.ReadAsStreamAsync(token))
                                using (var fileStream = new FileStream(localFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true))
                                {
                                    byte[] buffer = new byte[8192];
                                    int bytesRead;
                                    long fileBytesDownloaded = 0;

                                    while ((bytesRead = await downloadStream.ReadAsync(buffer, 0, buffer.Length, token)) > 0)
                                    {
                                        await fileStream.WriteAsync(buffer, 0, bytesRead, token);
                                        fileBytesDownloaded += bytesRead;
                                        totalBytesDownloadedSoFar += bytesRead;

                                        int percent = fileSize > 0 ? (int)((fileBytesDownloaded * 100) / fileSize) : 100;
                                        string progressMsg = IsFrench
                                            ? $"Téléchargement {i + 1}/{downloads.Count} : {filename} — {percent}%"
                                            : $"Downloading {i + 1}/{downloads.Count} : {filename} — {percent}%";
                                        SafeSetLabel(labelStatus, progressMsg);

                                        progressBar.Invoke((Action)(() =>
                                        {
                                            progressBar.Value = (int)Math.Min(totalBytesDownloadedSoFar / 1024, progressBar.Maximum);
                                        }));

                                        // Update speed and ETA
                                        TimeSpan elapsed = DateTime.Now - _uploadStartTime;
                                        double secs = Math.Max(elapsed.TotalSeconds, 0.1);
                                        double moPerSec = (totalBytesDownloadedSoFar / 1024.0 / 1024.0) / secs;
                                        SafeSetLabel(labelStatSpeedValue, $"{moPerSec:F1}");
                                        SafeSetLabel(labelSpeed, $"{moPerSec:F1} {GetSpeedUnit()}");

                                        long remainingBytes = totalBytesToDownload - totalBytesDownloadedSoFar;
                                        TimeSpan eta = TimeSpan.FromSeconds(
                                            secs > 0 && totalBytesDownloadedSoFar > 0 ? remainingBytes / (totalBytesDownloadedSoFar / secs) : 0);
                                        SafeSetLabel(labelStatETAValue, $"{eta.Hours:00}:{eta.Minutes:00}:{eta.Seconds:00}");
                                    }
                                }
                            }
                        }
                        successCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        // If file was partially created, delete it
                        try { if (File.Exists(localFilePath)) File.Delete(localFilePath); } catch { }
                        
                        string errLog = $"{DateTime.Now:u} | Error downloading file {filename} (ID: {fileId}): {ex.Message}\n";
                        try { File.AppendAllText("download_errors.log", errLog); } catch { }
                    }
                }

                // Complete
                _timerElapsed.Stop();
                string completeMsg = IsFrench
                    ? $"Téléchargement terminé ! {successCount} réussi(s), {errorCount} échoué(s)."
                    : $"Download complete! {successCount} succeeded, {errorCount} failed.";
                MessageBox.Show(completeMsg, Localize("Completed", "Terminé"), MessageBoxButtons.OK, 
                    errorCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
            }
            catch (OperationCanceledException)
            {
                _timerElapsed.Stop();
                MessageBox.Show(
                    IsFrench ? "Téléchargement annulé par l'utilisateur." : "Download cancelled by the user.",
                    Localize("Cancelled", "Annulé"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            finally
            {
                ToggleControls(true);
                btnCancel.Visible = false;
                _timerElapsed.Stop();
                _cts?.Dispose();
                _cts = null;
                SafeSetLabel(labelStatus, IsFrench ? "Prêt" : "Ready");
                progressBar.Value = 0;
            }
        }
    }

    public static class Prompt
    {
        public static string ShowDialog(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 500,
                Height = 165,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterScreen,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 10, Text = text, Width = 450, Height = 40 };
            TextBox textBox = new TextBox() { Left = 20, Top = 55, Width = 450, Text = defaultValue };
            Button confirmation = new Button() { Text = "OK", Left = 370, Width = 100, Top = 90, DialogResult = DialogResult.OK };
            confirmation.Click += (sender, e) => { prompt.Close(); };
            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }

    public class DataverseFilesResponse
    {
        public string Status { get; set; }
        public List<DataverseFileItem> Data { get; set; }
    }

    public class DataverseFileItem
    {
        public string Label { get; set; }
        public string DirectoryLabel { get; set; }
        public DataverseFileInfo DataFile { get; set; }
    }

    public class DataverseFileInfo
    {
        public long Id { get; set; }
        public long Filesize { get; set; }
        public DataverseChecksumInfo Checksum { get; set; }
        public string CreationDate { get; set; }
    }

    public class DataverseChecksumInfo
    {
        public string Type { get; set; }
        public string Value { get; set; }
    }

    public class FolderPickerDialog : Form
    {
        private TreeView treeView;
        private Button btnOk;
        private Button btnCancel;
        private Label lblPrompt;
        public string SelectedFolderPath { get; private set; }

        public FolderPickerDialog(TreeView sourceTreeView, bool isFrench)
        {
            this.Text = isFrench ? "Déplacer sur le serveur" : "Move on server";
            this.Size = new Size(400, 450);
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.StartPosition = FormStartPosition.CenterParent;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            lblPrompt = new Label
            {
                Text = isFrench ? "Sélectionnez le dossier de destination :" : "Select destination folder:",
                Location = new Point(12, 12),
                Width = 360,
                Height = 20
            };

            treeView = new TreeView
            {
                Location = new Point(12, 35),
                Width = 360,
                Height = 310,
                ImageList = sourceTreeView.ImageList
            };

            btnOk = new Button
            {
                Text = "OK",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 365),
                Width = 80,
                Height = 30
            };

            btnCancel = new Button
            {
                Text = isFrench ? "Annuler" : "Cancel",
                DialogResult = DialogResult.Cancel,
                Location = new Point(292, 365),
                Width = 80,
                Height = 30
            };

            var btnNewFolder = new Button
            {
                Text = isFrench ? "Nouveau dossier..." : "New folder...",
                Location = new Point(12, 365),
                Width = 140,
                Height = 30
            };

            btnNewFolder.Click += (s, e) =>
            {
                TreeNode selected = treeView.SelectedNode;
                if (selected == null)
                {
                    string alertMsg = isFrench 
                        ? "Veuillez d'abord sélectionner un dossier parent dans la liste." 
                        : "Please select a parent folder in the list first.";
                    MessageBox.Show(alertMsg, isFrench ? "Information" : "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                string promptMsg = isFrench
                    ? "Entrez le nom du nouveau dossier :"
                    : "Enter the name of the new folder:";
                string promptTitle = isFrench
                    ? "Nouveau dossier"
                    : "New Folder";

                string folderName = Prompt.ShowDialog(promptMsg, promptTitle, "");
                if (string.IsNullOrWhiteSpace(folderName)) return;

                folderName = folderName.Trim().Replace('\\', '/');
                if (folderName.Contains("/"))
                {
                    string errMsg = isFrench 
                        ? "Le nom du dossier ne doit pas contenir de sous-dossiers (pas de caractère '/')." 
                        : "The folder name must not contain subdirectories (no '/' character).";
                    MessageBox.Show(errMsg, isFrench ? "Erreur" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                char[] invalidChars = Path.GetInvalidFileNameChars();
                if (folderName.IndexOfAny(invalidChars) >= 0)
                {
                    string errMsg = isFrench 
                        ? "Le nom de dossier contient des caractères invalides." 
                        : "The folder name contains invalid characters.";
                    MessageBox.Show(errMsg, isFrench ? "Erreur" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                foreach (TreeNode child in selected.Nodes)
                {
                    if (string.Equals(child.Text, folderName, StringComparison.OrdinalIgnoreCase))
                    {
                        string errMsg = isFrench
                            ? "Un dossier avec ce nom existe déjà à cet endroit."
                            : "A folder with this name already exists in this location.";
                        MessageBox.Show(errMsg, isFrench ? "Erreur" : "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        treeView.SelectedNode = child;
                        return;
                    }
                }

                TreeNode newNode = new TreeNode(folderName)
                {
                    ImageKey = "folder",
                    SelectedImageKey = "folder"
                };
                selected.Nodes.Add(newNode);
                selected.Expand();
                treeView.SelectedNode = newNode;
            };

            this.Controls.Add(lblPrompt);
            this.Controls.Add(treeView);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.Controls.Add(btnNewFolder);

            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;

            foreach (TreeNode root in sourceTreeView.Nodes)
            {
                CopyFoldersOnly(root, treeView.Nodes);
            }

            if (treeView.Nodes.Count > 0)
            {
                treeView.Nodes[0].Expand();
                treeView.SelectedNode = treeView.Nodes[0];
            }

            btnOk.Click += (s, e) =>
            {
                TreeNode selected = treeView.SelectedNode;
                if (selected != null)
                {
                    var parts = new List<string>();
                    TreeNode curr = selected;
                    while (curr != null)
                    {
                        parts.Insert(0, curr.Text);
                        curr = curr.Parent;
                    }
                    if (parts.Count > 0)
                    {
                        parts.RemoveAt(0); // Remove "Racine (/)" / "Root (/)"
                    }
                    SelectedFolderPath = string.Join("/", parts);
                }
                this.Close();
            };
        }

        private void CopyFoldersOnly(TreeNode source, TreeNodeCollection targetCollection)
        {
            if (source.ImageKey == "folder")
            {
                TreeNode newFolder = new TreeNode(source.Text)
                {
                    ImageKey = "folder",
                    SelectedImageKey = "folder"
                };
                targetCollection.Add(newFolder);

                foreach (TreeNode child in source.Nodes)
                {
                    CopyFoldersOnly(child, newFolder.Nodes);
                }
            }
        }
    }
}
