# BugFlow

Aplicație mobilă de gestionare a proiectelor, issue-urilor și comentariilor, realizată cu .NET MAUI și SQLite.

## Ce face aplicația

- CRUD pentru: `Proiect`, `MembruEchipa`, `Issue`, `Comentariu`
- Filtrare/căutare în paginile de listă
- Validare formulare în timp real (behaviors)
- Relații între entități cu reguli clare la ștergere
- Pagină de raport cu statistici pe status și prioritate
- Suită de 27 teste unitare pentru persistență și reguli de business

## Stack

- .NET MAUI (.NET 8)
- C# + XAML
- SQLite (`sqlite-net-pcl`, `SQLiteNetExtensions`)
- xUnit

## Structură

```text
BugFlow/
├── Models/          # Entități
├── Data/            # Acces date + operații SQLite
├── Domain/          # Reguli și calcule de raport
├── Pages/           # UI (List + Detail)
├── Behaviors/       # Validări reutilizabile
├── Converters/      # Conversii pentru UI
└── BugFlow.Tests/   # Teste unitare
```

## Politica la ștergerea unui membru

- Comentariile autorului se șterg
- Issue-urile rămân în proiect, dar devin neasignate (`MembruEchipaId = 0`)

## Rulare

```bash
dotnet restore
dotnet build
dotnet test BugFlow.Tests/BugFlow.Tests.csproj
```

Pentru rulare MAUI pe un target specific (ex. Android), proiectul poate fi lansat din Visual Studio/Rider sau prin target-ul corespunzător din CLI.

## Status

Proiect funcțional, pregătit pentru prezentare.
