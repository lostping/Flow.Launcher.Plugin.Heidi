using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.Heidi.Helper
{
    public class HeidiSessionService : IHeidiSessionService
    {
        public static List<HeidiSession> results = new List<HeidiSession>();

        /// <summary>
        /// Returns a List of all Heidi Sessions
        /// </summary>
        /// <returns>A List of all Heidi Sessions</returns>
        public IEnumerable<HeidiSession> GetAll(Settings settings, PluginInitContext context)
        {
            results.Clear();
            if (settings.IsHeidiPortable)
            {
                string sessionsPath = Path.Combine(Path.GetDirectoryName(settings.HeidiExePath), "portable_settings.txt");
                // let's read the portable config file if it exists
                if (File.Exists(sessionsPath))
                {
                    try
                    {
                        // parse file line by line
                        foreach (string line in File.ReadLines(sessionsPath))
                        {
                            Match match = HeidiRegex.theSession.Match(line);
                            // add matches to results
                            if (match.Success) {
                                results.Add(new HeidiSession {
                                    Identifier = match.Groups["Session"].Value,
                                });
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        // If there's an exception the settings file is locked for some reason
                        string trError = context.API.GetTranslation("flowlauncher_plugin_Heidi_error");
                        context.API.ShowMsg(trError, sessionsPath, ex.Message);
                    }
                }
                else
                {
                    // no config file found
                    string trSessMiss = context.API.GetTranslation("flowlauncher_plugin_Heidi_sessionFolderMissing");
                    context.API.ShowMsg(trSessMiss, sessionsPath, "");
                }
            }
            else
            {
                // let's read the registry key for heidi sessions
                // the sessions can be in a folder structure, so we need to recurse
                // through the whole structure capturing only matches
                using (var root = Registry.CurrentUser.OpenSubKey("Software\\HeidiSQL\\Servers"))
                {
                    if (root == null)
                    {
                        // do nothing if no registry entry for HeidiSQL
                    }
                    // trigger ReadSubkeys method initially
                    ReadSubkeys(root);
                }
            }

            return results;
        }

        /// <summary>
        /// Read subkeys. Check if registry key contains "Host" value.
        /// Calls itself for all subkeys found in current registry key
        /// </summary>
        /// <param name="subkeys">Registry key</param>
        public static void ReadSubkeys(RegistryKey subkeys)
        {
            var subkeyNames = subkeys.GetSubKeyNames();
            var valueNames = subkeys.GetValueNames();
            bool vnHasHost = valueNames.Contains<string>("Host");

            if (vnHasHost)
            {
                // "host" value name found - should be a session
                // remove path up to servers we only need the latter
                var sessionName = subkeys.ToString().Replace(@"HKEY_CURRENT_USER\Software\HeidiSQL\Servers\", "");
                results.Add(new HeidiSession
                    {
                        Identifier = sessionName,
                    });
            }

            foreach (var subkey in subkeyNames)
            {
                RegistryKey rk = subkeys.OpenSubKey(subkey);
                ReadSubkeys(rk);
            }
        }
    }
}
