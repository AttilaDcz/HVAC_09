using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;

namespace HVACDesigner.EngineeringData.Rules.Common
{
    public sealed class RuleParameterSet
    {
        private readonly ReadOnlyDictionary<string, string> _values;
        public IReadOnlyDictionary<string, string> Values => _values;

        public RuleParameterSet(IDictionary<string, string> values)
        {
            var copy = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (values != null)
            {
                foreach (KeyValuePair<string, string> pair in values)
                {
                    if (string.IsNullOrWhiteSpace(pair.Key))
                        throw new ArgumentException("A paraméterkulcs nem lehet üres.", nameof(values));
                    copy[pair.Key.Trim()] = pair.Value?.Trim() ?? string.Empty;
                }
            }
            _values = new ReadOnlyDictionary<string, string>(copy);
        }

        public bool Contains(string key) =>
            !string.IsNullOrWhiteSpace(key) && _values.ContainsKey(key.Trim());

        public bool TryGetString(string key, out string value)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                value = null;
                return false;
            }
            return _values.TryGetValue(key.Trim(), out value);
        }

        public string GetRequiredString(string key)
        {
            if (!TryGetString(key, out string value))
                throw new KeyNotFoundException("A kötelező szabályparaméter nem található: " + key + ".");
            return value;
        }

        public double GetRequiredDouble(string key)
        {
            string value = GetRequiredString(key);
            if (!double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) ||
                double.IsNaN(result) || double.IsInfinity(result))
                throw new FormatException("A szabályparaméter nem szám: " + key + "=" + value + ".");
            return result;
        }

        public int GetRequiredInt(string key)
        {
            string value = GetRequiredString(key);
            if (!int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out int result))
                throw new FormatException("A szabályparaméter nem egész szám: " + key + "=" + value + ".");
            return result;
        }

        public bool GetRequiredBool(string key)
        {
            string value = GetRequiredString(key);
            if (!bool.TryParse(value, out bool result))
                throw new FormatException("A szabályparaméter nem logikai érték: " + key + "=" + value + ".");
            return result;
        }

        public string GetStringOrDefault(string key, string defaultValue) =>
            TryGetString(key, out string value) ? value : defaultValue;

        public double GetDoubleOrDefault(string key, double defaultValue)
        {
            if (!TryGetString(key, out string value))
                return defaultValue;
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double result) &&
                   !double.IsNaN(result) && !double.IsInfinity(result)
                ? result
                : defaultValue;
        }
    }
}
