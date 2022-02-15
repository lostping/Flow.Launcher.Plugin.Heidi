using System;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Flow.Launcher.Plugin.Heidi.ViewModels;
using Flow.Launcher.Plugin.Heidi.Helper;
using Microsoft.Win32;
namespace Flow.Launcher.Plugin.Heidi
{
    /// <summary>
    /// Interaktionslogik für HeidiSettings.xaml
    /// </summary>
    public partial class HeidiSettings : UserControl
    {
        private const string HeidiUpdateCheckURL = "https://www.heidisql.com/updatecheck.php";
        private const string HeidiPortableDLNameSchema = "https://www.heidisql.com/downloads/releases/HeidiSQL_{VERSION}_64_Portable.zip";
        string HeidiSQLPath = string.Empty;
        string ZipDownloadPath = string.Empty;

        public PluginInitContext context { get; set; }
        public Settings settings { get; }
        private SettingsViewModel vm;

        public HeidiSettings(PluginInitContext context, Settings settings, SettingsViewModel vm)
        {
            this.context = context;
            this.settings = settings;
            this.vm = vm;
            DataContext = vm;

            InitializeComponent();
        }

        private void SettingsOpenHeidiPath_Click(object sender, RoutedEventArgs e)
        {
            string trExeFile = context.API.GetTranslation("flowlauncher_plugin_Heidi_openDialogExe");
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = trExeFile + " (*.exe)| *.exe";
            if (!string.IsNullOrEmpty(settings.HeidiExePath))
                openFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(settings.HeidiExePath);

            if (openFileDialog.ShowDialog() == true)
            {
                settings.HeidiExePath = openFileDialog.FileName;
            }

            SettingsHeidiExecutablePath.Text = settings.HeidiExePath;
        }

        private void HeidiSettingsView_Loaded(object sender, RoutedEventArgs e)
        {
            SettingsAddHeidiExeToResults.IsChecked = settings.AddHeidiExeToResults;
            SettingsAddHeidiExeToResults.Checked += (o, e) =>
            {
                settings.AddHeidiExeToResults = true;
            };
            SettingsAddHeidiExeToResults.Unchecked += (o, e) =>
            {
                settings.AddHeidiExeToResults = false;
            };

            SettingsIsHeidiPortable.IsChecked = settings.IsHeidiPortable;
            SettingsIsHeidiPortable.Checked += (o, e) =>
            {
                settings.IsHeidiPortable = true;
            };
            SettingsIsHeidiPortable.Unchecked += (o, e) =>
            {
                settings.IsHeidiPortable = false;
            };

            SettingsHeidiExecutablePath.Text = settings.HeidiExePath;
        }

        /// <summary>
        /// Download portable Heidi to plugin directory and set as Heidi instance
        /// </summary>
        private void DownloadHeidi_Click(object sender, RoutedEventArgs e)
        {
            // download async to not block the main thread and listen to the completed event
            try
            {
                // Save to the plugin settings folder.
                string PluginSettingsDir = Paths.ReturnPluginSettingsFolder(context);
                GetHeidiSQL(PluginSettingsDir);
            }
            catch (Exception ex)
            {
                string trError = context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                string trDown = context.API.GetTranslation("flowlauncher_plugin_Heidi_download");
                context.API.ShowMsg(trDown + " " + trError +": ", ex.Message, "");
            }
        }

        /// <summary>
        /// Start HeidiSQL version extraction and download process
        /// </summary>
        /// <param name="PluginSettingsFolder"></param>
        /// <returns></returns>
        public void GetHeidiSQL(string PluginSettingsFolder)
        {
            HeidiSQLPath = Path.Combine(PluginSettingsFolder, "HeidiSQL");
            if (!Directory.Exists(HeidiSQLPath))
            {
                Directory.CreateDirectory(HeidiSQLPath);
            }
            string CurrentINI = Path.Combine(HeidiSQLPath, "CURRENT_DL_VERSION.INI");
            DownloadVersion(HeidiUpdateCheckURL, CurrentINI);
            try
            {
                Ini data = new Ini(CurrentINI);
                string ReleaseVersion = data.GetValue("Version", "Release");
                string ZipReleaseURL = HeidiPortableDLNameSchema.Replace("{VERSION}", ReleaseVersion);
                Uri reluri = new Uri(ZipReleaseURL);
                string ZipFileName = Path.GetFileName(reluri.LocalPath);
                ZipDownloadPath = Path.Combine(HeidiSQLPath, ZipFileName);
                DownloadZip(ZipReleaseURL, ZipDownloadPath);
            }
            catch (Exception ex)
            {
                string trError = context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                string trDown = context.API.GetTranslation("flowlauncher_plugin_Heidi_download");
                context.API.ShowMsg(trDown + " " + trError + ": ", ex.Message, "");
            }

            // now let's set the default values for downloaded portable Heidi
            string HeidiExeFullPath = Path.Combine(HeidiSQLPath, "heidisql.exe");
            settings.HeidiExePath = HeidiExeFullPath;
            SettingsHeidiExecutablePath.Text = HeidiExeFullPath;
            settings.IsHeidiPortable = true;
            SettingsIsHeidiPortable.IsChecked = true;
        }

        private void DownloadVersion(string downloadURL, string localFileFullPath)
        {
            try
            {
                WebClient webClient = new WebClient();
                webClient.DownloadFile(new Uri(downloadURL), localFileFullPath);
            }
            catch (Exception ex)
            {
                string trError = context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                string trDown = context.API.GetTranslation("flowlauncher_plugin_Heidi_download");
                context.API.ShowMsg(trDown + " " + trError + ": ", ex.Message, "");
            }
        }

        private void DownloadZip(string downloadURL, string localFileFullPath)
        {
            WebClient webClient = new WebClient();
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);

            webClient.DownloadFileAsync(new Uri(downloadURL), localFileFullPath);
        }

        /// <summary>
        /// Handle Heidi download complete event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                if (File.Exists(ZipDownloadPath))
                {
                    ZipFile.ExtractToDirectory(ZipDownloadPath, HeidiSQLPath);
                    File.Delete(ZipDownloadPath);
                }
                string HeidiExeFullPath = Path.Combine(HeidiSQLPath, "heidisql.exe");
                if (File.Exists(HeidiExeFullPath))
                {
                    // start process once and kill it to create portable_settings.txt file
                    string sessionsPath = Path.Combine(HeidiSQLPath, "portable_settings.txt");
                    if (!File.Exists(sessionsPath))
                    {
                        File.Create(sessionsPath);
                    }
                }
            }
            catch (Exception ex)
            {
                string trError = context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                string trDown = context.API.GetTranslation("flowlauncher_plugin_Heidi_download");
                context.API.ShowMsg(trDown + " " + trError + ": ", ex.Message, "");
            }
        }
    }
}