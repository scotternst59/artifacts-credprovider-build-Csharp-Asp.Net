﻿// Copyright (c) Microsoft. All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using NuGetCredentialProvider.Logging;

namespace NuGetCredentialProvider.Util
{
    public static class EnvUtil
    {
        public const string LogPathEnvVar = "NUGET_CREDENTIALPROVIDER_LOG_PATH";
        public const string SessionTokenCacheEnvVar = "NUGET_CREDENTIALPROVIDER_SESSIONTOKENCACHE_ENABLED";

        public const string AuthorityEnvVar = "NUGET_CREDENTIALPROVIDER_ADAL_AUTHORITY";
        public const string AdalFileCacheEnvVar = "NUGET_CREDENTIALPROVIDER_ADAL_FILECACHE_ENABLED";
        public const string PpeHostsEnvVar = "NUGET_CREDENTIALPROVIDER_ADAL_PPEHOSTS";

        public const string SupportedHostsEnvVar = "NUGET_CREDENTIALPROVIDER_VSTS_HOSTS";
        public const string SessionTimeEnvVar = "NUGET_CREDENTIALPROVIDER_VSTS_SESSIONTIMEMINUTES";

        public const string BuildTaskUriPrefixes = "VSS_NUGET_URI_PREFIXES";
        public const string BuildTaskAccessToken = "VSS_NUGET_ACCESSTOKEN";
        public const string BuildTaskExternalEndpoints = "VSS_NUGET_EXTERNAL_FEED_ENDPOINTS";

        private static readonly string LocalAppDataLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "MicrosoftCredentialProvider");

        public static string AdalTokenCacheLocation { get; } = Path.Combine(LocalAppDataLocation, "ADALTokenCache.dat");

        public static string FileLogLocation { get; } = Environment.GetEnvironmentVariable(LogPathEnvVar);

        public static string SessionTokenCacheLocation { get; } = Path.Combine(LocalAppDataLocation, "SessionTokenCache.dat");

        public static Uri GetAuthorityFromEnvironment(ILogger logger)
        {
            var environmentAuthority = Environment.GetEnvironmentVariable(AuthorityEnvVar);
            if (environmentAuthority == null)
            {
                return null;
            }

            if (Uri.TryCreate(environmentAuthority, UriKind.Absolute, out Uri parsedUri))
            {
                logger.Verbose(string.Format(Resources.AADAuthorityOverrideFound, parsedUri.ToString()));
                return parsedUri;
            }
            else
            {
                logger.Warning(string.Format(Resources.CouldNotParseAADAuthorityOverride, environmentAuthority));
            }

            return null;
        }

        public static IList<string> GetHostsFromEnvironment(ILogger logger, string envVar, IEnumerable<string> defaultHosts, [CallerMemberName] string collectionName = null)
        {
            var hosts = new List<string>();

            var hostsFromEnvironment = Environment.GetEnvironmentVariable(envVar)?.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            if (hostsFromEnvironment != null)
            {
                logger.Verbose(string.Format(Resources.FoundHostsInEnvironment, collectionName));
                logger.Verbose(string.Join(",", hostsFromEnvironment));
                hosts.AddRange(hostsFromEnvironment);
            }

            hosts.AddRange(defaultHosts);
            return hosts;
        }

        public static bool AdalFileCacheEnabled()
        {
            return GetEnabledFromEnvironment(AdalFileCacheEnvVar);
        }

        public static bool SessionTokenCacheEnabled()
        {
            return GetEnabledFromEnvironment(SessionTokenCacheEnvVar);
        }

        public static TimeSpan? GetSessionTimeFromEnvironment(ILogger logger)
        {
            var minutes = Environment.GetEnvironmentVariable(SessionTimeEnvVar);
            if (minutes != null)
            {
                if (double.TryParse(minutes, out double parsedMinutes))
                {
                    return TimeSpan.FromMinutes(parsedMinutes);
                }

                logger.Warning(string.Format(Resources.CouldNotParseSessionTimeOverride, minutes));
            }

            return null;
        }

        private static bool GetEnabledFromEnvironment(string envVar, bool defaultValue = true)
        {
            if (bool.TryParse(Environment.GetEnvironmentVariable(envVar), out bool result))
            {
                return result;
            }

            return defaultValue;
        }
    }
}
