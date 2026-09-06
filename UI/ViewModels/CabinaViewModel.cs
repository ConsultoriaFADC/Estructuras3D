using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.UI.ViewModels;

// ═══════════════════════════════════════════════════════════════════════════════
//  CABINA VIEW MODEL
//  Implementa INotifyPropertyChanged con SetField<T> genérico.
//  Cada propiedad observable mapea 1:1 a un campo de CabinaParameters.
//
//  ToParameters() es el único método que sabe cómo pasar de UI → dominio.
//  El resto del sistema trabaja con CabinaParameters, nunca con este ViewModel.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// ViewModel MVVM para la paleta de configuración de la cabina.
/// </summary>
public sealed class CabinaViewModel : INotifyPropertyChanged
{
    // ── CASETA ────────────────────────────────────────────────────────────────

    private double _casetaWidth = 6.0;
    public double CasetaWidth
    {
        get => _casetaWidth;
        set => SetField(ref _casetaWidth, Math.Max(3.0, value));
    }

    private double _casetaDepth = 4.0;
    public double CasetaDepth
    {
        get => _casetaDepth;
        set => SetField(ref _casetaDepth, Math.Max(2.0, value));
    }

    private double _casetaHeight = 3.0;
    public double CasetaHeight
    {
        get => _casetaHeight;
        set => SetField(ref _casetaHeight, Math.Max(2.40, value));
    }

    private double _casetaWallThickness = 0.20;
    public double CasetaWallThickness
    {
        get => _casetaWallThickness;
        set => SetField(ref _casetaWallThickness, Math.Clamp(value, 0.10, 0.40));
    }

    private int _casetaDoorCount = 2;
    public int CasetaDoorCount
    {
        get => _casetaDoorCount;
        set => SetField(ref _casetaDoorCount, Math.Clamp(value, 1, 4));
    }

    private bool _casetaIncludeVentilation = true;
    public bool CasetaIncludeVentilation
    {
        get => _casetaIncludeVentilation;
        set => SetField(ref _casetaIncludeVentilation, value);
    }

    // ── TRANSFORMADOR ─────────────────────────────────────────────────────────

    private double _trafoPowerKva = 630;
    public double TrafoPowerKva
    {
        get => _trafoPowerKva;
        set => SetField(ref _trafoPowerKva, value);
    }

    private double _trafoTankWidth = 1.40;
    public double TrafoTankWidth
    {
        get => _trafoTankWidth;
        set => SetField(ref _trafoTankWidth, Math.Max(0.5, value));
    }

    private double _trafoTankDepth = 0.80;
    public double TrafoTankDepth
    {
        get => _trafoTankDepth;
        set => SetField(ref _trafoTankDepth, Math.Max(0.3, value));
    }

    private double _trafoTankHeight = 1.60;
    public double TrafoTankHeight
    {
        get => _trafoTankHeight;
        set => SetField(ref _trafoTankHeight, Math.Max(0.5, value));
    }

    private int _trafoRadiatorColumns = 6;
    public int TrafoRadiatorColumns
    {
        get => _trafoRadiatorColumns;
        set => SetField(ref _trafoRadiatorColumns, Math.Clamp(value, 2, 16));
    }

    private bool _trafoIncludeTerminals = true;
    public bool TrafoIncludeTerminals
    {
        get => _trafoIncludeTerminals;
        set => SetField(ref _trafoIncludeTerminals, value);
    }

    private double _trafoOffsetLeft = 1.50;
    public double TrafoOffsetLeft
    {
        get => _trafoOffsetLeft;
        set => SetField(ref _trafoOffsetLeft, Math.Max(0.5, value));
    }

    // ── CELDAS MT ─────────────────────────────────────────────────────────────

    private int _celdasModuleCount = 3;
    public int CeldasModuleCount
    {
        get => _celdasModuleCount;
        set => SetField(ref _celdasModuleCount, Math.Clamp(value, 1, 10));
    }

    private double _celdasModuleWidth = 0.75;
    public double CeldasModuleWidth
    {
        get => _celdasModuleWidth;
        set => SetField(ref _celdasModuleWidth, Math.Max(0.50, value));
    }

    private double _celdasModuleDepth = 0.80;
    public double CeldasModuleDepth
    {
        get => _celdasModuleDepth;
        set => SetField(ref _celdasModuleDepth, Math.Max(0.40, value));
    }

    private double _celdasModuleHeight = 2.40;
    public double CeldasModuleHeight
    {
        get => _celdasModuleHeight;
        set => SetField(ref _celdasModuleHeight, Math.Max(1.0, value));
    }

    private bool _celdasIncludeBusbars = true;
    public bool CeldasIncludeBusbars
    {
        get => _celdasIncludeBusbars;
        set => SetField(ref _celdasIncludeBusbars, value);
    }

    // ── CUADRO BT ─────────────────────────────────────────────────────────────

    private double _cuadroWidth = 1.60;
    public double CuadroWidth
    {
        get => _cuadroWidth;
        set => SetField(ref _cuadroWidth, Math.Max(0.60, value));
    }

    private double _cuadroHeight = 2.10;
    public double CuadroHeight
    {
        get => _cuadroHeight;
        set => SetField(ref _cuadroHeight, Math.Max(1.0, value));
    }

    private int _cuadroColumnCount = 2;
    public int CuadroColumnCount
    {
        get => _cuadroColumnCount;
        set => SetField(ref _cuadroColumnCount, Math.Clamp(value, 1, 4));
    }

    // ── ACOMETIDA ─────────────────────────────────────────────────────────────

    private bool _acometidaIsUnderground = true;
    public bool AcometidaIsUnderground
    {
        get => _acometidaIsUnderground;
        set => SetField(ref _acometidaIsUnderground, value);
    }

    private double _acometidaLength = 5.0;
    public double AcometidaLength
    {
        get => _acometidaLength;
        set => SetField(ref _acometidaLength, Math.Max(1.0, value));
    }

    private int _acometidaCableCount = 3;
    public int AcometidaCableCount
    {
        get => _acometidaCableCount;
        set => SetField(ref _acometidaCableCount, Math.Clamp(value, 1, 6));
    }

    private double _acometidaCableDiameter = 0.05;
    public double AcometidaCableDiameter
    {
        get => _acometidaCableDiameter;
        set => SetField(ref _acometidaCableDiameter, Math.Clamp(value, 0.01, 0.15));
    }

    // ── INSTALACIONES ─────────────────────────────────────────────────────────

    private bool _instIncludeCableTrays = true;
    public bool InstIncludeCableTrays
    {
        get => _instIncludeCableTrays;
        set => SetField(ref _instIncludeCableTrays, value);
    }

    private bool _instIncludeEarthingRing = true;
    public bool InstIncludeEarthingRing
    {
        get => _instIncludeEarthingRing;
        set => SetField(ref _instIncludeEarthingRing, value);
    }

    // ── CONVERSIÓN AL MODELO DE DOMINIO ──────────────────────────────────────

    /// <summary>
    /// Proyecta el estado actual de la UI hacia un objeto de parámetros inmutable.
    /// Este es el único puente entre la capa de presentación y la de dominio.
    /// </summary>
    public CabinaParameters ToParameters(Point3d insertionPoint) => new()
    {
        InsertionPoint = insertionPoint,

        Caseta = new CasetaParameters
        {
            Width             = CasetaWidth,
            Depth             = CasetaDepth,
            Height            = CasetaHeight,
            WallThickness     = CasetaWallThickness,
            DoorCount         = CasetaDoorCount,
            IncludeVentilation = CasetaIncludeVentilation
        },

        Transformador = new TransformadorParameters
        {
            PowerKva          = TrafoPowerKva,
            TankWidth         = TrafoTankWidth,
            TankDepth         = TrafoTankDepth,
            TankHeight        = TrafoTankHeight,
            RadiatorColumns   = TrafoRadiatorColumns,
            IncludeTerminals  = TrafoIncludeTerminals,
            OffsetFromLeftWall = TrafoOffsetLeft
        },

        CeldasMT = new CeldasMTParameters
        {
            ModuleCount      = CeldasModuleCount,
            ModuleWidth      = CeldasModuleWidth,
            ModuleDepth      = CeldasModuleDepth,
            ModuleHeight     = CeldasModuleHeight,
            IncludeBusbars   = CeldasIncludeBusbars
        },

        CuadroBT = new CuadroBTParameters
        {
            Width        = CuadroWidth,
            Height       = CuadroHeight,
            ColumnCount  = CuadroColumnCount
        },

        Acometida = new AcometidaParameters
        {
            IsUnderground  = AcometidaIsUnderground,
            Length         = AcometidaLength,
            CableCount     = AcometidaCableCount,
            CableDiameter  = AcometidaCableDiameter
        },

        Installations = new InstallationsParameters
        {
            IncludeCableTrays   = InstIncludeCableTrays,
            IncludeEarthingRing = InstIncludeEarthingRing
        }
    };

    // ── INPC INFRASTRUCTURE ───────────────────────────────────────────────────

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? name = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        OnPropertyChanged(name);
        return true;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  RELAY COMMAND — implementación mínima de ICommand sin dependencias externas
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Implementación genérica de <see cref="ICommand"/> sin librerías externas.
/// </summary>
public sealed class RelayCommand(Action execute, Func<bool>? canExecute = null) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add    => CommandManager.RequerySuggested += value;
        remove => CommandManager.RequerySuggested -= value;
    }

    public bool CanExecute(object? parameter) => canExecute?.Invoke() ?? true;
    public void Execute(object? parameter)    => execute();
}
