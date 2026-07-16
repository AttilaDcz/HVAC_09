using System;
using System.IO;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Services;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.CoreUI.Workspace.Air;
using HVACDesigner.CoreUI.Workspace.Dashboard;
using HVACDesigner.Data.Providers;
using HVACDesigner.EngineeringData;
using HVACDesigner.Services;
using HVACDesigner.Services.Export.Pdf;

namespace HVACDesigner.Bootstrap
{
    /// <summary>
    /// Az alkalmazas inditasi eletciklusanak egyetlen gazdaja.
    /// Itt allnak ossze a kozos szolgaltatasok, adatforrasok es modulgyarak.
    /// </summary>
    public sealed class AppBootstrapper
    {
        public void Run()
        {
            InitializePdf();
            InitializeServices();
            InitializeTheme();

            Form mainForm = CreateMainForm();
            Application.Run(mainForm);
        }

        private static void InitializePdf()
        {
            QuestPdfBootstrapper.Initialize();
        }

        private static void InitializeServices()
        {
            NavigationService navigation = new NavigationService();
            ModuleService modules = new ModuleService();
            ModuleRegistry moduleRegistry = new ModuleRegistry(modules);
            ProjectService project = new ProjectService();
            SettingsService settings = new SettingsService();

            try
            {
                string baseDirectory = AppContext.BaseDirectory;
                string xmlRootPath = Path.Combine(baseDirectory, "Data", "Xml");

                EngineeringDataBootstrapResult engineeringData =
                    new EngineeringDataBootstrapService()
                        .Bootstrap(xmlRootPath);

                IDuctDataProvider ductDataProvider =
                    new XmlDuctDataProvider(
                        Path.Combine(xmlRootPath, "ductdata.xml"));

                IMaterialProvider materialDataProvider =
                    new XmlMaterialProvider(
                        Path.Combine(xmlRootPath, "materialdata.xml"));

                DuctNetworkController ductController =
                    new DuctNetworkController();

                RegisterModules(moduleRegistry);

                var services = new ApplicationServices(
                    navigation,
                    modules,
                    moduleRegistry,
                    project,
                    settings,
                    ductDataProvider,
                    materialDataProvider,
                    ductController,
                    engineeringData);

                ServiceLocator.Configure(services);
            }
            catch (Exception exception)
            {
                throw new InvalidOperationException(
                    "A gepeszeti adatforrasok inicializalasa nem sikerult. " +
                    "Ellenorizd a Data/Xml mappa es az XML-fajlok megletet es epseget.",
                    exception);
            }
        }

        private static void RegisterModules(ModuleRegistry registry)
        {
            registry.Register(
                ModuleKeys.Dashboard,
                services =>
                {
                    var dashControl = new DashboardControl();
                    dashControl.ModuleClicked += clickedName =>
                    {
                        string targetButtonText = "";
                        if (clickedName.Contains("Energetika"))
                            targetButtonText = ModuleKeys.BuildingUValue;
                        else if (clickedName.Contains("Fűtés"))
                            targetButtonText = ModuleKeys.HeatingPipePressureDrop;
                        else if (clickedName.Contains("Légtechnika"))
                            targetButtonText = ModuleKeys.AirVelocity;

                        if (!string.IsNullOrEmpty(targetButtonText))
                            services.Navigation.NavigateTo(targetButtonText);
                    };
                    return dashControl;
                });

            registry.Register(
                ModuleKeys.BuildingUValue,
                services => new CoreUI.Workspace.BuildingThermal.UValueCalculationControl(
                    services.MaterialDataProvider));

            registry.RegisterPlaceholder(
                ModuleKeys.BuildingInventory,
                "BuildingThermal",
                "integralt modul fejlesztese folyamatban");

            registry.RegisterPlaceholder(
                ModuleKeys.BuildingHeatLoad,
                "BuildingThermal",
                "integralt modul fejlesztese folyamatban");

            registry.RegisterPlaceholder(
                ModuleKeys.BuildingCertification,
                "BuildingThermal",
                "integralt modul fejlesztese folyamatban");

            registry.RegisterPlaceholder(
                ModuleKeys.HeatingPipePressureDrop,
                "ThermalHydraulics",
                "gepeszeti almodul integracioja kovetkezik");

            registry.RegisterPlaceholder(
                ModuleKeys.HeatingPumpSelection,
                "ThermalHydraulics",
                "gepeszeti almodul integracioja kovetkezik");

            registry.Register(
                ModuleKeys.WaterPeakDemand,
                _ => new CoreUI.Workspace.Water.WaterDemandControl());

            registry.RegisterPlaceholder(
                ModuleKeys.WaterDailyDemand,
                "Sanitary",
                "szakagi szamitas elokeszitese");

            registry.RegisterPlaceholder(
                ModuleKeys.WaterDrainagePeak,
                "Sanitary",
                "szakagi szamitas elokeszitese");

            registry.RegisterPlaceholder(
                ModuleKeys.WaterDhwCirculation,
                "Sanitary",
                "szakagi szamitas elokeszitese");

            registry.RegisterPlaceholder(
                ModuleKeys.WaterLongProfile,
                "Sanitary",
                "szakagi szamitas elokeszitese");

            registry.Register(
                ModuleKeys.AirVelocity,
                services =>
                {
                    var velocityControl =
                        new CoreUI.Workspace.Air.AirVelocityControl();
                    velocityControl.InitializeDataProvider(
                        services.DuctController.DataProvider);
                    return velocityControl;
                });

            registry.Register(
                ModuleKeys.AirDuctSizing,
                _ => new CoreUI.Workspace.Air.AirDuctsizeControl());

            registry.Register(
                ModuleKeys.AirDuctNetwork,
                _ => new CoreUI.Workspace.Air.DuctNetworkPanel());

            registry.Register(
                ModuleKeys.FlueGas,
                _ => new CoreUI.Workspace.FlueGas.FlueGasSandboxControl());

            registry.Register(
                ModuleKeys.EngineeringDataSandbox,
                _ => new CoreUI.Workspace.EngineeringData.EngineeringDataSandboxControl());
        }

        private static void InitializeTheme()
        {
            try
            {
                var settings = ServiceLocator.Settings.SelectedSettings;
                if (settings != null)
                {
                    if (Enum.TryParse<AppThemeMode>(settings.CurrentTheme, out var themeMode))
                    {
                        ThemeManager.CurrentThemeMode = themeMode;
                    }
                    if (Enum.TryParse<AppSizeMode>(settings.CurrentSizeMode, out var sizeMode))
                    {
                        ThemeManager.CurrentSizeMode = sizeMode;
                    }
                }
            }
            catch
            {
                // Fallback az alapértelmezett témákra, ha valamilyen hiba lép fel
            }
        }

        private static Form CreateMainForm()
        {
            var mainForm = new Form1();
            ThemeApplicator.MonitorControl(mainForm);
            return mainForm;
        }
    }
}
