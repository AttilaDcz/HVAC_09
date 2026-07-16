using System;
using System.Reflection;

namespace HVACDesigner.Services
{
    public sealed class ApplicationVersionService
    {
        public string ProductName => "HVAC Designer";

        public string VersionText
        {
            get
            {
                Version? version = Assembly.GetExecutingAssembly().GetName().Version;
                if (version == null)
                    return "v0.0.0";

                return "v" + version.Major + "." + version.Minor + "." + version.Build;
            }
        }

        public string BuildDateText =>
            DateTime.Today.ToString("yyyy.MM.dd");
    }
}
