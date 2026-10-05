<p align="center"><img src="images/logo.png" width="72" alt="LogBeam Uplink"></p>

<h1 align="center">LogBeam Uplink 2.5 manual</h1>

<p align="center"><a href="MANUAL.pt.md">Português</a> · <b>English</b> · <a href="MANUAL.es.md">Español</a> · <a href="MANUAL.fr.md">Français</a></p>

LogBeam Uplink is a Windows application that receives QSOs from **N1MM+**, **WSJT-X**, **JTDX** and **Log4OM** as soon as they are logged and sends them to one or several [LogBeam](https://logbeam.org) logbooks and, if you turn it on, to **ClubLog**.

**Contents:** [1. Installing](#1-installing) · [2. First start](#2-first-start) · [3. The window and the notification area](#3-the-window-and-the-notification-area) · [4. Tabs](#4-tabs) · [5. Without internet](#5-without-internet) · [6. Exporting the session](#6-exporting-the-session) · [7. Updates](#7-updates) · [8. Troubleshooting](#8-troubleshooting) · [9. Uninstalling](#9-uninstalling)

## 1. Installing

1. Download the installer from **https://logbeam.org/uplink/** and check the SHA-256 fingerprint published on that page. In PowerShell, in the file's folder: `Get-FileHash .\LogBeamUplink-Setup-2.5.0.exe`.
2. Run the installer. It needs neither administrator rights nor .NET: it installs to `%LOCALAPPDATA%\Programs\LogBeam Uplink` and adds a Start menu shortcut.

> ⚠️ The program is free and open source, and is distributed **without a digital signature**. Because of this:
> - **SmartScreen:** if "Windows protected your PC" appears, click "More info" → "Run anyway".
> - **Smart App Control (Windows 11):** may block the installer with no option to continue. In that case it has to be turned off in Windows Security → App & browser control → Smart App Control.

## 2. First start

The first time, Uplink asks whether it may send **installation data** and **error reports** to LogBeam. The window lists exactly what is sent; nothing is sent without "Allow". You can change your choice at any time in the **Advanced** tab. If you withdraw your permission, the data already sent is deleted from the server.

<p align="center"><img src="images/en-consentimento.png" width="420" alt="Permission request"></p>

## 3. The window and the notification area

- Closing the window with the **X** does not end the program: Uplink keeps working, with its icon in the notification area. To quit, use the **Exit** button or the icon menu.
- Double-clicking the Uplink icon in the notification area opens the window.
- Uplink only opens once: starting it again brings the window that is already open to the front.
- The **PT · EN · ES · FR** buttons at the top of the window change the language.

**Icon menu** (right-click the Uplink icon):

| Option | What for |
|---|---|
| Open | Shows the window |
| Send to | Turns each logbook on or off with one click (e.g. to switch to a contest logbook). The change is saved immediately |
| Start Service · Stop Service | Turns receiving and sending QSOs on or off |
| Exit | Ends the program |

**Notifications (next to the clock):**

| Notification | When |
|---|---|
| Failed to send QSO | Always |
| QSO sent | Only with "Notify for every QSO sent" (Advanced tab) |
| QSO was already in the logbook | The QSO was already there (the server detects duplicates to the minute). Only with the same option |
| ✓ Confirmed by another LogBeam | The other station also has the QSO in a LogBeam logbook |
| Port in use | A logging program cannot be received because another program is using the port (see [section 8](#8-troubleshooting)) |
| ClubLog: credentials rejected | ClubLog rejected the email, App Password or callsign (see [ClubLog](#clublog)). Shown only once |

## 4. Tabs

After changing anything, click **Save**: the service restarts with the new settings. **Cancel** restores what was saved.

### Station

**Callsign:** used for ClubLog uploads, unless another one is set in the ClubLog tab. Correspondent locations are handled by the LogBeam server.

### LogBeam API

Each row in the grid is a destination LogBeam logbook. The QSO is sent to every one with **Use logbook** ticked (for example, an expedition's logbook and your own).

<p align="center"><img src="images/en-api.png" width="520" alt="LogBeam API tab"></p>

1. **Add Logbook**.
2. **Name:** free text, to recognise the logbook in the icon menu.
3. **Instance ID:** the 10 characters of the `view.php?id=XXXXXXXXXX` address, or the full logbook link (Uplink extracts the identifier).
4. **API Key:** generated at app.logbeam.org → open the logbook → "Manage Logbook" → "API Keys" tab. It is 64 characters long.
5. **Test Connection:** checks the key and shows whose logbook it is ("Connection successful — logbook of CT7XXX").
6. **Save**.

Keys are masked in the grid (only the last 4 characters are shown); **Show keys** shows them in full. While a cell is being edited, the key is shown in full.

**Base URL**, **Timeout**, **Retries** and **Retry Delay** do not need to be changed.

### N1MM+

<p align="center"><img src="images/en-n1mm.png" width="520" alt="N1MM+ tab"></p>

- **Configure automatically:** with N1MM+ closed, sets N1MM+ to send contacts to Uplink. A backup copy of the N1MM+ configuration file is made before changing it, and any destinations already there are kept.
- **Detect N1MM+:** shows whether N1MM+ is already configured.
- **UDP Port** (12060 by default) and **Address** (127.0.0.1 = this computer only; 0.0.0.0 = any computer on the local network).

If another program (e.g. Log4OM) already receives N1MM+ on port 12060, use another port here (e.g. 12061) and click "Configure automatically": N1MM+ will send to both.

To configure it by hand, see **Help → Configure N1MM+...**

### WSJT-X / JTDX

<p align="center"><img src="images/en-wsjtx.png" width="520" alt="WSJT-X / JTDX tab"></p>

1. Tick **Enable WSJT-X / JTDX reception**.
2. In WSJT-X: File → Settings → Reporting → UDP Server **127.0.0.1**, port **2237** (the defaults).
3. In JTDX: the same settings, and also tick **"Enable sending logged QSO ADIF data"**.

> 💡 **With JTAlert or GridTracker on the same port:** two programs cannot receive from the same ordinary port. Use a **multicast** address, for example **239.255.0.1**, in WSJT-X/JTDX (UDP Server), in those programs and in Uplink's **Address** field. That way they all receive the same packets. In multicast mode, Uplink only accepts packets from this computer.

### Log4OM

<p align="center"><img src="images/en-log4om.png" width="520" alt="Log4OM tab"></p>

1. Tick **Enable Log4OM reception**.
2. In Log4OM: Settings → Program Configuration → Software Integration → Connections. Add a **UDP OUTBOUND** connection with the **ADIF_MESSAGE** message, destination **127.0.0.1**, port **2333** and **"Broadcast" turned off**. Save and restart Log4OM.

### ClubLog

1. Tick **Enable real-time upload**.
2. **ClubLog Email** and **App Password**: an "Application Password" created on ClubLog, not your website login password.
3. **Callsign (optional):** if blank, the one in the Station tab is used; if that is blank too, the logging program's.

It receives QSOs from every program (N1MM+, WSJT-X, JTDX and Log4OM) and uses the same offline queue as the logbooks. If ClubLog rejects the email, App Password or callsign, Uplink warns you once (a Windows notification and a red message in this tab) and stops uploading to ClubLog, because ClubLog blocks the IP address of anyone who keeps trying with wrong credentials. QSOs stay in the queue and are sent once the details are corrected and saved.

### Advanced

<p align="center"><img src="images/en-avancado.png" width="520" alt="Advanced tab"></p>

| Option | What for |
|---|---|
| Log Level | How much information goes into the log file. "Information" is enough day to day; "Debug" to investigate a problem |
| Log File | Where the log is kept (by default, Uplink's `logs` folder) |
| Notify for every QSO sent | Notification also for QSOs sent successfully |
| Start with Windows, without opening the window | Uplink starts with Windows, with only its icon in the notification area |
| Send installation data and error reports | The permission asked at first start; "What is sent?" shows the list |

## 5. Without internet

If a QSO cannot be sent (no network, server unavailable), it is queued and Uplink retries every minute, in order of arrival. The queue survives restarting Uplink and Windows.

Errors that retrying will not fix, such as a revoked API key or a deleted logbook, are not queued: the failure notification appears and the reason is written to the log.

## 6. Exporting the session

**File → Export session to ADIF...** saves to an ADIF file the QSOs sent successfully since the service started.

## 7. Updates

Uplink checks at start-up whether a new version is available and tells you. You can also check in **Help → Check for Updates...** New versions are at https://logbeam.org/uplink/.

## 8. Troubleshooting

| Symptom | What to do |
|---|---|
| "Port in use" | Another program uses the same port. WSJT-X/JTDX: use multicast ([section 4](#wsjt-x--jtdx)). N1MM+: use another port and "Configure automatically" |
| "Invalid API Key" or "Invalid Instance ID" when saving | Check the key (64 characters) and the logbook link; use "Test Connection" |
| QSOs do not arrive | Check the logging program's settings ([section 4](#4-tabs)) and that the service is started (icon menu). Set the log level to "Debug", make a QSO and look at the log file |
| A QSO was corrected or deleted in the logging program | In version 2.5.0, edits and deletions do not reach LogBeam or ClubLog. This is planned for 2.6 |
| Windows blocks the installer or the program | See the notes on SmartScreen and Smart App Control ([section 1](#1-installing)) |

**Files**, in the installation folder (`%LOCALAPPDATA%\Programs\LogBeam Uplink`):

| File | Contents |
|---|---|
| `settings.json` | Settings. API keys and passwords are encrypted, readable only by the Windows user who saved them |
| `queue.json` | QSOs waiting to be sent |
| `logs\` | One log file per day, kept for 30 days |
| `telemetry.json` | Error reports waiting to be sent (only with permission) |

## 9. Uninstalling

Windows Settings → Apps → LogBeam Uplink → Uninstall. The uninstaller removes the program, the logs and the start-with-Windows entry. The settings (`settings.json`) and the queue (`queue.json`) stay in the folder so that a reinstall can reuse them; to remove them, delete the `%LOCALAPPDATA%\Programs\LogBeam Uplink` folder.

---

© 2026 Octávio Filipe Pereira Gonçalves (CT7BFV) · [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html)
