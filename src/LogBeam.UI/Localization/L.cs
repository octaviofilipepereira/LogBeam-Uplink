// Code & Powered by: Octávio Filipe Pereira Gonçalves / CT7BFV
// © 2026 Octávio Filipe Pereira Gonçalves
// Licença GPL v3 (https://www.gnu.org/licenses/gpl-3.0.pt-br.html | https://www.gnu.org/licenses/gpl-3.0.html.en)

namespace LogBeam.UI.Localization;

public static class L
{
    public static string Lang = "PT";

    private static readonly Dictionary<string, (string PT, string EN)> _s = new()
    {
        // Tray / service
        ["tray_running_count"]   = ("LogBeam Uplink — {0} QSOs enviados", "LogBeam Uplink — {0} QSOs sent"),
        ["tray_stopped"]         = ("LogBeam Uplink parado",             "LogBeam Uplink stopped"),
        ["tray_open"]            = ("Abrir",                            "Open"),
        ["tray_start"]           = ("Iniciar Serviço",                  "Start Service"),
        ["tray_stop"]            = ("Parar Serviço",                    "Stop Service"),
        ["tray_exit"]            = ("Sair",                             "Exit"),
        ["svc_started"]          = ("Serviço iniciado.",                "Service started."),
        ["svc_stopped"]          = ("Serviço parado.",                  "Service stopped."),
        ["svc_error"]            = ("Erro no serviço.",                 "Service error."),
        ["balloon_qso_sent"]     = ("QSO enviado",                      "QSO sent"),
        ["balloon_qso_failed"]   = ("Falha ao enviar QSO",              "Failed to send QSO"),
        ["balloon_confirmed_title"] = ("✓ Confirmado por outro LogBeam", "✓ Confirmed by another LogBeam"),
        ["balloon_confirmed_text"]  = ("{0} confirmado automaticamente por outro logbook LogBeam!",
                                       "{0} automatically confirmed by another LogBeam logbook!"),

        // Menu
        ["menu_file"]            = ("Ficheiro",                          "File"),
        ["menu_export_adif"]     = ("Exportar sessão para ADIF...",      "Export session to ADIF..."),
        ["status_export_empty"]  = ("Ainda não há QSOs enviados nesta sessão.", "No QSOs sent in this session yet."),
        ["status_export_ok"]     = ("{0} QSOs exportados com sucesso.",  "{0} QSOs exported successfully."),
        ["status_export_failed"] = ("Falha ao exportar ADIF",            "Failed to export ADIF"),
        ["menu_check_update"]    = ("Verificar Actualizações...",       "Check for Updates..."),
        ["update_available_title"] = ("LogBeam Uplink",                "LogBeam Uplink"),
        ["update_available_body"]  = ("Está disponível a versão {0} (tens a {1}).\n\nAbrir a página de download?",
                                      "Version {0} is available (you have {1}).\n\nOpen the download page?"),
        ["update_up_to_date"]      = ("Já tens a versão mais recente.", "You already have the latest version."),
        ["update_check_failed"]    = ("Não foi possível verificar actualizações agora.", "Could not check for updates right now."),
        ["menu_help"]            = ("Ajuda",                             "Help"),
        ["menu_about"]           = ("Sobre o LogBeam Uplink...",        "About LogBeam Uplink..."),
        ["about_title"]          = ("Sobre o LogBeam Uplink 2.5",       "About LogBeam Uplink 2.5"),
        ["about_body"]           = (
"LogBeam Uplink 2.5\n" +
"N1MM+ → API REST → Globo 3D em tempo real\n\n" +
"Desenvolvido por:\n" +
"  Octávio Filipe Pereira — CT7BFV\n\n" +
"Copyright © 2026 CT7BFV.\n\n" +
"Licença: GNU General Public License v3.0 (GPL-3.0)\n\n" +
"Este programa é software livre: podes redistribuí-lo\n" +
"e/ou modificá-lo sob os termos da GNU General Public\n" +
"License publicada pela Free Software Foundation,\n" +
"versão 3 ou (à tua escolha) qualquer versão posterior.\n\n" +
"Distribuído SEM QUALQUER GARANTIA; sem mesmo a garantia\n" +
"implícita de COMERCIALIZAÇÃO ou ADEQUAÇÃO A UM FIM.\n\n" +
"Detalhes: https://www.gnu.org/licenses/gpl-3.0.html"
,
"LogBeam Uplink 2.5\n" +
"N1MM+ → REST API → Real-time 3D Globe\n\n" +
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
"Details: https://www.gnu.org/licenses/gpl-3.0.html"),

        // Tabs
        ["tab_station"]          = ("Estação",                          "Station"),
        ["tab_api"]              = ("API LogBeam",                      "API LogBeam"),
        ["tab_hamqth"]           = ("HamQTH",                           "HamQTH"),
        ["tab_advanced"]         = ("Avançado",                         "Advanced"),
        ["tab_n1mm"]             = ("N1MM+",                            "N1MM+"),
        ["tab_wsjtx"]            = ("WSJT-X",                           "WSJT-X"),

        // N1MM tab
        ["lbl_n1mm_port"]        = ("Porta UDP:",                       "UDP Port:"),
        ["lbl_n1mm_addr"]        = ("Endereço:",                        "Address:"),
        ["hint_n1mm"]            = ("Porta UDP onde o N1MM+ faz broadcast. Por defeito: 12060. Endereço: 127.0.0.1 aceita só este computador; 0.0.0.0 aceita pacotes de qualquer computador da rede local.",
                                    "UDP port where N1MM+ broadcasts. Default: 12060. Address: 127.0.0.1 accepts only this computer; 0.0.0.0 accepts packets from any computer on the local network."),
        ["btn_n1mm_detect"]      = ("Detectar N1MM+",                   "Detect N1MM+"),
        ["btn_n1mm_autoconfig"]  = ("Configurar automaticamente",       "Configure automatically"),
        ["n1mm_status_not_found"]      = ("N1MM+ não encontrado. Instalação esperada em Documentos\\N1MM Logger+.",
                                          "N1MM+ not found. Expected install at Documents\\N1MM Logger+."),
        ["n1mm_status_ok"]             = ("N1MM+ encontrado e já configurado correctamente para o LogBeam.",
                                          "N1MM+ found and already correctly configured for LogBeam."),
        ["n1mm_status_wrong_port"]     = ("N1MM+ encontrado, mas o broadcast está configurado para outra porta. Clica \"Configurar automaticamente\".",
                                          "N1MM+ found, but broadcast is configured for a different port. Click \"Configure automatically\"."),
        ["n1mm_status_not_configured"] = ("N1MM+ encontrado, mas o broadcast de contactos ainda não está activado.",
                                          "N1MM+ found, but contact broadcast is not enabled yet."),
        ["n1mm_status_ini_missing"]    = ("Não foi possível ler o ficheiro de configuração do N1MM+.",
                                          "Could not read the N1MM+ configuration file."),
        ["n1mm_status_configured"]     = ("Configuração aplicada com sucesso. Reinicia o N1MM+ para ter efeito.",
                                          "Configuration applied successfully. Restart N1MM+ for it to take effect."),
        ["n1mm_status_config_failed"]  = ("Falha ao configurar o N1MM+. Consulta os logs para detalhes.",
                                          "Failed to configure N1MM+. Check the logs for details."),
        ["n1mm_confirm_title"]  = ("N1MM+ está a correr",               "N1MM+ is running"),
        ["n1mm_confirm_running"] = ("O N1MM+ está aberto. Alterar a configuração agora pode ser substituído se o N1MM+ gravar as suas próprias definições. Recomenda-se fechar o N1MM+ primeiro.\n\nContinuar mesmo assim?",
                                    "N1MM+ is currently open. Changing the configuration now may get overwritten if N1MM+ saves its own settings. It's recommended to close N1MM+ first.\n\nContinue anyway?"),
        ["lbl_wsjtx_enabled"]   = ("Activar recepção do WSJT-X",         "Enable WSJT-X reception"),
        ["lbl_wsjtx_port"]      = ("Porta UDP WSJT-X:",                 "WSJT-X UDP Port:"),
        ["hint_wsjtx"]          = ("No WSJT-X: File → Settings → Reporting → activar \"Accept UDP requests\" e definir esta porta (padrão: 2237). Compatível com JTDX e outros que sigam o mesmo protocolo.",
                                    "In WSJT-X: File → Settings → Reporting → enable \"Accept UDP requests\" and set this port (default: 2237). Compatible with JTDX and others following the same protocol."),

        // N1MM help submenu
        ["menu_n1mm_help"]       = ("Configurar o N1MM+...",           "Configure N1MM+..."),
        ["n1mm_help_title"]      = ("Como configurar o N1MM+",         "How to configure N1MM+"),
        ["n1mm_help_body"]       = (
"CONFIGURAR O N1MM+ PARA O LOGBEAM\n" +
"═══════════════════════════════════════════════════\n\n" +
"1. ACTIVAR O UDP BROADCAST\n" +
"   No N1MM+: Config → Configure → tab 'Broadcast'\n" +
"   (ou 'Network' / 'UDP' dependendo da versão)\n\n" +
"   ☑  Enable UDP Broadcasts\n" +
"   ☑  Score to Network Devices\n" +
"   ☑  Contacts to Network Devices\n\n" +
"2. PORTA UDP\n" +
"   Campo 'Port' → definir como 12060\n\n" +
"3. ENDEREÇO DE DESTINO\n" +
"   Deixar em branco ou 127.0.0.1 se o LogBeam\n" +
"   correr no mesmo PC que o N1MM+.\n\n" +
"4. GUARDAR E REINICIAR O N1MM+\n" +
"   Clicar OK → fechar e reabrir o N1MM+.\n\n" +
"5. VERIFICAR\n" +
"   Iniciar o LogBeam → fazer um QSO de teste\n" +
"   → verificar logs em %AppData%\\LogBeam\\logs\\"
,
"CONFIGURE N1MM+ FOR LOGBEAM\n" +
"═══════════════════════════════════════════════════\n\n" +
"1. ENABLE UDP BROADCAST\n" +
"   In N1MM+: Config → Configure → 'Broadcast' tab\n" +
"   (or 'Network' / 'UDP' depending on version)\n\n" +
"   ☑  Enable UDP Broadcasts\n" +
"   ☑  Score to Network Devices\n" +
"   ☑  Contacts to Network Devices\n\n" +
"2. UDP PORT\n" +
"   Set 'Port' field to 12060\n\n" +
"3. DESTINATION ADDRESS\n" +
"   Leave blank or 127.0.0.1 if LogBeam runs\n" +
"   on the same PC as N1MM+.\n\n" +
"4. SAVE AND RESTART N1MM+\n" +
"   Click OK → close and reopen N1MM+.\n\n" +
"5. VERIFY\n" +
"   Start LogBeam → make a test QSO\n" +
"   → check logs at %AppData%\\LogBeam\\logs\\"),

        // Station
        ["lbl_callsign"]         = ("Callsign:",                        "Callsign:"),
        ["lbl_lat"]              = ("Latitude:",                        "Latitude:"),
        ["lbl_lon"]              = ("Longitude:",                       "Longitude:"),
        ["hint_qth"]             = ("Coordenadas GPS do teu QTH (localização da estação). Ex: Portugal → Lat 38.7167 / Lon -9.1333",
                                    "GPS coordinates of your QTH (station location). Ex: Portugal → Lat 38.7167 / Lon -9.1333"),

        // API
        ["lbl_api_url"]          = ("URL Base:",                        "Base URL:"),
        ["hint_api_key"]         = ("Cada linha é um logbook LogBeam de destino — o QSO é enviado para todos os que estiverem activos (útil para expedições). Gera a API Key em app.logbeam.org → abre o logbook → \"Gerir Logbook\" → separador \"API Keys\". Instance ID: os 10 caracteres visíveis no URL view.php?id=XXXXXXXXXX",
                                    "Each row is a destination LogBeam logbook — the QSO is sent to every enabled one (useful for expeditions). Generate the API Key at app.logbeam.org → open the logbook → \"Manage Logbook\" → \"API Keys\" tab. Instance ID: the 10 characters shown in the URL view.php?id=XXXXXXXXXX"),
        ["lbl_api_timeout"]      = ("Timeout (s):",                     "Timeout (s):"),
        ["lbl_api_retry"]        = ("Tentativas:",                      "Retries:"),
        ["lbl_api_retry_delay"]  = ("Delay Retry (s):",                 "Retry Delay (s):"),
        ["btn_test_api"]         = ("Testar Ligação",                   "Test Connection"),
        ["btn_profile_add"]      = ("Adicionar Logbook",                "Add Logbook"),
        ["btn_profile_remove"]   = ("Remover",                          "Remove"),
        ["col_profile_name"]     = ("Nome",                             "Name"),
        ["col_profile_instance"] = ("Instance ID",                      "Instance ID"),
        ["col_profile_key"]      = ("API Key",                          "API Key"),

        // HamQTH
        ["hint_hamqth"]          = ("Conta HamQTH (hamqth.com) para lookup automático de localização dos correspondentes (gratuito).",
                                    "HamQTH account (hamqth.com) for automatic location lookup of correspondents (free)."),
        ["lbl_hamqth_user"]      = ("Username:",                        "Username:"),
        ["lbl_hamqth_pass"]      = ("Password:",                        "Password:"),
        ["lbl_hamqth_cache"]     = ("Cache (min):",                     "Cache (min):"),

        // ClubLog
        ["tab_clublog"]          = ("ClubLog",                          "ClubLog"),
        ["hint_cl"]              = ("Envia cada QSO automaticamente para o ClubLog em tempo real.\n" +
                                    "Usa uma \"Application Password\" do ClubLog (não a tua password de login).\n" +
                                    "Callsign: deixa em branco para usar o callsign da tab Estação.",
                                    "Sends each QSO automatically to ClubLog in real-time.\n" +
                                    "Use a ClubLog \"Application Password\" (not your login password).\n" +
                                    "Callsign: leave blank to use the callsign from the Station tab."),
        ["lbl_cl_enabled"]       = ("Activar upload em tempo real",     "Enable real-time upload"),
        ["lbl_cl_email"]         = ("Email ClubLog:",                   "ClubLog Email:"),
        ["lbl_cl_pass"]          = ("App Password:",                    "App Password:"),
        ["lbl_cl_call"]          = ("Callsign (opcional):",             "Callsign (optional):"),

        // Advanced
        ["lbl_log_level"]        = ("Nível de Log:",                    "Log Level:"),
        ["lbl_log_path"]         = ("Ficheiro Log:",                    "Log File:"),

        // Buttons
        ["btn_save"]             = ("Guardar",                          "Save"),
        ["btn_cancel"]           = ("Cancelar",                         "Cancel"),
        ["btn_exit"]             = ("Sair",                             "Exit"),

        // Status messages
        ["status_saved"]         = ("Configurações guardadas com sucesso.", "Settings saved successfully."),
        ["status_error"]         = ("Erro ao guardar",                  "Error saving"),
        ["status_no_url"]        = ("Introduz a URL da API primeiro.",   "Enter the API URL first."),
        ["status_no_credentials"] = ("Introduz o Instance ID e a API Key primeiro.", "Enter the Instance ID and API Key first."),
        ["status_testing"]       = ("A testar ligação...",              "Testing connection..."),
        ["status_api_ok"]        = ("Ligação estabelecida — logbook encontrado.", "Connection successful — logbook found."),
        ["status_api_unauthorized"] = ("API Key inválida ou revogada.", "Invalid or revoked API Key."),
        ["status_api_not_found"] = ("Logbook não encontrado. Verifica o Instance ID.", "Logbook not found. Check the Instance ID."),
        ["status_api_fail"]      = ("Falha na ligação",                 "Connection failed"),
    };

    public static string Get(string key) =>
        _s.TryGetValue(key, out var p) ? (Lang == "EN" ? p.EN : p.PT) : key;
}
