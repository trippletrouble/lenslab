# LensLab – Analog Film Emulation & Photo Editor
## Dokumentation zur Studienarbeit (.NET-Programmierung mit C# und WPF)

**Studiengang:** Informatik / Media Engineering / Computer Science  
**Semester:** Sommersemester 2026  
**Technologie-Stack:** C#, .NET 8 LTS, WPF, MVVM, Entity Framework Core, SQLite, xUnit  

---

## Inhaltsverzeichnis
1. [Beschreibung der Anwendungsidee](#1-beschreibung-der-anwendungsidee)
   - 1.1 [Problemstellung & Motivation](#11-problemstellung--motivation)
   - 1.2 [Zielgruppe](#12-zielgruppe)
   - 1.3 [Most Valuable Features (MVF)](#13-most-valuable-features-mvf)
2. [Benutzerhandbuch](#2-benutzerhandbuch)
   - 2.1 [Schritt-für-Schritt-Anleitung für Erstanwender](#21-schritt-für-schritt-anleitung-für-erstanwender)
   - 2.2 [Funktionsübersicht im Detail](#22-funktionsübersicht-im-detail)
3. [Technische Architektur & MVVM](#3-technische-architektur--mvvm)
   - 3.1 [Struktur der Komponenten (MVVM-Pattern)](#31-struktur-der-komponenten-mvvm-pattern)
   - 3.2 [Asynchronität & Performance-Konzept](#32-asynchronität--performance-konzept)
   - 3.3 [Datenbank-Persistenz mit EF Core & SQLite](#33-datenbank-persistenz-mit-ef-core--sqlite)
   - 3.4 [Mathematische Algorithmen der Bildverarbeitung](#34-mathematische-algorithmen-der-bildverarbeitung)
4. [Verwendeter Fremdcode & Transparenter KI-Einsatz](#4-verwendeter-fremdcode--transparenter-ki-einsatz)
   - 4.1 [Externe Bibliotheken & Frameworks](#41-externe-bibliotheken--frameworks)
   - 4.2 [Transparenzangabe zum KI-Einsatz](#42-transparenzangabe-zum-ki-einsatz)
5. [Ausblick & Geplante Erweiterungen (Roadmap)](#5-ausblick--geplante-erweiterungen-roadmap)

---

## 1. Beschreibung der Anwendungsidee

### 1.1 Problemstellung & Motivation
Digitale Fotos von modernen Kameras und Smartphones zeichnen sich durch extrem hohe Auflösung und Schärfe aus, wirken jedoch oft klinisch rein, steril und emotional distanziert. Analoge Filmaufnahmen (z. B. auf Kodak Portra 400, Fujifilm Velvia oder Ilford HP5) besitzen hingegen durch ihre charakteristischen Farbverschiebungen, weichen Schattenverläufe und ihr organisches Filmkorn eine zeitlose Ästhetik.

Kommerzielle Bildbearbeitungssoftware wie Adobe Lightroom oder Capture One ist für Gelegenheitsnutzer oft zu komplex, teuer und durch Abo-Modelle attraktivitätsarm. Zudem überfordern hunderte Menüpunkte Anwender, die lediglich gezielt authentische Filmlooks auf ihre Fotos anwenden möchten. 

**LensLab** schließt diese Lücke: Es ist eine schlanke, spezialisierte WPF-Desktop-Anwendung, die legendäre analoge Film-Emulationen mit einer modernen, schnellen und intuitiven Benutzeroberfläche verbindet.

### 1.2 Zielgruppe
- **Hobby-Fotografen & Enthusiasten analoger Fotografie:** Nutzer, die ohne aufwendiges Einarbeiten in komplexe Gradationskurven ihren digitalen Aufnahmen den Look klassischer Filmrollen verleihen möchten.
- **Content Creator & Social-Media-Nutzer:** Anwender, die konsistente Farbwelten für ihre Bilder suchen und eigene Looks als Presets sichern oder teilen wollen.

### 1.3 Most Valuable Features (MVF)
1. **Live Film-Emulation (10+ Presets):** Sofortige Vorschau von Filmklassikern wie *Fujifilm Classic Chrome*, *Kodak Gold 200*, *Cinestill 800T* oder *Kodak Tri-X 400*.
2. **Dynamic Opacity Blending:** Stufenlose Regelung der Filter-Deckkraft (0–100 %), um den Filmlook dezent mit den Originalfarben des Bildes zu mischen.
3. **Analoge Textureffekte & Belichtungskorrekturen:** Mathematische Simulation von monochromatischem Filmkorn (`Grain`), radialer Randabdunkelung (`Vignette`), mattem Schwarzpunkt (`Fade`) sowie gezielter Highlights- und Shadows-Anpassung.
4. **Custom Presets (JSON Import & Export):** Mischen von Presets und eigenen Slider-Anpassungen, dateibasiertes Speichern unter `CustomPresets/` sowie Teilen als `.json`-Dateien.
5. **Lossless High-Res Export:** Hintergrund-Rendering und verlustfreies Speichern des bearbeiteten Fotos in voller Originalauflösung (JPG/PNG).
6. **Start-Dashboard & SQLite History:** Automatisches Erfassen der zuletzt geöffneten Dateien mittels Entity Framework Core und SQLite zur schnellen Wiederaufnahme der Bearbeitung.

---

## 2. Benutzerhandbuch

### 2.1 Schritt-für-Schritt-Anleitung für Erstanwender

#### Schritt 1: Anwendung starten & Bild laden
Beim Start von LensLab befindet sich der Anwender auf dem **Start-Dashboard**. 
- Klicke auf die zentrale Schaltfläche **"Select Photo to Edit"**, um ein Bild (JPG, PNG, BMP, TIFF) über den Windows-Dateidialog auszuwählen.
- Alternativ kannst du eine Bilddatei einfach per **Drag & Drop** an eine beliebige Stelle im Anwendungsfenster ziehen.
- Hast du LensLab bereits genutzt, werden unter *"Zuletzt geöffnete Bilder"* deine letzten Fotos aufgelistet. Ein Klick darauf öffnet die Datei direkt wieder.

#### Schritt 2: Film-Look auswählen
Sobald das Bild geladen ist, schaltet die Ansicht automatisch in den **Editor-Modus**. In der linken Spalte erscheint die **Filter-Galerie**.
- Klicke auf eine Miniaturansicht (z. B. *"Fujifilm Classic Chrome"*). Das Hauptfenster zeigt sofort das gerenderte Bild mit dem ausgewählten Filmlook.
- Du kannst auch die **Pfeiltasten (Hoch / Runter)** auf der Tastatur nutzen, um schnell durch die Filterliste zu wechseln.

#### Schritt 3: Filter-Stärke anpassen (Opacity Blending)
Ganz oben im rechten Bedienfeld findest du den Regler **"Filter Strength"**.
- Möchtest du den Filmlook nur dezent beimischen, ziehe den Regler z. B. auf 50 %.
- Bei 100 % wirkt der volle Filmlook; bei 0 % siehst du wieder das unberührte Originalfoto.

#### Schritt 4: Manuelle Feinabstimmung über Slider
Verwende die Regler in der rechten Leiste für individuelle Anpassungen:
- **Belichtung & Farben:** Helligkeit, Kontrast, Farbsättigung, Farbtemperatur (Warm/Kalt) und Tint (Grün/Magenta).
- **Analoge Effekte:** Körnung (`Grain`), Vignetten-Intensität (`Vignette`) und matten Schwarzpunkt (`Fade`).
- **Präzisions-Bedienung:** Nutze die Chevron-Buttons (`<` `>`) neben den Slidern für genaue Schritt-Anpassungen. Ein **Doppelklick auf einen Slider** setzt dessen Wert sofort wieder auf den Standardwert (`0.0`) zurück.

#### Schritt 5: Custom Preset speichern & Bild exportieren
- Klicke im rechten Panel auf **"Save as Custom Preset"**, um deine aktuelle Regler-Kombination als eigenes Film-Preset zu speichern. Es erscheint sofort in der linken Galerie unter der Kategorie *"User"*.
- Klicke in der oberen Leiste auf **"Export Image"**, wähle den Dateipfad sowie das Format (JPG oder PNG). LensLab rendert dein Bild in voller Originalauflösung im Hintergrund.

---

### 2.2 Funktionsübersicht im Detail

| Komponente | Funktion | Beschreibung |
| :--- | :--- | :--- |
| **Start-Dashboard** | Recent Files & Landingpage | Übersichtliche Startansicht mit Schnellzugriff auf die zuletzt genutzten Dateien. |
| **Filter-Galerie** | Parallel Thumbnails | Generiert kleine 120px-Vorschauen aller Filter parallel via `Task.WhenAll`. |
| **Preset Management** | JSON Import / Export / Delete | Eigene Looks als `.json`-Datei sichern, importieren oder wieder löschen. |
| **Render-Engine** | Bgra32 Pixel Pipeline | Schnelle Farbraum-Konvertierung (RGB ↔ HSL) und in-place Pixel-Manipulation. |
| **High-Res Export** | Off-Thread Rendering | Rendert das volle Originalbild ohne UI-Blockierung auf dem Thread-Pool. |
| **Datenbank** | EF Core + SQLite | Vergleicht und speichert die Top 5 zuletzt geöffneten Dateien in `lenslab.db`. |

---

## 3. Technische Architektur & MVVM

### 3.1 Struktur der Komponenten (MVVM-Pattern)
LensLab baut konsequent auf dem **Model-View-ViewModel (MVVM)** Entwurfsmuster auf, welches von Microsoft für WPF-Anwendungen empfohlen wird:

- **Models (`LensLab.Models` & `LensLab.Data.Entities`):** Reine Datencontainer ohne UI-Logik.
  - `ImageModel.cs` (`LensLab/Models/ImageModel.cs`): Verwaltet das hochauflösende Originalbild, die 1200px-Arbeitskopie und das gerenderte Zielbild.
  - `FilterPreset.cs` (`LensLab/Models/FilterPreset.cs`): Repräsentiert die Parameter eines Film-Presets (mapped via `System.Text.Json`).
  - `RecentFile.cs` (`LensLab/Data/Entities/RecentFile.cs`): EF Core Entität für die Datenbankhistorie.
- **Views (`LensLab.Views`):** Deklarative Benutzeroberflächen in XAML.
  - `MainWindow.xaml` (`LensLab/Views/MainWindow.xaml`): Hauptfenster mit 3-Spalten-Layout und dynamic Grid-Triggern.
  - `FilterGalleryView.xaml` (`LensLab/Views/FilterGalleryView.xaml`) & `AdjustmentPanel.xaml` (`LensLab/Views/AdjustmentPanel.xaml`): Modulare UserControls.
  - *Code-Behind (`.xaml.cs`)* enthält ausschließlich UI-Events wie Drag & Drop oder Key-Routing.
- **ViewModels (`LensLab.ViewModels`):** Zustandsverwaltung und Logikbindung.
  - `MainViewModel.cs` (`LensLab/ViewModels/MainViewModel.cs`): Hauptsteuerung für Bild-Laden, Export, Save-Dialoge und Datenbank-Aufrufe.
  - `FilterGalleryViewModel.cs` (`LensLab/ViewModels/FilterGalleryViewModel.cs`): Thumbnail-Generierung und Filterauswahl.
  - `AdjustmentViewModel.cs` (`LensLab/ViewModels/AdjustmentViewModel.cs`): Slider-State und Opacity-Steuerung.
- **Services (`LensLab.Services`):** Entkoppelte Hilfsklassen.
  - `ImageProcessingService.cs` (`LensLab/Services/ImageProcessingService.cs`): Reines Pixel-Rendering im `Bgra32`-Format.
  - `FilterLoaderService.cs` (`LensLab/Services/FilterLoaderService.cs`): Asynchrones Parsen der Preset-JSONs.

### 3.2 Asynchronität & Performance-Konzept
1. **Zwei-Stufen-Rendering:** Um flüssige 60 FPS bei Slider-Bewegungen zu garantieren, werden Slider-Anpassungen in Echtzeit auf einer auf **1200 Pixel Kantenlänge skalierten Vorschau (`PreviewImage`)** berechnet. Erst beim finale Export wird die Pixel-Pipeline asynchron auf das volle `OriginalImage` angewendet.
2. **Debouncing & Cancellation Tokens:** Schnelle Slider-Bewegungen lösen viele PropertyChanged-Events aus. Über ein `CancellationTokenSource` (`_renderCts?.Cancel()`) wird ein laufender Render-Task sofort abgebrochen, wenn ein neuer Slider-Wert eintrifft.
3. **Thread-Safety via `.Freeze()`:** Da Pixel-Berechnungen in `Task.Run` auf Hintergrund-Threads laufen, werden alle erzeugten `BitmapSource`-Objekte mittels `.Freeze()` schreibgeschützt gemacht. Dadurch können sie thread-sicher an den WPF-UI-Thread übergeben werden.

### 3.3 Datenbank-Persistenz mit EF Core & SQLite
Die Klasse `LensLabDbContext.cs` (`LensLab/Data/LensLabDbContext.cs`) konfiguriert eine lokale SQLite-Datenbank (`lenslab.db`). Beim Start führt die Anwendung `await db.Database.EnsureCreatedAsync()` aus, um die Datenbanktabelle `RecentFiles` automatisch anzulegen. 

Um Thread-Konflikte zwischen Asynchronität und WPF-UI zu vermeiden, werden Aktualisierungen der gebundenen `ObservableCollection` sauber über den **WPF Dispatcher** (`dispatcher.Invoke(...)`) ausgeführt. Veraltete oder nicht mehr auf der Festplatte existierende Pfade werden mit `File.Exists()` gefiltert.

### 3.4 Mathematische Algorithmen der Bildverarbeitung
Die Pixel-Verarbeitung in `ImageProcessingService.cs` stützt sich auf direkte Array-Manipulationen im **Bgra32**-Format (4 Bytes pro Pixel: Blau, Grün, Rot, Alpha). Für ein Bild der Breite $W$ und Zeilenbreite (Stride) $S = W \cdot 4$ berechnet sich die Array-Adresse eines Pixels an Position $(x, y)$ über:

$$\text{index}(x, y) = y \cdot S + x \cdot 4$$

#### 1. RGB ↔ HSL Farbraum-Konvertierung
Um Farbsättigung ($S$) und Farbton ($H$) unabhängig von der Helligkeit ($L$) anzupassen, werden die normierten Farbwerte $r', g', b' \in [0, 1]$ in den HSL-Farbraum transformiert mit $M = \max(r', g', b')$, $m = \min(r', g', b')$ und $\Delta = M - m$:

- **Helligkeit (Luminance $L$):**
  $$L = \frac{M + m}{2}$$

- **Farbsättigung (Saturation $S$):**
  $$S = \begin{cases} 0 & \text{falls } \Delta = 0 \\ \frac{\Delta}{2.0 - M - m} & \text{falls } L > 0.5 \\ \frac{\Delta}{M + m} & \text{falls } L \le 0.5 \end{cases}$$

- **Farbton (Hue $H$):**
  $$H = \begin{cases} 0^\circ & \text{falls } \Delta = 0 \\ 60^\circ \cdot \left(\frac{g' - b'}{\Delta} \bmod 6\right) & \text{falls } M = r' \\ 60^\circ \cdot \left(\frac{b' - r'}{\Delta} + 2\right) & \text{falls } M = g' \\ 60^\circ \cdot \left(\frac{r' - g'}{\Delta} + 4\right) & \text{falls } M = b' \end{cases}$$

#### 2. Nicht-lineare Kontrast-Transformation
Der Kontrastwert $c \in [-100, 100]$ verändert die Pixelwerte um den Grauwert 128 als Mittelpunkt:

$$\text{factor} = \frac{259 \cdot (c + 255)}{255 \cdot (259 - c)}$$
$$V_{\text{neu}} = \operatorname{Clamp}_0^{255}\left(\text{factor} \cdot (V_{\text{alt}} - 128) + 128\right)$$

#### 3. Radiale Vignetten-Abdunkelung
Die Abdunkelung der Bildecken wird über die quadratische Distanz des Pixels $(x, y)$ zum Mittelpunkt $(cx, cy) = (W/2, H/2)$ berechnet:

$$d_{\text{norm}}^2 = \frac{(x - cx)^2 + (y - cy)^2}{cx^2 + cy^2}$$
$$f_{\text{vignette}} = \max\left(0, 1.0 - \frac{v}{100} \cdot d_{\text{norm}}^2\right) \quad \text{mit } v \in [0, 100]$$
$$V_{\text{neu}} = \operatorname{Clamp}_0^{255}\left(V_{\text{alt}} \cdot f_{\text{vignette}}\right)$$

#### 4. Filter Deckkraft-Mischung (Opacity Blending)
Die stufenlose Einmischung des Filmlooks erfolgt per linearer Interpolation (Lerp) zwischen dem unbehandelten Originalpixel $P_{\text{orig}}$ und dem gefilterten Pixel $P_{\text{filter}}$ basierend auf der Deckkraft $\alpha \in [0, 100]$:

$$P_{\text{gemischt}} = P_{\text{orig}} \cdot \left(1.0 - \frac{\alpha}{100}\right) + P_{\text{filter}} \cdot \left(\frac{\alpha}{100}\right)$$

#### 5. Film Grain / Monochromes Rauschen
Simulation von analogem Filmkorn mit reproduzierbarem Zufalls-Seed $s$:

$$n = \operatorname{Random}_{s}(-g, +g) \quad \text{mit } g \in [0, 100]$$
$$V_{\text{neu}} = \operatorname{Clamp}_0^{255}\left(V_{\text{alt}} + n\right)$$

---

## 4. Verwendeter Fremdcode & Transparenter KI-Einsatz

### 4.1 Externe Bibliotheken & Frameworks

| Paket / Framework | Version | Verwendungszweck |
| :--- | :--- | :--- |
| **`CommunityToolkit.Mvvm`** | `8.2.2` | MVVM-Boilerplate-Reduzierung (`[ObservableProperty]`, `[RelayCommand]`, `ObservableObject`). |
| **`MahApps.Metro`** | `2.4.10` | Modernes Dark-Theme UI Framework (`MetroWindow`, Controls, Resource Dictionaries). |
| **`MahApps.Metro.IconPacks`** | `4.11.0` | Bereitstellung sauberer Vektor-Icons (`PackIconMaterial`). |
| **`Microsoft.EntityFrameworkCore.Sqlite`** | `8.0.2` | Object-Relational Mapper (ORM) & SQLite-Treiber für die History-Datenbank (`lenslab.db`). |
| **`xUnit`** | `2.6.6` | Test-Framework für automatisierte Unit-Tests (`LensLab.Tests`). |

---

### 4.2 Transparenzangabe zum KI-Einsatz

Gemäß den Vorgaben der Studienarbeit wird der Einsatz von KI-Unterstützung (Antigravity / LLM-Assistenten) wie folgt transparent angegeben:

- **Konkrete Einsatzbereiche der KI:**
  1. **Mathematische Formeln & Algorithmen:** Unterstützung bei der präzisen mathematischen Formulierung von Farbraum-Konvertierungen (RGB ↔ HSL) sowie der radialen Distanzberechnung für Vignetteneffekte im `ImageProcessingService`.
  2. **Debugging:** Hilfe bei der Analyse von Asynchronitäts- und Threading-Konflikten (WPF Dispatcher Marshaling bei `ObservableCollection`).
  3. **Code Quality Reviewing:** Überprüfung der MVVM-Konformität (z. B. Umwandlung von Code-Behind Dialogen in XAML Popup-Overlays).
  4. **Dokumentation:** Unterstützung beim Verfassen sauberer XML-Dokumentationskommentare (`/// <summary>`) für C#-Klassen und öffentliche Methoden.
- **Eigenleistung & Code-Verständnis:**
  Die Konzeption der Anwendung, die Verteilung der Architektur, die Gestaltung der XAML-Benutzeroberflächen, die Zusammenstellung der Film-Presets sowie die Implementierung der Geschäftslogik wurden eigenständig durchgeführt. Der gesamte Quellcode und die technischen Zusammenhänge wurden vollständig verstanden und können im Kolloquium vollumfänglich erklärt werden.

---

## 5. Ausblick & Geplante Erweiterungen (Roadmap)

Um LensLab in zukünftigen Versionen noch vielseitiger einzusetzen, sind folgende funktionale Erweiterungen als nächste Entwicklungsstufe angedacht:

1. **Social Media Crop-Tool:** 
   Ein interaktives Zuschnitt-Werkzeug mit vordefinierten Seitenverhältnissen für gängige Netzwerke (z. B. `1:1` Instagram Square, `4:5` Portrait, `9:16` Story/Reels) inklusive visueller Overlay-Maske direkt im Viewport.
2. **Multi-Filter Layering & Mixing (Filter-Blending):**
   Das Stapeln mehrerer Film-Presets übereinander (z. B. Kombination aus dem kühlen Schatten-Look von *Cinestill 800T* mit den warmen Hauttönen von *Kodak Portra 400*).
3. **3D LUT (Look-Up Table) Import (`.cube`):**
   Unterstützung von professionellen 3D LUT-Dateien aus der Videobearbeitung zur hardwarebeschleunigten Farbtransformation via HLSL/Direct3D-Shader.
