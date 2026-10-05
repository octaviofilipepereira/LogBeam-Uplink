<p align="center"><img src="docs/images/logo.png" width="96" alt="LogBeam Uplink"></p>

<h1 align="center">LogBeam Uplink</h1>

<p align="center"><b>Cada QSO de N1MM+, WSJT-X, JTDX y Log4OM en su logbook de LogBeam, en el momento en que se registra.</b></p>

<p align="center">
  <a href="https://logbeam.org/uplink/"><img alt="Descargar" src="https://img.shields.io/badge/descargar-logbeam.org%2Fuplink-2f81f7"></a>
  <img alt="Windows 10 y 11" src="https://img.shields.io/badge/Windows-10%20%7C%2011-0078d4">
  <a href="https://www.gnu.org/licenses/gpl-3.0.html"><img alt="Licencia GPL v3" src="https://img.shields.io/badge/licencia-GPL%20v3-blue"></a>
</p>

<p align="center"><a href="README.md">Português</a> · <a href="README.en.md">English</a> · <b>Español</b> · <a href="README.fr.md">Français</a></p>

---

LogBeam Uplink es una aplicación para Windows que se queda en el área de notificación, junto al reloj. Recibe cada QSO en el momento en que se registra en el programa de log y lo envía a su logbook en [LogBeam](https://logbeam.org) y, si lo activa, a ClubLog. El globo 3D de LogBeam se actualiza solo, sin exportar ni importar archivos ADIF.

<p align="center"><a href="https://logbeam.org"><img src="https://logbeam.org/assets/og-image-en.png" width="720" alt="Globo 3D de LogBeam con los QSOs"></a></p>

**Índice:** [Funcionalidades](#funcionalidades) · [Capturas de pantalla](#capturas-de-pantalla) · [Descargar e instalar](#descargar-e-instalar) · [Primeros pasos](#primeros-pasos) · [Manual](#manual) · [Limitaciones](#limitaciones-de-la-versión-250) · [Privacidad](#privacidad) · [Compilar](#compilar) · [Contribuir](#contribuir-y-seguridad) · [Licencia](#licencia)

## Funcionalidades

### Programas de log

| Programa | Cómo se conecta a Uplink | Manual |
|---|---|---|
| **N1MM+** | Configuración automática con un clic. Se hace una copia de seguridad de la configuración de N1MM+ y los destinos que ya existían se mantienen | [N1MM+](docs/MANUAL.es.md#n1mm) |
| **WSJT-X** y **JTDX** | Por el UDP Server del propio programa (puerto 2237). Comparte el puerto con JTAlert y GridTracker mediante multicast | [WSJT-X / JTDX](docs/MANUAL.es.md#wsjt-x--jtdx) |
| **Log4OM** | Por una conexión UDP OUTBOUND con el mensaje ADIF_MESSAGE (puerto 2333) | [Log4OM](docs/MANUAL.es.md#log4om) |

### Destinos

| Funcionalidad | Descripción | Manual |
|---|---|---|
| Varios logbooks de LogBeam | Cada QSO se envía a todos los logbooks activos, por ejemplo el de una expedición y el personal. Cada logbook se activa o desactiva con un clic en el menú del icono | [API LogBeam](docs/MANUAL.es.md#api-logbeam) |
| Probar conexión | Comprueba la API key y muestra de quién es el logbook | [API LogBeam](docs/MANUAL.es.md#api-logbeam) |
| ClubLog en tiempo real | Envío con una Application Password de ClubLog. Si ClubLog rechaza las credenciales, el envío se detiene hasta que se corrijan, para que no se bloquee la IP | [ClubLog](docs/MANUAL.es.md#clublog) |

### Fiabilidad

| Funcionalidad | Descripción | Manual |
|---|---|---|
| Cola sin internet | Los QSOs que no se pueden enviar quedan en una cola, que se conserva al reiniciar Uplink y Windows. Nuevo intento cada minuto, por orden de llegada | [Sin internet](docs/MANUAL.es.md#5-sin-internet) |
| Duplicados | El servidor detecta los QSOs repetidos al minuto y Uplink avisa de que el QSO ya existía | [Avisos](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |
| Puerto ocupado | Aviso cuando otro programa ocupa el puerto de un programa de log | [Solución de problemas](docs/MANUAL.es.md#8-solución-de-problemas) |
| Exportar la sesión | Guarda en un archivo ADIF los QSOs enviados desde que arrancó el servicio | [Exportar la sesión](docs/MANUAL.es.md#6-exportar-la-sesión) |
| Logs diarios | Un archivo de log por día, conservado 30 días, con nivel de detalle ajustable | [Avanzado](docs/MANUAL.es.md#avanzado) |

### En el día a día

| Funcionalidad | Descripción | Manual |
|---|---|---|
| Área de notificación | Cerrar la ventana no cierra el programa. El menú del icono activa y desactiva el servicio y cada logbook | [La ventana y el área de notificación](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |
| Avisos | Errores de envío, QSOs enviados (opcional), duplicados y puertos ocupados | [La ventana y el área de notificación](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |
| ✓ Confirmado por otro LogBeam | Aviso cuando el corresponsal también tiene el QSO en un logbook de LogBeam | [La ventana y el área de notificación](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |
| Iniciar con Windows | Uplink arranca con Windows, sin abrir la ventana | [Avanzado](docs/MANUAL.es.md#avanzado) |
| Una sola instancia | Abrir Uplink de nuevo trae al frente la ventana que ya estaba abierta | [La ventana y el área de notificación](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |
| Actualizaciones | Uplink comprueba al arrancar si hay una versión nueva y avisa | [Actualizaciones](docs/MANUAL.es.md#7-actualizaciones) |
| Cuatro idiomas | Interfaz en portugués, inglés, español y francés, elegido con los botones de la parte superior de la ventana | [La ventana y el área de notificación](docs/MANUAL.es.md#3-la-ventana-y-el-área-de-notificación) |

### Seguridad y privacidad

| Funcionalidad | Descripción | Manual |
|---|---|---|
| Claves cifradas | Las API keys y las contraseñas las cifra Windows, legibles solo por el usuario que las guardó. En la tabla aparecen enmascaradas | [Solución de problemas](docs/MANUAL.es.md#8-solución-de-problemas) |
| Solo este ordenador | Por defecto, Uplink solo acepta paquetes de este ordenador. Recibir de la red local hay que pedirlo | [N1MM+](docs/MANUAL.es.md#n1mm) |
| Datos solo con autorización | Los datos de la instalación y los informes de errores solo se envían con autorización, y se borran del servidor cuando se retira | [Primer arranque](docs/MANUAL.es.md#2-primer-arranque) |
| Instalación sencilla | Sin privilegios de administrador y sin instalar .NET | [Instalar](docs/MANUAL.es.md#1-instalar) |

## Capturas de pantalla

<table>
  <tr>
    <td align="center"><a href="docs/images/es-api.png"><img src="docs/images/es-api.png" width="380" alt="Pestaña API LogBeam"></a><br><sub>Logbooks de destino</sub></td>
    <td align="center"><a href="docs/images/es-n1mm.png"><img src="docs/images/es-n1mm.png" width="380" alt="Pestaña N1MM+"></a><br><sub>N1MM+</sub></td>
  </tr>
  <tr>
    <td align="center"><a href="docs/images/es-wsjtx.png"><img src="docs/images/es-wsjtx.png" width="380" alt="Pestaña WSJT-X / JTDX"></a><br><sub>WSJT-X / JTDX</sub></td>
    <td align="center"><a href="docs/images/es-log4om.png"><img src="docs/images/es-log4om.png" width="380" alt="Pestaña Log4OM"></a><br><sub>Log4OM</sub></td>
  </tr>
  <tr>
    <td align="center"><a href="docs/images/es-avancado.png"><img src="docs/images/es-avancado.png" width="380" alt="Pestaña Avanzado"></a><br><sub>Avanzado</sub></td>
    <td align="center"><a href="docs/images/es-consentimento.png"><img src="docs/images/es-consentimento.png" width="300" alt="Solicitud de autorización"></a><br><sub>Solicitud de autorización en el primer arranque</sub></td>
  </tr>
</table>

## Descargar e instalar

La versión oficial está en **https://logbeam.org/uplink/**, con la huella digital SHA-256 del instalador.

- **Requisitos:** Windows 10 u 11, 64 bits.
- **Instalación:** sin privilegios de administrador y sin necesidad de .NET.
- **Comprobar el instalador:** en PowerShell, `Get-FileHash .\LogBeamUplink-Setup-X.Y.Z.exe`. El resultado debe coincidir con el SHA-256 publicado en la página.

> ⚠️ El programa es gratuito y de código abierto, y se distribuye **sin firma digital**:
> - **SmartScreen:** Windows puede mostrar «Windows protegió su PC». Para continuar: «Más información» → «Ejecutar de todas formas».
> - **Control inteligente de aplicaciones (Windows 11):** puede bloquear el instalador sin opción de continuar. En ese caso hay que desactivarlo en Seguridad de Windows → Control de aplicaciones y navegador → Control inteligente de aplicaciones.

## Primeros pasos

1. En [LogBeam](https://app.logbeam.org), abra el logbook → «Gestionar Logbook» → pestaña «API Keys» y genere una clave.
2. En Uplink, pestaña **API LogBeam** → «Añadir logbook»: pegue el enlace del logbook (o el Instance ID) y la API key → «Probar conexión» → «Guardar».
3. Conecte el programa de log a Uplink:
   - **N1MM+:** pestaña N1MM+ → «Configurar automáticamente», con N1MM+ cerrado.
   - **WSJT-X / JTDX:** File → Settings → Reporting → UDP Server 127.0.0.1, puerto 2237. En JTDX, marque también «Enable sending logged QSO ADIF data».
   - **Log4OM:** conexión UDP OUTBOUND con el mensaje ADIF_MESSAGE, destino 127.0.0.1, puerto 2333 y «Broadcast» desactivado.

> 💡 Si JTAlert o GridTracker usan el puerto de WSJT-X, vea cómo [compartir el puerto mediante multicast](docs/MANUAL.es.md#wsjt-x--jtdx).

## Manual

El manual explica cada pestaña, los avisos, la cola sin internet y la solución de problemas.

| Idioma | Manual |
|---|---|
| Português | [docs/MANUAL.pt.md](docs/MANUAL.pt.md) |
| English | [docs/MANUAL.en.md](docs/MANUAL.en.md) |
| Español | [docs/MANUAL.es.md](docs/MANUAL.es.md) |
| Français | [docs/MANUAL.fr.md](docs/MANUAL.fr.md) |

## Limitaciones de la versión 2.5.0

- Un QSO editado o borrado en el programa de log después de enviado sigue en LogBeam y en ClubLog tal como se envió. La propagación de ediciones y borrados está prevista para la versión 2.6.
- Solo para Windows.

## Privacidad

Los datos de la instalación y los informes de errores solo se envían con autorización del operador, solicitada en el primer arranque y modificable en la pestaña «Avanzado». Nunca se envían QSOs, el nombre del equipo ni el nombre de usuario. Vea la [política de privacidad](https://logbeam.org/#privacy).

## Compilar

<details>
<summary>Instrucciones para compilar Uplink y el instalador</summary>

<br>

Requisitos: Windows 10 o superior y el [SDK de .NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

```
dotnet build LogBeam.sln -c Release
dotnet test LogBeam.sln -c Release
```

Instalador ([Inno Setup 6](https://jrsoftware.org/isinfo.php)): publique primero el ejecutable y después compile `LogBeam.iss`, que lee la versión del ejecutable publicado:

```
dotnet publish src/LogBeam.UI/LogBeam.UI.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
ISCC.exe LogBeam.iss
```

La versión está en un único sitio, `Directory.Build.props`. Las versiones oficiales las compila `.github/workflows/release.yml` al crear una etiqueta `vX.Y.Z`.

**ClubLog:** el envío a ClubLog necesita una clave de aplicación de ClubLog, que no está en el repositorio. Sin ella, la aplicación compila y funciona, pero el envío a ClubLog queda desactivado. Para incluirla en una compilación propia, defina la variable de entorno `CLUBLOG_API_KEY` o cree el archivo `clublog.key` en la raíz del repositorio (no versionado). La clave la solicita a ClubLog quien distribuye la aplicación.

</details>

## Contribuir y seguridad

- Contribuciones: vea [CONTRIBUTING.md](CONTRIBUTING.md).
- Fallos de seguridad: comuníquelos en privado, como se indica en [SECURITY.md](SECURITY.md).

## Licencia

LogBeam Uplink es software libre, distribuido con la [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). El texto completo está en el archivo [LICENSE](LICENSE).

## Aviso

- El software se distribuye sin garantía de ningún tipo.
- Las versiones modificadas son responsabilidad de quien las modifica y distribuye; el autor no responde por ellas.
- Las versiones oficiales son solo las publicadas en https://logbeam.org/uplink/, con el SHA-256 indicado en la página.
- El nombre LogBeam y el icono identifican la versión oficial; las versiones modificadas deben usar otro nombre (GPL v3, sección 7, apartado e).

---

<p align="center">© 2026 Octávio Filipe Pereira Gonçalves (CT7BFV) · <a href="https://logbeam.org">logbeam.org</a></p>
