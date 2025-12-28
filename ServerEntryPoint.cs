using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;
using SeasonMarkerCopier.Services;

namespace SeasonMarkerCopier
{
    public class ServerEntryPoint : IServerEntryPoint
    {
        private readonly ISessionManager _sessionManager;
        private readonly IMediaSourceManager _mediaSourceManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger _logger;
        private PlaybackEventListener _listener;

        public ServerEntryPoint(ISessionManager sessionManager, IMediaSourceManager mediaSourceManager, ILibraryManager libraryManager, ILogManager logManager)
        {
            _sessionManager = sessionManager;
            _mediaSourceManager = mediaSourceManager;
            _libraryManager = libraryManager;
            _logger = logManager.GetLogger("SeasonMarkerCopier");
        }

        public void Run()
        {
            var markerManager = new MarkerManager(_mediaSourceManager, _libraryManager, _logger);
            _listener = new PlaybackEventListener(_sessionManager, markerManager, _logger);
        }

        public void Dispose() => _listener?.Dispose();
    }
}
