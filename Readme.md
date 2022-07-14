# Critical Thinking Stations (CTS)
Partizipative Medienstationen **Critical Thinking Stations** für das *Deutsche Auswandererhaus* in Bremerhaven


# Inhaltsverzeichnis

1. [Kurzbeschreibung](#kurzbeschreibung)
2. [Installation](#installation)
3. [Einzelapplikationen](#einzelapplikationen)
4. [Server](#server)
5. [Inhalte](#inhalte)
6. [Benutzung](#benutzung)
7. [Lizenz](#lizenz)
8. [Credits / Projektpartner](#creditsprojektpartner)


# Kurzbeschreibung

Diese auf Basis des eigenen gemelo Exponatframeworks mit C#/WPF/XAML errichteten .NET 6 Applikationen bilden im *Deutschen Auswandererhaus (DAH)* interaktive Medienstationen, die dem Besucher auf dem Ausstellungsrundgang kritische Fragen stellen, die beantwortet und zentral ausgewertet werden. Diese Auswertung ist für alle Besucher sichtbar und lädt zu weiteren Gedanken ein.

Die Besucher melden sich an den Stationen mit der im DAH verwendeten RFID-Karte an.

Zunächst werden den Besuchern an den sogenannten TouchBeamter-Stationen Fragen zum Themenkomplex Ein- und Auswanderung gestellt. Diese Stationen sind in historischen Kulissen wie alten Koffern eingearbeitet und arbeiten mit einem auf Lidar basierenden Touchsystem.

An den DeepDive-Stationen können die Besucher zu einzelnen Fragen vertiefende Informationen abrufen - zum Beispiel in Form von Videos.

Die Ergebnisse der Ausstellung werden auf einer großen Videowall in Form von Diagrammen präsentiert.

Dieses Critical Thinking Stations Projekt ist entstanden im Verbundprojekt 
**museum4punkt0** - Digitale Strategien für das Museum der Zukunft
**Teilprojekt Migrationsgeschichte digital erleben**. 

Das Projekt museum4punkt0 wird gefördert durch die Beauftragte der Bundesregierung für Kultur und Medien aufgrund eines Beschlusses des Deutschen Bundestages.

![BKM-Logo](https://github.com/museum4punkt0/Object-by-Object/blob/77bba25aa5a7f9948d4fd6f0b59f5bfb56ae89e2/04%20Logos/BKM_Fz_2017_Web_de.gif)
![NeustartKultur](https://github.com/museum4punkt0/Object-by-Object/blob/22f4e86d4d213c87afdba45454bf62f4253cada1/04%20Logos/BKM_Neustart_Kultur_Wortmarke_pos_RGB_RZ_web.jpg)

Weitere Informationen: [museum4punkt0](https://www.museum4punkt0.de)

# Installation

## Hardware / Betriebssystem
Die Software läuft auf einem handelsüblichen PC unter dem Betriebssystem Microsoft Windows 10 oder 11. Sie benötigt das Framework [Microsoft .NET 6.0.2 oder höher](https://dotnet.microsoft.com/en-us/download). An die Hardware werden keine speziellen Anforderungen gestellt, am rechnenintensivsten ist die Darstellung von Videos. Für die TouchBeamer-Stationen werden LIDAR-Sensoren [Hokuyo URG-04LX-UG01](https://www.hokuyo-aut.jp/search/single.php?serial=166) benötigt. Sowohl DeepDive als auch TouchBeamer-Stationen benötigen RFID-Leser. Hier können unterschiedliche Varianten zum Einsatz kommen (s. Startparameter).

## Schriften
Folgende Schriftarten müssen auf dem PC installiert sein:
* Franzika OT
* Weissenhof Grotesk

jeweils in verschiedenen Schriftschnitten. Die Schriften liegen im Verzeichnis Fonts. Dabei sind die Lizenzen der Schriftarten zu beachten. Die Schriftarten sind nicht Teil dieses Projekts und stehen deshalb nicht unter einer freien Lizenz zur Verfügung.

## RFID-Simulation
Die Anwendungen können auch ohne RFID-Leser genutzt werden. Dazu kann über Tastenkombinationen das Auflegen einer RFID-Karte simuliert werden. Das funktioniert auch über Fernwartung, selbst wenn ein physikalischer RFID-Leser eingerichtet wurde.

Dazu werden die Tasten **A** bis **D** zusammen mit der **Strg**-Taste genutzt. **Strg-Minus** wiederum entspricht dem Entfernen der Karte.


# Einzelapplikationen
Die Installation besteht aus mehreren Einzelapplikationen, die im Folgenden beschriebenen werden:

1. [Touchbeamer](#ctstouchbeamer)
2. [DeepDive](#ctsdeepdive)
3. [Wall (Auswertung)](#ctswall)
3. [Import](#ctsimport)

## Cts.TouchBeamer
Die TouchBeamer-Stationen sind der Eintrittspunkt in das System. Hier kann der Besucher Fragen zu verschiedenen Bereichen beantworten. Die Stationen zeigen dabei unterschiedliche Inhalte/Fragen. Die Bedienung kann optional über einen LIDAR basierten Touch erfolgen.

![Screenshot TouchBeamer 1](/Documentation/Screenshots/ScreenTouchBeamer01.jpg)
![Screenshot TouchBeamer 2](/Documentation/Screenshots/ScreenTouchBeamer02.jpg)

### Startparameter

#### -station-id:\<station-id>
Gibt die sogenannte Stations-ID an, die festlegt, welche Inhalte die Station anzeigt (s. unten). Diese Angabe ist **obligatorisch**, da die Station ansonsten keine Inhalte anzeigen könnte.

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -rfid:\<serial-port>
Legt die serielle Schnittstelle für den RFID-Leser fest, also z. B. *COM3*. Wird diese Angabe nicht gemacht, kann das Einlesen von RFID-Karten nur per Tastatur zu Testzwecken simuliert werden. Es werden verschiedene Hersteller von RFID-Lesern unterstützt (siehe die nächsten Parameter). Per Default wird ein Leser von Feig verwendet.

#### -feig
Legt den Typ des RFID-Lesers auf FEIG fest.

#### -tagscan
Legt den Typ des RFID-Lesers auf TAGscan fest (z. B. TAGscan 003 Industry).

#### -winmate
Legt den Typ des RFID-Lesers auf WinMate fest. Dieser RFID-Leser wird in den Tablets mit integriertem RFID-Leser im DAH verwendet.

#### -rfidpos:\<rfid-position>
Gibt an, auf welcher Stelle der Bildschirmschoner für das Einlesen der RFID-Karten verweisen soll, also wo im tatsächlichen Exponat der Leser verbaut wurde. Mögliche Parameter sind:
*Right*, *Left*, *BottomRight*, *BottomLeft*, *TopRight*.

#### -lidar:\<serial-port>
Legt die serielle Schnittstelle fest, die für den LIDAR-Sensor genutzt wird, also z. B. *COM5*. Wird diese Angabe nicht gemacht, wird auch kein LIDAR-Touch verwendet.

#### -s
Legt fest, dass die Applikation automatisch auf die Bildschirmgröße skaliert wird.

#### -c
Legt fest, dass die Anzeige des Mauscursors unterdrückt wird.


### Sichtbaren Ausschnitt festlegen
Die Stationen projezieren das Bild auf Gegenstände. Dabei kann nicht immer der volle Projektionsbereich für die Darstellung genutzt werden. Deshalb kann man den für die Darstellung verwendeten Bereich festlegen.

Mit **F4** kann man einen Rahmen anzeigen lassen, der den sichtbaren Bereich eingrenzt. Ist dieser Bereich sichtbar lässt er sich mit den **Cursortasten** verändern. Die Cursortasten ändern dabei immer den entsprechend Rand, also ändert die **Cursor-Hoch-Taste** zum Beispiel den oberen Rand.

Mit gleichzeitiger **Strg**-Taste wird der Bereich verkleinert, sonst vergrößert. Mit **Shift** werden die größere Änderungen (10-fach) durchgeführt.

Die Einstellungen zum Ausschnitt werden automatisch gespeichert.


### LIDAR-Kalibrierung
Um die LIDAR-Touch-Funktion nutzen zu können, muss diese Kalibriert werden. Dazu dienen einige Tastaturbefehle.

Mit **F6** startet der Kalibrierungsmodus für den LIDAR-Touch. Es wird eine Visualisierung der vom LIDAR erkannten Punkte dargestellt. Die Darstellung der Visualisierung kann auf folgende Arten angepasst werden:

- **V** – Spiegelt die Sensor-Darstellung vertikal
- **H** – Spiegelt die Sensor-Darstellung horizontal

Solange mit der Kalibrierung noch nicht begonnen wurde, kann der Messbereich des LIDAR-Sensors, der überhaupt berücksichtigt wird folgendermaßen angepasst werden:

- **Q** und **W** – Stellt den Start-Winkel ein, der berücksichtigt wird (zusammen mit **Shift** ändert sich der Wert schneller)
- **A** und **S** – Stellt den End-Winkel ein, der berücksichtigt wird (zusammen mit **Shift** ändert sich der Wert schneller)
- **Y** und **X** – Stellt die maximale Distanz ein, die berücksichtig wird (zusammen mit **Shift** ändert sich der Wert schneller)
- **0** bis **9** – Stellt maximale Reichweiten für 10 gleichbreite Segmente des Messbereichs ein. Damit können Störungen durch Erhebungen außerhalb des eigentlich Touchbereichs entfernt werden. Zusammen mit **Strg** wird der ausgeschlossene Bereich verkleinert. Mit **Shift** ändert sich der Wert schneller.

Für die eigentliche Kalibrierung werden der Reihe nach drei Punkte abgefragt. Diese Punkte müssen jeweils per Finger "berührt" werden. Dabei muss der Finger die einzig erkannte Berühung darstellen (kann auf der Visualisierung kontrolliert werden). Mit **N** wechselt man zum nächsten Kalibrierungspunkt. Nach dem letzten Punkt kann die Kalibrierung noch einmal kontrolliert werden und mit einer weiteren Druck auf **N** wird sie gespeichert.

## Cts.DeepDive
Die DeepDive-Stationen sind die Vertiefungsstationen der Installation. Hier bekommt der Besucher noch einmal seine bisherigen Antworten gezeigt und kann zu einigen Fragen vertiefende Informationen abrufen.

![Screenshot DeepDive 1](/Documentation/Screenshots/ScreenDeepDive01.jpg)
![Screenshot DeepDive 2](/Documentation/Screenshots/ScreenDeepDive02.jpg)
![Screenshot DeepDive 3](/Documentation/Screenshots/ScreenDeepDive03.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -rfid:\<serial-port>
Legt die serielle Schnittstelle für den RFID-Leser fest, also z. B. *COM3*. Wird diese Angabe nicht gemacht, kann das Einlesen von RFID-Karten nur per Tastatur zu Testzwecken simuliert werden. Es werden verschiedene Hersteller von RFID-Lesern unterstützt (siehe die nächsten Parameter). Per Default wird ein Leser von Feig verwendet.

#### -feig
Legt den Typ des RFID-Lesers auf FEIG fest.

#### -tagscan
Legt den Typ des RFID-Lesers auf TAGscan fest (z. B. TAGscan 003 Industry).

#### -winmate
Legt den Typ des RFID-Lesers auf WinMate fest. Dieser RFID-Leser wird in den Tablets mit integriertem RFID-Leser im DAH verwendet.

#### -rfidpos:\<rfid-position>
Gibt an, auf welcher Stelle der Bildschirmschoner für das Einlesen der RFID-Karten verweisen soll, also wo im tatsächlichen Exponat der Leser verbaut wurde. Mögliche Parameter sind:
*Right*, *Left*, *BottomRight*, *BottomLeft*, *TopRight*.

#### -16to10
Ändert die Darstellung auf 16:10. Überlicherweise wird ein Bildschirmverhältnis von 16:9 verwendet. Läuft die Software jedoch auf den Tablets vom DAH sollte das Bildschirmverhältnis auf 16:10 geändert werden.

#### -showrfid
Zeigt ein RFID-Symbol am oberen rechten Rand an. Diese Darstellung wird für die Tablets verwendet, da diese in der oberen rechten Ecke einen RFID-Leser verbaut haben.

#### -s
Legt fest, dass die Applikation automatisch auf die Bildschirmgröße skaliert wird.

#### -c
Legt fest, dass die Anzeige des Mauscursors unterdrückt wird.

## Cts.Wall
Die Wall-Station zeigt die Auswertungen zu den einzelnen Fragen an. Dazu ruft sie die Ergebnisse am Server ab und stellt die Ergebnisse in Form von Diagrammen für die Fragen dar. Da nicht alle Auswertungen auf eine Bildschirmseite passen, wechselt die Darstellung.

![Screenshot Wall 1](/Documentation/Screenshots/ScreenWall01.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -16to9
Normalerweise ist die Darstellung der Software für eine Videowall im Format 20:9 ausgelegt (5 x 4 Videowall-Displays). Um die Software auch auf Bildschirmen im verbreiteten 16:9-Format nutzen zu können, kann die Darstellung mit diesem Parameter umgestellt werden.

#### -demoresults
Um die Darstellung zu testen, ohne dass schon eine ausreichende Anzahl von Antworten zur Verfügung steht, können zufällige Beispielresultate mit diesem Parameter genutzt werden.

### Anpassen der Darstellung
Mit folgenden Tastaturbefehlen kann die Darstellung der Auswertungen auf der Videowall optimiert werden, damit keine Texte oder Balken durch einen Steg zwischen Videowall-Screens aufgeteilt werden.

#### F4
Mit **F4** kann ein Raster angezeigt werden, das dem Raster der Videowallscreens (5 x 4) entspricht.

#### F5
Mit **F5** kann auf die nächste Auswertungsseite gewechselt werden.

#### 0 bis 9 bzw. A bis K
Mit diesen Tasten kann die Auswertung von einer Frage ausgewählt werden. Die Taste **1** entspricht der ersten Auswertung, **9** der zehnten Auswertung, **A** der elften usw.

#### + und -
Mit **+** und **-** kann der vertikale Abstand der vorher ausgewählten Auswertung zur vorherigen Auswertung vergrößert bzw. verkleinert werden.

## Cts.Import
Die Import-Applikation wird nicht direkt in der Ausstellung verwendet, sondern dient dazu die Inhalte für die Fragen und Vertiefungsinformationen auf dem Server einzupflegen. Dazu wird eine Excel-Tabelle eingelesen (s. [Inhalte](#Inhalte)). 

Außerdem können mit der Anwendung die Eingaben der Besucher gelöscht werden oder zumindest Eingaben, die über die simulierten RFID-Karten eingegeben wurden (s. [RFID-Simulation](##RFID-Simulation)).

![Screenshot Import 1](/Documentation/Screenshots/ScreenImport01.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -database:\<database-server>
Gibt den Namen des Microsoft SQL-Datenbank-Servers, der für das CMS verwendet wird, an.

# Server
Die Serversoftware basiert auf einer Microsoft SQL-Datenbank und einer per HTTP zugreifbaren HTTP-Schnittstelle, über die die Stationen zum einen die Inhalte abfragen können und zum anderen auch die Ergebnisse der Besucher an den Server melden können.

## Datenbank
Die Datenbank basiert auf dem [Microsoft SQL Server](https://www.microsoft.com/de-de/sql-server) ab Version 2019. Die kostenlose Express-Version ist dabei ausreichend.

Die Datenbank kann mit dem SQL-Script unter **Cts.Database/Scripts** angelegt werden.

## Server-Dienst
Die HTTP-Schnittstelle wird über einen Windows-Server-Dienst bereitgestellt. Die HTTP-Schnittstelle läuft auf Port 8536. Über **http://\<server>:8536/cts/hello** kann die Schnittstelle getestet werden.

Der eigentliche Serverdienst wird von der Anwendung **Cts.Server.Service** bereitgestellt. Die Applikation **Cts.Server.Cmd** dient zum Testen des Servers in der Kommandozeile und ist nur für die Entwicklung ausgelegt.

### Installation
Der Dienst läuft auf Windows-Betriebssystemen (ab Windows 10 bzw. Windows Server 2019) mit installiertem .NET 6 inkl. ASP.NET. Zum Erstellen des Dienstes müssen folgende Aufgaben erfüllt werden. Die Befehle werden teilweise in der PowerShell ausgeführt.

#### Die Einstellungen anpassen
In der **appsettings.json** müssen das Verzeichnis und die SQL Server Datenbank Angaben angepasst werden.

#### Neuen Benutzer erstellen
- PS-Befehl: New-LocalUser -Name CtsServer
- Starkes Passwort vergeben (und ablegen)

#### Dem Benutzer das Recht geben, als Dienst zu starten
- Open the Local Security Policy editor by running secpol.msc.
- Expand the Local Policies node and select User Rights Assignment.
- Open the Log on as a service policy.
- Select Add User or Group.
- Provide the object name (user account) using either of the following approaches:
- Type the user account ({DOMAIN OR COMPUTER NAME\USER}) in the object name field and select OK to add the user to the policy.
- Select Advanced. Select Find Now. Select the user account from the list. Select OK. Select OK again to add the user to the policy.
- Select OK or Apply to accept the changes.

#### Dienst erstellen
\{EXE PATH} gegen das Installations_verzeichnis_ von der EXE-Datei des Dienstes ersetzen
\{EXE FILE PATH} ist der vollständige Pfad der EXE-Datei
\{DOMAIN OR COMPUTER NAME\USER} gegen den <Computernamen>\CtsServer ersetzen

```
$acl = Get-Acl "{EXE PATH}"
$aclRuleArgs = "{DOMAIN OR COMPUTER NAME\USER}", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "{EXE PATH}"
```

New-Service -Name CtsServer -BinaryPathName "{EXE FILE PATH}" -Credential "{DOMAIN OR COMPUTER NAME\USER}" -Description "Server für CTS Stationen des DAH" -DisplayName "gemelo DAH CTS Server" -StartupType Automatic


#### Dienst starten
Der Dienst kann entweder über PS "Start-Service" gestartet werden oder einfach über die Dienste-Verwaltung von Windows.

# Inhalte
Die Inhalte werden auf dem Server abgerufen. Um die Inhalte anzupassen, wird die Import-Applikation verwendet. Diese importiert die Inhalte aus einer Excel-Datei mit mehreren Arbeitsblättern.

## Excel-Datei zum Importieren
Die Excel-Datei besteht aus folgenden Tabellen. Die einzelnen Daten sind selbsterklärend.

### Tabelle *Questions*
In dieser Tabelle werden alle Fragen angelegt. Die *Station ID* gibt an, auf welcher TouchBeamer-Station die Fragen angezeigt werden. Jede Frage muss über eine eindeutige ID verfügen. Fragen können zu Gruppen zusammengefasst werden, was dann Auswirkungen auf die Darstellung auf der [Wall](#ctswall) hat. Es folgen Text, jeweils in Deutsch und English. In *Type* kann angegeben werden, ob es sich nicht um eine klassische Multiple Choice Frage handelt, sondern um eine Sonderfrage handelt (aktuell gibt es nur das Geburtsjahr *YearOfBirth*). Unter *Max Answers* kann eingetragen werden, wie viele Antworten gleichzeitig ausgewählt werden dürfen. Ist dort nichts eingetragen, sind beliebig viele Antworten erlaubt.

Ab Spalte *L* folgen die Angaben für die Darstellung auf der [Wall](#ctswall).

### Tabelle *TouchBeamer*
Diese Tabelle enthält allgemeine Texte und Einstellungen zu den TouchBeamer-Stationen. In der ersten Spalte steht jeweils eine ID für die entsprechende Einstellung. Die zweite Spalte legt fest, für welche Station ID die Werte gelten sollen. Es können also Werte unterschiedlich für unterschiedliche Station IDs festgelegt werden. 

In der dritten und vierten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabelle *DeepDive*
Diese Tabelle enthält allgemeine Texte und Einstellungen zu den DeepDive-Stationen. In der ersten Spalte steht jeweils eine ID für die entsprechende Einstellung.

In der zweiten und dritten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabelle *Wall*
Diese Tabelle enthält allgemeine Texte und Einstellungen zu der Wall-Station. In der ersten Spalte steht jeweils eine ID für die entsprechende Einstellung.

In der zweiten und dritten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabellen, die mit *DD-* starten
Auf die oben genannten Tabellen folgen für jede Frage, für die eine DeepDive-Vertiefung angezeigt werden soll, eine eigene Tabelle mit dem Namen *DD-\<ID der Frage>*.

Diese Tabellen führen die einzelnen Abschnitte der Vertiefungsinformationen auf. Die Abschnitte werden in der DeepDive-Software untereinander dargestellt. Es gibt folgende Abschnittstypen, die in der ersten Spalte ausgewählt werden.

#### Headline, SubHeadline, ParagraphHeadline
Verschiedene Abstufungen von Überschriften

#### Paragraph
Ein Textabschnitt

#### Media
Ein Video oder ein Audiobeitrag

## Mediendateien
Die Mediendateien müssen in dem beim Server hinterlegten Verzeichnis liegen.

### Videos
Die Bitrate der Videos sollten bei maximal 25 Mbit/s liegen, je nach Leistungsfähigkeit der verwendeten Hardware.
Kodierung: H.264
Container: MP4

### Audios
Die Audios sollten als MP3-Dateien vorliegen mit einer Bitrate zwischen 128 und 256 KBit/s.

# Benutzung
Nach Programmstart laden die Applikationen zunächst die aktuelle Konfiguration und die aktuellen Daten vom Server. Anschließend begrüßen die Anwendungen den Besucher mit Anweisungen, was zu tun ist. Bei den Stationen [TouchBeamer](#ctstouchbeamer) und [DeepDive](#ctsdeepDive) muss vor der Benutzung durch den Besucher die RFID-Karte, die der Besucher an der Kasse erhält, aufgelegt werden. Dabei wird auch die Sprache an die vom Besucher an der Kasse ausgewählten Sprache umgestellt.

Die Wall-Software funktioniert ohne Interaktion. Sie wechselt in dem in den [Einstellungen](#tabelle-wall) definiertem Intervall zwischen den Auswertungsseiten.


# Lizenz
Copyright © 2020 bis 2021, Deutsches Auswandererhaus Bremerhaven / gemelo GmbH, Hamburg, Germany

Die vom Auftragnehmer im Rahmen des Projektes erstellten Programmcodes werden im Rahmen der MIT License bereitgestellt.

## MIT license

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


# Credits/Projektpartner

Diese Anwendung ist entstanden im Verbundprojekt museum4punkt0 – Digitale Strategien für das Museum der Zukunft, Teilprojekt *Deutsches Auswandererhaus*. Weitere Informationen: www.museum4punkt0.de.

Das Projekt museum4punkt0 wird gefördert durch die Beauftragte der Bundesregierung für Kultur und Medien aufgrund eines Beschlusses des Deutschen Bundestages.

Auftraggeber: Deutsches Auswandererhaus, Columbusstraße 65, 27568 Bremerhaven

Auftragnehmer: **gemelo GmbH**, Stresemannstraße 375, 22761 Hamburg, Telefon +49-40-3553060
Ansprechpartner: Peer Lessing, info@gemelo.de

Graphische Gestaltung: **Studio Andreas Heller GmbH** Architects & Designers, Am Sandtorkai 48 / Hamburg America Center, 20457 Hamburg
