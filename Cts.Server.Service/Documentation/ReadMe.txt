Zum Erstellen des Dienstes müssen folgende Aufgaben erfüllt werden. Die Befehle werden in der PowerShell ausgeführt.

* Die Einstellungen anpassen *

- In der appsettings.json müssen das Verzeichnis und die SQL Server Datenbank Angaben angepasst werden.


* Neuen Benutzer erstellen *

- PS-Befehl: New-LocalUser -Name CtsServer
- Starkes Passwort vergeben (und ablegen)


* Dem Benutzer das Recht geben, als Dienst zu starten *

- Open the Local Security Policy editor by running secpol.msc.
- Expand the Local Policies node and select User Rights Assignment.
- Open the Log on as a service policy.
- Select Add User or Group.
- Provide the object name (user account) using either of the following approaches:
- Type the user account ({DOMAIN OR COMPUTER NAME\USER}) in the object name field and select OK to add the user to the policy.
- Select Advanced. Select Find Now. Select the user account from the list. Select OK. Select OK again to add the user to the policy.
- Select OK or Apply to accept the changes.


* Dienst erstellen *

{EXE PATH} gegen das Installations_verzeichnis_ von der EXE-Datei des Dienstes ersetzen
{EXE FILE PATH} ist der vollständige Pfad der EXE-Datei
{DOMAIN OR COMPUTER NAME\USER} gegen den <Computernamen>\CtsServer ersetzen

$acl = Get-Acl "{EXE PATH}"
$aclRuleArgs = "{DOMAIN OR COMPUTER NAME\USER}", "Read,Write,ReadAndExecute", "ContainerInherit,ObjectInherit", "None", "Allow"
$accessRule = New-Object System.Security.AccessControl.FileSystemAccessRule($aclRuleArgs)
$acl.SetAccessRule($accessRule)
$acl | Set-Acl "{EXE PATH}"

New-Service -Name CtsServer -BinaryPathName "{EXE FILE PATH}" -Credential "{DOMAIN OR COMPUTER NAME\USER}" -Description "Server für CTS Stationen des DAH" -DisplayName "gemelo DAH CTS Server" -StartupType Automatic


* Dienst starten *

Der Dienst kann entweder über PS "Start-Service" gestartet werden oder einfach über die Dienste-Verwaltung von Windows.