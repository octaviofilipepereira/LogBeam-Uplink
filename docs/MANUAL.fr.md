<p align="center"><img src="images/logo.png" width="72" alt="LogBeam Uplink"></p>

<h1 align="center">Manuel de LogBeam Uplink 2.5</h1>

<p align="center"><a href="MANUAL.pt.md">Português</a> · <a href="MANUAL.en.md">English</a> · <a href="MANUAL.es.md">Español</a> · <b>Français</b></p>

LogBeam Uplink est une application pour Windows qui reçoit les QSO de **N1MM+**, **WSJT-X**, **JTDX** et **Log4OM** dès qu'ils sont enregistrés et les envoie vers un ou plusieurs logbooks [LogBeam](https://logbeam.org) et, si vous l'activez, vers **ClubLog**.

**Sommaire :** [1. Installer](#1-installer) · [2. Premier démarrage](#2-premier-démarrage) · [3. La fenêtre et la zone de notification](#3-la-fenêtre-et-la-zone-de-notification) · [4. Onglets](#4-onglets) · [5. Sans internet](#5-sans-internet) · [6. Exporter la session](#6-exporter-la-session) · [7. Mises à jour](#7-mises-à-jour) · [8. Dépannage](#8-dépannage) · [9. Désinstaller](#9-désinstaller)

## 1. Installer

1. Téléchargez l'installateur sur **https://logbeam.org/uplink/** et vérifiez l'empreinte SHA-256 publiée sur la page. Dans PowerShell, dans le dossier du fichier : `Get-FileHash .\LogBeamUplink-Setup-2.5.0.exe`.
2. Lancez l'installateur. Il ne demande ni droits d'administrateur ni .NET : il s'installe dans `%LOCALAPPDATA%\Programs\LogBeam Uplink` et crée un raccourci dans le menu Démarrer.

> ⚠️ Le programme est gratuit et open source, et il est distribué **sans signature numérique**. C'est pourquoi :
> - **SmartScreen :** si « Windows a protégé votre ordinateur » apparaît, cliquez sur « Informations complémentaires » → « Exécuter quand même ».
> - **Contrôle intelligent des applications (Windows 11) :** il peut bloquer l'installateur sans possibilité de continuer. Il faut alors le désactiver dans Sécurité Windows → Contrôle des applications et du navigateur → Contrôle intelligent des applications.

## 2. Premier démarrage

La première fois, Uplink demande s'il peut envoyer à LogBeam les **données d'installation** et les **rapports d'erreurs**. La fenêtre énumère exactement ce qui est envoyé ; rien n'est envoyé sans « Autoriser ». Ce choix peut être modifié à tout moment dans l'onglet **Avancé**. Si vous retirez l'autorisation, les données déjà envoyées sont supprimées du serveur.

<p align="center"><img src="images/fr-consentimento.png" width="420" alt="Demande d'autorisation"></p>

## 3. La fenêtre et la zone de notification

- Fermer la fenêtre avec le **X** ne quitte pas le programme : Uplink continue de fonctionner, avec son icône dans la zone de notification. Pour quitter, utilisez le bouton **Quitter** ou le menu de l'icône.
- Un double-clic sur l'icône d'Uplink, dans la zone de notification, ouvre la fenêtre.
- Uplink ne s'ouvre qu'une fois : le relancer ramène au premier plan la fenêtre déjà ouverte.
- Les boutons **PT · EN · ES · FR**, en haut de la fenêtre, changent la langue.

**Menu de l'icône** (clic droit sur l'icône d'Uplink) :

| Option | À quoi sert-elle |
|---|---|
| Ouvrir | Affiche la fenêtre |
| Envoyer vers | Active ou désactive chaque logbook d'un clic (p. ex. pour passer au logbook d'un concours). La modification est enregistrée immédiatement |
| Démarrer le service · Arrêter le service | Active ou désactive la réception et l'envoi des QSO |
| Quitter | Ferme le programme |

**Notifications (à côté de l'horloge) :**

| Notification | Quand |
|---|---|
| Échec de l'envoi du QSO | Toujours |
| QSO envoyé | Seulement avec « Notifier chaque QSO envoyé » (onglet Avancé) |
| Le QSO existait déjà dans le logbook | Le QSO y était déjà (le serveur détecte les doublons à la minute près). Seulement avec la même option |
| ✓ Confirmé par un autre LogBeam | Le correspondant a aussi le QSO dans un logbook LogBeam |
| Port occupé | Un logiciel de log ne peut pas être reçu, car un autre logiciel occupe le port (voir la [section 8](#8-dépannage)) |

## 4. Onglets

Après toute modification, cliquez sur **Enregistrer** : le service redémarre avec les nouveaux réglages. **Annuler** rétablit ce qui était enregistré.

### Station

**Indicatif :** utilisé pour l'envoi vers ClubLog, sauf si un autre est indiqué dans l'onglet ClubLog. La position des correspondants est gérée par le serveur LogBeam.

### API LogBeam

Chaque ligne du tableau est un logbook LogBeam de destination. Le QSO est envoyé à tous ceux pour lesquels **Utiliser le logbook** est coché (par exemple, le logbook d'une expédition et le vôtre).

<p align="center"><img src="images/fr-api.png" width="520" alt="Onglet API LogBeam"></p>

1. **Ajouter un logbook**.
2. **Nom :** libre, pour reconnaître le logbook dans le menu de l'icône.
3. **Instance ID :** les 10 caractères de l'adresse `view.php?id=XXXXXXXXXX`, ou le lien complet du logbook (Uplink en extrait l'identifiant).
4. **API Key :** générée sur app.logbeam.org → ouvrir le logbook → « Gérer le logbook » → onglet « API Keys ». Elle compte 64 caractères.
5. **Tester la connexion :** vérifie la clé et indique à qui appartient le logbook (« Connexion établie — logbook de CT7XXX. »).
6. **Enregistrer**.

Les clés sont masquées dans le tableau (seuls les 4 derniers caractères sont visibles) ; **Afficher les clés** les montre en entier. Pendant la modification d'une cellule, la clé apparaît en entier.

**URL de base**, **Délai d'attente**, **Tentatives** et **Intervalle** n'ont pas besoin d'être modifiés.

### N1MM+

<p align="center"><img src="images/fr-n1mm.png" width="520" alt="Onglet N1MM+"></p>

- **Configurer automatiquement :** N1MM+ étant fermé, configure N1MM+ pour envoyer les contacts à Uplink. Une copie de sauvegarde du fichier de configuration de N1MM+ est faite avant la modification, et les destinations déjà présentes sont conservées.
- **Détecter N1MM+ :** indique si N1MM+ est déjà configuré.
- **Port UDP** (12060 par défaut) et **Adresse** (127.0.0.1 = cet ordinateur seulement ; 0.0.0.0 = tout ordinateur du réseau local).

Si un autre logiciel (p. ex. Log4OM) reçoit déjà N1MM+ sur le port 12060, utilisez ici un autre port (p. ex. 12061) et cliquez sur « Configurer automatiquement » : N1MM+ enverra aux deux.

Pour le configurer manuellement, voir le menu **Aide → Configurer N1MM+...**

### WSJT-X / JTDX

<p align="center"><img src="images/fr-wsjtx.png" width="520" alt="Onglet WSJT-X / JTDX"></p>

1. Cochez **Activer la réception de WSJT-X / JTDX**.
2. Dans WSJT-X : File → Settings → Reporting → UDP Server **127.0.0.1**, port **2237** (les valeurs par défaut).
3. Dans JTDX : les mêmes réglages, en cochant aussi **« Enable sending logged QSO ADIF data »**.

> 💡 **Avec JTAlert ou GridTracker sur le même port :** deux logiciels ne peuvent pas recevoir sur le même port ordinaire. Utilisez une adresse **multicast**, par exemple **239.255.0.1**, dans WSJT-X/JTDX (UDP Server), dans ces logiciels et dans le champ **Adresse** d'Uplink. Ainsi, tous reçoivent les mêmes paquets. En multicast, Uplink n'accepte que les paquets de cet ordinateur.

### Log4OM

<p align="center"><img src="images/fr-log4om.png" width="520" alt="Onglet Log4OM"></p>

1. Cochez **Activer la réception de Log4OM**.
2. Dans Log4OM : Settings → Program Configuration → Software Integration → Connections. Ajoutez une connexion **UDP OUTBOUND** avec le message **ADIF_MESSAGE**, destination **127.0.0.1**, port **2333** et **« Broadcast » désactivé**. Enregistrez et redémarrez Log4OM.

### ClubLog

1. Cochez **Activer l'envoi en temps réel**.
2. **E-mail ClubLog** et **App Password** : un « Application Password » créé sur ClubLog, pas le mot de passe de connexion au site.
3. **Indicatif (facultatif) :** vide, c'est celui de l'onglet Station qui est utilisé ; s'il est vide lui aussi, celui du logiciel de log.

ClubLog reçoit les QSO de tous les logiciels (N1MM+, WSJT-X, JTDX et Log4OM) et utilise la même file d'attente hors ligne que les logbooks. Si ClubLog refuse les identifiants, Uplink cesse d'envoyer jusqu'à ce qu'ils soient corrigés et enregistrés : ClubLog bloque l'adresse IP de quiconque insiste avec des identifiants erronés.

### Avancé

<p align="center"><img src="images/fr-avancado.png" width="520" alt="Onglet Avancé"></p>

| Option | À quoi sert-elle |
|---|---|
| Niveau de journal | Quantité d'informations dans le fichier journal. « Information » suffit au quotidien ; « Debug » pour étudier un problème |
| Fichier journal | Emplacement du journal (par défaut, le dossier `logs` d'Uplink) |
| Notifier chaque QSO envoyé | Notification aussi pour les QSO envoyés avec succès |
| Démarrer avec Windows, sans ouvrir la fenêtre | Uplink démarre avec Windows, avec seulement son icône dans la zone de notification |
| Envoyer les données d'installation et les rapports d'erreurs | L'autorisation du premier démarrage ; « Qu'est-ce qui est envoyé ? » affiche la liste |

## 5. Sans internet

Si un QSO ne peut pas être envoyé (pas de réseau, serveur indisponible), il est mis en file d'attente et Uplink réessaie chaque minute, dans l'ordre d'arrivée. La file d'attente est conservée au redémarrage d'Uplink et de Windows.

Les erreurs qu'un nouvel envoi ne résout pas, comme une API Key révoquée ou un logbook supprimé, ne sont pas mises en file d'attente : la notification d'échec apparaît et la raison est écrite dans le journal.

## 6. Exporter la session

**Fichier → Exporter la session en ADIF...** enregistre dans un fichier ADIF les QSO envoyés avec succès depuis le démarrage du service.

## 7. Mises à jour

Uplink vérifie au démarrage si une nouvelle version est disponible et vous prévient. Vous pouvez aussi vérifier dans **Aide → Rechercher des mises à jour...** Les nouvelles versions sont sur https://logbeam.org/uplink/.

## 8. Dépannage

| Symptôme | Que faire |
|---|---|
| « Port occupé » | Un autre logiciel utilise le même port. WSJT-X/JTDX : utilisez le multicast ([section 4](#wsjt-x--jtdx)). N1MM+ : utilisez un autre port et « Configurer automatiquement » |
| « API Key invalide » ou « Instance ID invalide » à l'enregistrement | Vérifiez la clé (64 caractères) et le lien du logbook ; utilisez « Tester la connexion » |
| Les QSO n'arrivent pas | Vérifiez les réglages du logiciel de log ([section 4](#4-onglets)) et que le service est démarré (menu de l'icône). Réglez le niveau de journal sur « Debug », faites un QSO et consultez le fichier journal |
| Un QSO a été corrigé ou supprimé dans le logiciel de log | Dans la version 2.5.0, les modifications et suppressions n'arrivent ni à LogBeam ni à ClubLog. C'est prévu pour la 2.6 |
| Windows bloque l'installateur ou le programme | Voir les avertissements sur SmartScreen et le Contrôle intelligent des applications ([section 1](#1-installer)) |

**Fichiers**, dans le dossier d'installation (`%LOCALAPPDATA%\Programs\LogBeam Uplink`) :

| Fichier | Contenu |
|---|---|
| `settings.json` | Réglages. Les API Keys et les mots de passe sont chiffrés, lisibles uniquement par l'utilisateur Windows qui les a enregistrés |
| `queue.json` | QSO en attente d'envoi |
| `logs\` | Un fichier journal par jour, conservés 30 jours |
| `telemetry.json` | Rapports d'erreurs en attente d'envoi (seulement avec autorisation) |

## 9. Désinstaller

Paramètres Windows → Applications → LogBeam Uplink → Désinstaller. Le programme de désinstallation supprime le programme, les journaux et le démarrage avec Windows. Les réglages (`settings.json`) et la file d'attente (`queue.json`) restent dans le dossier pour qu'une réinstallation les reprenne ; pour les supprimer, effacez le dossier `%LOCALAPPDATA%\Programs\LogBeam Uplink`.

---

© 2026 Octávio Filipe Pereira Gonçalves (CT7BFV) · [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html)
