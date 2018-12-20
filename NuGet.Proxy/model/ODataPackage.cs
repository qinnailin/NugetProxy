// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace NuGet.Proxy
{
    public class ODataPackage
    {
        public string Id { get; set; }

        public string Version { get; set; }

        public string NormalizedVersion { get; set; }

        public string IsPrerelease { get; set; }

        public string Title { get; set; }

        public string Authors { get; set; }

        public string Owners { get; set; }

        public string IconUrl { get; set; }

        public string LicenseUrl { get; set; }

        public string ProjectUrl { get; set; }

        public string DownloadCount { get; set; }

        public string RequireLicenseAcceptance { get; set; }

        public string DevelopmentDependency { get; set; }

        public string Description { get; set; }

        public string Summary { get; set; }

        public string ReleaseNotes { get; set; }

        public string Published { get; set; }

        public string LastUpdated { get; set; }

        public string Dependencies { get; set; }

        public string PackageHash { get; set; }

        public string PackageHashAlgorithm { get; set; }

        public string PackageSize { get; set; }

        public string Copyright { get; set; }

        public string Tags { get; set; }

        public string IsAbsoluteLatestVersion { get; set; }

        public string IsLatestVersion { get; set; }

        public string Listed { get; set; }

        public string VersionDownloadCount { get; set; }

        public string MinClientVersion { get; set; }

        public string Language { get; set; }

        public bool IsDowning { get; set; }
    }
}