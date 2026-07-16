# HVAC Designer - Development Roadmap

Elő fejlesztesi utemterv. Celja, hogy roviden latszodjon, hol tart a projekt es mi kovetkezik.

## Aktualis allapot

| Terulet | Allapot |
| --- | --- |
| Alap architektura | Kesz, finomitas alatt |
| CoreUI | Eros MVP, folyamatos csiszolas |
| EngineeringData/XML | MVP mukodik, adatfeltoltes es adapterek bovulnek |
| Project/Settings | Alap mukodik, tovabbi moduladat-mentes kell |
| Water modul | Elso komoly tesztmodul, mukodo MVP |
| Air modul | Reszben uj UI, szamitasi modulok kesobb migraltak |
| Thermal/EKM | Elokeszites |
| PDF export | Alap mukodik, Water riporttal |
| AI | Tervezett, kesobbi reteg |

## Kesz fontos alapok

- ApplicationServices/ServiceLocator tisztabb szerkezete.
- ProjectData verziozas es projektadat UI frissites.
- UserSettings bovites, tema es mertekegyseg mentese.
- EngineeringData XML bootstrap alap.
- UnitContext + QuantityUnitService + EngineeringUnitSelector.
- CoreUI komponenskeszlet: TextBox, ComboBox, Button, CheckBox, Slider, TabHost, CardPanel, ResultCard, Dialog, Notification, Status.
- Header modernizalas es uj ikonrendszer.
- Water modulhoz dinamikus bemeneti mezok es PDF export alap.

## Kozeli celok

1. Water modul stabilizalasa
   - tobb epulettipus valos tesztje;
   - ivoviz/szennyviz/tetoviz/szurkeviz eredmenyek ellenorzese;
   - moduladatok projektbe mentese;
   - PDF tartalom finomitasa.

2. EngineeringData/XML
   - valos szabvanyadatok feltoltese;
   - hianyzo display nevek es magyar feliratok potlasa;
   - Rule adapterek kovetkezetesitese;
   - felhasznaloi/gyartoi XML csomagok kesobbi kezelese.

3. CoreUI
   - tablazat es custom scroll egységesitese;
   - tooltip/help rendszer finomitasa;
   - status bar es notification tovabbi hasznalata modulokban;
   - dialogok es export beallitasok finomitasa.

4. Projekt es beallitasok
   - modulonkénti projektadatok tarolasa;
   - PDF export beallitasok modulonkénti megjegyzese;
   - ApplicationPaths es felhasznaloi konyvtarak veglegesitese.

## Kozep tav

- Air modul teljes atallitasa az uj CoreUI es EngineeringData modellre.
- DuctNetwork/DuctElement panelek fokozatos ujrairasa.
- Thermal anyagadatok, retegrend, U-ertek es paratechnika alapok.
- EKM kovetelmeny- es tanusitasi alapok.
- EngineeringData debug/explorer nezet.

## Technikai adossag

- Legacy UI elemek maradvanyainak kivezetese.
- Regi XML/adatprovider megoldasok fokozatos cseréje.
- Modulazonositok sztring alapu hasznalatanak csokkentese.
- Magyar ekezetes forrasfajlok kodolasanak figyelese.
- Build output/publish XML masolas ellenorzese.

## Merfoldkovek

- **M1:** Water modul vegponttol vegpontig: projektadat -> XML szabaly -> kalkulator -> CoreUI -> PDF.
- **M2:** Valos tarsashazi vizigeny teszt.
- **M3:** Air modul uj architektura szerinti elso mukodo valtozata.
- **M4:** Thermal/U-ertek alapmodul.
- **M5:** EKM/tanusitasi elokeszito modul.

**Dokumentum allapota:** frissitve, tomor aktualis verzio.
