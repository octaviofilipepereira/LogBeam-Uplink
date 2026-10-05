<p align="center"><img src="images/logo.png" width="72" alt="LogBeam Uplink"></p>

<h1 align="center">Manual do LogBeam Uplink 2.5</h1>

<p align="center"><b>Português</b> · <a href="MANUAL.en.md">English</a> · <a href="MANUAL.es.md">Español</a> · <a href="MANUAL.fr.md">Français</a></p>

O LogBeam Uplink é uma aplicação para Windows que recebe os QSOs do **N1MM+**, do **WSJT-X**, do **JTDX** e do **Log4OM** no momento em que são registados e os envia para um ou vários logbooks [LogBeam](https://logbeam.org) e, se o activar, para o **ClubLog**.

**Índice:** [1. Instalar](#1-instalar) · [2. Primeiro arranque](#2-primeiro-arranque) · [3. A janela e a área de notificação](#3-a-janela-e-a-área-de-notificação) · [4. Separadores](#4-separadores) · [5. Sem internet](#5-sem-internet) · [6. Exportar a sessão](#6-exportar-a-sessão) · [7. Actualizações](#7-actualizações) · [8. Resolução de problemas](#8-resolução-de-problemas) · [9. Desinstalar](#9-desinstalar)

## 1. Instalar

1. Descarregar o instalador em **https://logbeam.org/uplink/** e confirmar a impressão digital SHA-256 publicada na página. No PowerShell, na pasta do ficheiro: `Get-FileHash .\LogBeamUplink-Setup-2.5.0.exe`.
2. Correr o instalador. Não pede privilégios de administrador nem o .NET: instala em `%LOCALAPPDATA%\Programs\LogBeam Uplink` e cria um atalho no menu Iniciar.

> ⚠️ O programa é gratuito e de código aberto, e é distribuído **sem assinatura digital**. Por isso:
> - **SmartScreen:** se aparecer «O Windows protegeu o seu PC», carregar em «Mais informações» → «Executar mesmo assim».
> - **Controlo Inteligente de Aplicações (Windows 11):** pode bloquear o instalador sem opção de continuar. Nesse caso é preciso desligá-lo em Segurança do Windows → Controlo de aplicações e do browser → Controlo Inteligente de Aplicações.

## 2. Primeiro arranque

Na primeira vez, o Uplink pergunta se pode enviar ao LogBeam os **dados da instalação** e os **relatórios de erros**. A janela lista exactamente o que é enviado; nada é enviado sem "Autorizar". A escolha pode ser mudada a qualquer momento no separador **Avançado**. Ao retirar a autorização, os dados já enviados são apagados do servidor.

<p align="center"><img src="images/pt-consentimento.png" width="420" alt="Pedido de autorização"></p>

## 3. A janela e a área de notificação

- Fechar a janela no **X** não termina o programa: o Uplink continua a trabalhar, com o ícone na área de notificação. Para terminar, usar o botão **Sair** ou o menu do ícone.
- Um duplo clique no ícone do Uplink, na área de notificação, abre a janela.
- O Uplink só abre uma vez: abri-lo de novo traz a janela que já está aberta para a frente.
- Os botões **PT · EN · ES · FR**, no topo da janela, mudam a língua.

**Menu do ícone** (botão direito no ícone do Uplink):

| Opção | Para quê |
|---|---|
| Abrir | Mostra a janela |
| Enviar para | Liga ou desliga cada logbook com um clique (ex. passar ao logbook de um concurso). A alteração é gravada logo |
| Iniciar serviço · Parar serviço | Liga ou desliga a recepção e o envio de QSOs |
| Sair | Termina o programa |

**Avisos (junto ao relógio):**

| Aviso | Quando |
|---|---|
| Falha ao enviar o QSO | Sempre |
| QSO enviado | Só com "Avisar a cada QSO enviado" (separador Avançado) |
| QSO já existia no logbook | O QSO já lá estava (o servidor detecta duplicados ao minuto). Só com a mesma opção |
| ✓ Confirmado por outro LogBeam | O correspondente também tem o QSO num logbook LogBeam |
| Porta ocupada | Um programa de log não pode ser recebido, porque outro programa ocupa a porta (ver a [secção 8](#8-resolução-de-problemas)) |
| ClubLog: credenciais recusadas | O ClubLog recusou o e-mail, a App Password ou o indicativo (ver [ClubLog](#clublog)). Aparece uma só vez |

## 4. Separadores

Depois de mudar o que quer que seja, carregar em **Guardar**: o serviço reinicia com as definições novas. **Cancelar** repõe o que estava gravado.

### Estação

**Indicativo:** é usado no envio para o ClubLog, se não for indicado outro no separador ClubLog. A localização dos correspondentes é tratada pelo servidor do LogBeam.

### API LogBeam

Cada linha da grelha é um logbook LogBeam de destino. O QSO é enviado para todos os que tiverem **Usar logbook** marcado (por exemplo, o logbook de uma expedição e o pessoal).

<p align="center"><img src="images/pt-api.png" width="520" alt="Separador API LogBeam"></p>

1. **Adicionar logbook**.
2. **Nome:** livre, para reconhecer o logbook no menu do ícone.
3. **Instance ID:** os 10 caracteres do endereço `view.php?id=XXXXXXXXXX`, ou o link completo do logbook (o Uplink extrai o identificador).
4. **API Key:** gerada em app.logbeam.org → abrir o logbook → "Gerir Logbook" → separador "API Keys". Tem 64 caracteres.
5. **Testar ligação:** confirma a chave e mostra de quem é o logbook ("Ligação estabelecida — logbook de CT7XXX").
6. **Guardar**.

As chaves aparecem mascaradas na grelha (só os últimos 4 caracteres); **Mostrar chaves** mostra-as inteiras. Ao editar a célula, a chave aparece em claro.

**URL base**, **Tempo limite**, **Tentativas** e **Intervalo** não precisam de ser alterados.

### N1MM+

<p align="center"><img src="images/pt-n1mm.png" width="520" alt="Separador N1MM+"></p>

- **Configurar automaticamente:** com o N1MM+ fechado, põe o N1MM+ a enviar os contactos para o Uplink. É feita uma cópia de segurança do ficheiro de configuração do N1MM+ antes de o alterar, e os destinos que já lá estivessem mantêm-se.
- **Detectar N1MM+:** mostra se o N1MM+ já está configurado.
- **Porta UDP** (por omissão 12060) e **Endereço** (127.0.0.1 = só este computador; 0.0.0.0 = qualquer computador da rede local).

Se outro programa (ex. o Log4OM) já recebe o N1MM+ na porta 12060, usar aqui outra porta (ex. 12061) e carregar em "Configurar automaticamente": o N1MM+ passa a enviar para os dois.

Para configurar à mão, ver o menu **Ajuda → Configurar o N1MM+...**

### WSJT-X / JTDX

<p align="center"><img src="images/pt-wsjtx.png" width="520" alt="Separador WSJT-X / JTDX"></p>

1. Marcar **Activar recepção do WSJT-X / JTDX**.
2. No WSJT-X: File → Settings → Reporting → UDP Server **127.0.0.1**, porta **2237** (os valores por omissão).
3. No JTDX: a mesma configuração e, além disso, marcar **"Enable sending logged QSO ADIF data"**.

> 💡 **Com o JTAlert ou o GridTracker na mesma porta:** dois programas não podem receber da mesma porta normal. Usar um endereço **multicast**, por exemplo **239.255.0.1**, no WSJT-X/JTDX (UDP Server), nesses programas e no campo **Endereço** do Uplink. Assim todos recebem os mesmos pacotes. Em multicast, o Uplink só aceita pacotes deste computador.

### Log4OM

<p align="center"><img src="images/pt-log4om.png" width="520" alt="Separador Log4OM"></p>

1. Marcar **Activar recepção do Log4OM**.
2. No Log4OM: Settings → Program Configuration → Software Integration → Connections. Acrescentar uma ligação **UDP OUTBOUND** com a mensagem **ADIF_MESSAGE**, destino **127.0.0.1**, porta **2333** e **"Broadcast" desligado**. Gravar e reiniciar o Log4OM.

### ClubLog

1. Marcar **Activar envio em tempo real**.
2. **E-mail do ClubLog** e **App Password**: uma "Application Password" criada no ClubLog, não a password de acesso ao site.
3. **Indicativo (opcional):** em branco usa o do separador Estação; se também estiver em branco, o do programa de log.

Recebe os QSOs de todos os programas (N1MM+, WSJT-X, JTDX e Log4OM) e usa a mesma fila sem internet dos logbooks. Se o ClubLog recusar o e-mail, a App Password ou o indicativo, o Uplink avisa uma vez (notificação do Windows e mensagem a vermelho neste separador) e pára o envio para o ClubLog, porque o ClubLog bloqueia o IP a quem insiste com credenciais erradas. Os QSOs ficam na fila e são enviados depois de corrigir os dados e carregar em Guardar.

### Avançado

<p align="center"><img src="images/pt-avancado.png" width="520" alt="Separador Avançado"></p>

| Opção | Para quê |
|---|---|
| Nível de log | Quantidade de informação no ficheiro de log. "Information" chega para o dia-a-dia; "Debug" para investigar um problema |
| Ficheiro de log | Onde fica o log (por omissão, a pasta `logs` do Uplink) |
| Avisar a cada QSO enviado | Aviso também para os QSOs enviados com sucesso |
| Iniciar com o Windows, sem abrir a janela | O Uplink arranca com o Windows e fica só o ícone na área de notificação |
| Enviar dados da instalação e relatórios de erros | A autorização do primeiro arranque; "O que é enviado?" mostra a lista |

## 5. Sem internet

Se um QSO não puder ser enviado (sem rede, servidor indisponível), fica numa fila e o Uplink volta a tentar a cada minuto, pela ordem de chegada. A fila sobrevive a reinícios do Uplink e do Windows.

Erros que reenviar não resolve, como uma API key revogada ou um logbook apagado, não ficam na fila: aparece o aviso de falha e o motivo fica no log.

## 6. Exportar a sessão

**Ficheiro → Exportar sessão para ADIF...** grava num ficheiro ADIF os QSOs enviados com sucesso desde que o serviço arrancou.

## 7. Actualizações

O Uplink verifica no arranque se há uma versão nova e avisa. Também se pode verificar em **Ajuda → Verificar actualizações...** As versões novas estão em https://logbeam.org/uplink/.

## 8. Resolução de problemas

| Sintoma | O que fazer |
|---|---|
| "Porta ocupada" | Outro programa usa a mesma porta. WSJT-X/JTDX: usar multicast ([secção 4](#wsjt-x--jtdx)). N1MM+: usar outra porta e "Configurar automaticamente" |
| "API Key inválida" ou "Instance ID inválido" ao guardar | Confirmar a chave (64 caracteres) e o link do logbook; usar "Testar ligação" |
| Os QSOs não chegam | Confirmar a configuração do programa de log ([secção 4](#4-separadores)) e que o serviço está iniciado (menu do ícone). Pôr o nível de log em "Debug", fazer um QSO e ver o ficheiro de log |
| Um QSO foi corrigido ou apagado no programa de log | Na versão 2.5.0, as edições e eliminações não chegam ao LogBeam nem ao ClubLog. Está previsto para a 2.6 |
| O Windows bloqueia o instalador ou o programa | Ver os avisos sobre o SmartScreen e o Controlo Inteligente de Aplicações ([secção 1](#1-instalar)) |

**Ficheiros**, na pasta de instalação (`%LOCALAPPDATA%\Programs\LogBeam Uplink`):

| Ficheiro | Conteúdo |
|---|---|
| `settings.json` | Definições. As API keys e as passwords ficam cifradas, só legíveis pelo utilizador do Windows que as gravou |
| `queue.json` | QSOs à espera de envio |
| `logs\` | Um ficheiro de log por dia, guardados 30 dias |
| `telemetry.json` | Relatórios de erros à espera de envio (só com autorização) |

## 9. Desinstalar

Definições do Windows → Aplicações → LogBeam Uplink → Desinstalar. O desinstalador apaga o programa, os logs e o arranque com o Windows. As definições (`settings.json`) e a fila (`queue.json`) ficam na pasta, para uma reinstalação as aproveitar; para as remover, apagar a pasta `%LOCALAPPDATA%\Programs\LogBeam Uplink`.

---

© 2026 Octávio Filipe Pereira Gonçalves (CT7BFV) · [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html)
