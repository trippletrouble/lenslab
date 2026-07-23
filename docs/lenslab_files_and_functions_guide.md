# LensLab – Dateien & wichtige Funktionen (Erklär-Guide)

Dieser Guide erklärt Datei für Datei alle wichtigen Klassen und ihre zentralen Methoden, damit du im Kolloquium genau beschreiben kannst, wo welche Logik liegt und was die einzelnen Funktionen tun.

---

## 1. Services (Geschäfts- & Bildlogik)

### ImageProcessingService.cs
Das mathematische Herzstück der App. Führt alle Pixel-Manipulationen aus.

* **`GetPixelData(BitmapSource source)`**:
  Wandelt eine `BitmapSource` bei Bedarf in `Bgra32` um und liest alle Pixel über `CopyPixels` in ein rohes `byte[]`-Array aus. (4 Bytes pro Pixel: Blau, Grün, Rot, Alpha).
* **`CreateBitmapFromPixels(byte[] pixels, int width, int height, ...)`**:
  Erzeugt aus einem bearbeiteten `byte[]`-Array wieder eine `BitmapSource` und ruft `.Freeze()` auf, damit das Bild thread-sicher von der UI angezeigt werden kann.
* **`RgbToHsl(byte r, byte g, byte b)` / `HslToRgb(double h, double s, double l)`**:
  Wandelt RGB-Farbwerte in den HSL-Farbraum (Hue/Farbton, Saturation/Sättigung, Lightness/Helligkeit) um und zurück. Wird für genaue Farbanpassungen benötigt.
* **`ApplyBrightness` / `ApplyContrast` / `ApplySaturation`**:
  Verändern Helligkeit (Offset auf RGB), Kontrast (nicht-lineare Faktor-Formel um den Grauwert 128) und Sättigung (über HSL) direkt im Pixel-Array.
* **`ApplyTemperature` / `ApplyTint` / `ApplyRgbMultipliers`**:
  Passen die Farbtemperatur (Rot vs. Blau), den Tint (Grün vs. Magenta) und einzelne Kanal-Multiplikatoren an.
* **`ApplyHighlights` / `ApplyShadows`**:
  Verändern gezielt helle Pixel (Luminanz > 170) oder dunkle Pixel (Luminanz < 85) mittels stufenloser Gewichtung.
* **`ApplyGrain` / `ApplyVignette` / `ApplyFade`**:
  - `ApplyGrain`: Addiert monochromes Zufalls-Rauschen (`Random` mit festem Seed) auf die Pixel.
  - `ApplyVignette`: Dunkelt die Bildecken basierend auf der quadratischen Distanz zum Mittelpunkt ab.
  - `ApplyFade`: Hebt den Schwarzpunkt an, um den matten Analog-Papier-Look zu erzeugen.
* **`ApplyHslOffsets(byte[] pixels, FilterPreset filter, ...)`**:
  Passt 8 spezifische Farbbereiche (Rot, Orange, Gelb, Grün, Aqua, Blau, Violett, Magenta) individuell in Farbton, Sättigung und Helligkeit an.
* **`ApplyEditAsync(BitmapSource source, AdjustmentSettings settings, FilterPreset? filter, double filterOpacity, CancellationToken ct)`**:
  Die Haupt-Pipeline: Führt das Bild-Rendering asynchron aus. Wendet zuerst den Film-Filter an, mischt diesen nach `filterOpacity` mit dem Originalbild und rechnet danach die manuellen Slider-Einstellungen oben drauf.

---

### FilterLoaderService.cs
Lädt die analogen Film-Presets aus JSON-Dateien.

* **`LoadAllFiltersAsync()`**:
  Scannt die Ordner `Resources/Filters/` und `CustomPresets/` nach `.json`-Dateien, liest sie über `System.Text.Json` ein und gibt eine sortierte Liste aller Presets zurück. Fehlerhafte JSONs werden abgefangen und übersprungen.

---

### FileDialogService.cs & IFileDialogService.cs
Kapselt die Windows-Dateiauswahl.

* **`OpenImageFile()`**:
  Öffnet den Standard-Windows-Dateidialog für Bildformate (JPG, PNG, BMP, TIFF) und gibt den gewählten Dateipfad zurück.

---

## 2. ViewModels (MVVM Steuerung)

### MainViewModel.cs
Das Haupt-ViewModel. Steuert den Gesamtzustand und verknüpft alle Komponenten.

* **`InitializeAsync()`**:
  Wird beim App-Start aufgerufen und lädt alle Filter über den `FilterLoaderService`.
* **`OpenImageAsync(string? filePath)`**:
  Lädt eine Bilddatei asynchron mit `BitmapCacheOption.OnLoad` (verhindert Dateisperren auf der Festplatte) und `.Freeze()`, erstellt ein auf max. 1200px skaliertes `PreviewImage` für flüssiges Arbeiten, speichert den Pfad in der SQLite-Datenbank und stößt die Thumbnail-Generierung an.
* **`OnAdjustmentChanged(...)` / `OnFilterSelected(...)`**:
  Event-Handler, die auf Slider-Bewegungen oder Filter-Auswahlen reagieren. Brechen über `_renderCts?.Cancel()` vorherige Render-Tasks ab und starten sofort ein neues Rendering.
* **`ExportImageAsync()`**:
  Rendert die gesamte Bearbeitungskette auf das hochauflösende `OriginalImage` im Hintergrund und speichert das Ergebnis als JPG oder PNG.
* **`ShowSavePresetDialog()` / `SavePresetInternalAsync()`**:
  Öffnet den Dialog zum Speichern eigener Presets und schreibt die kombinierten Einstellungen als neue JSON-Datei in den Ordner `CustomPresets/`.
* **`LoadRecentFilesAsync()` / `AddRecentFileAsync()` / `OpenRecentFileAsync()`**:
  Verwaltet über Entity Framework Core (SQLite) die Top 5 der zuletzt geöffneten Bilder für das Start-Dashboard.

---

### FilterGalleryViewModel.cs
Verwaltet die linke Filterleiste.

* **`LoadFiltersAsync(BitmapSource sourceImage)`**:
  Skaliert das Originalbild vorab auf 120px Breite und berechnet alle Filter-Thumbnails parallel via `Task.WhenAll`.
* **`SelectNextFilter()` / `SelectPreviousFilter()`**:
  Ermöglicht die Navigation durch die Filterliste mittels Tastatur-Pfeiltasten.
* **`ImportPresetAsync()` / `ExportPreset()` / `ConfirmDelete()`**:
  Logik zum Importieren, Exportieren und Löschen von benutzerdefinierten JSON-Preset-Dateien.

---

### AdjustmentViewModel.cs
Verwaltet die manuellen Slider-Werte.

* **`Settings`**: Hält das AdjustmentSettings-Modell.
* **`FilterOpacity`**: Eigenschaft für die Filter-Stärke (0–100%).
* **`ResetAll()`**: Setzt alle Slider-Werte auf `0.0` und die Filter-Stärke auf `100%` zurück.
* **`AdjustmentChanged` (Event)**: Informiert das `MainViewModel`, sobald sich ein Slider bewegt.

---

## 3. Models (Datenstrukturen)

* **ImageModel.cs**: Hält `OriginalImage` (hohe Auflösung), `PreviewImage` (1200px Arbeitskopie), `CurrentImage` (aktuell gerendertes Bild) sowie Metadaten (`FilePath`, `FileName`, `Width`, `Height`).
* **FilterPreset.cs**: Reiner Datencontainer mit `[JsonPropertyName]`-Attributen für alle Filter-Parameter (RGB, HSL, Körnung, Vignette etc.).
* **FilterPreviewItem.cs**: Verbindet ein `FilterPreset` mit seinem berechneten `Thumbnail` (BitmapSource) und dem Auswahlstatus `IsSelected`.
* **AdjustmentSettings.cs**: Hält die Werte der manuellen Slider und bietet die Methode `Reset()`.

---

## 4. Data & Datenbank (EF Core + SQLite)

* **LensLabDbContext.cs**: EF Core DbContext. Konfiguriert in `OnConfiguring` die lokale SQLite-Datenbank `lenslab.db`.
* **RecentFile.cs**: Datenbank-Entität für die zuletzt geöffneten Dateien (`Id`, `FilePath`, `FileName`, `LastOpened`).

---

## 5. Views & Converters (UI-Schicht)

* **MainWindow.xaml & MainWindow.xaml.cs**: Hauptfenster mit 3-Spalten-Layout. Behandelt Drag & Drop (`OnDrop`) und Tastatureingaben (`OnPreviewKeyDown`).
* **FilterGalleryView.xaml**: UserControl für die linke Spalte. Zeigt die Thumbnails gruppiert nach Kategorien an.
* **AdjustmentPanel.xaml**: UserControl für die rechte Spalte mit allen Slidern, Chevrons und Buttons.
* **BoolToVisibilityConverter.cs**: Wandelt Bools/Null-Werte für XAML-Elemente in `Visibility.Visible` oder `Visibility.Collapsed` um.
