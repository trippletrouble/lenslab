# LensLab – Film Emulation Photo Editor

LensLab is a professional, high-performance WPF desktop application built on **.NET 8 LTS** and **C#**. It is designed as a modern photo editor that simulates the aesthetic characteristics of classic analog film stocks (such as Fujifilm, Kodak, Ilford, and Agfa) through custom digital pixel processing algorithms.

---

## 🚀 Key Features

- **Modern Dark-Mode UI:** A clean, responsive interface powered by `MahApps.Metro` with a focus on usability and aesthetics.
- **Drag & Drop Loading:** Direct dragging of image files (JPEG, PNG, BMP, TIFF) into the workspace for instant loading.
- **Film Emulation Engine:** Custom presets loaded asynchronously from JSON files containing real-world color science offsets.
- **Off-Thread Pixel Processing:** Asynchronous execution (`Task.Run` with `CancellationToken` support) to keep the UI thread completely fluid during CPU-intensive filter calculations.
- **Extensible Settings:** Slider controls for Brightness, Contrast, Saturation, Temperature, Tint, Grain, Vignette, Fade, Highlights, and Shadows.

---

## 🛠️ Technology Stack

- **Framework:** Windows Presentation Foundation (WPF) on `.NET 8.0-windows`
- **Architectural Pattern:** Model-View-ViewModel (MVVM) utilizing the `CommunityToolkit.Mvvm` library
- **UI & Icons:** `MahApps.Metro` and `MahApps.Metro.IconPacks.Material`
- **Serialization:** `System.Text.Json` for loading customizable preset configuration files
- **Image Pipeline:** WPF's native `System.Windows.Media.Imaging` namespace for high-fidelity pixel array manipulation (no legacy `System.Drawing` dependencies)

---

## 📁 Repository Structure

```
LensLab/
├── Models/              - Pure data containers (e.g., ImageModel, FilterPreset)
├── ViewModels/          - App state controllers and commands (e.g., MainViewModel)
├── Views/               - Reusable XAML UserControls and MainWindow
├── Services/            - Business logic services (e.g., FileDialogService, FilterLoaderService)
├── Converters/          - WPF value converters (e.g., BoolToVisibilityConverter)
├── Resources/           - XAML Styles and JSON film presets
│   ├── Styles/          - Centralized UI styles
│   └── Filters/         - 10 detailed film preset configurations
└── App.xaml / App.cs    - Programmatic application entry point and bootstrapping
```

---

## ⚙️ Getting Started

### Prerequisites
- [Visual Studio 2022](https://visualstudio.microsoft.com/) (Community, Professional, or Enterprise)
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Running the Application
1. Clone this repository to your local machine.
2. Open the solution file `LensLab.sln` or `LensLab_Naren.slnx` in Visual Studio.
3. Restore the NuGet packages (`CommunityToolkit.Mvvm`, `MahApps.Metro`, `MahApps.Metro.IconPacks.Material`).
4. Press **F5** or click **Start** in Visual Studio to build and run the application.

Or build and run via CLI:
```bash
dotnet build
dotnet run --project LensLab/LensLab.csproj
```

---

## 🎨 Design Philosophy & Coding Standards

- **Simplicity & Reusability (React-like):** The visual layout is decomposed into small, isolated `UserControls` and `DataTemplates` to ensure modularity and clean separation of concerns.
- **Zero Blocked Threads:** Long-running file loading and rendering pipelines are awaited asynchronously.
- **Null Safety:** Fully compiled with `<Nullable>enable</Nullable>` to eliminate runtime null-reference exceptions.
- **Clean Code & Self-Documentation:** Follows strict C# naming conventions and contains extensive XML documentation (`/// <summary>`) on all classes, services, ViewModels, and logic methods.
