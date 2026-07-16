using System;
using System.Collections.Generic;
using System.IO;

namespace HVACDesigner.EngineeringData.Abstractions
{
    /// <summary>
    /// Egy megnyitott tartalomkészlet technológiafüggetlen olvasási fogantyúja.
    /// A tulajdonában lévő Stream lezárása a példány Dispose metódusának feladata.
    /// </summary>
    public sealed class DataPackageContent : IDisposable
    {
        private bool _disposed;

        public string PackageId { get; }
        public string ContentSetId { get; }
        public string MediaType { get; }
        public Stream Stream { get; }
        public IReadOnlyDictionary<string, string> Metadata { get; }

        public DataPackageContent(
            string packageId,
            string contentSetId,
            Stream stream,
            string mediaType = "application/octet-stream",
            IReadOnlyDictionary<string, string>? metadata = null)
        {
            if (string.IsNullOrWhiteSpace(packageId))
                throw new ArgumentException(
                    "A csomagazonosító nem lehet üres.",
                    nameof(packageId));

            if (string.IsNullOrWhiteSpace(contentSetId))
                throw new ArgumentException(
                    "A tartalomkészlet-azonosító nem lehet üres.",
                    nameof(contentSetId));

            Stream =
                stream ??
                throw new ArgumentNullException(nameof(stream));

            if (!stream.CanRead)
            {
                throw new ArgumentException(
                    "A megadott adatfolyam nem olvasható.",
                    nameof(stream));
            }

            PackageId = packageId.Trim();
            ContentSetId = contentSetId.Trim();
            MediaType =
                string.IsNullOrWhiteSpace(mediaType)
                    ? "application/octet-stream"
                    : mediaType.Trim();

            Metadata =
                metadata ??
                new Dictionary<string, string>();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stream.Dispose();
            _disposed = true;
        }
    }

    /// <summary>
    /// XML-, JSON-, SQL-, API- vagy más adatforrás egységes szerződése.
    /// A konkrét forrástechnológia nem szivároghat ki az importáló rétegbe.
    /// </summary>
    public interface IDataPackageSource
    {
        DataPackageSourceType SourceType { get; }

        bool PackageExists(
            EngineeringDataPackageManifest manifest);

        bool ContentSetExists(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor);

        DataPackageContent OpenContentSet(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor);

        IReadOnlyDictionary<string, string> GetPackageMetadata(
            EngineeringDataPackageManifest manifest);
    }
}
