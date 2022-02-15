using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Heidi.Helper
{
    public class Paths
    {
        /// <summary>
        /// Returns a shortend path
        /// </summary>
        /// <param name="inputPath">Path to shorten</param>
        /// <param name="removeDepth">Depth to remove</param>
        /// <returns></returns>
        public static string GetParent(string inputPath, int removeDepth)
        {
            for(int i = 0; i < removeDepth; i++)
            {
                inputPath = Path.GetDirectoryName(inputPath);
            }
            return inputPath;
        }

        /// <summary>
        /// Returns if current plugindirectory is portable
        /// </summary>
        /// <param name="context">PluginInitContext</param>
        /// <returns>bool</returns>
        public static bool IsPortable(PluginInitContext context)
        {
            if (context.CurrentPluginMetadata.PluginDirectory.Contains("UserData", StringComparison.OrdinalIgnoreCase))
            { 
                return true; 
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the PluginSettings Folder for portable and non portable mode
        /// </summary>
        /// <param name="context">PluginInitContext</param>
        /// <returns>string</returns>
        public static string ReturnPluginSettingsFolder(PluginInitContext context)
        {
            if(IsPortable(context))
            {
                string userData = Paths.GetParent(context.CurrentPluginMetadata.PluginDirectory, 2);
                return Path.Combine(userData, "Settings", "Plugins", "Flow.Launcher.Plugin.Heidi");
            } else
            {
                string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                return Path.Combine(appData, "FlowLauncher", "Settings", "Plugins", "Flow.Launcher.Plugin.Heidi");
            }
        }
    }
}
