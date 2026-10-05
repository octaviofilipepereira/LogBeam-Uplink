# Contribuir · Contributing · Contribuir · Contribuer

[Português](#português) · [English](#english) · [Español](#español) · [Français](#français)

---

## Português

Contribuições são bem-vindas: correcções, melhorias, traduções, testes com outros programas de log e relatórios de problemas.

### *Issues*

- Antes de abrir uma, veja se já existe uma igual.
- Indique a versão do LogBeam Uplink e do Windows, o programa de log e a sua versão, o que esperava e o que aconteceu.
- O ficheiro de log do dia (pasta `logs` do Uplink) ajuda muito. Retire dele o que não quiser tornar público.
- Falhas de segurança **não** vão para as *issues*: ver [SECURITY.md](SECURITY.md).
- Pode escrever em português ou em inglês.

### *Pull requests*

1. Os *pull requests* são feitos para o ramo **`unstable`**. O `main` só recebe o que já foi verificado no `unstable`.
2. Uma alteração por *pull request*, com uma descrição do que muda e porquê.
3. O código tem de compilar sem avisos (`dotnet build`) e passar em todos os testes (`dotnet test`). Cada alteração de comportamento vem acompanhada dos testes que a verificam. Nos programas de log, os testes devem usar, sempre que possível, pacotes reais capturados do programa, sem dados pessoais, guardados em `tests/LogBeam.Tests/Fixtures/`.
4. Textos da interface: em `src/LogBeam.UI/Localization/L.cs`, sempre nas quatro línguas (PT, EN, ES, FR). Um teste falha se faltar alguma.
5. Datas DD/MM/AAAA, horas HH:MM em 24 h e QSOs em UTC.
6. Não são permitidos segredos no código: chaves, passwords ou endereços internos.
7. Ficheiros novos levam o mesmo cabeçalho de licença dos restantes.

### Licença das contribuições

O projecto é distribuído com a [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). Ao enviar uma contribuição, aceita que seja distribuída com a mesma licença.

### Compilar e testar

Ver a secção "Compilar" do [README.md](README.md#compilar).

---

## English

Contributions are welcome: fixes, improvements, translations, testing with other logging programs and bug reports.

### Issues

- Before opening one, check whether it already exists.
- Include the LogBeam Uplink and Windows versions, the logging program and its version, what you expected and what happened.
- The day's log file (Uplink's `logs` folder) helps a lot. Remove anything you do not want to make public.
- Security issues do **not** go into issues: see [SECURITY.md](SECURITY.md).
- You may write in Portuguese or English.

### Pull requests

1. Pull requests target the **`unstable`** branch. `main` only receives what has already been verified on `unstable`.
2. One change per pull request, with a description of what changes and why.
3. The code must build with no warnings (`dotnet build`) and pass every test (`dotnet test`). Every change in behaviour comes with the tests that check it. For logging programs, tests should use, whenever possible, real packets captured from the program, with personal data removed, stored in `tests/LogBeam.Tests/Fixtures/`.
4. Interface texts: in `src/LogBeam.UI/Localization/L.cs`, always in the four languages (PT, EN, ES, FR). A test fails if any is missing.
5. Dates DD/MM/YYYY, times HH:MM in 24 h and QSOs in UTC.
6. Secrets are not allowed in the code: keys, passwords or internal addresses.
7. New files carry the same licence header as the others.

### Licence of contributions

The project is distributed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). By submitting a contribution, you agree that it is distributed under the same licence.

### Building and testing

See the "Building" section of [README.en.md](README.en.md#building).

---

## Español

Las contribuciones son bienvenidas: correcciones, mejoras, traducciones, pruebas con otros programas de log e informes de problemas.

### *Issues*

- Antes de abrir una, compruebe si ya existe una igual.
- Indique la versión de LogBeam Uplink y de Windows, el programa de log y su versión, lo que esperaba y lo que ocurrió.
- El archivo de log del día (carpeta `logs` de Uplink) ayuda mucho. Quite de él lo que no quiera hacer público.
- Los fallos de seguridad **no** van a las *issues*: vea [SECURITY.md](SECURITY.md).
- Puede escribir en portugués o en inglés.

### *Pull requests*

1. Los *pull requests* se hacen a la rama **`unstable`**. `main` solo recibe lo que ya se ha verificado en `unstable`.
2. Un cambio por *pull request*, con una descripción de lo que cambia y por qué.
3. El código debe compilar sin avisos (`dotnet build`) y superar todas las pruebas (`dotnet test`). Cada cambio de comportamiento va acompañado de las pruebas que lo verifican. Para los programas de log, las pruebas deben usar, siempre que sea posible, paquetes reales capturados del programa, sin datos personales, guardados en `tests/LogBeam.Tests/Fixtures/`.
4. Textos de la interfaz: en `src/LogBeam.UI/Localization/L.cs`, siempre en los cuatro idiomas (PT, EN, ES, FR). Una prueba falla si falta alguno.
5. Fechas DD/MM/AAAA, horas HH:MM en 24 h y QSOs en UTC.
6. No se permiten secretos en el código: claves, contraseñas o direcciones internas.
7. Los archivos nuevos llevan la misma cabecera de licencia que los demás.

### Licencia de las contribuciones

El proyecto se distribuye con la [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). Al enviar una contribución, acepta que se distribuya con la misma licencia.

### Compilar y probar

Vea la sección «Compilar» de [README.es.md](README.es.md#compilar).

---

## Français

Les contributions sont les bienvenues : corrections, améliorations, traductions, tests avec d'autres logiciels de log et signalements de problèmes.

### *Issues*

- Avant d'en ouvrir une, vérifiez qu'elle n'existe pas déjà.
- Indiquez la version de LogBeam Uplink et de Windows, le logiciel de log et sa version, ce que vous attendiez et ce qui s'est passé.
- Le fichier journal du jour (dossier `logs` d'Uplink) aide beaucoup. Retirez-en ce que vous ne voulez pas rendre public.
- Les failles de sécurité **ne** vont **pas** dans les *issues* : voir [SECURITY.md](SECURITY.md).
- Vous pouvez écrire en portugais ou en anglais.

### *Pull requests*

1. Les *pull requests* visent la branche **`unstable`**. `main` ne reçoit que ce qui a déjà été vérifié sur `unstable`.
2. Une modification par *pull request*, avec une description de ce qui change et pourquoi.
3. Le code doit compiler sans avertissements (`dotnet build`) et réussir tous les tests (`dotnet test`). Chaque changement de comportement s'accompagne des tests qui le vérifient. Pour les logiciels de log, les tests doivent utiliser, autant que possible, des paquets réels capturés depuis le logiciel, sans données personnelles, placés dans `tests/LogBeam.Tests/Fixtures/`.
4. Textes de l'interface : dans `src/LogBeam.UI/Localization/L.cs`, toujours dans les quatre langues (PT, EN, ES, FR). Un test échoue s'il en manque une.
5. Dates JJ/MM/AAAA, heures HH:MM sur 24 h et QSO en UTC.
6. Les secrets ne sont pas autorisés dans le code : clés, mots de passe ou adresses internes.
7. Les nouveaux fichiers portent le même en-tête de licence que les autres.

### Licence des contributions

Le projet est distribué sous la [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html). En envoyant une contribution, vous acceptez qu'elle soit distribuée sous la même licence.

### Compiler et tester

Voir la section « Compiler » de [README.fr.md](README.fr.md#compiler).
