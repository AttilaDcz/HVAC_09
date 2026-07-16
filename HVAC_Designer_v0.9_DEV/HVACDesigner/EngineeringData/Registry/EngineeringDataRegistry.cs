using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using HVACDesigner.EngineeringData.Abstractions;

namespace HVACDesigner.EngineeringData.Registry
{
    public sealed class EngineeringDataRegistration
    {
        public string PackageId { get; }
        public string ContentSetId { get; }
        public string Version { get; }
        public EngineeringContentKind ContentKind { get; }
        public Type RecordType { get; }
        public object Value { get; }

        public EngineeringDataRegistration(
            string packageId,
            string contentSetId,
            string version,
            EngineeringContentKind contentKind,
            Type recordType,
            object value)
        {
            PackageId = RequireText(packageId, nameof(packageId));
            ContentSetId = RequireText(
                contentSetId,
                nameof(contentSetId));
            Version = RequireText(version, nameof(version));
            ContentKind = contentKind;
            RecordType =
                recordType ??
                throw new ArgumentNullException(nameof(recordType));
            Value =
                value ??
                throw new ArgumentNullException(nameof(value));
        }

        private static string RequireText(
            string? value,
            string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);

            return value.Trim();
        }
    }

    /// <summary>
    /// Betöltött típusos mérnöki adatkészletek központi registryje.
    /// Nem tölt fájlt, nem importál és nem választ automatikusan
    /// újabb szabályverziót egy régi projekt helyett.
    /// </summary>
    public sealed class EngineeringDataRegistry
    {
        private readonly object _syncRoot = new object();

        private readonly Dictionary<RegistryKey, EngineeringDataRegistration>
            _registrations =
                new Dictionary<RegistryKey, EngineeringDataRegistration>();

        public void Register<T>(
            string packageId,
            string contentSetId,
            string version,
            EngineeringContentKind contentKind,
            T value)
            where T : class
        {
            if (value == null)
                throw new ArgumentNullException(nameof(value));

            var registration =
                new EngineeringDataRegistration(
                    packageId,
                    contentSetId,
                    version,
                    contentKind,
                    typeof(T),
                    value);

            var key =
                new RegistryKey(
                    contentSetId,
                    version,
                    typeof(T));

            lock (_syncRoot)
            {
                if (_registrations.ContainsKey(key))
                {
                    throw new InvalidOperationException(
                        "Az adatkészlet már regisztrálva van: " +
                        $"{contentSetId}, verzió: {version}, " +
                        $"típus: {typeof(T).FullName}.");
                }

                _registrations.Add(key, registration);
            }
        }

        public bool TryGet<T>(
            string contentSetId,
            string version,
            out T? value)
            where T : class
        {
            var key =
                new RegistryKey(
                    contentSetId,
                    version,
                    typeof(T));

            lock (_syncRoot)
            {
                if (_registrations.TryGetValue(
                    key,
                    out EngineeringDataRegistration? registration))
                {
                    value = (T)registration.Value;
                    return true;
                }
            }

            value = null;
            return false;
        }

        public T GetRequired<T>(
            string contentSetId,
            string version)
            where T : class
        {
            if (TryGet<T>(
                contentSetId,
                version,
                out T? value))
            {
                return value!;
            }

            throw new KeyNotFoundException(
                "A kért mérnöki adatkészlet nem érhető el: " +
                $"{contentSetId}, verzió: {version}, " +
                $"típus: {typeof(T).FullName}.");
        }

        public bool IsRegistered<T>(
            string contentSetId,
            string version)
            where T : class
        {
            return TryGet<T>(
                contentSetId,
                version,
                out _);
        }

        public IReadOnlyList<string> GetVersions<T>(
            string contentSetId)
            where T : class
        {
            lock (_syncRoot)
            {
                return new ReadOnlyCollection<string>(
                    _registrations
                    .Where(pair =>
                        string.Equals(
                            pair.Key.ContentSetId,
                            contentSetId,
                            StringComparison.OrdinalIgnoreCase) &&
                        pair.Key.RecordType == typeof(T))
                    .Select(pair => pair.Key.Version)
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .OrderBy(item => item, StringComparer.OrdinalIgnoreCase)
                    .ToList());
            }
        }

        public IReadOnlyList<EngineeringDataRegistration>
            GetRegistrations()
        {
            lock (_syncRoot)
            {
                return new ReadOnlyCollection<EngineeringDataRegistration>(
                    _registrations.Values.ToList());
            }
        }

        public bool Remove<T>(
            string contentSetId,
            string version)
            where T : class
        {
            var key =
                new RegistryKey(
                    contentSetId,
                    version,
                    typeof(T));

            lock (_syncRoot)
            {
                return _registrations.Remove(key);
            }
        }

        private sealed class RegistryKey :
            IEquatable<RegistryKey>
        {
            public string ContentSetId { get; }
            public string Version { get; }
            public Type RecordType { get; }

            public RegistryKey(
                string contentSetId,
                string version,
                Type recordType)
            {
                if (string.IsNullOrWhiteSpace(contentSetId))
                    throw new ArgumentException(
                        "A tartalomkészlet-azonosító nem lehet üres.",
                        nameof(contentSetId));

                if (string.IsNullOrWhiteSpace(version))
                    throw new ArgumentException(
                        "A verzió nem lehet üres.",
                        nameof(version));

                ContentSetId = contentSetId.Trim();
                Version = version.Trim();
                RecordType =
                    recordType ??
                    throw new ArgumentNullException(nameof(recordType));
            }

            public bool Equals(RegistryKey? other)
            {
                if (ReferenceEquals(null, other))
                    return false;

                if (ReferenceEquals(this, other))
                    return true;

                return string.Equals(
                           ContentSetId,
                           other.ContentSetId,
                           StringComparison.OrdinalIgnoreCase) &&
                       string.Equals(
                           Version,
                           other.Version,
                           StringComparison.OrdinalIgnoreCase) &&
                       RecordType == other.RecordType;
            }

            public override bool Equals(object? obj)
            {
                return Equals(obj as RegistryKey);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hash = 17;

                    hash =
                        hash * 31 +
                        StringComparer.OrdinalIgnoreCase
                            .GetHashCode(ContentSetId);

                    hash =
                        hash * 31 +
                        StringComparer.OrdinalIgnoreCase
                            .GetHashCode(Version);

                    hash =
                        hash * 31 +
                        RecordType.GetHashCode();

                    return hash;
                }
            }
        }
    }
}
