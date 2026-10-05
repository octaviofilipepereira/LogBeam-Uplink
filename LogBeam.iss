// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)
// Ficheiro em UTF-8 com BOM: sem ele, o Inno Setup lê os acentos como ANSI.

#define AppName "LogBeam Uplink"
; Versão lida do LogBeam.exe publicado (origem única: Directory.Build.props). Publicar antes de compilar.
#define AppVersion GetStringFileInfo(SourcePath + "publish\LogBeam.exe", "ProductVersion")
#if AppVersion == ""
  #error Falta publish\LogBeam.exe: correr primeiro o dotnet publish.
#endif
#define AppPublisher "Octávio Filipe Pereira Gonçalves (CT7BFV)"
#define AppExeName "LogBeam.exe"

[Setup]
AppId={{A1E2C3D4-5678-9ABC-DEF0-123456789ABC}
AppName={#AppName}
AppVersion={#AppVersion}
AppVerName={#AppName} {#AppVersion}
AppPublisher={#AppPublisher}
AppPublisherURL=https://logbeam.org
AppSupportURL=https://logbeam.org
AppCopyright=© 2026 Octávio Filipe Pereira Gonçalves
; Instalação por-utilizador (sem admin): a app grava settings.json/logs/queue.json
; junto ao .exe — em Program Files isso falharia para utilizadores sem privilégios.
DefaultDirName={localappdata}\Programs\{#AppName}
DefaultGroupName={#AppName}
DisableProgramGroupPage=yes
OutputDir=installer
OutputBaseFilename=LogBeamUplink-Setup-{#AppVersion}
SetupIconFile=src\LogBeam.UI\Resources\logbeam.ico
UninstallDisplayIcon={app}\{#AppExeName}
Compression=lzma2/ultra64
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
; O Uplink aberto (instância única) é detectado; o instalador pede para o fechar
AppMutex=LogBeamUplink.Instance
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0

[Languages]
; Português: cópia da tradução do Inno Setup com a grafia anterior ao AO90
Name: "portuguese"; MessagesFile: "setup\Portuguese.isl"
Name: "english";    MessagesFile: "compiler:Default.isl"
Name: "spanish";    MessagesFile: "compiler:Languages\Spanish.isl"
Name: "french";     MessagesFile: "compiler:Languages\French.isl"

; Página "programa sem assinatura" (ver [Code]): o instalador e a aplicação não têm assinatura
; digital, e o Windows pode avisar (SmartScreen) ou bloquear (Controlo Inteligente de Aplicações).
[CustomMessages]
portuguese.UnsignedTitle=Programa sem assinatura digital
portuguese.UnsignedSubtitle=Porque é que o Windows pode mostrar avisos
portuguese.UnsignedText=O LogBeam Uplink é gratuito e de código aberto, e é distribuído sem assinatura digital.%n%nPor isso, o Windows pode mostrar «O Windows protegeu o seu PC»; «Mais informações» → «Executar mesmo assim» permite continuar.%n%nNo Windows 11, o Controlo Inteligente de Aplicações pode bloquear o LogBeam Uplink sem dar opção de continuar. Nesse caso é preciso desligá-lo em Segurança do Windows → Controlo de aplicações e do browser → Controlo Inteligente de Aplicações, e mantê-lo desligado enquanto usar o programa.%n%nConfirme que o instalador foi descarregado de https://logbeam.org/uplink/ e que o SHA-256 coincide com o publicado nessa página.
english.UnsignedTitle=Software without a digital signature
english.UnsignedSubtitle=Why Windows may show warnings
english.UnsignedText=LogBeam Uplink is free and open source, and is distributed without a digital signature.%n%nThis is why Windows may show "Windows protected your PC"; "More info" → "Run anyway" lets you continue.%n%nOn Windows 11, Smart App Control may block LogBeam Uplink with no option to continue. In that case it has to be turned off in Windows Security → App & browser control → Smart App Control, and kept off while you use the program.%n%nMake sure the installer was downloaded from https://logbeam.org/uplink/ and that its SHA-256 matches the one published on that page.
spanish.UnsignedTitle=Programa sin firma digital
spanish.UnsignedSubtitle=Por qué Windows puede mostrar avisos
spanish.UnsignedText=LogBeam Uplink es gratuito y de código abierto, y se distribuye sin firma digital.%n%nPor eso, Windows puede mostrar «Windows protegió su PC»; «Más información» → «Ejecutar de todas formas» permite continuar.%n%nEn Windows 11, el Control inteligente de aplicaciones puede bloquear LogBeam Uplink sin opción de continuar. En ese caso hay que desactivarlo en Seguridad de Windows → Control de aplicaciones y navegador → Control inteligente de aplicaciones, y mantenerlo desactivado mientras use el programa.%n%nCompruebe que el instalador se descargó de https://logbeam.org/uplink/ y que su SHA-256 coincide con el publicado en esa página.
french.UnsignedTitle=Logiciel sans signature numérique
french.UnsignedSubtitle=Pourquoi Windows peut afficher des avertissements
french.UnsignedText=LogBeam Uplink est gratuit et open source, et il est distribué sans signature numérique.%n%nC'est pourquoi Windows peut afficher « Windows a protégé votre ordinateur » ; « Informations complémentaires » → « Exécuter quand même » permet de continuer.%n%nSous Windows 11, le contrôle intelligent des applications peut bloquer LogBeam Uplink sans possibilité de continuer. Dans ce cas, il faut le désactiver dans Sécurité Windows → Contrôle des applications et du navigateur → Contrôle intelligent des applications, et le laisser désactivé pendant l'utilisation du programme.%n%nVérifiez que le programme d'installation a été téléchargé depuis https://logbeam.org/uplink/ et que son SHA-256 correspond à celui publié sur cette page.

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "publish\*"; DestDir: "{app}"; Excludes: "*.pdb"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{group}\{cm:UninstallProgram,{#AppName}}"; Filename: "{uninstallexe}"
Name: "{userdesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent

[Registry]
; "Iniciar com o Windows" é ligado pela aplicação; a desinstalação apaga o valor
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "LogBeam Uplink"; Flags: uninsdeletevalue dontcreatekey

[UninstallDelete]
Type: filesandordirs; Name: "{app}\logs"
Type: files; Name: "{app}\telemetry.json"

[Code]
procedure InitializeWizard;
begin
  CreateOutputMsgPage(wpWelcome,
    CustomMessage('UnsignedTitle'),
    CustomMessage('UnsignedSubtitle'),
    CustomMessage('UnsignedText'));
end;
