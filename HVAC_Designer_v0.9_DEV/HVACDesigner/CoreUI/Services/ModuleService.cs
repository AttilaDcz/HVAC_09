using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace HVACDesigner.CoreUI.Services
{
    // A szakági modulok életciklusáért és gyorsítótárazásáért (Cache) felelős szolgáltatás
    public class ModuleService
    {
        // A háttérben élő modulokat tároló gyorsítótár (szótár struktúra)
        private readonly Dictionary<string, UserControl> _moduleCache = new Dictionary<string, UserControl>();

        // Lekéri a kért modult a gyorsítótárból, vagy ha még nem létezik, létrehozza azt
        public UserControl? GetModule(string moduleName, Func<UserControl>? factoryMethod)
        {
            if (string.IsNullOrEmpty(moduleName)) return null;

            // Ha a modul már szerepel a memóriában, azonnal visszaadjuk a meglévő állapotot
            if (_moduleCache.ContainsKey(moduleName))
            {
                return _moduleCache[moduleName];
            }

            // Ha még sosem volt megnyitva, a gyári függvénnyel példányosítjuk
            if (factoryMethod != null)
            {
                var newModule = factoryMethod.Invoke();
                _moduleCache[moduleName] = newModule;
                return newModule;
            }

            return null;
        }

        // Lehetővé teszi a gyorsítótár teljes törlését projekt bezárásakor
        public void ClearCache()
        {
            foreach (var module in _moduleCache.Values)
            {
                module.Dispose();
            }
            _moduleCache.Clear();
        }
    }
}
