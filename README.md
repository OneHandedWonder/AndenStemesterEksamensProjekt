# AndenStemesterEksamensProjekt
EksamensProjekt Til 2. semester På Zealand, Datamatiker. 

# Opsætning af DB mm.

## Forudsætninger
- Docker Desktop skal være installeret og køre
- Docker Compose skal være tilgængelig (følger med Docker Desktop)

## Sådan kører du scriptet

### Windows (PowerShell)
```powershell
.\dbConf.ps1
```

### Linux/macOS/WSL (Bash)
Først gør filen eksekverbar (kun første gang):
```bash
chmod +x dbConf.sh
```
Derefter kør:
```bash
./dbConf.sh
```

## Hvad gør scriptet?
1. Tjekker at Docker kører
2. **ADVARSEL**: Fjerner eksisterende database og SLETTER ALLE DATA
3. Beder om bekræftelse (skriv "YES" for at fortsætte)
4. Starter PostgreSQL container via docker-compose
5. Venter på at databasen er klar
6. Kører alle .sql filer fra `dbInit/` mappen i alfabetisk rækkefølge

## Kør kun SQL filer igen (uden at slette data)
Hvis du vil køre SQL filerne igen uden at slette hele databasen:

Windows: `.\dbConf.ps1 -Reinit`  
Linux/macOS: `./dbConf.sh --reinit`

Dette er nyttigt når du har tilføjet nye .sql filer eller ændret eksisterende, og vil opdatere databasen uden at starte forfra.

## Spring bekræftelse over (til automation)
Windows: `.\dbConf.ps1 -Force`  
Linux/macOS: `./dbConf.sh --force`

Du kan også kombinere flags: `.\dbConf.ps1 -Reinit -Force`

## Tilføj dine SQL scripts
Placer dine .sql filer i `dbInit/` mappen. De køres automatisk i alfabetisk rækkefølge, så navngiv dem fx:
- `01-create-tables.sql`
- `02-insert-data.sql`
- `03-create-views.sql`