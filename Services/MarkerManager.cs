using System;
using System.Collections.Generic;
using System.Reflection;
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
                // 方案 A：尝试通过 ILibraryManager 调用 (某些 4.9 版本在这里)
                // 方案 B：通过反射强制调用，避开编译器的接口检查
                var method = _mediaSourceManager.GetType().GetMethod("SaveChapters", BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy);
                
                if (method != null)
                {
                    // 尝试匹配参数：(long, List<ChapterInfo>) 或 (string, List<ChapterInfo>)
                    var parameters = method.GetParameters();
                    if (parameters[0].ParameterType == typeof(string))
                    {
                        method.Invoke(_mediaSourceManager, new object[] { episode.InternalId.ToString(), chapters });
                    }
                    else
                    {
                        method.Invoke(_mediaSourceManager, new object[] { episode.InternalId, chapters });
                    }
                }
                else
                {
                    // 如果 IMediaSourceManager 没找到，去 ILibraryManager 找
                    var libMethod = _libraryManager.GetType().GetMethod("SaveChapters", BindingFlags.Instance | BindingFlags.Public);
                    libMethod?.Invoke(_libraryManager, new object[] { episode.InternalId, chapters });
                }

                // 核心：通知 Emby 元数据已更改，这会刷新播放器的 PlaybackInfo
                _libraryManager.UpdateItem(episode, episode.GetParent(), ItemUpdateType.MetadataEdit, null);
                
                _logger.Info("SeasonMarkerCopier: 标记已保存并尝试刷新 - {0}", episode.Name);
            }
            catch (Exception ex)
            {
                _logger.Error("SeasonMarkerCopier: 写入失败。错误详情: {0}", ex.Message);
            }
        }
    }
}
