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
                "------------------------------------------------------\n" +
                "Intern at CRAN, currently studying for a BTS CIEL\n" +
                "(Cybersecurity, IT and Networks, Electronics),\n" +
                "Option A:\n" +
                "Computer Science and Networks.",
                "Créé et maintenu par Lucas FRENOT\n" +
                "------------------------------------------------------\n" +
                "Stagiaire au CRAN, actuellement en BTS CIEL\n" +
                "(Cybersécurité, Informatique et réseaux, Électronique),\n" +
                "Option A :\n" +
                "Informatique et Réseaux.");
        }

        private void UpdateVersionLabel()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = version == null
                ? "Version 1.3.0"
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
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "https://recherche.data.gouv.fr/",
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
    }
}
