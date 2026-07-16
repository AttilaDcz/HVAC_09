using HVACDesigner.CoreUI.Layout; // Az új Layout réteg beemelése
using HVACDesigner.CoreUI.Services;
using System;
using System.Windows.Forms;
using HVACDesigner.Services; // Az új szolgáltatások és a ServiceLocator beemelése

namespace HVACDesigner
{
    public partial class Form1 : Form
    {
        // A teljes felületért felelős Application Shell
        private ApplicationShell? _shell;

        public Form1()
        {
            InitializeComponent();

            Load += (sender, args) =>
            {
                _shell = new ApplicationShell(this);
                _shell.Initialize();

                ServiceLocator.Navigation.NavigateTo(ModuleKeys.Dashboard);
            };
        }

    }
}
