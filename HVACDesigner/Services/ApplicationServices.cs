using System;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Workspace.Air;
using HVACDesigner.Data.Providers;
using HVACDesigner.EngineeringData;
using HVACDesigner.EngineeringData.Registry;
using HVACDesigner.EngineeringData.Rules.Common;

namespace HVACDesigner.Services
{
    public sealed class ApplicationServices
    {
        public ApplicationServices(
            NavigationService navigation,
            ModuleService modules,
            ModuleRegistry moduleRegistry,
            ProjectService project,
            SettingsService settings,
            IDuctDataProvider ductDataProvider,
            IMaterialProvider materialDataProvider,
            DuctNetworkController ductController,
            EngineeringDataBootstrapResult engineeringData)
        {
            Navigation = navigation ?? throw new ArgumentNullException(nameof(navigation));
            Modules = modules ?? throw new ArgumentNullException(nameof(modules));
            ModuleRegistry = moduleRegistry ?? throw new ArgumentNullException(nameof(moduleRegistry));
            Project = project ?? throw new ArgumentNullException(nameof(project));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
            DuctDataProvider = ductDataProvider ?? throw new ArgumentNullException(nameof(ductDataProvider));
            MaterialDataProvider = materialDataProvider ?? throw new ArgumentNullException(nameof(materialDataProvider));
            DuctController = ductController ?? throw new ArgumentNullException(nameof(ductController));
            EngineeringData = engineeringData ?? throw new ArgumentNullException(nameof(engineeringData));
        }

        public NavigationService Navigation { get; }
        public ModuleService Modules { get; }
        public ModuleRegistry ModuleRegistry { get; }
        public ProjectService Project { get; }
        public SettingsService Settings { get; }
        public IDuctDataProvider DuctDataProvider { get; }
        public IMaterialProvider MaterialDataProvider { get; }
        public DuctNetworkController DuctController { get; }
        public EngineeringDataBootstrapResult EngineeringData { get; }

        public EngineeringDataRegistry EngineeringDataRegistry =>
            EngineeringData.DataRegistry;

        public EngineeringRuleRegistry EngineeringRuleRegistry =>
            EngineeringData.RuleRegistry;
    }
}
