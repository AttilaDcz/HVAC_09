# HVAC Designer - Architecture

**Cel:** rovid fejlesztoi architektura-leiras. A felhasznaloi dokumentacio ettol kulon keszul.

## Alapelv

- Modularis WinForms alapu mernoki platform.
- A UI, a projektkezeles, az EngineeringData, a szabalyretegek es a kalkulatorok elvalnak.
- A kalkulator nem olvas XML-t es nem ismeri a UI-t.
- A szamitas belul SI egysegekben tortenik, a megjelenites `UnitContext` + `QuantityUnitService` alapon valtozik.
- A szabvany- es katalogusadatok elsoleges forrasa XML.
- A felhasznalo fele magyar nyelvu szovegek jelennek meg, a belso kod lehet angol.

## Aktualis retegek

```text
CoreUI
ApplicationServices / ServiceLocator
ProjectService / SettingsService
EngineeringDataRegistry / Rule registry
Calculations
Export / PDF
```

## CoreUI allapot

Az uj CoreUI mar tartalmazza:

- sotet-vilagos tema alapok;
- sajat SVG ikonrendszer;
- header, navigation, status bar, notification/toast;
- EngineeringTextBox, ComboBox, Button, CheckBox, Slider, TabHost;
- EngineeringCardPanel, ResultCard, Dialog;
- EngineeringChart alap;
- PDF export beallito dialog.

A regi/legacy modulok fokozatosan cserelhetok, de egyelore nem minden modul hasznalja az uj elemeket.

## Project reteg

A projektadatok a `ProjectData` modellen keresztul mennek. Fontos aktualis elemek:

- projekt neve es helyszinadatai;
- tervezoi es megbizoi adatok;
- `CountryCode`;
- `BuildingFunctionId`, `BuildingFunctionDisplayName`, `BuildingProfileId`;
- modulon beluli kesobbi adatok a `Modules` szotarban tarolhatok.

Az uj projekt letrehozas, projektadat szerkesztes es header frissites mar mukodo alapokon all, de a modul-specifikus projektadatok meg bovitesre varnak.

## EngineeringData es XML

Az XML-ek a `Data/Xml` alol toltodnek be. Jelenleg hasznalt/tervezett fajlok:

- `rules-water.xml`
- `rules-air.xml`
- `rules-ekm.xml`
- `rules-tnm-legacy.xml`
- `catalog-fixtures.xml`
- `catalog-materials.xml`
- `catalog-openings.xml`
- `functions-building.xml`
- `profiles-building.xml`
- `mappings-building.xml`
- `design-climate.xml`
- `engineering-dictionary-hu.xml`

Az XML-ek kozul tobb meg MVP/adatfeltoltesi allapotban van. Hosszu tavon gyari, felhasznaloi es projekt-specifikus adatforrasok is lehetnek.

## Water modul

Aktualis cel: az elso teljes vegponttol vegpontig mukodo szakagi minta.

Mar kesz vagy reszben kesz:

- dinamikus epulettipus/profil alapu bemenet;
- ivoviz, szennyviz, tetoviz, szurkeviz alap;
- szerelvenykatalogus hasznalata;
- validacios/diagnosztikai visszajelzesek;
- PDF jegyzokonyv alap QuestPDF-fel.

Tovabbi munka:

- XML szabalyadatok hiteles feltoltese;
- tobb epulettipus tesztje;
- moduladatok projektbe mentese;
- PDF tartalom finomitasa.

## PDF export

QuestPDF inicializalasa az alkalmazas bootstrap reszeben tortenik. A PDF nem tartalmazhat belso fejlesztoi adatokat, XML fajlneveket vagy registry azonositoakat. A PDF-ben felhasznaloi/mernoki tartalom legyen:

- projektadatok;
- tervezoi adatok;
- bemenetek;
- eredmenyek;
- alkalmazott szabvanyok;
- megjegyzesek;
- alairas;
- lablec programverzioval.

## AI reteg

Tervezett harom szint:

1. szabaly- es ellenorzesalapu javaslatok;
2. mernoki ajanlasok es magyarazatok;
3. valodi intelligens tervezesi asszisztens.

Az AI nem irhatja felul a szabvanyt es nem kerulhet a szamitasi magba.

## Fejlesztesi szabaly

- UI-ba ne keruljon fizikai szamitas.
- Kalkulatorba ne keruljon XML olvasas.
- Szabvanyertek ne legyen C# kodba egetve, ha XML-bol kezelheto.
- Uj modul az `EngineeringData -> Rule -> Calculator -> Result -> UI -> Export` lancba illeszkedjen.

**Dokumentum allapota:** frissitve, tomor aktualis verzio.
