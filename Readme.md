# Critical Thinking Stations (CTS)
Partizipative Medienstationen **Critical Thinking Stations** f�r das *Deutsche Auswandererhaus* in Bremerhaven


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

Diese auf Basis des eigenen gemelo Exponatframeworks mit C#/WPF/XAML errichteten .NET 6 Applikationen bilden im *Deutschen Auswandererhaus (DAH)* interaktive Medienstationen, die dem Besucher auf dem Ausstellungsrundgang kritische Fragen stellen, die beantwortet und zentral ausgewertet werden. Diese Auswertung ist f�r alle Besucher sichtbar und l�dt zu weiteren Gedanken ein.

Die Besucher melden sich an den Stationen mit der im DAH verwendeten RFID-Karte an.

Zun�chst werden den Besuchern an den sogenannten TouchBeamter-Stationen Fragen zum Themenkomplex Ein- und Auswanderung gestellt. Diese Stationen sind in historischen Kulissen wie alten Koffern eingearbeitet und arbeiten mit einem auf Lidar basierenden Touchsystem.

An den DeepDive-Stationen k�nnen die Besucher zu einzelnen Fragen vertiefende Informationen abrufen - zum Beispiel in Form von Videos.

Die Ergebnisse der Ausstellung werden auf einer gro�en Videowall in Form von Diagrammen pr�sentiert.

Dieses Critical Thinking Stations Projekt ist entstanden im Verbundprojekt 
**museum4punkt0** - Digitale Strategien f�r das Museum der Zukunft
Teilprojekt *Deutsches Auswandererhaus* - Migrationsgeschichte digital erleben. 

Das Projekt museum4punkt0 wird gef�rdert durch die Beauftragte der Bundesregierung f�r Kultur und Medien aufgrund eines Beschlusses des Deutschen Bundestages.

Weitere Informationen: [museum4punkt0](https://www.museum4punkt0.de)

# Installation

## Hardware / Betriebssystem
Die Software l�uft auf einem handels�blichen PC unter dem Betriebssystem Microsoft Windows 10 oder 11. Sie ben�tigt das Framework [Microsoft .NET 6.0.2 oder h�her](https://dotnet.microsoft.com/en-us/download). An die Hardware werden keine speziellen Anforderungen gestellt, am rechnenintensivsten ist die Darstellung von Videos. F�r die TouchBeamer-Stationen werden LIDAR-Sensoren [Hokuyo URG-04LX-UG01](https://www.hokuyo-aut.jp/search/single.php?serial=166) ben�tigt. Sowohl DeepDive als auch TouchBeamer-Stationen ben�tigen RFID-Leser. Hier k�nnen unterschiedliche Varianten zum Einsatz kommen (s. Startparameter).

## Schriften
Folgende Schriftarten m�ssen auf dem PC installiert sein:
* Franzika OT
* Weissenhof Grotesk

jeweils in verschiedenen Schriftschnitten. Die Schriften liegen im Verzeichnis Fonts. Dabei sind die Lizenzen der Schriftarten zu beachten. Die Schriftarten sind nicht Teil dieses Projekts und stehen deshalb nicht unter einer freien Lizenz zur Verf�gung.

## RFID-Simulation
Die Anwendungen k�nnen auch ohne RFID-Leser genutzt werden. Dazu kann �ber Tastenkombinationen das Auflegen einer RFID-Karte simuliert werden. Das funktioniert auch �ber Fernwartung, selbst wenn ein physikalischer RFID-Leser eingerichtet wurde.

Dazu werden die Tasten **A** bis **D** zusammen mit der **Strg**-Taste genutzt. **Strg-Minus** wiederum entspricht dem Entfernen der Karte.


# Einzelapplikationen
Die Installation besteht aus mehreren Einzelapplikationen, die im Folgenden beschriebenen werden:

1. [Touchbeamer](#ctstouchbeamer)
2. [DeepDive](#ctsdeepdive)
3. [Wall (Auswertung)](#ctswall)
3. [Import](#ctsimport)

## Cts.TouchBeamer
Die TouchBeamer-Stationen sind der Eintrittspunkt in das System. Hier kann der Besucher Fragen zu verschiedenen Bereichen beantworten. Die Stationen zeigen dabei unterschiedliche Inhalte/Fragen. Die Bedienung kann optional �ber einen LIDAR basierten Touch erfolgen.

![Screenshot TouchBeamer 1](/Documentation/Screenshots/ScreenTouchBeamer01.jpg)
![Screenshot TouchBeamer 2](/Documentation/Screenshots/ScreenTouchBeamer02.jpg)

### Startparameter

#### -station-id:\<station-id>
Gibt die sogenannte Stations-ID an, die festlegt, welche Inhalte die Station anzeigt (s. unten). Diese Angabe ist **obligatorisch**, da die Station ansonsten keine Inhalte anzeigen k�nnte.

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -rfid:\<serial-port>
Legt die serielle Schnittstelle f�r den RFID-Leser fest, also z. B. *COM3*. Wird diese Angabe nicht gemacht, kann das Einlesen von RFID-Karten nur per Tastatur zu Testzwecken simuliert werden. Es werden verschiedene Hersteller von RFID-Lesern unterst�tzt (siehe die n�chsten Parameter). Per Default wird ein Leser von Feig verwendet.

#### -feig
Legt den Typ des RFID-Lesers auf FEIG fest.

#### -tagscan
Legt den Typ des RFID-Lesers auf TAGscan fest (z. B. TAGscan 003 Industry).

#### -winmate
Legt den Typ des RFID-Lesers auf WinMate fest. Dieser RFID-Leser wird in den Tablets mit integriertem RFID-Leser im DAH verwendet.

#### -rfidpos:\<rfid-position>
Gibt an, auf welcher Stelle der Bildschirmschoner f�r das Einlesen der RFID-Karten verweisen soll, also wo im tats�chlichen Exponat der Leser verbaut wurde. M�gliche Parameter sind:
*Right*, *Left*, *BottomRight*, *BottomLeft*, *TopRight*.

#### -lidar:\<serial-port>
Legt die serielle Schnittstelle fest, die f�r den LIDAR-Sensor genutzt wird, also z. B. *COM5*. Wird diese Angabe nicht gemacht, wird auch kein LIDAR-Touch verwendet.

#### -s
Legt fest, dass die Applikation automatisch auf die Bildschirmgr��e skaliert wird.

#### -c
Legt fest, dass die Anzeige des Mauscursors unterdr�ckt wird.


### Sichtbaren Ausschnitt festlegen
Die Stationen projezieren das Bild auf Gegenst�nde. Dabei kann nicht immer der volle Projektionsbereich f�r die Darstellung genutzt werden. Deshalb kann man den f�r die Darstellung verwendeten Bereich festlegen.

Mit **F4** kann man einen Rahmen anzeigen lassen, der den sichtbaren Bereich eingrenzt. Ist dieser Bereich sichtbar l�sst er sich mit den **Cursortasten** ver�ndern. Die Cursortasten �ndern dabei immer den entsprechend Rand, also �ndert die **Cursor-Hoch-Taste** zum Beispiel den oberen Rand.

Mit gleichzeitiger **Strg**-Taste wird der Bereich verkleinert, sonst vergr��ert. Mit **Shift** werden die gr��ere �nderungen (10-fach) durchgef�hrt.

Die Einstellungen zum Ausschnitt werden automatisch gespeichert.


### LIDAR-Kalibrierung
Um die LIDAR-Touch-Funktion nutzen zu k�nnen, muss diese Kalibriert werden. Dazu dienen einige Tastaturbefehle.

Mit **F6** startet der Kalibrierungsmodus f�r den LIDAR-Touch. Es wird eine Visualisierung der vom LIDAR erkannten Punkte dargestellt. Die Darstellung der Visualisierung kann auf folgende Arten angepasst werden:

- **V** � Spiegelt die Sensor-Darstellung vertikal
- **H** � Spiegelt die Sensor-Darstellung horizontal

Solange mit der Kalibrierung noch nicht begonnen wurde, kann der Messbereich des LIDAR-Sensors, der �berhaupt ber�cksichtigt wird folgenderma�en angepasst werden:

- **Q** und **W** � Stellt den Start-Winkel ein, der ber�cksichtigt wird (zusammen mit **Shift** �ndert sich der Wert schneller)
- **A** und **S** � Stellt den End-Winkel ein, der ber�cksichtigt wird (zusammen mit **Shift** �ndert sich der Wert schneller)
- **Y** und **X** � Stellt die maximale Distanz ein, die ber�cksichtig wird (zusammen mit **Shift** �ndert sich der Wert schneller)
- **0** bis **9** � Stellt maximale Reichweiten f�r 10 gleichbreite Segmente des Messbereichs ein. Damit k�nnen St�rungen durch Erhebungen au�erhalb des eigentlich Touchbereichs entfernt werden. Zusammen mit **Strg** wird der ausgeschlossene Bereich verkleinert. Mit **Shift** �ndert sich der Wert schneller.

F�r die eigentliche Kalibrierung werden der Reihe nach drei Punkte abgefragt. Diese Punkte m�ssen jeweils per Finger "ber�hrt" werden. Dabei muss der Finger die einzig erkannte Ber�hung darstellen (kann auf der Visualisierung kontrolliert werden). Mit **N** wechselt man zum n�chsten Kalibrierungspunkt. Nach dem letzten Punkt kann die Kalibrierung noch einmal kontrolliert werden und mit einer weiteren Druck auf **N** wird sie gespeichert.

## Cts.DeepDive
Die DeepDive-Stationen sind die Vertiefungsstationen der Installation. Hier bekommt der Besucher noch einmal seine bisherigen Antworten gezeigt und kann zu einigen Fragen vertiefende Informationen abrufen.

![Screenshot DeepDive 1](/Documentation/Screenshots/ScreenDeepDive01.jpg)
![Screenshot DeepDive 2](/Documentation/Screenshots/ScreenDeepDive02.jpg)
![Screenshot DeepDive 3](/Documentation/Screenshots/ScreenDeepDive03.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -rfid:\<serial-port>
Legt die serielle Schnittstelle f�r den RFID-Leser fest, also z. B. *COM3*. Wird diese Angabe nicht gemacht, kann das Einlesen von RFID-Karten nur per Tastatur zu Testzwecken simuliert werden. Es werden verschiedene Hersteller von RFID-Lesern unterst�tzt (siehe die n�chsten Parameter). Per Default wird ein Leser von Feig verwendet.

#### -feig
Legt den Typ des RFID-Lesers auf FEIG fest.

#### -tagscan
Legt den Typ des RFID-Lesers auf TAGscan fest (z. B. TAGscan 003 Industry).

#### -winmate
Legt den Typ des RFID-Lesers auf WinMate fest. Dieser RFID-Leser wird in den Tablets mit integriertem RFID-Leser im DAH verwendet.

#### -rfidpos:\<rfid-position>
Gibt an, auf welcher Stelle der Bildschirmschoner f�r das Einlesen der RFID-Karten verweisen soll, also wo im tats�chlichen Exponat der Leser verbaut wurde. M�gliche Parameter sind:
*Right*, *Left*, *BottomRight*, *BottomLeft*, *TopRight*.

#### -16to10
�ndert die Darstellung auf 16:10. �berlicherweise wird ein Bildschirmverh�ltnis von 16:9 verwendet. L�uft die Software jedoch auf den Tablets vom DAH sollte das Bildschirmverh�ltnis auf 16:10 ge�ndert werden.

#### -showrfid
Zeigt ein RFID-Symbol am oberen rechten Rand an. Diese Darstellung wird f�r die Tablets verwendet, da diese in der oberen rechten Ecke einen RFID-Leser verbaut haben.

#### -s
Legt fest, dass die Applikation automatisch auf die Bildschirmgr��e skaliert wird.

#### -c
Legt fest, dass die Anzeige des Mauscursors unterdr�ckt wird.

## Cts.Wall
Die Wall-Station zeigt die Auswertungen zu den einzelnen Fragen an. Dazu ruft sie die Ergebnisse am Server ab und stellt die Ergebnisse in Form von Diagrammen f�r die Fragen dar. Da nicht alle Auswertungen auf eine Bildschirmseite passen, wechselt die Darstellung.

![Screenshot Wall 1](/Documentation/Screenshots/ScreenWall01.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -16to9
Normalerweise ist die Darstellung der Software f�r eine Videowall im Format 20:9 ausgelegt (5 x 4 Videowall-Displays). Um die Software auch auf Bildschirmen im verbreiteten 16:9-Format nutzen zu k�nnen, kann die Darstellung mit diesem Parameter umgestellt werden.

#### -demoresults
Um die Darstellung zu testen, ohne dass schon eine ausreichende Anzahl von Antworten zur Verf�gung steht, k�nnen zuf�llige Beispielresultate mit diesem Parameter genutzt werden.

### Anpassen der Darstellung
Mit folgenden Tastaturbefehlen kann die Darstellung der Auswertungen auf der Videowall optimiert werden, damit keine Texte oder Balken durch einen Steg zwischen Videowall-Screens aufgeteilt werden.

#### F4
Mit **F4** kann ein Raster angezeigt werden, das dem Raster der Videowallscreens (5 x 4) entspricht.

#### F5
Mit **F5** kann auf die n�chste Auswertungsseite gewechselt werden.

#### 0 bis 9 bzw. A bis K
Mit diesen Tasten kann die Auswertung von einer Frage ausgew�hlt werden. Die Taste **1** entspricht der ersten Auswertung, **9** der zehnten Auswertung, **A** der elften usw.

#### + und -
Mit **+** und **-** kann der vertikale Abstand der vorher ausgew�hlten Auswertung zur vorherigen Auswertung vergr��ert bzw. verkleinert werden.

## Cts.Import
Die Import-Applikation wird nicht direkt in der Ausstellung verwendet, sondern dient dazu die Inhalte f�r die Fragen und Vertiefungsinformationen auf dem Server einzupflegen. Dazu wird eine Excel-Tabelle eingelesen (s. [Inhalte](#Inhalte)). 

Au�erdem k�nnen mit der Anwendung die Eingaben der Besucher gel�scht werden oder zumindest Eingaben, die �ber die simulierten RFID-Karten eingegeben wurden (s. [RFID-Simulation](##RFID-Simulation)).

![Screenshot Import 1](/Documentation/Screenshots/ScreenImport01.jpg)

### Startparameter

#### -server:\<server-address>
Gibt die Netzwerkadresse des Servers (s. unten) an. Die Netzwerkadresse kann dabei entweder eine IP-Adresse oder ein DNS-Name sein. Wird der Server nicht angegeben, wird der Serverdienst auf dem lokalen Rechner erwartet.

#### -database:\<database-server>
Gibt den Namen des Microsoft SQL-Datenbank-Servers, der f�r das CMS verwendet wird, an.

# Server
Die Serversoftware basiert auf einer Microsoft SQL-Datenbank und einer per HTTP zugreifbaren HTTP-Schnittstelle, �ber die die Stationen zum einen die Inhalte abfragen k�nnen und zum anderen auch die Ergebnisse der Besucher an den Server melden k�nnen.

## Datenbank
Die Datenbank basiert auf dem [Microsoft SQL Server](https://www.microsoft.com/de-de/sql-server) ab Version 2019. Die kostenlose Express-Version ist dabei ausreichend.

Die Datenbank kann mit dem SQL-Script unter **Cts.Database/Scripts** angelegt werden.

## Server-Dienst
Die HTTP-Schnittstelle wird �ber einen Windows-Server-Dienst bereitgestellt. Die HTTP-Schnittstelle l�uft auf Port 8536. �ber **http://\<server>:8536/cts/hello** kann die Schnittstelle getestet werden.

Der eigentliche Serverdienst wird von der Anwendung **Cts.Server.Service** bereitgestellt. Die Applikation **Cts.Server.Cmd** dient zum Testen des Servers in der Kommandozeile und ist nur f�r die Entwicklung ausgelegt.

### Installation
Der Dienst l�uft auf Windows-Betriebssystemen (ab Windows 10 bzw. Windows Server 2019) mit installiertem .NET 6 inkl. ASP.NET. Zum Erstellen des Dienstes m�ssen folgende Aufgaben erf�llt werden. Die Befehle werden teilweise in der PowerShell ausgef�hrt.

#### Die Einstellungen anpassen
In der **appsettings.json** m�ssen das Verzeichnis und die SQL Server Datenbank Angaben angepasst werden.

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
\{EXE FILE PATH} ist der vollst�ndige Pfad der EXE-Datei
\{DOMAIN OR COMPUTER NAME\USER} gegen den <Computernamen>\CtsServer ersetzen

```
$acl = Get-Acl "{EXE PATH}"
$aclRuleArgs = "{DOMAIN OR COMPUTER NAME\USER}", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "{EXE PATH}"
```

New-Service -Name CtsServer -BinaryPathName "{EXE FILE PATH}" -Credential "{DOMAIN OR COMPUTER NAME\USER}" -Description "Server f�r CTS Stationen des DAH" -DisplayName "gemelo DAH CTS Server" -StartupType Automatic


#### Dienst starten
Der Dienst kann entweder �ber PS "Start-Service" gestartet werden oder einfach �ber die Dienste-Verwaltung von Windows.

# Inhalte
Die Inhalte werden auf dem Server abgerufen. Um die Inhalte anzupassen, wird die Import-Applikation verwendet. Diese importiert die Inhalte aus einer Excel-Datei mit mehreren Arbeitsbl�ttern.

## Excel-Datei zum Importieren
Die Excel-Datei besteht aus folgenden Tabellen. Die einzelnen Daten sind selbsterkl�rend.

### Tabelle *Questions*
In dieser Tabelle werden alle Fragen angelegt. Die *Station ID* gibt an, auf welcher TouchBeamer-Station die Fragen angezeigt werden. Jede Frage muss �ber eine eindeutige ID verf�gen. Fragen k�nnen zu Gruppen zusammengefasst werden, was dann Auswirkungen auf die Darstellung auf der [Wall](#ctswall) hat. Es folgen Text, jeweils in Deutsch und English. In *Type* kann angegeben werden, ob es sich nicht um eine klassische Multiple Choice Frage handelt, sondern um eine Sonderfrage handelt (aktuell gibt es nur das Geburtsjahr *YearOfBirth*). Unter *Max Answers* kann eingetragen werden, wie viele Antworten gleichzeitig ausgew�hlt werden d�rfen. Ist dort nichts eingetragen, sind beliebig viele Antworten erlaubt.

Ab Spalte *L* folgen die Angaben f�r die Darstellung auf der [Wall](#ctswall).

### Tabelle *TouchBeamer*
Diese Tabelle enth�lt allgemeine Texte und Einstellungen zu den TouchBeamer-Stationen. In der ersten Spalte steht jeweils eine ID f�r die entsprechende Einstellung. Die zweite Spalte legt fest, f�r welche Station ID die Werte gelten sollen. Es k�nnen also Werte unterschiedlich f�r unterschiedliche Station IDs festgelegt werden. 

In der dritten und vierten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabelle *DeepDive*
Diese Tabelle enth�lt allgemeine Texte und Einstellungen zu den DeepDive-Stationen. In der ersten Spalte steht jeweils eine ID f�r die entsprechende Einstellung.

In der zweiten und dritten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabelle *Wall*
Diese Tabelle enth�lt allgemeine Texte und Einstellungen zu der Wall-Station. In der ersten Spalte steht jeweils eine ID f�r die entsprechende Einstellung.

In der zweiten und dritten Spalte sind die deutschen und englischen Texte bzw. in der dritten Spalte ein Wert, falls es sich nicht um einen Text handelt.

### Tabellen, die mit *DD-* starten
Auf die oben genannten Tabellen folgen f�r jede Frage, f�r die eine DeepDive-Vertiefung angezeigt werden soll, eine eigene Tabelle mit dem Namen *DD-\<ID der Frage>*.

Diese Tabellen f�hren die einzelnen Abschnitte der Vertiefungsinformationen auf. Die Abschnitte werden in der DeepDive-Software untereinander dargestellt. Es gibt folgende Abschnittstypen, die in der ersten Spalte ausgew�hlt werden.

#### Headline, SubHeadline, ParagraphHeadline
Verschiedene Abstufungen von �berschriften

#### Paragraph
Ein Textabschnitt

#### Media
Ein Video oder ein Audiobeitrag

## Mediendateien
Die Mediendateien m�ssen in dem beim Server hinterlegten Verzeichnis liegen.

### Videos
Die Bitrate der Videos sollten bei maximal 25 Mbit/s liegen, je nach Leistungsf�higkeit der verwendeten Hardware.
Kodierung: H.264
Container: MP4

### Audios
Die Audios sollten als MP3-Dateien vorliegen mit einer Bitrate zwischen 128 und 256 KBit/s.

# Benutzung
Nach Programmstart laden die Applikationen zun�chst die aktuelle Konfiguration und die aktuellen Daten vom Server. Anschlie�end begr��en die Anwendungen den Besucher mit Anweisungen, was zu tun ist. Bei den Stationen [TouchBeamer](#ctstouchbeamer) und [DeepDive](#ctsdeepDive) muss vor der Benutzung durch den Besucher die RFID-Karte, die der Besucher an der Kasse erh�lt, aufgelegt werden. Dabei wird auch die Sprache an die vom Besucher an der Kasse ausgew�hlten Sprache umgestellt.

Die Wall-Software funktioniert ohne Interaktion. Sie wechselt in dem in den [Einstellungen](#tabelle-wall) definiertem Intervall zwischen den Auswertungsseiten.


# Lizenz
Copyright � 2020 bis 2021, Deutsches Auswandererhaus Bremerhaven / gemelo GmbH, Hamburg, Germany

Die vom Auftragnehmer im Rahmen des Projektes erstellten Programmcodes werden im Rahmen der MIT License bereitgestellt.

## MIT license

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.


# Credits/Projektpartner

Diese Anwendung ist entstanden im Verbundprojekt museum4punkt0 � Digitale Strategien f�r das Museum der Zukunft, Teilprojekt *Deutsches Auswandererhaus*. Weitere Informationen: www.museum4punkt0.de.

Das Projekt museum4punkt0 wird gef�rdert durch die Beauftragte der Bundesregierung f�r Kultur und Medien aufgrund eines Beschlusses des Deutschen Bundestages.

Auftraggeber: Deutsches Auswandererhaus, Columbusstra�e 65, 27568 Bremerhaven

Auftragnehmer: **gemelo GmbH**, Stresemannstra�e 375, 22761 Hamburg, Telefon +49-40-3553060
Ansprechpartner: Peer Lessing, info@gemelo.de

Graphische Gestaltung: **Studio Andreas Heller GmbH** Architects & Designers, Am Sandtorkai 48 / Hamburg America Center, 20457 Hamburg
