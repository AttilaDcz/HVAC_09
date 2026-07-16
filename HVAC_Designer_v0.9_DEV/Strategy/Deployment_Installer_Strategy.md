# HVAC Designer - Deployment & Installer Strategy

Rovid telepitesi es kiadasi strategia.

## Cel

A program Windows alkalmazaskent legyen telepitheto ugy, hogy:

- a programfajlok es felhasznaloi adatok elvaljanak;
- a gyari XML adatok a telepitett programmal erkezzenek;
- kesobb felhasznaloi/gyartoi XML csomagok kulon kezelhetok legyenek;
- a projektfajlok a felhasznalo altal valasztott helyre keruljenek.

## Javasolt telepitett struktura

```text
C:\Program Files\HVAC Designer\
  HVACDesigner.exe
  *.dll
  Data\Xml\
  Resources\
```

Felhasznaloi adatok:

```text
%LocalAppData%\HVAC Designer\
  Settings\
  Autosave\
  Logs\
  Cache\
  UserData\Xml\
```

## Aktualis kodallapot

Mar van:

- `ApplicationPaths` alap;
- `SettingsService`;
- `ProjectService`;
- `UserSettings`;
- XML bootstrap;
- QuestPDF inicializalas;
- projektfajl mentes/megnyitas;
- autosave alap.

## Publish/installer teendok

- `Data/Xml/**/*.xml` bekeruljon build es publish outputba.
- A QuestPDF es minden runtime dependency bekeruljon publishba.
- A program `AppContext.BaseDirectory` alapon talalja meg a gyari XML-eket.
- A felhasznaloi beallitasok ne a telepitesi konyvtarba irodjanak.
- A log/autosave/cache konyvtarak LocalAppData alatt legyenek.

## XML csomag strategia

Hosszu tavon harom szint:

1. gyari XML, csak olvashato;
2. felhasznaloi/gyartoi XML csomag;
3. projekt-specifikus feluliras.

Ezeket nem kell most teljesen megirni, de az architekturaba bele kell ferjenek.

## Verziók

Kulon kezelendo:

- alkalmazas verzio;
- EngineeringData verzio;
- ProjectSchemaVersion.

A PDF lablec az alkalmazas verziojat olvassa, ne legyen beegetve.

## Elso telepito

Javasolt: Inno Setup. Kesobb lehet MSIX vagy WiX.

Publish javaslat:

- Release;
- win-x64;
- self-contained;
- nem single-file, hogy az XML es resource kezeles atlathato maradjon.

## Kiadas elotti rovid checklist

- [ ] Build hibamentes.
- [ ] Test hibamentes.
- [ ] XML-ek ott vannak outputban.
- [ ] Water modul indithato.
- [ ] PDF export nem hoz letre 0 bajtos fajlt.
- [ ] Projekt mentheto es visszanyithato.
- [ ] Settings megmarad ujrainditas utan.
- [ ] Tiszta Windows gepen tesztelve.

**Dokumentum allapota:** frissitve, tomor aktualis verzio.
