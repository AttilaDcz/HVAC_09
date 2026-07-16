# HVAC Designer - Engineering Standards

Ez szakmai iranytu, nem reszletes szabvanygyujtemeny.

## Alapelv

- A fizika es a mernoki logika az alap.
- A szabvanyok es nemzeti kovetelmenyek Rule/XML retegben jelennek meg.
- A program belso kodja lehet angol, a felhasznaloi kimenet magyar.
- A PDF es UI nem mutathat belso XML fajlneveket, registry azonositoakat vagy fejlesztoi adatokat.

## Water

Jelenlegi fo cel: epuleten beluli vizellatas, szennyviz, tetoviz es opcionális szurkeviz alapjan egy hasznalhato mertekado vizigeny modul.

Hasznalt iranyok:

- szerelvenykatalogus;
- epulettipus/profil alapu bemenet;
- napi vizigeny;
- mertekado ivoviz;
- mertekado szennyviz;
- tetoviz lefolyas;
- opcionális szurkeviz merleg.

Nem elso verzios cel:

- teljes kozmu halo;
- kulso csatornahalozat;
- szifonikus tetoviz rendszer;
- gyartoi tetosszefolyo-katalogus meretezes.

## Air

A legtechnika kesobb kulon Rule csomagra epul. A kovetkezo bemeneti logikak fontosak:

- szemelyszam;
- alapterulet;
- helyisegtipus;
- technologiai terheles;
- sebesseg- es nyomasveszteseg-ellenorzes.

A jelenlegi Air UI reszben atmeneti. A komoly panelek kesobb az uj CoreUI es EngineeringData szerint ujra lesznek rendezve.

## Thermal / EKM

Thermal feladat:

- anyagok;
- retegrend;
- U-ertek;
- hohidak;
- paratechnika.

EKM feladat:

- kovetelmenyek;
- hatarertekek;
- ellenorzesek;
- tanusitasi elokeszites.

Az EKM nem fizikai motor, hanem kovetelmeny- es validacios reteg.

## Epulettipusok

A projekt szinten van alap epulettipus/funkcio. Modulonként kesobb felulbiralhato.

Az XML-ek feladata:

- epulettipusok megnevezese;
- profilok;
- mapping a modulok sajat kategoriáihoz;
- magyar display nevek.

Ha nincs megfelelo XML elem, a felhasznalo egyedi funkciot adhasson meg.

## AI strategia

Az AI kesobbi reteg:

- Level 1: szabalyalapu figyelmeztetes es javaslat.
- Level 2: mernoki ajanlas es magyarazat.
- Level 3: valodi asszisztens.

Az AI nem modosithat szabvanyt es nem lehet egyetlen szamitasi eredmeny kizarolagos alapja.

## Dokumentacio/PDF

A PDF legyen mernoki jegyzokonyv, nem fejlesztoi export.

Megjelenhet:

- projektadat;
- tervezoi adat;
- bemeneti adat;
- eredmeny;
- alkalmazott szabvany;
- megjegyzes;
- alairas;
- programverzio.

Nem jelenhet meg:

- XML fajlnev;
- Rule package belso azonosito;
- registry;
- bootstrap informacio;
- fejlesztoi debug adat.

## Kovetkezo szakmai feladatok

- Water XML adatok hitelesitese.
- Tobb epulettipus vizigeny-szabalyainak ellenorzese.
- Szerelvenykatalogus bovites.
- Air szabalycsomag kutatasa es feltoltese.
- Thermal/EKM szabalyok strukturalasa.

**Dokumentum allapota:** frissitve, tomor aktualis verzio.
