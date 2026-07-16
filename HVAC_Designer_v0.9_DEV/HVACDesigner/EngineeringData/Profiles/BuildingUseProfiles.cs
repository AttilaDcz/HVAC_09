using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace HVACDesigner.EngineeringData.Profiles
{
    public enum ProfileResolutionMode
    {
        BuildingLevel,
        SpaceLevel,
        Mixed
    }

    public enum ProfileValueSource
    {
        Regulation,
        Standard,
        EngineeringRecommendation,
        ProjectDefault,
        UserDefined,
        Estimated
    }

    public enum ProfileRequirementLevel
    {
        Mandatory,
        Recommended,
        Default,
        Estimated
    }

    public sealed class ProfileValue
    {
        public string Value { get; }
        public string Unit { get; }
        public ProfileValueSource Source { get; }
        public ProfileRequirementLevel RequirementLevel { get; }
        public string Reference { get; }

        public ProfileValue(
            string value,
            string unit,
            ProfileValueSource source,
            ProfileRequirementLevel requirementLevel,
            string reference = "")
        {
            Value = value?.Trim() ?? string.Empty;
            Unit = unit?.Trim() ?? string.Empty;
            Source = source;
            RequirementLevel = requirementLevel;
            Reference = reference?.Trim() ?? string.Empty;
        }
    }

    public sealed class ProfileSection
    {
        private readonly ReadOnlyDictionary<string, ProfileValue> _values;
        public IReadOnlyDictionary<string, ProfileValue> Values => _values;

        public ProfileSection(IDictionary<string, ProfileValue> values)
        {
            var copy = new Dictionary<string, ProfileValue>(
                StringComparer.OrdinalIgnoreCase);

            if (values != null)
            {
                foreach (KeyValuePair<string, ProfileValue> pair in values)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        throw new ArgumentException(
                            "A profilmező neve nem lehet üres.",
                            nameof(values));

                    copy[pair.Key.Trim()] = pair.Value ??
                        throw new ArgumentException(
                            "A profilérték nem lehet null.",
                            nameof(values));
                }
            }

            _values = new ReadOnlyDictionary<string, ProfileValue>(copy);
        }

        public bool TryGet(string key, out ProfileValue value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }

            return _values.TryGetValue(key.Trim(), out value);
        }
    }

    public sealed class BuildingUseProfile
    {
        private readonly ReadOnlyDictionary<string, ProfileSection> _sections;
        private readonly ReadOnlyCollection<string> _spaceProfileIds;

        public string Id { get; }
        public string Version { get; }
        public string Name { get; }
        public string ParentProfileId { get; }
        public string FallbackProfileId { get; }
        public IReadOnlyDictionary<string, ProfileSection> Sections => _sections;
        public IReadOnlyList<string> SpaceProfileIds => _spaceProfileIds;

        public BuildingUseProfile(
            string id,
            string version,
            string name,
            string parentProfileId,
            string fallbackProfileId,
            IDictionary<string, ProfileSection> sections,
            IEnumerable<string> spaceProfileIds = null)
        {
            Id = RequireText(id, nameof(id));
            Version = RequireText(version, nameof(version));
            Name = RequireText(name, nameof(name));
            ParentProfileId = parentProfileId?.Trim() ?? string.Empty;
            FallbackProfileId = fallbackProfileId?.Trim() ?? string.Empty;

            _sections = new ReadOnlyDictionary<string, ProfileSection>(
                new Dictionary<string, ProfileSection>(
                    sections ?? new Dictionary<string, ProfileSection>(),
                    StringComparer.OrdinalIgnoreCase));

            _spaceProfileIds = new ReadOnlyCollection<string>(
                (spaceProfileIds ?? Enumerable.Empty<string>())
                .Where(item => !string.IsNullOrWhiteSpace(item))
                .Select(item => item.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList());
        }

        public string GetVersionedKey() => Id + "@" + Version;

        public bool TryGetSection(string sectionName, out ProfileSection section)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                section = null;
                return false;
            }

            return _sections.TryGetValue(sectionName.Trim(), out section);
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);

            return value.Trim();
        }
    }

    public sealed class SpaceUseProfile
    {
        private readonly ReadOnlyDictionary<string, ProfileSection> _sections;

        public string Id { get; }
        public string Version { get; }
        public string Name { get; }
        public string BuildingProfileId { get; }
        public string ParentSpaceProfileId { get; }
        public string FallbackProfileId { get; }
        public IReadOnlyDictionary<string, ProfileSection> Sections => _sections;

        public SpaceUseProfile(
            string id,
            string version,
            string name,
            string buildingProfileId,
            string parentSpaceProfileId,
            string fallbackProfileId,
            IDictionary<string, ProfileSection> sections)
        {
            Id = RequireText(id, nameof(id));
            Version = RequireText(version, nameof(version));
            Name = RequireText(name, nameof(name));
            BuildingProfileId = buildingProfileId?.Trim() ?? string.Empty;
            ParentSpaceProfileId = parentSpaceProfileId?.Trim() ?? string.Empty;
            FallbackProfileId = fallbackProfileId?.Trim() ?? string.Empty;

            _sections = new ReadOnlyDictionary<string, ProfileSection>(
                new Dictionary<string, ProfileSection>(
                    sections ?? new Dictionary<string, ProfileSection>(),
                    StringComparer.OrdinalIgnoreCase));
        }

        public string GetVersionedKey() => Id + "@" + Version;

        public bool TryGetSection(string sectionName, out ProfileSection section)
        {
            if (string.IsNullOrWhiteSpace(sectionName))
            {
                section = null;
                return false;
            }

            return _sections.TryGetValue(sectionName.Trim(), out section);
        }

        private static string RequireText(string value, string parameterName)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException(
                    "Az érték nem lehet üres.",
                    parameterName);

            return value.Trim();
        }
    }

    public sealed class BuildingUseProfileCatalog
    {
        private readonly Dictionary<string, BuildingUseProfile> _buildings =
            new Dictionary<string, BuildingUseProfile>(
                StringComparer.OrdinalIgnoreCase);

        private readonly Dictionary<string, SpaceUseProfile> _spaces =
            new Dictionary<string, SpaceUseProfile>(
                StringComparer.OrdinalIgnoreCase);

        public void Register(BuildingUseProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string key = profile.GetVersionedKey();

            if (_buildings.ContainsKey(key))
                throw new InvalidOperationException(
                    "Az épületprofil már létezik: " + key + ".");

            _buildings.Add(key, profile);
        }

        public void Register(SpaceUseProfile profile)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            string key = profile.GetVersionedKey();

            if (_spaces.ContainsKey(key))
                throw new InvalidOperationException(
                    "A helyiségprofil már létezik: " + key + ".");

            _spaces.Add(key, profile);
        }

        public bool TryGetBuilding(
            string id,
            string version,
            out BuildingUseProfile profile) =>
            _buildings.TryGetValue(CreateKey(id, version), out profile);

        public bool TryGetSpace(
            string id,
            string version,
            out SpaceUseProfile profile) =>
            _spaces.TryGetValue(CreateKey(id, version), out profile);

        private static string CreateKey(string id, string version)
        {
            if (string.IsNullOrWhiteSpace(id) ||
                string.IsNullOrWhiteSpace(version))
                throw new ArgumentException("Az id és version kötelező.");

            return id.Trim() + "@" + version.Trim();
        }
    }
}
