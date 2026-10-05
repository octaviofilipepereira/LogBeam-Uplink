# Segurança · Security · Seguridad · Sécurité

[Português](#português) · [English](#english) · [Español](#español) · [Français](#français)

---

## Português

### Versões suportadas

Só a versão mais recente publicada em https://logbeam.org/uplink/ recebe correcções de segurança.

### Comunicar uma falha

**Não abra uma *issue* pública** para falhas de segurança. Use a comunicação privada do GitHub: separador **Security** deste repositório → **Report a vulnerability**. Só o autor vê o relatório.

Inclua, se possível:

- a versão do LogBeam Uplink e do Windows;
- os passos para reproduzir a falha;
- o efeito (por exemplo, QSOs falsos num logbook, exposição de uma API key ou de uma password);
- uma proposta de correcção, se a tiver.

A recepção é confirmada em até 7 dias. A correcção é publicada numa versão nova, e a falha só é descrita publicamente depois disso. Se quiser, o seu indicativo ou nome aparece nas notas da versão.

### O que está no âmbito

- A aplicação LogBeam Uplink e o seu instalador (este repositório).
- Falhas no servidor do LogBeam (logbeam.org, app.logbeam.org, api.logbeam.org) podem ser comunicadas pelo mesmo canal.

### Boas práticas já seguidas

- As API keys e as passwords ficam cifradas no `settings.json`, só legíveis pelo utilizador do Windows que as gravou.
- Os receptores UDP escutam por omissão só neste computador (127.0.0.1). Escutar na rede local tem de ser pedido expressamente.
- A chave de aplicação do ClubLog não está no código.

---

## English

### Supported versions

Only the latest release published at https://logbeam.org/uplink/ receives security fixes.

### Reporting a vulnerability

**Do not open a public issue** for security problems. Use GitHub private reporting: this repository's **Security** tab → **Report a vulnerability**. Only the author sees the report.

If possible, include:

- the LogBeam Uplink and Windows versions;
- the steps to reproduce it;
- the impact (for example, fake QSOs in a logbook, exposure of an API key or a password);
- a suggested fix, if you have one.

Receipt is acknowledged within 7 days. The fix is published in a new release, and the issue is only described publicly after that. If you wish, your callsign or name is credited in the release notes.

### Scope

- The LogBeam Uplink application and its installer (this repository).
- Issues in the LogBeam server (logbeam.org, app.logbeam.org, api.logbeam.org) may be reported through the same channel.

### Practices already in place

- API keys and passwords are encrypted in `settings.json`, readable only by the Windows user who saved them.
- UDP receivers listen only on this computer (127.0.0.1) by default. Listening on the local network has to be requested explicitly.
- The ClubLog application key is not in the code.

---

## Español

### Versiones con soporte

Solo la versión más reciente publicada en https://logbeam.org/uplink/ recibe correcciones de seguridad.

### Comunicar un fallo

**No abra una *issue* pública** para fallos de seguridad. Use la comunicación privada de GitHub: pestaña **Security** de este repositorio → **Report a vulnerability**. Solo el autor ve el informe.

Incluya, si es posible:

- la versión de LogBeam Uplink y de Windows;
- los pasos para reproducir el fallo;
- el efecto (por ejemplo, QSOs falsos en un logbook, exposición de una API key o de una contraseña);
- una propuesta de corrección, si la tiene.

La recepción se confirma en un plazo de 7 días. La corrección se publica en una versión nueva, y el fallo solo se describe públicamente después. Si lo desea, su indicativo o nombre aparece en las notas de la versión.

### Alcance

- La aplicación LogBeam Uplink y su instalador (este repositorio).
- Los fallos en el servidor de LogBeam (logbeam.org, app.logbeam.org, api.logbeam.org) pueden comunicarse por el mismo canal.

### Buenas prácticas ya aplicadas

- Las API keys y las contraseñas se guardan cifradas en `settings.json`, legibles solo por el usuario de Windows que las guardó.
- Los receptores UDP escuchan por defecto solo en este ordenador (127.0.0.1). Escuchar en la red local hay que pedirlo expresamente.
- La clave de aplicación de ClubLog no está en el código.

---

## Français

### Versions prises en charge

Seule la dernière version publiée sur https://logbeam.org/uplink/ reçoit des correctifs de sécurité.

### Signaler une faille

**N'ouvrez pas d'*issue* publique** pour une faille de sécurité. Utilisez le signalement privé de GitHub : onglet **Security** de ce dépôt → **Report a vulnerability**. Seul l'auteur voit le signalement.

Indiquez, si possible :

- la version de LogBeam Uplink et de Windows ;
- les étapes pour reproduire la faille ;
- l'impact (par exemple, de faux QSO dans un logbook, l'exposition d'une API Key ou d'un mot de passe) ;
- une proposition de correction, si vous en avez une.

La réception est confirmée sous 7 jours. Le correctif est publié dans une nouvelle version, et la faille n'est décrite publiquement qu'ensuite. Si vous le souhaitez, votre indicatif ou votre nom figure dans les notes de version.

### Périmètre

- L'application LogBeam Uplink et son installateur (ce dépôt).
- Les failles du serveur LogBeam (logbeam.org, app.logbeam.org, api.logbeam.org) peuvent être signalées par le même canal.

### Bonnes pratiques déjà en place

- Les API Keys et les mots de passe sont chiffrés dans `settings.json`, lisibles uniquement par l'utilisateur Windows qui les a enregistrés.
- Les récepteurs UDP n'écoutent par défaut que sur cet ordinateur (127.0.0.1). L'écoute sur le réseau local doit être demandée explicitement.
- La clé d'application ClubLog n'est pas dans le code.
