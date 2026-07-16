using System;
using System.Collections.Generic;

namespace HVACDesigner.CoreUI.Services
{
    public class NavigationService
    {
        private readonly Stack<string> backStack = new Stack<string>();
        private readonly Stack<string> forwardStack = new Stack<string>();
        private string currentModuleName = string.Empty;

        public event Action<string>? NavigationRequested;
        public event EventHandler? NavigationStateChanged;
        public event Action<bool>? MenuStateChanged;

        public string CurrentModuleName => currentModuleName;
        public bool CanGoBack => backStack.Count > 0;
        public bool CanGoForward => forwardStack.Count > 0;

        public void NavigateTo(string targetModuleName)
        {
            string normalizedTarget = (targetModuleName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedTarget))
                return;

            if (string.Equals(
                currentModuleName,
                normalizedTarget,
                StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(currentModuleName))
            {
                backStack.Push(currentModuleName);
                forwardStack.Clear();
            }

            currentModuleName = normalizedTarget;
            RaiseNavigation(currentModuleName);
        }

        public void GoBack()
        {
            if (!CanGoBack)
                return;

            if (!string.IsNullOrWhiteSpace(currentModuleName))
                forwardStack.Push(currentModuleName);

            currentModuleName = backStack.Pop();
            RaiseNavigation(currentModuleName);
        }

        public void GoForward()
        {
            if (!CanGoForward)
                return;

            if (!string.IsNullOrWhiteSpace(currentModuleName))
                backStack.Push(currentModuleName);

            currentModuleName = forwardStack.Pop();
            RaiseNavigation(currentModuleName);
        }

        public void UpdateMenuState(bool isExpanded)
        {
            MenuStateChanged?.Invoke(isExpanded);
        }

        private void RaiseNavigation(string moduleName)
        {
            NavigationRequested?.Invoke(moduleName);
            NavigationStateChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
