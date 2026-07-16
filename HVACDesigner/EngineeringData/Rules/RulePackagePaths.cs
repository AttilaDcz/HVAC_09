using System;
using System.IO;

namespace HVACDesigner.EngineeringData.Rules
{
    public static class RulePackagePaths
    {
        public static string GetDefaultXmlRoot()
        {
            return Path.Combine(AppContext.BaseDirectory, "Data", "Xml");
        }

        public static string ResolveXmlRoot(string explicitRootPath = null)
        {
            string path = string.IsNullOrWhiteSpace(explicitRootPath)
                ? GetDefaultXmlRoot()
                : explicitRootPath.Trim();

            return Path.GetFullPath(path);
        }

        public static bool XmlRootExists(string explicitRootPath = null)
        {
            return Directory.Exists(ResolveXmlRoot(explicitRootPath));
        }
    }
}
