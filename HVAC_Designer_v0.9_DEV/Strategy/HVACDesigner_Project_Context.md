# HVAC Designer - Project Context

Rovid atadasi dokumentum Codexnek es kesobbi fejlesztoknek.

## Projekt celja

A HVAC Designer magyar epületgepeszeti tervezoszoftver. A cel, hogy a szabvanyadatok, szamitasi motorok, projektadatok, UI es PDF dokumentacio egymastol szetvalasztva fejlodjenek.

## Aktualis kep

### CoreUI

Komoly uj CoreUI alap keszult:

- sotet/vilagos tema;
- SVG ikonrendszer;
- header, navigation, status, notification;
- sajat text box, combo box, button, checkbox, slider, tabhost;
- cardpanel, resultcard, dialog, chart;
- tooltip/help alapok;
- PDF export dialog.

### EngineeringData/XML

Az XML infrastruktura mukodo MVP. Az adatok egy resze meg minta vagy hianyos, de a cel mar egyertelmu: a szabvanyok es katalogusok XML-bol jojjenek, ne C# kodbol.

### Project/Settings

Van:

- ProjectService;
- SettingsService;
- ApplicationServices;
- UserSettings;
- projektadat form;
- beallitasok form;
- tema es mertekegyseg kezeles.

Figyelni kell arra, hogy a modulok sajat adatai kesobb projektbe menthetok legyenek.

### Water

A viz modul az elso komoly integracios modul:

- epulettipus/profil alapu bemenet;
- szerelvenykatalogus;
- ivoviz/szennyviz/tetoviz/szurkeviz alap;
- validacio es eredmenykartyak;
- PDF jegyzokonyv alap.

Meg ellenorizni kell valos peldakon es boviteni kell az XML adatokat.

### PDF

QuestPDF hasznalatban van. A riport strukturaja kulon export retegben keszul, nem a UserControlbol. A Water riport az elso minta.

## Fontos szabalyok fejleszteskor

- Ne tegyel szamitast UI-ba.
- Ne tegyel XML olvasast kalkulatorba.
- Ne egess szabvanyerteket C# kodba, ha adatkent kezelheto.
- A felhasznalo fele magyar szoveg jelenjen meg.
- Belső debug/registry/XML informacio ne keruljon PDF-be.
- Uj UI elem illeszkedjen a CoreUI tema- es ikonrendszerehez.

## Legfontosabb kovetkezo feladatok

1. Water modul stabil tesztelese.
2. Water PDF tartalom finomitasa.
3. Moduladatok projektbe mentese.
4. XML adatfeltoltes es display nevek rendezese.
5. Air modul fokozatos ujraepitese.
6. Thermal/EKM alapok elokeszitese.

**Dokumentum allapota:** frissitve, tomor aktualis verzio.
