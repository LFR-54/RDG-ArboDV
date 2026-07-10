using Newtonsoft.Json;
using System.Diagnostics;
using System.IO.Compression;
using System.Net.Http.Headers;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.RegularExpressions;

namespace RDG_Uploader_GUI
{
    internal sealed class UpdateService : IDisposable
    {
        private const string Repository = "LFR-54/RDG-ArboDV";
        private const string ReleasesEndpoint = "https://api.github.com/repos/" + Repository + "/releases?per_page=50";
        internal const string PackageFileName = "RDG-ArboDV-win-x64.zip";

        private readonly HttpClient _httpClient;
        private readonly bool _disposeHttpClient;

        public UpdateService(HttpClient httpClient = null)
        {
            _disposeHttpClient = httpClient == null;
            _httpClient = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };

            if (!_httpClient.DefaultRequestHeaders.UserAgent.Any())
                _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("RDG-ArboDV", GetCurrentVersion().ToString()));

            if (!_httpClient.DefaultRequestHeaders.Accept.Any())
                _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        }

        public static ReleaseVersion GetCurrentVersion()
        {
            Version assemblyVersion = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0);
            return new ReleaseVersion(
                Math.Max(0, assemblyVersion.Major),
                Math.Max(0, assemblyVersion.Minor),
                Math.Max(0, assemblyVersion.Build),
                null);
        }

        public async Task<UpdateCheckResult> CheckForUpdatesAsync(bool includePrereleases, CancellationToken cancellationToken = default)
        {
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(ReleasesEndpoint, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    return UpdateCheckResult.Failed($"GitHub returned {(int)response.StatusCode} ({response.ReasonPhrase}).");
                }

                string json = await response.Content.ReadAsStringAsync(cancellationToken);
                List<GitHubRelease> releases = JsonConvert.DeserializeObject<List<GitHubRelease>>(json) ?? new List<GitHubRelease>();
                ReleaseVersion currentVersion = GetCurrentVersion();

                var candidate = releases
                    .Where(release => !release.Draft && (includePrereleases || !release.Prerelease))
                    .Select(release => new { Release = release, ParsedVersion = ReleaseVersion.TryParse(release.TagName, out ReleaseVersion parsed) ? parsed : (ReleaseVersion?)null })
                    .Where(item => item.ParsedVersion.HasValue && item.ParsedVersion.Value.CompareTo(currentVersion) > 0)
                    .OrderByDescending(item => item.ParsedVersion.Value)
                    .FirstOrDefault();

                if (candidate == null)
                    return UpdateCheckResult.UpToDate(currentVersion);

                GitHubAsset package = candidate.Release.Assets.FirstOrDefault(asset =>
                    string.Equals(asset.Name, PackageFileName, StringComparison.OrdinalIgnoreCase));

                if (package == null)
                {
                    return UpdateCheckResult.Failed(
                        $"Release {candidate.Release.TagName} does not contain {PackageFileName}.",
                        candidate.Release.TagName);
                }

                string expectedSha256 = ExtractSha256(package.Digest);
                if (string.IsNullOrEmpty(expectedSha256))
                {
                    GitHubAsset checksumAsset = candidate.Release.Assets.FirstOrDefault(asset =>
                        string.Equals(asset.Name, PackageFileName + ".sha256", StringComparison.OrdinalIgnoreCase) ||
                        string.Equals(asset.Name, "checksums.txt", StringComparison.OrdinalIgnoreCase));

                    if (checksumAsset != null)
                        expectedSha256 = await DownloadChecksumAsync(checksumAsset.BrowserDownloadUrl, package.Name, cancellationToken);
                }

                if (string.IsNullOrEmpty(expectedSha256))
                {
                    return UpdateCheckResult.Failed(
                        $"Release {candidate.Release.TagName} does not provide a SHA-256 checksum for {PackageFileName}.",
                        candidate.Release.TagName);
                }

                var release = new UpdateRelease(
                    candidate.Release.TagName,
                    candidate.ParsedVersion.Value,
                    candidate.Release.Name,
                    candidate.Release.Body,
                    candidate.Release.HtmlUrl,
                    package.Name,
                    package.BrowserDownloadUrl,
                    expectedSha256,
                    candidate.Release.Prerelease);

                return UpdateCheckResult.UpdateAvailable(currentVersion, release);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                return UpdateCheckResult.Failed(ex.Message);
            }
        }

        public async Task<string> DownloadUpdateAsync(
            UpdateRelease release,
            IProgress<int> progress = null,
            CancellationToken cancellationToken = default)
        {
            string updateDirectory = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "RDG_ArboDV",
                "updates",
                release.Version.ToString() + "-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(updateDirectory);

            string archivePath = Path.Combine(updateDirectory, Path.GetFileName(release.PackageName));
            string partialArchivePath = archivePath + ".part";

            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(
                    release.DownloadUrl,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);
                response.EnsureSuccessStatusCode();

                long? contentLength = response.Content.Headers.ContentLength;
                await using Stream source = await response.Content.ReadAsStreamAsync(cancellationToken);
                await using (FileStream destination = new FileStream(
                    partialArchivePath,
                    FileMode.Create,
                    FileAccess.Write,
                    FileShare.None,
                    81920,
                    useAsync: true))
                {
                    byte[] buffer = new byte[81920];
                    long received = 0;
                    int read;
                    while ((read = await source.ReadAsync(buffer.AsMemory(0, buffer.Length), cancellationToken)) > 0)
                    {
                        await destination.WriteAsync(buffer.AsMemory(0, read), cancellationToken);
                        received += read;

                        if (contentLength.HasValue && contentLength.Value > 0)
                        {
                            int percentage = (int)Math.Clamp(received * 100L / contentLength.Value, 0, 100);
                            progress?.Report(percentage);
                        }
                    }

                    await destination.FlushAsync(cancellationToken);
                }

                // The stream must be closed before Windows can rename the .part file.
                File.Move(partialArchivePath, archivePath, true);

                string actualSha256 = await CalculateSha256Async(archivePath, cancellationToken);
                if (!string.Equals(actualSha256, release.Sha256, StringComparison.OrdinalIgnoreCase))
                {
                    File.Delete(archivePath);
                    throw new UpdateException("The downloaded update does not match the SHA-256 checksum published with the release.");
                }

                progress?.Report(100);
                return archivePath;
            }
            catch
            {
                if (File.Exists(partialArchivePath))
                    File.Delete(partialArchivePath);
                throw;
            }
        }

        public static void ApplyUpdateAndRestart(string archivePath, string applicationPath)
        {
            if (string.IsNullOrWhiteSpace(archivePath) || !File.Exists(archivePath))
                throw new UpdateException("The downloaded update archive is unavailable.");
            if (string.IsNullOrWhiteSpace(applicationPath) || !File.Exists(applicationPath))
                throw new UpdateException("The current application executable is unavailable.");

            string applicationDirectory = Path.GetDirectoryName(applicationPath);
            EnsureDirectoryIsWritable(applicationDirectory);

            string stagingDirectory = Path.Combine(Path.GetDirectoryName(archivePath), "extracted");
            Directory.CreateDirectory(stagingDirectory);
            ZipFile.ExtractToDirectory(archivePath, stagingDirectory, overwriteFiles: true);

            string stagedExecutable = FindStagedExecutable(stagingDirectory, Path.GetFileName(applicationPath));
            if (stagedExecutable == null)
                throw new UpdateException("The update archive does not contain an application executable.");

            string scriptPath = Path.Combine(Path.GetDirectoryName(archivePath), "apply-update-" + Guid.NewGuid().ToString("N") + ".cmd");
            int currentProcessId = Environment.ProcessId;
            string script = BuildUpdateScript(currentProcessId, stagedExecutable, applicationPath);
            File.WriteAllText(scriptPath, script);

            Process.Start(new ProcessStartInfo
            {
                FileName = scriptPath,
                WorkingDirectory = Path.GetDirectoryName(scriptPath),
                UseShellExecute = true,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
        }

        private async Task<string> DownloadChecksumAsync(string checksumUrl, string packageName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(checksumUrl))
                return null;

            string content = await _httpClient.GetStringAsync(checksumUrl, cancellationToken);
            foreach (string line in content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                Match match = Regex.Match(line, "(?i)\\b([a-f0-9]{64})\\b");
                if (!match.Success)
                    continue;

                if (string.Equals(Path.GetFileName(packageName), Path.GetFileName(line.Split(new[] { ' ', '\t', '*' }, StringSplitOptions.RemoveEmptyEntries).LastOrDefault()), StringComparison.OrdinalIgnoreCase) ||
                    content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length == 1)
                {
                    return match.Groups[1].Value;
                }
            }

            return null;
        }

        private static string ExtractSha256(string digest)
        {
            Match match = Regex.Match(digest ?? string.Empty, "(?i)^(?:sha256:)?([a-f0-9]{64})$");
            return match.Success ? match.Groups[1].Value : null;
        }

        private static async Task<string> CalculateSha256Async(string path, CancellationToken cancellationToken)
        {
            await using FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, useAsync: true);
            byte[] hash = await SHA256.HashDataAsync(stream, cancellationToken);
            return Convert.ToHexString(hash);
        }

        private static void EnsureDirectoryIsWritable(string directory)
        {
            try
            {
                string probePath = Path.Combine(directory, ".rdg-arbodv-update-" + Guid.NewGuid().ToString("N") + ".tmp");
                using (new FileStream(probePath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
                {
                }
                File.Delete(probePath);
            }
            catch (Exception ex)
            {
                throw new UpdateException("The application folder is not writable. Move RDG ArboDV to a writable folder, then retry the update.", ex);
            }
        }

        private static string FindStagedExecutable(string stagingDirectory, string currentExecutableName)
        {
            string expectedPath = Directory.GetFiles(stagingDirectory, currentExecutableName, SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(expectedPath))
                return expectedPath;

            string brandedPath = Directory.GetFiles(stagingDirectory, "RDG-ArboDV.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (!string.IsNullOrEmpty(brandedPath))
                return brandedPath;

            string[] executables = Directory.GetFiles(stagingDirectory, "*.exe", SearchOption.AllDirectories);
            return executables.Length == 1 ? executables[0] : null;
        }

        private static string BuildUpdateScript(int processId, string stagedExecutable, string applicationPath)
        {
            return $"@echo off{Environment.NewLine}" +
                   "setlocal enableextensions" + Environment.NewLine +
                   $"powershell.exe -NoProfile -ExecutionPolicy Bypass -Command \"Get-Process -Id {processId} -ErrorAction SilentlyContinue ^| Wait-Process\"" + Environment.NewLine +
                   "timeout /t 1 /nobreak >nul" + Environment.NewLine +
                   $"copy /Y \"{stagedExecutable}\" \"{applicationPath}\" >nul" + Environment.NewLine +
                   "if errorlevel 1 (" + Environment.NewLine +
                   $"  start \"\" \"{stagedExecutable}\"" + Environment.NewLine +
                   "  exit /b 1" + Environment.NewLine +
                   ")" + Environment.NewLine +
                   $"start \"\" \"{applicationPath}\"" + Environment.NewLine +
                   "del \"%~f0\"" + Environment.NewLine;
        }

        public void Dispose()
        {
            if (_disposeHttpClient)
                _httpClient.Dispose();
        }
    }

    internal sealed class UpdateCheckResult
    {
        private UpdateCheckResult(ReleaseVersion currentVersion, UpdateRelease release, string errorMessage)
        {
            CurrentVersion = currentVersion;
            Release = release;
            ErrorMessage = errorMessage;
        }

        public ReleaseVersion CurrentVersion { get; }
        public UpdateRelease Release { get; }
        public string ErrorMessage { get; }
        public bool IsUpdateAvailable => Release != null;
        public bool IsSuccess => string.IsNullOrEmpty(ErrorMessage);

        public static UpdateCheckResult UpToDate(ReleaseVersion currentVersion) => new UpdateCheckResult(currentVersion, null, null);
        public static UpdateCheckResult UpdateAvailable(ReleaseVersion currentVersion, UpdateRelease release) => new UpdateCheckResult(currentVersion, release, null);
        public static UpdateCheckResult Failed(string errorMessage, string ignoredTag = null) => new UpdateCheckResult(default, null, errorMessage);
    }

    internal sealed class UpdateRelease
    {
        public UpdateRelease(string tagName, ReleaseVersion version, string name, string notes, string releasePageUrl, string packageName, string downloadUrl, string sha256, bool prerelease)
        {
            TagName = tagName;
            Version = version;
            Name = name;
            Notes = notes ?? string.Empty;
            ReleasePageUrl = releasePageUrl;
            PackageName = packageName;
            DownloadUrl = downloadUrl;
            Sha256 = sha256;
            Prerelease = prerelease;
        }

        public string TagName { get; }
        public ReleaseVersion Version { get; }
        public string Name { get; }
        public string Notes { get; }
        public string ReleasePageUrl { get; }
        public string PackageName { get; }
        public string DownloadUrl { get; }
        public string Sha256 { get; }
        public bool Prerelease { get; }
    }

    internal sealed class UpdateException : Exception
    {
        public UpdateException(string message) : base(message)
        {
        }

        public UpdateException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }

    internal readonly struct ReleaseVersion : IComparable<ReleaseVersion>
    {
        public ReleaseVersion(int major, int minor, int patch, string prerelease)
        {
            Major = major;
            Minor = minor;
            Patch = patch;
            Prerelease = prerelease;
        }

        public int Major { get; }
        public int Minor { get; }
        public int Patch { get; }
        public string Prerelease { get; }

        public static bool TryParse(string value, out ReleaseVersion version)
        {
            Match match = Regex.Match(value ?? string.Empty, "(?i)^v?(\\d+)\\.(\\d+)\\.(\\d+)(?:-([0-9a-z.-]+))?(?:\\+[0-9a-z.-]+)?$");
            if (!match.Success ||
                !int.TryParse(match.Groups[1].Value, out int major) ||
                !int.TryParse(match.Groups[2].Value, out int minor) ||
                !int.TryParse(match.Groups[3].Value, out int patch))
            {
                version = default;
                return false;
            }

            version = new ReleaseVersion(major, minor, patch, match.Groups[4].Success ? match.Groups[4].Value : null);
            return true;
        }

        public int CompareTo(ReleaseVersion other)
        {
            int comparison = Major.CompareTo(other.Major);
            if (comparison != 0) return comparison;
            comparison = Minor.CompareTo(other.Minor);
            if (comparison != 0) return comparison;
            comparison = Patch.CompareTo(other.Patch);
            if (comparison != 0) return comparison;

            bool thisIsStable = string.IsNullOrEmpty(Prerelease);
            bool otherIsStable = string.IsNullOrEmpty(other.Prerelease);
            if (thisIsStable != otherIsStable)
                return thisIsStable ? 1 : -1;
            if (thisIsStable)
                return 0;

            string[] thisParts = Prerelease.Split('.');
            string[] otherParts = other.Prerelease.Split('.');
            int count = Math.Min(thisParts.Length, otherParts.Length);
            for (int index = 0; index < count; index++)
            {
                bool thisNumber = int.TryParse(thisParts[index], out int thisValue);
                bool otherNumber = int.TryParse(otherParts[index], out int otherValue);
                if (thisNumber && otherNumber)
                {
                    comparison = thisValue.CompareTo(otherValue);
                }
                else if (thisNumber != otherNumber)
                {
                    comparison = thisNumber ? -1 : 1;
                }
                else
                {
                    comparison = string.Compare(thisParts[index], otherParts[index], StringComparison.OrdinalIgnoreCase);
                }

                if (comparison != 0)
                    return comparison;
            }

            return thisParts.Length.CompareTo(otherParts.Length);
        }

        public override string ToString() => $"{Major}.{Minor}.{Patch}" + (string.IsNullOrEmpty(Prerelease) ? string.Empty : "-" + Prerelease);
    }

    internal sealed class GitHubRelease
    {
        [JsonProperty("tag_name")]
        public string TagName { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("body")]
        public string Body { get; set; }

        [JsonProperty("html_url")]
        public string HtmlUrl { get; set; }

        [JsonProperty("draft")]
        public bool Draft { get; set; }

        [JsonProperty("prerelease")]
        public bool Prerelease { get; set; }

        [JsonProperty("assets")]
        public List<GitHubAsset> Assets { get; set; } = new List<GitHubAsset>();
    }

    internal sealed class GitHubAsset
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("browser_download_url")]
        public string BrowserDownloadUrl { get; set; }

        [JsonProperty("digest")]
        public string Digest { get; set; }
    }
}
