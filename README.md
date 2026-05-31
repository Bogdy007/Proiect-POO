# 🏰 Evadare din Castelul Bran
### Motor de poveste interactivă pe bază de blocuri

> **Disciplina:** Programare Orientată pe Obiecte (POO)  
> **Tehnologie:** C# · .NET 10 · Windows Forms  
> **Tip proiect:** Aplicație desktop — echipă de 4 persoane

---

## 👥 Echipa

| Persoană | Nume | Rol |
|----------|------|-----|
| Persoana 1 | **Moroșanu Răzvan** | Arhitect date & motor (`Story.Core`) |
| Persoana 2 | **Panainte Bogdan** | Dezvoltator Reader (player) |
| Persoana 3 | **Neculcea Sabin** | Dezvoltator Editor (structura principală) |
| Persoana 4 | **Pricop Andrei** | Decizii, condiții, validare & documentație |

---

## 📖 Descriere generală

**Evadare din Castelul Bran** este un sistem pentru povești interactive scris în C#, în care povestea este structurată ca un **graf orientat de blocuri narative**. Jucătorul citește textul unui bloc, alege o decizie și, în funcție de alegere, ajunge într-un alt bloc. Firul narativ poate diverge, converge și poate produce finaluri diferite.

Pe lângă text, povestea menține o **stare numerică** formată din atribute (ex: viață, suspiciune, galbeni). Deciziile pot modifica aceste valori și pot fi condiționate de ele — o decizie apare doar dacă sunt îndeplinite anumite condiții logice.

Soluția este formată din **două aplicații Windows Forms** și o **bibliotecă comună**:

- **`Story.Core`** — biblioteca de date și logică, fără interfață grafică; refolosită de ambele aplicații
- **`EvadareBranEditor`** — aplicație pentru autori: creează și modifică povești fără a edita JSON manual
- **`EvadareBranReader`** — aplicație pentru jucători: încarcă povestea și o parcurge vizual și interactiv

Povestea se salvează ca o **arhivă ZIP** ce conține `story.json` și un folder `images/` cu resursele grafice.

---

## 🛠️ Tehnologii utilizate

| Componentă | Tehnologie |
|---|---|
| Limbaj / platformă | C# pe .NET 10 |
| Interfață grafică | Windows Forms (WinForms) |
| Serializare date | `System.Text.Json` |
| Arhive ZIP | `System.IO.Compression` / `ZipFile` |
| Imagini | `System.Drawing` (Image, Bitmap, PictureBox) |
| Arhitectură | 3 proiecte: bibliotecă comună + 2 aplicații WinForms |

---

## 🗂️ Structura soluției

```
EvadareBranReader.slnx
│
├── Story.Core/                          ← BIBLIOTECA COMUNĂ (fără interfață)
│   ├── Models/
│   │   └── Poveste.cs                   ← Toate clasele de date (model)
│   └── Engine/
│       └── MotorPoveste.cs              ← Logica de joc (motorul)
│
├── EvadareBranReader/                   ← APLICAȚIA READER (player)
│   ├── Form1.cs / Form1.Designer.cs     ← Interfața player-ului
│   └── Program.cs
│
└── EvadareBranEditor/                   ← APLICAȚIA EDITOR
    ├── Form1.cs / Form1.Designer.cs     ← Fereastra principală (TreeView + editor)
    ├── FormDecizie.cs                   ← Dialog editare decizie
    ├── FormConditie.cs                  ← Editor condiții pe arbore (AST)
    └── Program.cs
```

---

## 🧱 Modelul de date (clasele)

Toate clasele de date se află în `Story.Core/Models/Poveste.cs`, namespace `Story.Core.Models`. Fiecare proprietate este adnotată cu `[JsonPropertyName]` pentru serializare/deserializare automată.

| Clasă | Rol |
|---|---|
| `Poveste` | Rădăcina: titlu, blocul de start, lista de atribute și lista de blocuri |
| `AtributPoveste` | O proprietate de stare: `key`, `min`, `max`, `initial`, etichetă HUD, vizibilitate, ordine, blocuri de redirecționare la min/max |
| `BlocPoveste` | Un nod din graf: `id`, text narativ, imagine, marcaj de bloc final, lista de decizii |
| `Decizie` | O alegere: text, bloc destinație, iconiță, condiție de afișare, listă de efecte |
| `Conditie` | Nod AST: `COMPARISON` / `AND` / `OR`, cu proprietate, operator, valoare și sub-condiții |
| `Efect` | Modificare de stare: tip (`ADD` / `SET`), proprietate, valoare |
| `StareJoc` | Snapshot salvabil al sesiunii: blocul curent + valorile curente ale atributelor |

---

## 📄 Formatul fișierului de poveste

O poveste este o **arhivă ZIP** cu următoarea structură (căile spre imagini sunt întotdeauna relative):

```
poveste.zip
├── story.json          ← definiția completă a poveștii
└── images/             ← imaginile de fundal și iconițele
    ├── temnita.jpg
    └── icon_cheie.png
```

### Exemplu `story.json` (simplificat)

```json
{
  "title": "Evadare din Castelul Bran",
  "startBlock": "intro.captiv",
  "properties": [
    {
      "key": "player.viata",
      "hudLabel": "Viata",
      "min": 0, "max": 100, "initial": 60,
      "visibleInHud": true, "hudOrder": 1,
      "onMinBlock": "ending.prins"
    }
  ],
  "blocks": [
    {
      "id": "intro.captiv",
      "text": "Te trezesti intr-o celula intunecata...",
      "image": "images/temnita.jpg",
      "isFinal": false,
      "decisions": [
        {
          "text": "Forteaza usa",
          "targetBlock": "holul.principal",
          "effects": [
            { "type": "ADD", "property": "player.viata", "value": -10 }
          ]
        }
      ]
    }
  ]
}
```

---

## ⚙️ Motorul de joc (`MotorPoveste`)

Clasa `MotorPoveste` din `Story.Core/Engine/MotorPoveste.cs` implementează întreaga logică de rulare, independent de orice interfață grafică. Metodele principale expuse:

| Metodă | Descriere |
|---|---|
| `IncarcaPovesteJson(string)` | Deserializează JSON-ul, construiește indexul de blocuri (`Dictionary`) pentru acces `O(1)` și inițializează atributele |
| `ObtineBlocCurent()` | Returnează structura blocului curent |
| `MutaLaBloc(string)` | Navighează la un bloc și salvează blocul anterior în stivă |
| `MergiInapoi()` | Revine la blocul anterior din stivă (funcția „Înapoi") |
| `Restart()` | Resetează povestea: atribute la valori inițiale, bloc de start, istoric gol |
| `AplicaEfecteSiObtineRedirectionare(Decizie)` | Aplică efectele `ADD`/`SET`, limitează valorile la `[min, max]`, returnează bloc de redirecționare dacă e cazul |
| `EvalueazaConditie(Conditie)` | Evaluează recursiv arborele AST de condiții (`AND`/`OR`/`COMPARISON`) |
| `ExportaStare()` | Serializează starea curentă ca JSON (save game) |
| `ImportaStare(string)` | Restaurează o stare salvată (load game) |

### Detalii de implementare

**Index blocuri cu `Dictionary<string, BlocPoveste>`** — blocurile sunt indexate după `Id` la încărcare, asigurând acces în timp constant `O(1)` față de `O(n)` la căutare secvențială într-o listă.

**Stivă pentru istoricul de navigare** — `Stack<string>` reține blocurile vizitate; `MutaLaBloc()` face `Push`, iar `MergiInapoi()` face `Pop`.

**Clampare valori** — formula `Math.Max(min, Math.Min(max, valoare))` garantează că niciun atribut nu depășește intervalul definit.

---

## 🔀 Sistemul de condiții (AST)

O condiție este un **arbore sintactic abstract (AST)** cu trei tipuri de noduri:

- **`COMPARISON`** (frunză) — compară un atribut cu o valoare; operatori acceptați: `==`, `!=`, `>`, `>=`, `<`, `<=`
- **`AND`** — toate sub-condițiile trebuie să fie adevărate
- **`OR`** — cel puțin o sub-condiție trebuie să fie adevărată

Evaluarea se face **recursiv** în `MotorPoveste.EvalueazaConditie()`. Dacă o decizie nu are condiție (`null`), este mereu vizibilă.

**Exemplu:** „galbeni ≥ 10 ȘI (energie > 20 SAU are cheia)"

```json
{
  "type": "AND",
  "conditions": [
    { "type": "COMPARISON", "property": "inventory.galbeni", "operator": ">=", "value": 10 },
    {
      "type": "OR",
      "conditions": [
        { "type": "COMPARISON", "property": "player.energie", "operator": ">", "value": 20 },
        { "type": "COMPARISON", "property": "story.areCheia", "operator": "==", "value": 1 }
      ]
    }
  ]
}
```

---

## 🎮 Aplicația Reader (Persoana 2 — Panainte Bogdan)

Player-ul permite utilizatorului să încarce o poveste și să o joace. Fluxul de rulare:

1. **Încărcare** — deschide arhiva ZIP sau JSON, extrage conținutul în folder temporar, inițializează starea
2. **Afișare** — titlul poveștii, HUD cu atributele vizibile (ordonate după `hudOrder`), imaginea de fundal, textul narativ și butoanele de decizii
3. **Filtrare decizii** — se afișează doar deciziile a căror condiție este îndeplinită (`EvalueazaConditie`)
4. **Alegere** — jucătorul apasă un buton; se aplică efectele, se verifică redirecționările automate
5. **Tranziție** — se trece la blocul țintă (sau la cel de redirecționare pe min/max)
6. **Final** — la bloc marcat `isFinal`, apare opțiunea de restart

**Funcționalități suplimentare:** suport imagini locale și URL (încărcare asincronă), buton „◄ Înapoi" (stivă din motor), salvare/încărcare stare de joc (JSON).

---

## ✏️ Aplicația Editor (Persoana 3 — Neculcea Sabin & Persoana 4 — Pricop Andrei)

Editor-ul permite crearea și modificarea poveștilor fără editare manuală de JSON. Interfața are în stânga un **TreeView** cu structura poveștii, iar în dreapta un panou de editare contextual.

**Funcționalități:**
- Editare metadate (titlu, bloc de start)
- Adăugare / editare / ștergere atribute cu toate câmpurile (9 câmpuri)
- Adăugare / editare / ștergere blocuri (text, marcaj final, imagine)
- Dialog `FormDecizie` — bloc destinație, iconiță, tabel de efecte
- Editor `FormConditie` — construire condiții `AND`/`OR`/`COMPARISON` pe arbore vizual
- Salvare ca ZIP cu copierea imaginilor în `images/`
- Deschiderea unei povești existente pentru editare
- **Jurnal de validare live** — erori afișate în timp real la fiecare modificare
- **Previzualizare imagine** — `PictureBox` cu imagine locală sau de pe URL

---

## ✅ Sistemul de validare (Persoana 4 — Pricop Andrei)

Înainte de salvare, editorul validează povestea și raportează erorile:

- Blocul de start există în lista de blocuri
- Fiecare decizie țintește un bloc existent
- Efectele și condițiile referențiază atribute existente
- Operatorii din condiții și tipurile de efecte sunt valide
- Imaginile referite există și au extensie compatibilă (`.jpg`, `.png`, `.bmp`, `.gif`)
- Blocuri inaccesibile (la care nu se poate ajunge) sunt semnalate

---

## 🚀 Compilare și rulare

### Cerințe
- Visual Studio 2022+ cu workload `.NET Desktop Development`
- .NET 10 SDK

### Pași
```bash
# Clonare repository
git clone https://github.com/<utilizator>/evadare-bran.git
cd evadare-bran

# Build din linie de comandă
dotnet build EvadareBranReader.slnx
```

**Din Visual Studio:**
- Pentru **Editor**: setează `EvadareBranEditor` ca Startup Project → `F5`
- Pentru **Reader**: setează `EvadareBranReader` ca Startup Project → `F5`

### Flux tipic de lucru
```
Creezi povestea în Editor → Salvezi ca .zip → Deschizi în Reader → Joci
```

---

## 🔧 Adăugiri de complexitate (față de cerințele minime)

Proiectul depășește cerințele minime prin trei adăugiri justificate tehnic:

1. **Bibliotecă comună `Story.Core`** — separarea în trei straturi (date+logică / player / editor), conform recomandării din temă (§13). Logica nu este duplicată; Editor-ul și Reader-ul depind doar de bibliotecă.

2. **Buton „Înapoi" + istoric de navigare** — o `Stack<string>` a blocurilor vizitate în Reader, pentru a reveni la orice bloc anterior fără re-aplicarea efectelor.

3. **Jurnal de validare live + previzualizare imagini în Editor** — panou cu erorile actualizat în timp real la fiecare modificare și `PictureBox` de previzualizare (recomandate în §14.1.3 din temă).

---

## 📁 Responsabilități pe fișiere

| Fișier | Autor | Descriere |
|---|---|---|
| `Story.Core/Models/Poveste.cs` | Moroșanu Răzvan | Toate clasele model cu adnotări JSON |
| `Story.Core/Engine/MotorPoveste.cs` | Moroșanu Răzvan | Logica completă de joc (motor) |
| `EvadareBranReader/Form1.cs` | Panainte Bogdan | Interfața și logica player-ului |
| `EvadareBranEditor/Form1.cs` | Neculcea Sabin | Fereastra principală a editorului |
| `EvadareBranEditor/FormDecizie.cs` | Pricop Andrei | Dialog editare decizie + efecte |
| `EvadareBranEditor/FormConditie.cs` | Pricop Andrei | Editor vizual condiții AST |

---

## 📜 Licență

Proiect academic — realizat în cadrul disciplinei **Programare Orientată pe Obiecte**, Universitatea „Ștefan cel Mare" Suceava, 2025–2026.
