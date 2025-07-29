﻿using MediaBrowser.Controller.Net;
using MediaBrowser.Model.Logging;
using MediaBrowser.Model.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EmbyIcons.Services
{
    [Route("/EmbyIcons/RefreshCache", "POST", Summary = "Forces the icon cache to be cleared and refreshed")]
    public class RefreshCacheRequest : IReturnVoid
    {
    }

    public class CacheManagerService : IService
    {
        private readonly ILogger _logger;
        private readonly EmbyIconsEnhancer _enhancer;

        public CacheManagerService(ILogManager logManager)
        {
            _logger = logManager.GetLogger(nameof(CacheManagerService));
            _enhancer = Plugin.Instance?.Enhancer ?? throw new InvalidOperationException("Enhancer is not available.");
        }

        public async Task Post(RefreshCacheRequest request)
        {
            _logger.Info("[EmbyIcons] Received request to clear all icon and data caches from the settings page.");
            IconManagerService.InvalidateCache();

            var plugin = Plugin.Instance;
            if (plugin == null)
            {
                _logger.Warn("[EmbyIcons] Plugin instance not available.");
                return;
            }

            var config = plugin.Configuration;
            var iconsFolder = config.IconsFolder;

            await _enhancer.ForceCacheRefreshAsync(iconsFolder, CancellationToken.None);

            config.PersistedVersion = Guid.NewGuid().ToString("N");
            plugin.SaveCurrentConfiguration();
            _logger.Info($"[EmbyIcons] Cache cleared. New cache-busting version is '{config.PersistedVersion}'.");
        }
    }
}