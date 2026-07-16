using System;
using System.Windows.Forms;
using HVACDesigner.Bootstrap;

namespace HVACDesigner
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            // Futtatjuk az adatbázis és funkcionális integrációs teszteket indításkor
            try
            {
                HVACDesigner.Tests.Integration.Thermal.BuildingThermalDataTests.Run();
                HVACDesigner.Tests.Integration.Water.WaterThirtyApartmentScenarioTests.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kritikus hiba az integrációs tesztek futtatásakor:\n\n{ex}",
                    "Tesztelési hiba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return;
            }

            Application.SetHighDpiMode(
                HighDpiMode.PerMonitorV2);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                var bootstrapper = new AppBootstrapper();
                bootstrapper.Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Kritikus hiba az alkalmazás indításakor:\n\n{ex.Message}",
                    "Alkalmazásindítási hiba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}