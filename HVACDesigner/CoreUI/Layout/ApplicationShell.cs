using System;
using System.Drawing;
using System.Windows.Forms;
using HVACDesigner.CoreUI.Notifications;
using HVACDesigner.CoreUI.Status;
using HVACDesigner.CoreUI.Theme;
using HVACDesigner.Services; // Beemeljük a szolgáltatásokat

namespace HVACDesigner.CoreUI.Layout
{
    // A teljes alkalmazás felületét és elrendezését vezérlő központi osztály
    public class ApplicationShell
    {
        private readonly Form _targetForm;
        private readonly ShellLayout _layoutEngine;
        private bool _isMenuExpanded = true;

        // A fizikai zónák konténerei (Hostok)
        public HostBase HeaderHost { get; private set; }
        public HostBase NavigationHost { get; private set; }
        public HostBase ContentHost { get; private set; }
        public HostBase StatusHost { get; private set; }
        public EngineeringNotificationHost NotificationHost { get; private set; }

        public ApplicationShell(Form targetForm)
        {
            _targetForm = targetForm ?? throw new ArgumentNullException(nameof(targetForm));
            _layoutEngine = new ShellLayout();

            // MÓDOSÍTVA: Az üres alap dobozok helyett az új, intelligens rajzolt hostokat indítjuk!
            HeaderHost = new HeaderHost();
            NavigationHost = new NavigationHost();
            ContentHost = new ContentHost();
            StatusHost = new StatusHost();
            NotificationHost = new EngineeringNotificationHost();
        }

        // Összerakja a struktúrát és ráköti a Form eseményeire
        public void Initialize()
        {
            _targetForm.SuspendLayout();

            _targetForm.BackColor = ThemeManager.CurrentPalette.Window;

            _targetForm.Controls.Add(HeaderHost);
            _targetForm.Controls.Add(NavigationHost);
            _targetForm.Controls.Add(ContentHost);
            _targetForm.Controls.Add(StatusHost);
            _targetForm.Controls.Add(NotificationHost);
            EngineeringNotificationService.RegisterHost(NotificationHost);
            EngineeringStatusMessages.SetReady();

            _targetForm.Resize += (s, e) => ArrangeComponents();

            ArrangeComponents();

            if (NavigationHost is NavigationHost nav)
            {
                nav.BuildMenuUI();
            }

            if (HeaderHost is HeaderHost header)
            {
                header.BuildHeaderUI();
            }

            _targetForm.ResumeLayout(true);
        }

        // Pixelpontosan elrendezi a Hostokat a matematika alapján
        public void ArrangeComponents()
        {
            if (_targetForm.ClientSize.Width == 0 || _targetForm.ClientSize.Height == 0) return;

            _layoutEngine.Recalculate(_targetForm.ClientSize, _isMenuExpanded);

            HeaderHost.Bounds = _layoutEngine.HeaderBounds;
            NavigationHost.Bounds = _layoutEngine.NavigationBounds;
            ContentHost.Bounds = _layoutEngine.ContentBounds;
            StatusHost.Bounds = _layoutEngine.StatusBarBounds;

            int notificationWidth = Math.Min(390, Math.Max(260, _targetForm.ClientSize.Width - 24));
            int notificationHeight = Math.Min(270, Math.Max(100, _layoutEngine.StatusBarBounds.Top - LayoutMetrics.HeaderHeight - 24));
            NotificationHost.Bounds = new Rectangle(
                _targetForm.ClientSize.Width - notificationWidth - 12,
                _layoutEngine.StatusBarBounds.Top - notificationHeight - 12,
                notificationWidth,
                notificationHeight);
            NotificationHost.BringToFront();
        }

        // Lehetővé teszi a menü szoftveres összecsukását és kinyitását
        public void ToggleMenu()
        {
            _isMenuExpanded = !_isMenuExpanded;
            ArrangeComponents();
            ServiceLocator.Navigation.UpdateMenuState(_isMenuExpanded);
        }
    }
}
