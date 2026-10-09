using System.Globalization;

namespace SchoolManagementSystem.Api.Services
{
    public sealed class ActivityFileStorage
    {
        private readonly string _rootPath;
        private readonly ILogger<ActivityFileStorage> _logger;

        public ActivityFileStorage(
            IWebHostEnvironment environment,
            IConfiguration configuration,
            ILogger<ActivityFileStorage> logger)
        {
            _logger = logger;
            var configuredPath = configuration["ActivityStorage:RootPath"];
            var rootPath = string.IsNullOrWhiteSpace(configuredPath)
                ? Path.Combine("App_Data", "ActivityMaterials")
                : configuredPath;

            _rootPath = Path.GetFullPath(rootPath, environment.ContentRootPath);

            var webRootPath = Path.GetFullPath(
                environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"));
            if (IsSameOrInside(_rootPath, webRootPath))
            {
                throw new InvalidOperationException(
                    "Activity storage must be outside the web-served directory.");
            }
        }

        public async Task<string> SaveAsync(int activityId, IFormFile file, CancellationToken cancellationToken)
        {
            var fileId = Guid.NewGuid();
            var storageKey = $"{activityId.ToString(CultureInfo.InvariantCulture)}/{fileId:N}";
            var filePath = ResolvePath(activityId, storageKey, createActivityDirectory: true);

            try
            {
                await using var output = new FileStream(
                    filePath,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    bufferSize: 81920,
                    useAsync: true);

                await file.CopyToAsync(output, cancellationToken);
                return storageKey;
            }
            catch
            {
                try
                {
                    Delete(activityId, storageKey);
                }
                catch (Exception cleanupException)
                {
                    try
                    {
                        _logger.LogError(cleanupException,
                            "Failed to clean up partially written activity file for activity {ActivityId}.",
                            activityId);
                    }
                    catch
                    {
                        // Preserve the original write/copy exception if a logging provider also fails.
                    }
                }

                throw;
            }
        }

        public FileStream? OpenRead(int activityId, string? storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                return null;

            string filePath;
            try
            {
                filePath = ResolvePath(activityId, storageKey, createActivityDirectory: false);
            }
            catch (InvalidDataException)
            {
                return null;
            }

            if (!File.Exists(filePath))
                return null;

            EnsureNotReparsePoint(filePath);
            return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Delete(int activityId, string? storageKey)
        {
            if (string.IsNullOrWhiteSpace(storageKey))
                return;

            var filePath = ResolvePath(activityId, storageKey, createActivityDirectory: false);
            if (!File.Exists(filePath))
                return;

            EnsureNotReparsePoint(filePath);
            File.Delete(filePath);
        }

        private string ResolvePath(int activityId, string storageKey, bool createActivityDirectory)
        {
            var parts = storageKey.Split('/');
            if (activityId <= 0 ||
                parts.Length != 2 ||
                !string.Equals(parts[0], activityId.ToString(CultureInfo.InvariantCulture), StringComparison.Ordinal) ||
                !Guid.TryParseExact(parts[1], "N", out var fileId))
            {
                throw new InvalidDataException("The activity file storage key is invalid.");
            }

            var activityDirectory = Path.GetFullPath(Path.Combine(
                _rootPath,
                activityId.ToString(CultureInfo.InvariantCulture)));
            var filePath = Path.GetFullPath(Path.Combine(activityDirectory, fileId.ToString("N")));
            if (!IsSameOrInside(activityDirectory, _rootPath) ||
                !IsSameOrInside(filePath, activityDirectory))
            {
                throw new InvalidDataException("The activity file path is outside its storage directory.");
            }

            if (createActivityDirectory)
                Directory.CreateDirectory(activityDirectory);

            if (Directory.Exists(activityDirectory))
                EnsureNotReparsePoint(activityDirectory);

            return filePath;
        }

        private static void EnsureNotReparsePoint(string path)
        {
            if ((File.GetAttributes(path) & FileAttributes.ReparsePoint) != 0)
                throw new InvalidDataException("Activity storage cannot use reparse points.");
        }

        private static bool IsSameOrInside(string candidatePath, string parentPath)
        {
            var relativePath = Path.GetRelativePath(parentPath, candidatePath);
            return relativePath == "." ||
                (!Path.IsPathRooted(relativePath) &&
                 relativePath != ".." &&
                 !relativePath.StartsWith(".." + Path.DirectorySeparatorChar, StringComparison.Ordinal) &&
                 !relativePath.StartsWith(".." + Path.AltDirectorySeparatorChar, StringComparison.Ordinal));
        }
    }
}
