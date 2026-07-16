using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Importing
{
    public enum ImportDiagnosticSeverity
    {
        Information,
        Warning,
        Error
    }

    public enum ImportFailureScope
    {
        None,
        Property,
        Record,
        ContentSet,
        Package
    }

    public sealed class ImportDiagnostic
    {
        public ImportDiagnosticSeverity Severity { get; }
        public ImportFailureScope FailureScope { get; }
        public string Code { get; }
        public string Message { get; }
        public string ContentSetId { get; }
        public string RecordId { get; }
        public string PropertyName { get; }
        public Exception? Exception { get; }

        public ImportDiagnostic(
            ImportDiagnosticSeverity severity,
            ImportFailureScope failureScope,
            string code,
            string message,
            string contentSetId = "",
            string recordId = "",
            string propertyName = "",
            Exception? exception = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException(
                    "A diagnosztikai kód nem lehet üres.",
                    nameof(code));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException(
                    "A diagnosztikai üzenet nem lehet üres.",
                    nameof(message));

            Severity = severity;
            FailureScope = failureScope;
            Code = code.Trim();
            Message = message.Trim();
            ContentSetId = contentSetId?.Trim() ?? string.Empty;
            RecordId = recordId?.Trim() ?? string.Empty;
            PropertyName = propertyName?.Trim() ?? string.Empty;
            Exception = exception;
        }
    }

    /// <summary>
    /// Egyetlen tartalomkészlet importeredménye.
    /// </summary>
    public sealed class ContentSetImportResult
    {
        private readonly ReadOnlyCollection<ImportDiagnostic> _diagnostics;

        public string ContentSetId { get; }
        public int ImportedCount { get; }
        public int SkippedCount { get; }
        public bool IsAvailable { get; }
        public IReadOnlyList<ImportDiagnostic> Diagnostics => _diagnostics;

        public bool HasWarnings =>
            _diagnostics.Any(
                item => item.Severity ==
                        ImportDiagnosticSeverity.Warning);

        public bool HasErrors =>
            _diagnostics.Any(
                item => item.Severity ==
                        ImportDiagnosticSeverity.Error);

        public bool Succeeded =>
            IsAvailable && !HasContentSetOrPackageError();

        public ContentSetImportResult(
            string contentSetId,
            int importedCount,
            int skippedCount,
            bool isAvailable,
            IEnumerable<ImportDiagnostic>? diagnostics = null)
        {
            if (string.IsNullOrWhiteSpace(contentSetId))
                throw new ArgumentException(
                    "A tartalomkészlet-azonosító nem lehet üres.",
                    nameof(contentSetId));

            if (importedCount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(importedCount));

            if (skippedCount < 0)
                throw new ArgumentOutOfRangeException(
                    nameof(skippedCount));

            ContentSetId = contentSetId.Trim();
            ImportedCount = importedCount;
            SkippedCount = skippedCount;
            IsAvailable = isAvailable;

            _diagnostics =
                new ReadOnlyCollection<ImportDiagnostic>(
                    (diagnostics ??
                     Enumerable.Empty<ImportDiagnostic>())
                    .ToList());
        }

        private bool HasContentSetOrPackageError()
        {
            return _diagnostics.Any(
                item =>
                    item.Severity ==
                    ImportDiagnosticSeverity.Error &&
                    (item.FailureScope ==
                     ImportFailureScope.ContentSet ||
                     item.FailureScope ==
                     ImportFailureScope.Package));
        }
    }

    /// <summary>
    /// Egy teljes adatcsomag importeredménye.
    /// </summary>
    public sealed class DataPackageImportResult
    {
        private readonly ReadOnlyCollection<ContentSetImportResult>
            _contentSetResults;
        private readonly ReadOnlyCollection<ImportDiagnostic>
            _packageDiagnostics;

        public string PackageId { get; }
        public string PackageVersion { get; }

        public IReadOnlyList<ContentSetImportResult>
            ContentSetResults => _contentSetResults;

        public IReadOnlyList<ImportDiagnostic>
            PackageDiagnostics => _packageDiagnostics;

        public int ImportedCount =>
            _contentSetResults.Sum(item => item.ImportedCount);

        public int SkippedCount =>
            _contentSetResults.Sum(item => item.SkippedCount);

        public bool HasWarnings =>
            _packageDiagnostics.Any(
                item => item.Severity ==
                        ImportDiagnosticSeverity.Warning) ||
            _contentSetResults.Any(item => item.HasWarnings);

        public bool HasErrors =>
            _packageDiagnostics.Any(
                item => item.Severity ==
                        ImportDiagnosticSeverity.Error) ||
            _contentSetResults.Any(item => item.HasErrors);

        /// <summary>
        /// Igaz, ha a csomagszint nem hibázott, és legalább egy
        /// tartalomkészlet elérhetővé vált.
        /// </summary>
        public bool Succeeded =>
            !_packageDiagnostics.Any(
                item =>
                    item.Severity ==
                    ImportDiagnosticSeverity.Error &&
                    item.FailureScope ==
                    ImportFailureScope.Package) &&
            _contentSetResults.Any(item => item.IsAvailable);

        public bool IsPartialSuccess =>
            Succeeded &&
            (_contentSetResults.Any(item => !item.IsAvailable) ||
             HasErrors ||
             HasWarnings);

        public DataPackageImportResult(
            string packageId,
            string packageVersion,
            IEnumerable<ContentSetImportResult> contentSetResults,
            IEnumerable<ImportDiagnostic>? packageDiagnostics = null)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentException(
                    "A csomagazonosító nem lehet üres.",
                    nameof(packageId));

            if (string.IsNullOrWhiteSpace(packageVersion))
                throw new ArgumentException(
                    "A csomagverzió nem lehet üres.",
                    nameof(packageVersion));

            PackageId = packageId.Trim();
            PackageVersion = packageVersion.Trim();

            _contentSetResults =
                new ReadOnlyCollection<ContentSetImportResult>(
                    (contentSetResults ??
                     throw new ArgumentNullException(
                         nameof(contentSetResults)))
                    .ToList());

            _packageDiagnostics =
                new ReadOnlyCollection<ImportDiagnostic>(
                    (packageDiagnostics ??
                     Enumerable.Empty<ImportDiagnostic>())
                    .ToList());
        }

        public bool TryGetContentSetResult(
            string contentSetId,
            out ContentSetImportResult? result)
        {
            result = _contentSetResults.FirstOrDefault(
                item => string.Equals(
                    item.ContentSetId,
                    contentSetId,
                    StringComparison.OrdinalIgnoreCase));

            return result != null;
        }
    }
}
