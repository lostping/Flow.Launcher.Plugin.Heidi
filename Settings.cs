using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.Heidi
{
    public class Settings
    {
        public bool AddHeidiExeToResults { get; set; } = true;
        public bool IsHeidiPortable { get; set; } = false;
        public string HeidiExePath { get; set; }
    }
}
