// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.UI.Localization;

/// <summary>
/// Textos da interface em PT, EN, ES e FR. Cada chave tem obrigatoriamente as quatro línguas
/// (o tuplo não compila sem elas). Português de Portugal, 3.ª pessoa formal.
/// </summary>
public static class L
{
    public static readonly string[] Languages = { "PT", "EN", "ES", "FR" };

    public static string Lang = "PT";

    private static readonly Dictionary<string, (string PT, string EN, string ES, string FR)> _s = new()
    {
        // Tray / service
        ["tray_running_count"]   = ("LogBeam Uplink — {0} QSOs enviados", "LogBeam Uplink — {0} QSOs sent",
                                    "LogBeam Uplink — {0} QSOs enviados", "LogBeam Uplink — {0} QSO envoyés"),
        ["tray_stopped"]         = ("LogBeam Uplink parado", "LogBeam Uplink stopped",
                                    "LogBeam Uplink detenido", "LogBeam Uplink arrêté"),
        ["tray_open"]            = ("Abrir", "Open", "Abrir", "Ouvrir"),
        ["tray_start"]           = ("Iniciar serviço", "Start Service", "Iniciar servicio", "Démarrer le service"),
        ["tray_stop"]            = ("Parar serviço", "Stop Service", "Detener servicio", "Arrêter le service"),
        ["tray_exit"]            = ("Sair", "Exit", "Salir", "Quitter"),
        ["svc_started"]          = ("Serviço iniciado.", "Service started.", "Servicio iniciado.", "Service démarré."),
        ["svc_stopped"]          = ("Serviço parado.", "Service stopped.", "Servicio detenido.", "Service arrêté."),
        ["svc_error"]            = ("Erro no serviço.", "Service error.", "Error en el servicio.", "Erreur du service."),
        ["balloon_qso_sent"]     = ("QSO enviado", "QSO sent", "QSO enviado", "QSO envoyé"),
        ["balloon_qso_failed"]   = ("Falha ao enviar o QSO", "Failed to send QSO",
                                    "Error al enviar el QSO", "Échec de l'envoi du QSO"),
        ["balloon_confirmed_title"] = ("✓ Confirmado por outro LogBeam", "✓ Confirmed by another LogBeam",
                                       "✓ Confirmado por otro LogBeam", "✓ Confirmé par un autre LogBeam"),
        ["balloon_confirmed_text"]  = ("{0} confirmado automaticamente por outro logbook LogBeam!",
                                       "{0} automatically confirmed by another LogBeam logbook!",
                                       "¡{0} confirmado automáticamente por otro logbook LogBeam!",
                                       "{0} confirmé automatiquement par un autre logbook LogBeam !"),

        // Menu
        ["menu_file"]            = ("Ficheiro", "File", "Archivo", "Fichier"),
        ["menu_export_adif"]     = ("Exportar sessão para ADIF...", "Export session to ADIF...",
                                    "Exportar sesión a ADIF...", "Exporter la session en ADIF..."),
        ["status_export_empty"]  = ("Ainda não há QSOs enviados nesta sessão.", "No QSOs sent in this session yet.",
                                    "Todavía no se han enviado QSOs en esta sesión.", "Aucun QSO envoyé dans cette session."),
        ["status_export_ok"]     = ("{0} QSOs exportados.", "{0} QSOs exported successfully.",
                                    "{0} QSOs exportados.", "{0} QSO exportés."),
        ["status_export_failed"] = ("Falha ao exportar ADIF", "Failed to export ADIF",
                                    "Error al exportar ADIF", "Échec de l'export ADIF"),
        ["menu_check_update"]    = ("Verificar actualizações...", "Check for Updates...",
                                    "Buscar actualizaciones...", "Rechercher des mises à jour..."),
        ["update_available_title"] = ("LogBeam Uplink", "LogBeam Uplink", "LogBeam Uplink", "LogBeam Uplink"),
        ["update_available_body"]  = ("Está disponível a versão {0} (tem a {1}).\n\nAbrir a página de download?",
                                      "Version {0} is available (you have {1}).\n\nOpen the download page?",
                                      "Está disponible la versión {0} (tiene la {1}).\n\n¿Abrir la página de descarga?",
                                      "La version {0} est disponible (vous avez la {1}).\n\nOuvrir la page de téléchargement ?"),
        ["update_up_to_date"]      = ("Já tem a versão mais recente.", "You already have the latest version.",
                                      "Ya tiene la versión más reciente.", "Vous avez déjà la dernière version."),
        ["update_check_failed"]    = ("Não foi possível verificar actualizações agora.", "Could not check for updates right now.",
                                      "No ha sido posible buscar actualizaciones ahora.", "Impossible de rechercher des mises à jour pour le moment."),
        ["menu_help"]            = ("Ajuda", "Help", "Ayuda", "Aide"),
        ["menu_about"]           = ("Sobre o LogBeam Uplink...", "About LogBeam Uplink...",
                                    "Acerca de LogBeam Uplink...", "À propos de LogBeam Uplink..."),
        ["about_title"]          = ("Sobre o LogBeam Uplink 2.5", "About LogBeam Uplink 2.5",
                                    "Acerca de LogBeam Uplink 2.5", "À propos de LogBeam Uplink 2.5"),
        ["about_body"]           = (
"LogBeam Uplink 2.5\n" +
"N1MM+ · WSJT-X · Log4OM → globo 3D em tempo real\n\n" +
"Desenvolvido por:\n" +
"  Octávio Filipe Pereira — CT7BFV\n\n" +
"Copyright © 2026 CT7BFV.\n\n" +
"Licença: GNU General Public License v3.0 (GPL-3.0)\n\n" +
"Este programa é software livre: pode redistribuí-lo\n" +
"e/ou modificá-lo nos termos da GNU General Public\n" +
"License publicada pela Free Software Foundation,\n" +
"versão 3 ou (à sua escolha) qualquer versão posterior.\n\n" +
"Distribuído SEM QUALQUER GARANTIA; nem mesmo a garantia\n" +
"implícita de COMERCIALIZAÇÃO ou ADEQUAÇÃO A UM FIM ESPECÍFICO.\n\n" +
"Detalhes: https://www.gnu.org/licenses/gpl-3.0.html"
,
"LogBeam Uplink 2.5\n" +
"N1MM+ · WSJT-X · Log4OM → real-time 3D globe\n\n" +
"Developed by:\n" +
"  Octávio Filipe Pereira — CT7BFV\n\n" +
"Copyright © 2026 CT7BFV.\n\n" +
"License: GNU General Public License v3.0 (GPL-3.0)\n\n" +
"This program is free software: you can redistribute it\n" +
"and/or modify it under the terms of the GNU General\n" +
"Public License as published by the Free Software\n" +
"Foundation, either version 3 or (at your option) any\n" +
"later version.\n\n" +
"Distributed WITHOUT ANY WARRANTY; without even the\n" +
"implied warranty of MERCHANTABILITY or FITNESS FOR A\n" +
"PARTICULAR PURPOSE.\n\n" +
"Details: https://www.gnu.org/licenses/gpl-3.0.html"
,
"LogBeam Uplink 2.5\n" +
"N1MM+ · WSJT-X · Log4OM → globo 3D en tiempo real\n\n" +
"Desarrollado por:\n" +
"  Octávio Filipe Pereira — CT7BFV\n\n" +
"Copyright © 2026 CT7BFV.\n\n" +
"Licencia: GNU General Public License v3.0 (GPL-3.0)\n\n" +
"Este programa es software libre: puede redistribuirlo\n" +
"y/o modificarlo según los términos de la GNU General\n" +
"Public License publicada por la Free Software Foundation,\n" +
"versión 3 o (a su elección) cualquier versión posterior.\n\n" +
"Se distribuye SIN NINGUNA GARANTÍA; ni siquiera la garantía\n" +
"implícita de COMERCIABILIDAD o IDONEIDAD PARA UN FIN DETERMINADO.\n\n" +
"Detalles: https://www.gnu.org/licenses/gpl-3.0.html"
,
"LogBeam Uplink 2.5\n" +
"N1MM+ · WSJT-X · Log4OM → globe 3D en temps réel\n\n" +
"Développé par :\n" +
"  Octávio Filipe Pereira — CT7BFV\n\n" +
"Copyright © 2026 CT7BFV.\n\n" +
"Licence : GNU General Public License v3.0 (GPL-3.0)\n\n" +
"Ce programme est un logiciel libre : vous pouvez le\n" +
"redistribuer et/ou le modifier selon les termes de la\n" +
"GNU General Public License publiée par la Free Software\n" +
"Foundation, version 3 ou (à votre choix) toute version\n" +
"ultérieure.\n\n" +
"Distribué SANS AUCUNE GARANTIE, sans même la garantie\n" +
"implicite de QUALITÉ MARCHANDE ou d'ADÉQUATION À UN\n" +
"USAGE PARTICULIER.\n\n" +
"Détails : https://www.gnu.org/licenses/gpl-3.0.html"),

        // Tabs
        ["tab_station"]          = ("Estação", "Station", "Estación", "Station"),
        ["tab_api"]              = ("API LogBeam", "LogBeam API", "API LogBeam", "API LogBeam"),
        ["tab_hamqth"]           = ("HamQTH", "HamQTH", "HamQTH", "HamQTH"),
        ["tab_advanced"]         = ("Avançado", "Advanced", "Avanzado", "Avancé"),
        ["tab_n1mm"]             = ("N1MM+", "N1MM+", "N1MM+", "N1MM+"),
        ["tab_wsjtx"]            = ("WSJT-X", "WSJT-X", "WSJT-X", "WSJT-X"),

        // N1MM tab
        ["lbl_n1mm_port"]        = ("Porta UDP:", "UDP Port:", "Puerto UDP:", "Port UDP :"),
        ["lbl_n1mm_addr"]        = ("Endereço:", "Address:", "Dirección:", "Adresse :"),
        ["hint_n1mm"]            = ("Porta UDP para onde o N1MM+ envia os contactos (por omissão: 12060). Endereço: 127.0.0.1 aceita só este computador; 0.0.0.0 aceita pacotes de qualquer computador da rede local.",
                                    "UDP port where N1MM+ sends contacts (default: 12060). Address: 127.0.0.1 accepts only this computer; 0.0.0.0 accepts packets from any computer on the local network.",
                                    "Puerto UDP al que N1MM+ envía los contactos (por defecto: 12060). Dirección: 127.0.0.1 acepta solo este ordenador; 0.0.0.0 acepta paquetes de cualquier ordenador de la red local.",
                                    "Port UDP vers lequel N1MM+ envoie les contacts (par défaut : 12060). Adresse : 127.0.0.1 n'accepte que cet ordinateur ; 0.0.0.0 accepte les paquets de tout ordinateur du réseau local."),
        ["btn_n1mm_detect"]      = ("Detectar N1MM+", "Detect N1MM+", "Detectar N1MM+", "Détecter N1MM+"),
        ["btn_n1mm_autoconfig"]  = ("Configurar automaticamente", "Configure automatically",
                                    "Configurar automáticamente", "Configurer automatiquement"),
        ["n1mm_status_not_found"]      = ("N1MM+ não encontrado. Instalação esperada em Documentos\\N1MM Logger+.",
                                          "N1MM+ not found. Expected install at Documents\\N1MM Logger+.",
                                          "N1MM+ no encontrado. Instalación esperada en Documentos\\N1MM Logger+.",
                                          "N1MM+ introuvable. Installation attendue dans Documents\\N1MM Logger+."),
        ["n1mm_status_ok"]             = ("N1MM+ encontrado e já configurado correctamente para o LogBeam.",
                                          "N1MM+ found and already correctly configured for LogBeam.",
                                          "N1MM+ encontrado y ya configurado correctamente para LogBeam.",
                                          "N1MM+ trouvé et déjà correctement configuré pour LogBeam."),
        ["n1mm_status_wrong_port"]     = ("N1MM+ encontrado, mas o envio está configurado para outra porta. Clique em \"Configurar automaticamente\".",
                                          "N1MM+ found, but broadcast is configured for a different port. Click \"Configure automatically\".",
                                          "N1MM+ encontrado, pero el envío está configurado para otro puerto. Haga clic en \"Configurar automáticamente\".",
                                          "N1MM+ trouvé, mais l'envoi est configuré vers un autre port. Cliquez sur « Configurer automatiquement »."),
        ["n1mm_status_not_configured"] = ("N1MM+ encontrado, mas o envio de contactos ainda não está activado.",
                                          "N1MM+ found, but contact broadcast is not enabled yet.",
                                          "N1MM+ encontrado, pero el envío de contactos todavía no está activado.",
                                          "N1MM+ trouvé, mais l'envoi des contacts n'est pas encore activé."),
        ["n1mm_status_ini_missing"]    = ("Não foi possível ler o ficheiro de configuração do N1MM+.",
                                          "Could not read the N1MM+ configuration file.",
                                          "No ha sido posible leer el archivo de configuración de N1MM+.",
                                          "Impossible de lire le fichier de configuration de N1MM+."),
        ["n1mm_status_configured"]     = ("Configuração aplicada. Reinicie o N1MM+ para que tenha efeito.",
                                          "Configuration applied successfully. Restart N1MM+ for it to take effect.",
                                          "Configuración aplicada. Reinicie N1MM+ para que surta efecto.",
                                          "Configuration appliquée. Redémarrez N1MM+ pour qu'elle prenne effet."),
        ["n1mm_status_config_failed"]  = ("Falha ao configurar o N1MM+. Consulte os logs para mais detalhes.",
                                          "Failed to configure N1MM+. Check the logs for details.",
                                          "Error al configurar N1MM+. Consulte los logs para más detalles.",
                                          "Échec de la configuration de N1MM+. Consultez les journaux pour plus de détails."),
        ["n1mm_confirm_title"]  = ("O N1MM+ está aberto", "N1MM+ is running", "N1MM+ está abierto", "N1MM+ est ouvert"),
        ["n1mm_confirm_running"] = ("O N1MM+ está aberto. Se gravar as suas próprias definições, pode substituir esta alteração. Recomenda-se fechar primeiro o N1MM+.\n\nContinuar mesmo assim?",
                                    "N1MM+ is currently open. Changing the configuration now may get overwritten if N1MM+ saves its own settings. It's recommended to close N1MM+ first.\n\nContinue anyway?",
                                    "N1MM+ está abierto. Si guarda su propia configuración, puede sobrescribir este cambio. Se recomienda cerrar N1MM+ primero.\n\n¿Continuar de todos modos?",
                                    "N1MM+ est ouvert. S'il enregistre ses propres paramètres, il peut écraser cette modification. Il est recommandé de fermer N1MM+ d'abord.\n\nContinuer quand même ?"),
        ["lbl_wsjtx_enabled"]   = ("Activar recepção do WSJT-X", "Enable WSJT-X reception",
                                   "Activar recepción de WSJT-X", "Activer la réception de WSJT-X"),
        ["lbl_wsjtx_port"]      = ("Porta UDP do WSJT-X:", "WSJT-X UDP Port:", "Puerto UDP de WSJT-X:", "Port UDP WSJT-X :"),
        ["hint_wsjtx"]          = ("No WSJT-X: File → Settings → Reporting → UDP Server: 127.0.0.1, porta 2237 (valores por omissão). Compatível com o JTDX e outros programas que usem o mesmo protocolo.",
                                   "In WSJT-X: File → Settings → Reporting → UDP Server: 127.0.0.1, port 2237 (the defaults). Compatible with JTDX and other programs using the same protocol.",
                                   "En WSJT-X: File → Settings → Reporting → UDP Server: 127.0.0.1, puerto 2237 (valores por defecto). Compatible con JTDX y otros programas que usen el mismo protocolo.",
                                   "Dans WSJT-X : File → Settings → Reporting → UDP Server : 127.0.0.1, port 2237 (valeurs par défaut). Compatible avec JTDX et les autres programmes utilisant le même protocole."),

        // N1MM help submenu
        ["menu_n1mm_help"]       = ("Configurar o N1MM+...", "Configure N1MM+...", "Configurar N1MM+...", "Configurer N1MM+..."),
        ["n1mm_help_title"]      = ("Como configurar o N1MM+", "How to configure N1MM+",
                                    "Cómo configurar N1MM+", "Comment configurer N1MM+"),
        ["n1mm_help_body"]       = (
"CONFIGURAR O N1MM+ PARA O LOGBEAM UPLINK\n" +
"═══════════════════════════════════════════════════\n\n" +
"Forma mais simples: separador N1MM+ →\n" +
"\"Configurar automaticamente\", com o N1MM+ fechado.\n\n" +
"Para configurar à mão:\n\n" +
"1. No N1MM+: Config → Configure Ports, Mode Control,\n" +
"   Winkey, etc... → separador \"Broadcast Data\".\n\n" +
"2. Marcar \"Contacts\" e indicar o endereço\n" +
"   127.0.0.1:12060.\n\n" +
"3. Carregar em OK e reiniciar o N1MM+.\n\n" +
"4. Fazer um QSO de teste e confirmar que aparece\n" +
"   no Uplink. Os registos ficam na pasta \"logs\"\n" +
"   do Uplink."
,
"CONFIGURE N1MM+ FOR LOGBEAM UPLINK\n" +
"═══════════════════════════════════════════════════\n\n" +
"Easiest: N1MM+ tab → \"Configure automatically\",\n" +
"with N1MM+ closed.\n\n" +
"To configure it manually:\n\n" +
"1. In N1MM+: Config → Configure Ports, Mode Control,\n" +
"   Winkey, etc... → \"Broadcast Data\" tab.\n\n" +
"2. Tick \"Contacts\" and enter the address\n" +
"   127.0.0.1:12060.\n\n" +
"3. Click OK and restart N1MM+.\n\n" +
"4. Make a test QSO and check that it shows up\n" +
"   in Uplink. Logs are kept in Uplink's \"logs\"\n" +
"   folder."
,
"CONFIGURAR N1MM+ PARA LOGBEAM UPLINK\n" +
"═══════════════════════════════════════════════════\n\n" +
"Lo más sencillo: pestaña N1MM+ →\n" +
"\"Configurar automáticamente\", con N1MM+ cerrado.\n\n" +
"Para configurarlo a mano:\n\n" +
"1. En N1MM+: Config → Configure Ports, Mode Control,\n" +
"   Winkey, etc... → pestaña \"Broadcast Data\".\n\n" +
"2. Marque \"Contacts\" e indique la dirección\n" +
"   127.0.0.1:12060.\n\n" +
"3. Pulse OK y reinicie N1MM+.\n\n" +
"4. Haga un QSO de prueba y compruebe que aparece\n" +
"   en Uplink. Los registros se guardan en la carpeta\n" +
"   \"logs\" de Uplink."
,
"CONFIGURER N1MM+ POUR LOGBEAM UPLINK\n" +
"═══════════════════════════════════════════════════\n\n" +
"Le plus simple : onglet N1MM+ →\n" +
"« Configurer automatiquement », avec N1MM+ fermé.\n\n" +
"Pour le configurer manuellement :\n\n" +
"1. Dans N1MM+ : Config → Configure Ports, Mode Control,\n" +
"   Winkey, etc... → onglet « Broadcast Data ».\n\n" +
"2. Cochez « Contacts » et indiquez l'adresse\n" +
"   127.0.0.1:12060.\n\n" +
"3. Cliquez sur OK et redémarrez N1MM+.\n\n" +
"4. Faites un QSO de test et vérifiez qu'il apparaît\n" +
"   dans Uplink. Les journaux sont conservés dans le\n" +
"   dossier « logs » d'Uplink."),

        // Station
        ["lbl_callsign"]         = ("Indicativo:", "Callsign:", "Indicativo:", "Indicatif :"),
        ["lbl_lat"]              = ("Latitude:", "Latitude:", "Latitud:", "Latitude :"),
        ["lbl_lon"]              = ("Longitude:", "Longitude:", "Longitud:", "Longitude :"),
        ["hint_qth"]             = ("Coordenadas GPS do QTH da estação. Ex.: Lisboa → Lat 38.7167 / Lon -9.1333",
                                    "GPS coordinates of the station QTH. E.g. Lisbon → Lat 38.7167 / Lon -9.1333",
                                    "Coordenadas GPS del QTH de la estación. Ej.: Lisboa → Lat 38.7167 / Lon -9.1333",
                                    "Coordonnées GPS du QTH de la station. Ex. : Lisbonne → Lat 38.7167 / Lon -9.1333"),

        // API
        ["lbl_api_url"]          = ("URL base:", "Base URL:", "URL base:", "URL de base :"),
        ["hint_api_key"]         = ("Cada linha é um logbook LogBeam de destino: o QSO é enviado para todos os que estiverem activos (útil em expedições). A API Key é gerada em app.logbeam.org → abrir o logbook → \"Gerir Logbook\" → separador \"API Keys\". Instance ID: os 10 caracteres visíveis no endereço view.php?id=XXXXXXXXXX",
                                    "Each row is a destination LogBeam logbook — the QSO is sent to every enabled one (useful for expeditions). Generate the API Key at app.logbeam.org → open the logbook → \"Manage Logbook\" → \"API Keys\" tab. Instance ID: the 10 characters shown in the URL view.php?id=XXXXXXXXXX",
                                    "Cada fila es un logbook LogBeam de destino: el QSO se envía a todos los que estén activos (útil en expediciones). La API Key se genera en app.logbeam.org → abrir el logbook → \"Gestionar Logbook\" → pestaña \"API Keys\". Instance ID: los 10 caracteres visibles en la dirección view.php?id=XXXXXXXXXX",
                                    "Chaque ligne est un logbook LogBeam de destination : le QSO est envoyé à tous ceux qui sont actifs (utile en expédition). L'API Key se génère sur app.logbeam.org → ouvrir le logbook → « Gérer le logbook » → onglet « API Keys ». Instance ID : les 10 caractères visibles dans l'adresse view.php?id=XXXXXXXXXX"),
        ["lbl_api_timeout"]      = ("Tempo limite (s):", "Timeout (s):", "Tiempo de espera (s):", "Délai d'attente (s) :"),
        ["lbl_api_retry"]        = ("Tentativas:", "Retries:", "Reintentos:", "Tentatives :"),
        ["lbl_api_retry_delay"]  = ("Intervalo (s):", "Retry Delay (s):", "Intervalo (s):", "Intervalle (s) :"),
        ["btn_test_api"]         = ("Testar ligação", "Test Connection", "Probar conexión", "Tester la connexion"),
        ["btn_profile_add"]      = ("Adicionar logbook", "Add Logbook", "Añadir logbook", "Ajouter un logbook"),
        ["btn_profile_remove"]   = ("Remover", "Remove", "Quitar", "Supprimer"),
        ["col_profile_name"]     = ("Nome", "Name", "Nombre", "Nom"),
        ["col_profile_instance"] = ("Instance ID", "Instance ID", "Instance ID", "Instance ID"),
        ["col_profile_key"]      = ("API Key", "API Key", "API Key", "API Key"),

        // HamQTH
        ["hint_hamqth"]          = ("Conta HamQTH (hamqth.com) para obter automaticamente a localização dos correspondentes (gratuita).",
                                    "HamQTH account (hamqth.com) for automatic location lookup of correspondents (free).",
                                    "Cuenta HamQTH (hamqth.com) para obtener automáticamente la ubicación de los corresponsales (gratuita).",
                                    "Compte HamQTH (hamqth.com) pour obtenir automatiquement la position des correspondants (gratuit)."),
        ["lbl_hamqth_user"]      = ("Utilizador:", "Username:", "Usuario:", "Utilisateur :"),
        ["lbl_hamqth_pass"]      = ("Password:", "Password:", "Contraseña:", "Mot de passe :"),
        ["lbl_hamqth_cache"]     = ("Cache (min):", "Cache (min):", "Caché (min):", "Cache (min) :"),

        // ClubLog
        ["tab_clublog"]          = ("ClubLog", "ClubLog", "ClubLog", "ClubLog"),
        ["hint_cl"]              = ("Envia cada QSO automaticamente para o ClubLog, em tempo real.\n" +
                                    "Use uma \"Application Password\" do ClubLog (não a password de acesso).\n" +
                                    "Indicativo: deixe em branco para usar o do separador Estação.",
                                    "Sends each QSO automatically to ClubLog in real time.\n" +
                                    "Use a ClubLog \"Application Password\" (not your login password).\n" +
                                    "Callsign: leave blank to use the one from the Station tab.",
                                    "Envía cada QSO automáticamente a ClubLog en tiempo real.\n" +
                                    "Utilice una \"Application Password\" de ClubLog (no su contraseña de acceso).\n" +
                                    "Indicativo: déjelo en blanco para usar el de la pestaña Estación.",
                                    "Envoie automatiquement chaque QSO à ClubLog en temps réel.\n" +
                                    "Utilisez un « Application Password » de ClubLog (pas votre mot de passe de connexion).\n" +
                                    "Indicatif : laissez vide pour utiliser celui de l'onglet Station."),
        ["lbl_cl_enabled"]       = ("Activar envio em tempo real", "Enable real-time upload",
                                    "Activar envío en tiempo real", "Activer l'envoi en temps réel"),
        ["lbl_cl_email"]         = ("E-mail do ClubLog:", "ClubLog Email:", "Correo de ClubLog:", "E-mail ClubLog :"),
        ["lbl_cl_pass"]          = ("App Password:", "App Password:", "App Password:", "App Password :"),
        ["lbl_cl_call"]          = ("Indicativo (opcional):", "Callsign (optional):", "Indicativo (opcional):", "Indicatif (facultatif) :"),

        // Advanced
        ["lbl_log_level"]        = ("Nível de log:", "Log Level:", "Nivel de log:", "Niveau de journal :"),
        ["lbl_log_path"]         = ("Ficheiro de log:", "Log File:", "Archivo de log:", "Fichier journal :"),

        // Buttons
        ["btn_save"]             = ("Guardar", "Save", "Guardar", "Enregistrer"),
        ["btn_cancel"]           = ("Cancelar", "Cancel", "Cancelar", "Annuler"),
        ["btn_exit"]             = ("Sair", "Exit", "Salir", "Quitter"),

        // Status messages
        ["status_saved"]         = ("Definições guardadas.", "Settings saved successfully.",
                                    "Configuración guardada.", "Paramètres enregistrés."),
        ["status_error"]         = ("Erro ao guardar", "Error saving", "Error al guardar", "Erreur lors de l'enregistrement"),
        ["status_no_url"]        = ("Indique primeiro o URL da API.", "Enter the API URL first.",
                                    "Indique primero la URL de la API.", "Indiquez d'abord l'URL de l'API."),
        ["status_no_credentials"] = ("Indique primeiro o Instance ID e a API Key.", "Enter the Instance ID and API Key first.",
                                     "Indique primero el Instance ID y la API Key.", "Indiquez d'abord l'Instance ID et l'API Key."),
        ["status_testing"]       = ("A testar a ligação...", "Testing connection...",
                                    "Probando la conexión...", "Test de la connexion..."),
        ["status_api_ok"]        = ("Ligação estabelecida — logbook encontrado.", "Connection successful — logbook found.",
                                    "Conexión establecida: logbook encontrado.", "Connexion établie — logbook trouvé."),
        ["status_api_unauthorized"] = ("API Key inválida ou revogada.", "Invalid or revoked API Key.",
                                       "API Key no válida o revocada.", "API Key invalide ou révoquée."),
        ["status_api_not_found"] = ("Logbook não encontrado. Verifique o Instance ID.", "Logbook not found. Check the Instance ID.",
                                    "Logbook no encontrado. Compruebe el Instance ID.", "Logbook introuvable. Vérifiez l'Instance ID."),
        ["status_api_fail"]      = ("Falha na ligação", "Connection failed", "Error de conexión", "Échec de la connexion"),
    };

    public static string Get(string key)
    {
        if (!_s.TryGetValue(key, out var t)) return key;
        return Lang switch
        {
            "EN" => t.EN,
            "ES" => t.ES,
            "FR" => t.FR,
            _    => t.PT
        };
    }

    /// <summary>Chaves com algum texto vazio numa das línguas (deve devolver sempre uma lista vazia).</summary>
    internal static IEnumerable<string> KeysWithMissingText() =>
        _s.Where(kv => new[] { kv.Value.PT, kv.Value.EN, kv.Value.ES, kv.Value.FR }.Any(string.IsNullOrWhiteSpace))
          .Select(kv => kv.Key);
}
