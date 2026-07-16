using System;

namespace HVACDesigner.Services.Export.Common
{
    public static class ReportText
    {
        public static string Clean(string value)
        {
            return string.IsNullOrWhiteSpace(value)
                ? string.Empty
                : value.Trim();
        }

        public static string FormatAddress(
            string zip,
            string settlement,
            string street,
            string streetType,
            string house,
            string building = "",
            string floor = "",
            string door = "")
        {
            string first = string.Join(
                " ",
                new[] { Clean(zip), Clean(settlement) }
                    .WhereNotEmpty());

            string second = string.Join(
                " ",
                new[] { Clean(street), Clean(streetType), Clean(house) }
                    .WhereNotEmpty());

            string third = string.Join(
                ", ",
                new[] { Clean(building), Clean(floor), Clean(door) }
                    .WhereNotEmpty());

            return string.Join(", ", new[] { first, second, third }.WhereNotEmpty());
        }

        private static string[] WhereNotEmpty(this string[] values)
        {
            return Array.FindAll(values, item => !string.IsNullOrWhiteSpace(item));
        }
    }
}
