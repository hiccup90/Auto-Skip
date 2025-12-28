using MediaBrowser.Model.Plugins;

namespace SeasonMarkerCopier.Configuration
{
    public class PluginConfiguration : BasePluginConfiguration
    {
        public int IntroEndSeconds { get; set; } = 90;
        public int OutroMinutes { get; set; } = 2;
        public bool EnableAutoSkip { get; set; } = false;
    }
}
