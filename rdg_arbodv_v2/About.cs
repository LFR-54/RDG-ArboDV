using System;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace RDG_Uploader_GUI
{
    public partial class About : Form
    {
        private const string GitHubRepositoryUrl = "https://github.com/LFR-54/RDG-ArboDV";
        private readonly AppLanguage _language;

        public About() : this(AppLanguage.English)
        {
        }

        public About(AppLanguage language)
        {
            _language = language;

            InitializeComponent();

            pictureBox1.Parent = pictureBoxBanner;
            pictureBox1.BackColor = Color.Transparent;

            labelTitre.Parent = pictureBoxBanner;
            labelTitre.BackColor = Color.Transparent;

            ApplyLanguage();
            UpdateVersionLabel();
            UpdateBuildDateLabel();
            checkBoxIncludePrereleaseUpdates.Checked = Properties.Settings.Default.IncludePrereleaseUpdates;
        }

        private bool IsFrench => _language == AppLanguage.French;

        private string Localize(string english, string french)
        {
            return IsFrench ? french : english;
        }

        private void ApplyLanguage()
        {
            this.Text = Localize("About RDG ArboDV", "À propos de RDG ArboDV");
            groupBox1.Text = Localize("Product", "Produit");
            groupBox2.Text = Localize("Author", "Auteur");
            TextBoxAuthors.Text = Localize(
                "Created and maintained by Lucas FRENOT\n" +
                "Intern at CRAN, currently studying for a BTS CIEL\n" +
                "(Cybersecurity, IT and Networks, Electronics),\n" +
                "Option A:\n" +
                "Computer Science and Networks.",
                "Créé et maintenu par Lucas FRENOT\n" +
                "Stagiaire au CRAN, actuellement en BTS CIEL\n" +
                "(Cybersécurité, Informatique et réseaux, Électronique),\n" +
                "Option A :\n" +
                "Informatique et Réseaux.");
            checkBoxIncludePrereleaseUpdates.Text = Localize("Include beta releases", "Inclure les versions bêta");
            btnCheckForUpdates.Text = Localize("Check for updates", "Vérifier les mises à jour");
            labelGitHubRepository.Text = "LFR-54/RDG-ArboDV";
        }

        private void UpdateVersionLabel()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = version == null
                ? "Version 2.1.2"
                : $"Version {version.Major}.{version.Minor}.{version.Build}";
        }

        private void UpdateBuildDateLabel()
        {
            string exePath = Environment.ProcessPath;
            DateTime lastWrite = DateTime.MinValue;
            
            if (!string.IsNullOrEmpty(exePath) && File.Exists(exePath))
            {
                lastWrite = File.GetLastWriteTime(exePath);
            }

            if (lastWrite == DateTime.MinValue)
            {
                labelBuildDateTime.Text = Localize("Build date unknown", "Date de build inconnue");
            }
            else
            {
                CultureInfo culture = CultureInfo.GetCultureInfo(IsFrench ? "fr-FR" : "en-US");
                string formatted = lastWrite.ToString("dd MMMM yyyy - HH:mm:ss", culture);
                labelBuildDateTime.Text = Localize("Build date ", "Date de build ") + formatted;
            }
        }

        private void labelSiteWeb_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl("https://recherche.data.gouv.fr/");
        }

        private void labelGitHubRepository_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            OpenUrl(GitHubRepositoryUrl);
        }

        private void pictureBoxGitHub_Click(object sender, EventArgs e)
        {
            OpenUrl(GitHubRepositoryUrl);
        }

        private void OpenUrl(string url)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Localize("Unable to open the browser.\n\n", "Impossible d’ouvrir le navigateur.\n\n") + ex.Message,
                    Localize("Error", "Erreur"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void checkBoxIncludePrereleaseUpdates_CheckedChanged(object sender, EventArgs e)
        {
            Properties.Settings.Default.IncludePrereleaseUpdates = checkBoxIncludePrereleaseUpdates.Checked;
            Properties.Settings.Default.Save();
        }

        private async void btnCheckForUpdates_Click(object sender, EventArgs e)
        {
            if (Owner is Form1 mainForm)
                await mainForm.CheckForUpdatesAsync(showWhenUpToDate: true);
        }
    }
}
