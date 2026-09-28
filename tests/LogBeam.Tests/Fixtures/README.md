# Pacotes reais para os testes

Pacotes UDP capturados a 28/09/2026 num PC com Windows 11, tal como os programas de logging os enviam. Os testes de leitura usam estes ficheiros em vez de pacotes escritos à mão.

| Pasta | Programa | Porta | Formato |
|---|---|---|---|
| `N1MM/` | N1MM Logger+ 1.0.11449 | 12060 | XML ("External UDP Broadcasts") |
| `Wsjtx/` | WSJT-X 3.0.2 | 2237 | Binário (QDataStream, big-endian) |
| `Log4OM/` | Log4OM 2.42.1 | 2333 | ADIF em texto (ligação UDP OUTBOUND, ADIF_MESSAGE) |

| Ficheiro | Conteúdo |
|---|---|
| `N1MM/contactinfo_ssb.xml` | QSO novo em SSB (20 m) |
| `N1MM/contactinfo_cw.xml` | QSO novo em CW (20 m) |
| `N1MM/contactreplace.xml` | QSO editado (RST recebido alterado de 599 para 555) |
| `N1MM/contactdelete.xml` | QSO apagado |
| `Wsjtx/logged_adif_ft8.bin` | Mensagem 12 ("Logged ADIF"), FT8 — o indicativo vem em minúsculas |
| `Wsjtx/logged_adif_ft4.bin` | Mensagem 12, FT4 — `MODE=MFSK` com `SUBMODE=FT4` |
| `Wsjtx/qso_logged_ft8.bin` | Mensagem 5 ("QSO Logged") |
| `Wsjtx/heartbeat.bin` | Mensagem 0 ("Heartbeat") |
| `Wsjtx/status.bin` | Mensagem 1 ("Status") |
| `Log4OM/adif_message_ssb.adi` | QSO novo em SSB — `MODE=SSB` com `SUBMODE=USB` |

## Dados pessoais

Os indicativos dos correspondentes, nomes, moradas, coordenadas, locator de 6 caracteres da estação, equipamento e nome do computador foram trocados por valores fictícios. Nos pacotes binários do WSJT-X as substituições mantêm o comprimento de cada texto, para os prefixos de tamanho continuarem certos; no ADIF do Log4OM os comprimentos foram recalculados.

Os ficheiros não podem ser alterados pelo Git (ver `.gitattributes` na raiz): os testes dependem dos bytes exactos.
