using System;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Serialization;
using SeasonMarkerCopier.Configuration;

namespace SeasonMarkerCopier
{
    public class Plugin : BasePlugin<PluginConfiguration>
    {
        public override string Name => "Season Marker Copier";
        public override Guid Id => Guid.Parse("7a8b9c1d-2e3f-4a5b-6c7d-8e9f0a1b2c3d");

        public static Plugin Instance { get; private set; }

        public Plugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
            : base(applicationPaths, xmlSerializer)
        {
            Instance = this;
        }

        // 修复点：改名为 InstanceOptions 并显式指向 base.Configuration
        public PluginConfiguration InstanceOptions => base.Configuration;
    }
}
