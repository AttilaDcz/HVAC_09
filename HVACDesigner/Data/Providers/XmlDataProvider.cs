using System;
using System.IO;
using System.Xml.Linq;
using System.Windows.Forms;

namespace HVACDesigner.Data.Providers
{
    public class XmlDataProvider
    {
        protected readonly XDocument Doc;

        public XmlDataProvider(string xmlPath)
        {
            if (!File.Exists(xmlPath))
            {
                MessageBox.Show($"Kritikus adatbázis fájl nem található a megadott útvonalon:\n{xmlPath}",
                    "Adatbázis hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw new FileNotFoundException("Nem található az XML adatbázis fájl.", xmlPath);
            }

            try
            {
                Doc = XDocument.Load(xmlPath);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt az XML fájl betöltése közben:\n{ex.Message}",
                    "Adatbázis sérült", MessageBoxButtons.OK, MessageBoxIcon.Error);
                throw;
            }
        }
    }
}