# LogBeam Uplink

[Português](#português) · [English](#english)

---

## Português

Aplicação para Windows que fica no tabuleiro do sistema, escuta o programa de logging (N1MM+ e WSJT-X) e envia cada QSO, no momento em que é registado, para o logbook do operador no [LogBeam](https://logbeam.org). O globo 3D actualiza-se sozinho, sem exportar nem importar ficheiros ADIF. Pode também enviar os QSOs para o ClubLog.

> **Em desenvolvimento.** Ainda não há versão publicada.

### Compilar

Requisitos: Windows 10 ou superior e o [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

```
dotnet build LogBeam.sln -c Release
dotnet test LogBeam.sln -c Release
```

O instalador é gerado com o [Inno Setup 6](https://jrsoftware.org/isinfo.php) a partir de `LogBeam.iss` — ver `.github/workflows/release.yml`.

**ClubLog:** o envio para o ClubLog precisa de uma chave de aplicação do ClubLog, que não está no repositório. Sem ela, a aplicação compila e funciona, mas o envio para o ClubLog fica desactivado. Para a incluir numa compilação própria, defina a variável de ambiente `CLUBLOG_API_KEY` ou crie o ficheiro `clublog.key` na raiz do repositório (não versionado). A chave é pedida ao ClubLog por quem distribui a aplicação.

### Contribuir

*Issues* e *pull requests* são bem-vindos. Os *pull requests* são feitos para o ramo `unstable`; o `main` só recebe o que já foi verificado no `unstable`.

### Licença

GNU General Public License v3.0 — ver [LICENSE](LICENSE).

### Aviso

- O software é distribuído sem garantia de qualquer tipo.
- As versões alteradas são da responsabilidade de quem as altera e distribui; o autor não responde por elas.
- As versões oficiais são apenas as publicadas em https://logbeam.org/uplink/, com o SHA-256 indicado na página.
- O nome LogBeam e o ícone identificam a versão oficial; as versões alteradas devem usar outro nome (GPL v3, secção 7, alínea e).

---

## English

Windows tray application that listens to your logging program (N1MM+ and WSJT-X) and sends each QSO, as soon as it is logged, to your logbook on [LogBeam](https://logbeam.org). The 3D globe updates by itself — no ADIF export or import. It can also upload your QSOs to ClubLog.

> **In development.** No release has been published yet.

### Building

Requirements: Windows 10 or later and the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (x64).

```
dotnet build LogBeam.sln -c Release
dotnet test LogBeam.sln -c Release
```

The installer is built with [Inno Setup 6](https://jrsoftware.org/isinfo.php) from `LogBeam.iss` — see `.github/workflows/release.yml`.

**ClubLog:** uploading to ClubLog requires a ClubLog application key, which is not in the repository. Without it the application builds and works, but ClubLog upload is disabled. To include one in your own build, set the `CLUBLOG_API_KEY` environment variable or create a `clublog.key` file at the repository root (not versioned). The key must be requested from ClubLog by whoever distributes the application.

### Contributing

Issues and pull requests are welcome. Pull requests target the `unstable` branch; `main` only receives what has already been verified on `unstable`.

### License

GNU General Public License v3.0 — see [LICENSE](LICENSE).

### Disclaimer

- The software is distributed without warranty of any kind.
- Modified versions are the responsibility of whoever modifies and distributes them; the author is not liable for them.
- Official releases are only those published at https://logbeam.org/uplink/, with the SHA-256 shown on that page.
- The LogBeam name and icon identify the official release; modified versions must use a different name (GPL v3, section 7, paragraph e).

---

© 2026 Octávio Filipe Pereira Gonçalves (CT7BFV)
