# CONTRIBUTING

## Workflow — Git Flow w wersji prostej

Stosujemy prosty model, żeby utrzymać porządek. Główne zasady są dwie: `main` jest święty, a cała praca deweloperska przechodzi przez `dev`.

### ✅ 1. Gałęzie (Branche)

- **`main`** → Gałąź produkcyjna. Zawsze stabilna, zawsze działająca. Kod trafia tu **tylko z `dev`** w ramach przygotowania nowej wersji. **Nigdy nie commitujemy bezpośrednio do `main`!**
- **`dev`** → Główna gałąź deweloperska. Tutaj integrujemy wszystkie nowe funkcje i poprawki. Może być chwilowo niestabilna.
- **`feature/nazwa-ficzera`** lub **`fix/nazwa-poprawki`** → Twoje gałęzie robocze. **Zawsze tworzysz je z `dev`**.

Przykłady nazw:

```
feature/dodanie-logowania
fix/poprawa-wyswietlania-kamery
```

### ✅ 2. Cykl pracy

1.  **Zawsze zaczynaj od aktualizacji `dev`:**
    ```bash
    git switch dev
    git pull origin dev
    ```
2.  **Stwórz nową gałąź roboczą z `dev`:**
    ```bash
    git switch -c feature/twoja-super-funkcja
    ```
3.  **Pracuj, rób commity.** Pisz kod, zapisuj zmiany często, z dobrymi opisami.
4.  **Zanim zrobisz Pull Request, zsynchronizuj się z `dev`:**
    Ktoś mógł w międzyczasie coś dodać do `dev`. Użyj `rebase`, żeby Twoje zmiany były "na wierzchu" i historia była czysta.
    ```bash
    # Będąc na swoim branchu (np. feature/twoja-super-funkcja)
    git fetch origin
    git rebase origin/dev
    ```
5.  **Rozwiąż konflikty (jeśli wystąpią) lokalnie.**
6.  **Wypchnij zmiany na serwer.** Po rebase musisz użyć `--force-with-lease` (jest bezpieczniejsze niż zwykłe `--force`).
    ```bash
    git push --force-with-lease
    ```
7.  **Stwórz Pull Request (PR) do `dev`**.

### ✅ 3. Pull Requesty (PR)

- Każdy PR z nową funkcją lub poprawką idzie **zawsze do `dev`**.
- Opis PR musi być zwięzły i jasny. Np. `Dodaje formularz rejestracji i walidację.`
- Każdy PR musi być sprawdzony i zatwierdzony (co najmniej 1 "Approve") przez kogoś innego, zanim zostanie zmergowany.
- Do mergowania **zawsze używaj opcji `Squash and merge`**. Dzięki temu cała Twoja praca z jednej gałęzi trafia do `dev` jako jeden, czysty commit.

### ✅ 4. Cykl wydawniczy

Gdy na `dev` uzbiera się wystarczająco dużo zmian na nową, stabilną wersję:

1.  Tworzymy Pull Request **z `dev` do `main`**.
2.  Ten PR jest dokładnie testowany.
3.  Po pomyślnych testach i akceptacji, mergujemy go do `main`.
4.  Opcjonalnie tworzymy taga na `main` (np. `v1.1.0`).

### ✅ 5. Co, jeśli coś się zepsuło?

- **Na `dev`**: Nie panikuj. Stwórz nową gałąź `fix/nazwa-problemu` z `dev`, napraw błąd i zrób normalny PR z powrotem do `dev`.
- **Na `main` (HOTFIX)**: Sytuacja krytyczna.
    1.  Stwórz gałąź `hotfix/pilna-poprawka` **z `main`**.
    2.  Napraw błąd.
    3.  Zrób PR i zmerguj go **do `main`**.
    4.  **Koniecznie zmerguj `main` z powrotem do `dev`**, żeby poprawka trafiła też do głównej gałęzi deweloperskiej!
        ```bash
        git switch dev
        git pull origin dev
        git merge main
        git push origin dev
        ```
