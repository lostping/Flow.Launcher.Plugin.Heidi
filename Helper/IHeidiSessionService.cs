using System.Collections.Generic;
using Flow.Launcher.Plugin.Heidi.Helper;

namespace Flow.Launcher.Plugin.Heidi.Helper
{
    public interface IHeidiSessionService
    {
        /// <summary>
        /// Returns a List of all Heidi Sessions
        /// </summary>
        /// <returns>A List of all Heidi Sessions</returns>
        IEnumerable<HeidiSession> GetAll(Settings settings, PluginInitContext context);
    }
}
