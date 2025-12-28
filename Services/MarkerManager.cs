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
            var options = Plugin.Instance.InstanceOptions;
            var chapters = new List<ChapterInfo>();

            chapters.Add(new ChapterInfo { Name = "Intro", StartPositionTicks = 0, MarkerType = MarkerType.IntroStart });
            chapters.Add(new ChapterInfo { Name = "Start of Content", StartPositionTicks = TimeSpan.FromSeconds(options.IntroEndSeconds).Ticks, MarkerType = MarkerType.IntroEnd });

            long runtime = episode.RunTimeTicks ?? 0;
            if (runtime > 0)
            {
                long outroStart = runtime - TimeSpan.FromMinutes(options.OutroMinutes).Ticks;
                chapters.Add(new ChapterInfo { Name = "Outro", StartPositionTicks = Math.Max(0, outroStart), MarkerType = MarkerType.CreditsStart });
            }

            try 
            {
                // 修复点：使用标准的官方保存方法
                // 如果直接调用报错，说明你的 DLL 版本中该方法签名不同
                _mediaSourceManager.SaveChapters(episode.InternalId, chapters);

                // 强制刷新：这是 4.9 激活按钮的关键
                _libraryManager.UpdateItem(episode, episode.GetParent(), ItemUpdateType.MetadataEdit, null);
                
                _logger.Info("Markers saved for: {0}", episode.Name);
            }
            catch (Exception ex)
            {
                _logger.Error("Error saving chapters: {0}", ex.Message);
            }
        }
    }
}
