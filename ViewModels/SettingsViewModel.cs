using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Heidi.ViewModels
{
    public class SettingsViewModel
    {
        private readonly PluginInitContext context;
        private Settings settings;

        public SettingsViewModel(Settings settings, PluginInitContext context)
        {
            this.settings = settings;
            this.context = context;
        }
    }
}
