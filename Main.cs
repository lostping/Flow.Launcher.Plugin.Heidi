using Flow.Launcher.Plugin.Heidi.Helper;
using Flow.Launcher.Plugin.Heidi.ViewModels;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Linq;
using System.IO;
using System.Diagnostics;
using System;

namespace Flow.Launcher.Plugin.Heidi
{
    public class Heidi : IPlugin, ISettingProvider, IPluginI18n
    {
        private PluginInitContext _context;
        private Settings _settings;
        public IHeidiSessionService HeidiSessionService { get; set; }
        public enum HeidiEntryType { None, UserEntry}

        public Heidi()
        {
            HeidiSessionService = new HeidiSessionService();
        }

        public void Init(PluginInitContext context)
        {
            _context = context;
            _settings = context.API.LoadSettingJsonStorage<Settings>();
        }

        public List<Result> Query(Query query)
        {
            var results = new List<Result>();
            // Add empty entry if option to add Heidi exe to results was checked
            if (_settings.AddHeidiExeToResults)
                results.Add(CreateResult(HeidiEntryType.None));

            // Add all session entries if no search query was entered
            var querySearch = query.Search;
            if (string.IsNullOrEmpty(querySearch))
            {
                var allHeidiSessions =
                    HeidiSessionService
                    .GetAll(_settings, _context)
                    .Select(HeidiSession => CreateResult(HeidiEntryType.UserEntry, HeidiSession.Identifier, HeidiSession.Identifier, HeidiSession:HeidiSession));

                return results.Concat(allHeidiSessions).ToList();
            }

            // Filter sessions for matches
            var queryHeidiSessions =
                HeidiSessionService.GetAll(_settings, _context)
                .Where(session => session.Identifier.ToLowerInvariant().Contains(querySearch.ToLowerInvariant()));

            // If no filtered elements found return result to connect Heidi to the entered hostname (or IP)
            if (!queryHeidiSessions.Any())
            {
                var session = string.Join(" ", _context.API.GetTranslation("flowlauncher_plugin_Heidi_start"), _context.API.GetTranslation("flowlauncher_plugin_Heidi_host"));
                results.Add(CreateResult(HeidiEntryType.None));

                return results;
            }

            // If filtered sessions matched add them to the results
            foreach (var HeidiSession in queryHeidiSessions)
            {
                // silently skip sessions without hostname or ip address
                if (!string.IsNullOrEmpty(HeidiSession.Identifier))
                {
                    results.Add(CreateResult(HeidiEntryType.UserEntry, HeidiSession.Identifier, HeidiSession.Identifier, HeidiSession: HeidiSession));
                }
            }

            return results;
        }

        public string GetTranslatedPluginTitle()
        {
            return _context.API.GetTranslation("flowlauncher_plugin_Heidi_plugin_name");
        }

        public string GetTranslatedPluginDescription()
        {
            return _context.API.GetTranslation("flowlauncher_plugin_Heidi_plugin_description");
        }

        public Control CreateSettingPanel()
        {
            return new HeidiSettings(_context, _settings, new SettingsViewModel(_settings, _context));
        }

        /// <summary>
        /// Creates a new Result item
        /// </summary>
        /// <returns>A Result object containing the HeidiSession identifier and its connection string</returns>
        private Result CreateResult(
            HeidiEntryType ket, string title = "", string subTitle = "", int score = 50, HeidiSession HeidiSession = null)
        {

            #region translations
            if (string.IsNullOrEmpty(title))
            {
                title = _context.API.GetTranslation("flowlauncher_plugin_Heidi_settings");
            }
            if (string.IsNullOrEmpty(subTitle))
            {
                subTitle = _context.API.GetTranslation("flowlauncher_plugin_Heidi_openConfigs");
            }

            string appName = "HeidiSQL";

            string trStart = _context.API.GetTranslation("flowlauncher_plugin_Heidi_start");
            string trOpen = _context.API.GetTranslation("flowlauncher_plugin_Heidi_open");
            string trWo = _context.API.GetTranslation("flowlauncher_plugin_Heidi_without");
            string trDefault = _context.API.GetTranslation("flowlauncher_plugin_Heidi_default");
            string trSession = _context.API.GetTranslation("flowlauncher_plugin_Heidi_session");
            string trSettings = _context.API.GetTranslation("flowlauncher_plugin_Heidi_settings");
            #endregion


            
            switch (ket)
            {
                case HeidiEntryType.None:
                    return new Result
                    {
                        Title = $"{trStart} {appName}",
                        SubTitle = $"{trOpen} {appName} {trWo} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchHeidiSession(ket, string.Empty),
                        Score = score = 0
                    };
                case HeidiEntryType.UserEntry:
                    // UserEntry starts selected session
                    return new Result
                    {
                        Title = new DirectoryInfo(HeidiSession.Identifier).Name,
                        SubTitle = $"{trOpen} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchHeidiSession(ket, title, HeidiSession),
                        Score = score
                    };
                default:
                    // this case shouldn't happen, at least we planned not to - so we skip silently to default
                    return new Result
                    {
                        Title = $"{trStart} {appName}",
                        SubTitle = $"{trOpen} {appName} {trWo} {trSession}",
                        IcoPath = "icon.png",
                        Action = context => LaunchHeidiSession(ket, string.Empty),
                        Score = score = 0
                    };
            }
        }

        /// <summary>
        /// Launches a new Putty session
        /// </summary>
        /// <returns>If launching Putty succeeded</returns>
        private bool LaunchHeidiSession(HeidiEntryType ket, string session, HeidiSession HeidiSession = null)
        {
            try
            {
                var HeidiPath = _settings.HeidiExePath;
                string sessionsPath = Path.Combine(Path.GetDirectoryName(_settings.HeidiExePath), "Sessions");
                if (string.IsNullOrEmpty(_settings.HeidiExePath))
                {
                    // output a message if path was not set to explain why next message shows up (exception)
                    string trPathNotSet = _context.API.GetTranslation("flowlauncher_plugin_Heidi_pathNotSet");
                    string trConfigPlugin = _context.API.GetTranslation("flowlauncher_plugin_Heidi_configPlugin");
                    _context.API.ShowMsg(trPathNotSet, trConfigPlugin, "");
                }

                var p = new Process { StartInfo = { FileName = HeidiPath } };


                switch (ket)
                {
                    case HeidiEntryType.None:
                        p.StartInfo.Arguments = "";
                        break;
                    case HeidiEntryType.UserEntry:
                        p.StartInfo.Arguments = "-d \"" + HeidiSession.Identifier + "\""; // start session
                        break;
                    default:
                        break;
                }

                p.Start();

                return true;

            }
            catch (Exception ex)
            {
                string trError = _context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                // Report the exception to the user. No further actions required
                _context.API.ShowMsg(trError + ": " + HeidiSession?.Identifier ?? session + " (" + _settings.HeidiExePath + ") ", ex.Message, "");

                return false;
            }
        }
    }
}