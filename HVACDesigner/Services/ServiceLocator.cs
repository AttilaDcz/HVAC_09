using System;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Workspace.Air;
using HVACDesigner.Data.Providers;
using HVACDesigner.EngineeringData;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Services
{
    /// <summary>
    /// A mar inicializalt alkalmazasszolgaltatasok kozponti,
    /// atmeneti eleresi pontja.
    ///
    /// A ServiceLocator nem hoz letre szolgaltatasokat,
    /// nem vegez fajlkezelest, es nem jelenit meg UI-uzeneteket.
    /// A konfiguracio kizarolag az AppBootstrapper feladata.
    /// </summary>
    public static class ServiceLocator
    {
        private static ApplicationServices? current;

        public static bool IsConfigured => current != null;

        public static ApplicationServices Current =>
            current ??
            throw new InvalidOperationException(
                "A ServiceLocator meg nincs konfiguralva.");

        public static NavigationService Navigation => Current.Navigation;

        public static ModuleService Modules => Current.Modules;

        public static ModuleRegistry ModuleRegistry => Current.ModuleRegistry;

        public static ProjectService Project => Current.Project;

        public static SettingsService Settings => Current.Settings;

        public static IDuctDataProvider DuctDataProvider => Current.DuctDataProvider;

        public static IMaterialProvider MaterialDataProvider => Current.MaterialDataProvider;

        public static DuctNetworkController DuctController => Current.DuctController;

        public static EngineeringDataBootstrapResult EngineeringData => Current.EngineeringData;

        public static EngineeringDataRegistry EngineeringDataRegistry =>
            Current.EngineeringDataRegistry;

        public static EngineeringRuleRegistry EngineeringRuleRegistry =>
            Current.EngineeringRuleRegistry;

        public static void Configure(ApplicationServices services)
        {
            if (current != null)
            {
                throw new InvalidOperationException(
                    "A ServiceLocator mar konfiguralva lett.");
            }

            current = services ??
                throw new ArgumentNullException(nameof(services));
        }
    }
}
