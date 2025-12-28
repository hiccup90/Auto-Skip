using System;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Controller.Session;
using MediaBrowser.Model.Logging;

namespace SeasonMarkerCopier.Services
{
    public class PlaybackEventListener : IDisposable
    {
        private readonly ISessionManager _sessionManager;
        private readonly MarkerManager _markerManager;
        private readonly ILogger _logger;

        public PlaybackEventListener(ISessionManager sessionManager, MarkerManager markerManager, ILogger logger)
        {
            _sessionManager = sessionManager;
            _markerManager = markerManager;
            _logger = logger;
            _sessionManager.PlaybackProgress += OnPlaybackProgress;
        }

        private void OnPlaybackProgress(object sender, PlaybackProgressEventArgs e)
        {
            if (e.Item is Episode episode && e.IsPaused)
            {
                _markerManager.WriteMarkers(episode);
            }
        }

        public void Dispose() => _sessionManager.PlaybackProgress -= OnPlaybackProgress;
    }
}
