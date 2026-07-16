using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml.Linq;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.Infrastructure.Xml
{
    /// <summary>
    /// Fájlrendszeri XML adatcsomagforrás.
    ///
    /// A manifest SourceLocation értéke lehet egy XML-fájl vagy egy könyvtár.
    /// Ha könyvtár, a ContentSetDescriptor SourcePath első része adja a fájl nevét.
    /// Példa: ductdata.xml|/DuctDatabase/Materials
    /// </summary>
    public sealed class XmlDataPackageSource : IDataPackageSource
    {
        public DataPackageSourceType SourceType =>
            DataPackageSourceType.Xml;

        public bool PackageExists(
            EngineeringDataPackageManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (File.Exists(manifest.SourceLocation))
                return true;

            return Directory.Exists(manifest.SourceLocation);
        }

        public bool ContentSetExists(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            string filePath =
                ResolveFilePath(manifest, descriptor);

            if (!File.Exists(filePath))
                return false;

            string xmlPath =
                GetXmlPath(descriptor.SourcePath);

            if (string.IsNullOrWhiteSpace(xmlPath))
                return true;

            try
            {
                XDocument document =
                    XDocument.Load(
                        filePath,
                        LoadOptions.None);

                return FindElement(
                    document,
                    xmlPath) != null;
            }
            catch
            {
                return false;
            }
        }

        public DataPackageContent OpenContentSet(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            if (descriptor == null)
                throw new ArgumentNullException(nameof(descriptor));

            string filePath =
                ResolveFilePath(manifest, descriptor);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException(
                    "Az XML adatforrás nem található.",
                    filePath);
            }

            var metadata =
                new ReadOnlyDictionary<string, string>(
                    new Dictionary<string, string>
                    {
                        ["FilePath"] = filePath,
                        ["XmlPath"] =
                            GetXmlPath(descriptor.SourcePath),
                        ["LastWriteTimeUtc"] =
                            File.GetLastWriteTimeUtc(filePath)
                                .ToString("O")
                    });

            return new DataPackageContent(
                manifest.PackageId,
                descriptor.ContentSetId,
                new FileStream(
                    filePath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read),
                "application/xml",
                metadata);
        }

        public IReadOnlyDictionary<string, string>
            GetPackageMetadata(
                EngineeringDataPackageManifest manifest)
        {
            if (manifest == null)
                throw new ArgumentNullException(nameof(manifest));

            var values =
                new Dictionary<string, string>
                {
                    ["SourceType"] = SourceType.ToString(),
                    ["SourceLocation"] =
                        manifest.SourceLocation ?? string.Empty
                };

            if (File.Exists(manifest.SourceLocation))
            {
                values["LastWriteTimeUtc"] =
                    File.GetLastWriteTimeUtc(
                        manifest.SourceLocation)
                    .ToString("O");
            }

            return new ReadOnlyDictionary<string, string>(
                values);
        }

        public static XElement FindElement(
            XDocument document,
            string xmlPath)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            if (document.Root == null)
                return null;

            if (string.IsNullOrWhiteSpace(xmlPath) ||
                xmlPath == "/")
            {
                return document.Root;
            }

            string[] parts =
                xmlPath.Trim()
                    .Trim('/')
                    .Split(
                        new[] { '/' },
                        StringSplitOptions.RemoveEmptyEntries);

            XElement current = document.Root;
            int startIndex = 0;

            if (parts.Length > 0 &&
                string.Equals(
                    parts[0],
                    current.Name.LocalName,
                    StringComparison.OrdinalIgnoreCase))
            {
                startIndex = 1;
            }

            for (int index = startIndex;
                 index < parts.Length;
                 index++)
            {
                XElement next = null;

                foreach (XElement child in current.Elements())
                {
                    if (string.Equals(
                        child.Name.LocalName,
                        parts[index],
                        StringComparison.OrdinalIgnoreCase))
                    {
                        next = child;
                        break;
                    }
                }

                if (next == null)
                    return null;

                current = next;
            }

            return current;
        }

        private static string ResolveFilePath(
            EngineeringDataPackageManifest manifest,
            ContentSetDescriptor descriptor)
        {
            if (File.Exists(manifest.SourceLocation))
                return Path.GetFullPath(manifest.SourceLocation);

            string sourceFile =
                GetSourceFile(descriptor.SourcePath);

            if (string.IsNullOrWhiteSpace(sourceFile))
            {
                throw new InvalidOperationException(
                    "Könyvtár alapú XML-forrásnál a SourcePath " +
                    "első részében meg kell adni a fájl nevét.");
            }

            return Path.GetFullPath(
                Path.Combine(
                    manifest.SourceLocation,
                    sourceFile));
        }

        private static string GetSourceFile(
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return string.Empty;

            int separatorIndex =
                sourcePath.IndexOf('|');

            if (separatorIndex < 0)
                return string.Empty;

            return sourcePath
                .Substring(0, separatorIndex)
                .Trim();
        }

        public static string GetXmlPath(
            string sourcePath)
        {
            if (string.IsNullOrWhiteSpace(sourcePath))
                return string.Empty;

            int separatorIndex =
                sourcePath.IndexOf('|');

            if (separatorIndex < 0)
                return sourcePath.Trim();

            return sourcePath
                .Substring(separatorIndex + 1)
                .Trim();
        }
    }
}
