# AndenStemesterEksamensProjekt
EksamensProjekt Til 2. semester På Zealand, Datamatiker.  
**Lavet af:** Sophie & Emil

---

## Hurtig Start Guide
### 1. Forudsætninger
Sørg for at følgende er installeret på din computer:
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (skal være startet og køre)
- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)

### 2. Klargør Projektet

#### Windows (PowerShell)
Åbn PowerShell i projektmappen og kør:
```powershell
# Kør database opsætning
.\dbConf.ps1

# Når databasen er klar, kør applikationen
cd AndenStemesterEksamensProjekt
dotnet run
```

#### Linux/macOS/WSL (Bash)
Åbn terminal i projektmappen og kør:
```bash
# Gør scriptet eksekverbart (kun første gang)
chmod +x dbConf.sh

# Kør database opsætning
./dbConf.sh

# Når databasen er klar, kør applikationen
cd AndenStemesterEksamensProjekt
dotnet run
```

### 3. Åbn Applikationen
Når applikationen kører, åbn din browser og gå til:
- **HTTP:** http://localhost:5146
- **HTTPS:** https://localhost:7129

### 4. Test Login
Projektet kommer med 100 forudindlæste testbrugere. Her er nogle eksempler:

| Email | Password | Rolle | Beskrivelse |
|-------|----------|-------|-------------|
| testuser1@example.com | password123 | Student | Almindelig studerende |
| testuser65@example.com | password123 | Lecturer | Underviser med ekstra rettigheder |
| testuser80@example.com | password123 | Planner | Kan administrere hold og eksamener |
| testuser99@example.com | password123 | Admin | Fuld adgang til systemet |

**Alle testbrugere (testuser1-100@example.com) bruger password:** `password123`

## Troubleshooting

### Problem: "Docker is not running"
**Løsning:** Start Docker Desktop og vent til den er helt klar.

### Problem: "Port 5432 is already in use"
**Løsning:** 
```bash
docker-compose down
docker ps -a
# Stop eventuelt andre PostgreSQL containers
```

### Problem: CSS/Bootstrap virker ikke
**Løsning:** 
```bash
cd AndenStemesterEksamensProjekt
dotnet tool install -g Microsoft.Web.LibraryManager.Cli
libman restore
```

### Problem: Kan ikke logge ind
**Løsning:** Tjek at databasen er korrekt opsat ved at køre `.\dbConf.ps1` igen.

---

## 👥 Testbrugere Oversigt

Systemet indeholder 100 testbrugere fordelt således:
- **60 studerende** (testuser1-60@example.com)
- **15 undervisere** (testuser61-75@example.com)
- **10 planlæggere** (testuser76-85@example.com)
- **10 gæster** (testuser86-95@example.com)
- **3 censorer** (testuser96-98@example.com)
- **2 administratorer** (testuser99-100@example.com)

---

