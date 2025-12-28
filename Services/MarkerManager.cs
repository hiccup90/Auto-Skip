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
            // 使用我们在 Plugin.cs 中定义的 InstanceOptions
            var options = Plugin.Instance.InstanceOptions;
            var chapters = new List<ChapterInfo>();

            // 1. 定义章节/标记
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
                // 核心修复点：
                // 在 Emby 4.9 中，SaveChapters 位于 IMediaSourceManager 接口
                // 参数通常是 (string itemId, List<ChapterInfo> chapters)
                // 或者是 (long itemId, List<ChapterInfo> chapters)
                // 我们尝试使用 .ToString() 确保兼容性，或者直接传 long
                
                _mediaSourceManager.SaveChapters(episode.InternalId, chapters);

                // 强制刷新 MediaSource 状态，这是激活官方按钮的必经之路
                _libraryManager.UpdateItem(episode, episode.GetParent(), ItemUpdateType.MetadataEdit, null);
                
                _logger.Info("SeasonMarkerCopier: Successfully saved markers for {0}", episode.Name);
            }
            catch (Exception ex)
            {
                _logger.Error("SeasonMarkerCopier: Failed to save chapters. Error: {0}", ex.Message);
            }
        }
    }
}
