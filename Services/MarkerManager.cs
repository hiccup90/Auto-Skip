using System;
using System.Collections.Generic;
using MediaBrowser.Controller.Entities.TV;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Logging;

namespace SeasonMarkerCopier.Services
{
    public class MarkerManager
    {
        private readonly IMediaSourceManager _mediaSourceManager;
        private readonly ILibraryManager _libraryManager;
        private readonly ILogger _logger;

        public MarkerManager(IMediaSourceManager mediaSourceManager, ILibraryManager libraryManager, ILogger logger)
        {
            _mediaSourceManager = mediaSourceManager;
            _libraryManager = libraryManager;
            _logger = logger;
        }

        public void WriteMarkers(Episode episode)
        {
            var options = Plugin.Instance.Configuration;
            var chapters = new List<ChapterInfo>();

            // 片头开始与结束 (激活按钮关键)
            chapters.Add(new ChapterInfo { Name = "Intro", StartPositionTicks = 0, MarkerType = MarkerType.IntroStart });
            chapters.Add(new ChapterInfo { Name = "Start of Content", StartPositionTicks = TimeSpan.FromSeconds(options.IntroEndSeconds).Ticks, MarkerType = MarkerType.IntroEnd });

            // 片尾
            long runtime = episode.RunTimeTicks ?? 0;
            if (runtime > 0)
            {
                long outroStart = runtime - TimeSpan.FromMinutes(options.OutroMinutes).Ticks;
                chapters.Add(new ChapterInfo { Name = "Outro", StartPositionTicks = Math.Max(0, outroStart), MarkerType = MarkerType.CreditsStart });
            }

            // 1. 保存章节数据
            _mediaSourceManager.SaveChapters(episode.InternalId, chapters);

            // 2. 强制触发元数据更新，使 PlaybackInfo 重新构建 Markers 字段
            _libraryManager.UpdateItem(episode, episode.GetParent(), ItemUpdateType.MetadataEdit, null);
            
            _logger.Info("Markers written and refreshed for: {0}", episode.Name);
        }
    }
}
