using Autodesk.AutoCAD.Geometry;

namespace CabinaElectrica3D.Models;

// ═══════════════════════════════════════════════════════════════════════════════
//  MODELO MAESTRO DE PARÁMETROS — punto único de verdad de toda la configuración.
//  Todos los record son inmutables (init-only). Se construyen desde el ViewModel
//  y se pasan al orquestador; nunca se mutan después de creados.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Parámetros completos de la cabina de transformación.
/// Agrupa todas las sub-configuraciones en un único objeto de dominio.
/// </summary>
public sealed record CabinaParameters
{
    /// <summary>Punto de inserción del conjunto en el espacio de modelo (metros).</summary>
    public Point3d InsertionPoint { get; init; } = Point3d.Origin;

    public CasetaParameters      Caseta        { get; init; } = new();
    public TransformadorParameters Transformador { get; init; } = new();
    public CeldasMTParameters    CeldasMT      { get; init; } = new();
    public CuadroBTParameters    CuadroBT      { get; init; } = new();
    public AcometidaParameters   Acometida     { get; init; } = new();
    public InstallationsParameters Installations { get; init; } = new();
}

// ─────────────────────────────────────────────────────────────────────────────
//  CASETA
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros geométricos de la caseta prefabricada.</summary>
public sealed record CasetaParameters
{
    /// <summary>Ancho en metros (eje X).</summary>
    public double Width { get; init; } = 6.0;

    /// <summary>Profundidad en metros (eje Y).</summary>
    public double Depth { get; init; } = 4.0;

    /// <summary>Altura interior libre en metros.</summary>
    public double Height { get; init; } = 3.0;

    /// <summary>Espesor de muros en metros.</summary>
    public double WallThickness { get; init; } = 0.20;

    /// <summary>Espesor de losa de piso y cubierta.</summary>
    public double SlabThickness { get; init; } = 0.15;

    /// <summary>Ancho de puerta principal (m).</summary>
    public double DoorWidth { get; init; } = 1.0;

    /// <summary>Alto de puerta principal (m).</summary>
    public double DoorHeight { get; init; } = 2.10;

    /// <summary>Número de puertas en fachada frontal.</summary>
    public int DoorCount { get; init; } = 2;

    /// <summary>Generar rejillas de ventilación en paredes laterales.</summary>
    public bool IncludeVentilation { get; init; } = true;
}

// ─────────────────────────────────────────────────────────────────────────────
//  TRANSFORMADOR
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros del transformador de potencia.</summary>
public sealed record TransformadorParameters
{
    /// <summary>Potencia nominal en kVA.</summary>
    public double PowerKva { get; init; } = 630;

    /// <summary>Tensión primaria de MT en kV.</summary>
    public double VoltageHvKv { get; init; } = 13.8;

    /// <summary>Tensión secundaria de BT en V.</summary>
    public double VoltageLvV { get; init; } = 400;

    /// <summary>Ancho de cuba (m).</summary>
    public double TankWidth { get; init; } = 1.40;

    /// <summary>Profundidad de cuba (m).</summary>
    public double TankDepth { get; init; } = 0.80;

    /// <summary>Altura de cuba (m).</summary>
    public double TankHeight { get; init; } = 1.60;

    /// <summary>Número de columnas de aletas de radiador por lado.</summary>
    public int RadiatorColumns { get; init; } = 6;

    /// <summary>Generar bornes de MT y BT en la tapa superior.</summary>
    public bool IncludeTerminals { get; init; } = true;

    /// <summary>Distancia desde la pared izquierda de la caseta (m).</summary>
    public double OffsetFromLeftWall { get; init; } = 1.50;
}

// ─────────────────────────────────────────────────────────────────────────────
//  CELDAS MT
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros del conjunto de celdas de media tensión.</summary>
public sealed record CeldasMTParameters
{
    /// <summary>Número de módulos de celda (entrada + protección + medida...).</summary>
    public int ModuleCount { get; init; } = 3;

    /// <summary>Ancho de cada módulo (m).</summary>
    public double ModuleWidth { get; init; } = 0.75;

    /// <summary>Profundidad de cada módulo (m).</summary>
    public double ModuleDepth { get; init; } = 0.80;

    /// <summary>Altura de cada módulo (m).</summary>
    public double ModuleHeight { get; init; } = 2.40;

    /// <summary>Distancia desde la pared izquierda de la caseta (m).</summary>
    public double OffsetFromLeftWall { get; init; } = 0.20;

    /// <summary>Generar barras de embarrado trifásico entre módulos.</summary>
    public bool IncludeBusbars { get; init; } = true;
}

// ─────────────────────────────────────────────────────────────────────────────
//  CUADRO BT
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros del cuadro de distribución en baja tensión.</summary>
public sealed record CuadroBTParameters
{
    /// <summary>Ancho total del cuadro (m).</summary>
    public double Width { get; init; } = 1.60;

    /// <summary>Profundidad (m).</summary>
    public double Depth { get; init; } = 0.30;

    /// <summary>Altura (m).</summary>
    public double Height { get; init; } = 2.10;

    /// <summary>Número de columnas de módulos DIN visibles en puerta.</summary>
    public int ColumnCount { get; init; } = 2;

    /// <summary>Distancia desde la pared derecha de la caseta (m).</summary>
    public double OffsetFromRightWall { get; init; } = 0.20;
}

// ─────────────────────────────────────────────────────────────────────────────
//  ACOMETIDA MT
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros de la acometida de media tensión.</summary>
public sealed record AcometidaParameters
{
    /// <summary>
    /// <c>true</c> = subterránea (zanja + ductos);
    /// <c>false</c> = aérea.
    /// </summary>
    public bool IsUnderground { get; init; } = true;

    /// <summary>Profundidad de zanja (m).</summary>
    public double TrenchDepth { get; init; } = 1.20;

    /// <summary>Ancho de zanja (m).</summary>
    public double TrenchWidth { get; init; } = 0.60;

    /// <summary>Longitud de la acometida desde la caseta (m).</summary>
    public double Length { get; init; } = 5.0;

    /// <summary>Número de cables individuales (3 = trifásico sin neutro).</summary>
    public int CableCount { get; init; } = 3;

    /// <summary>Diámetro exterior de cada cable (m).</summary>
    public double CableDiameter { get; init; } = 0.05;
}

// ─────────────────────────────────────────────────────────────────────────────
//  INSTALACIONES AUXILIARES
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>Parámetros de bandejas, tierra e iluminación.</summary>
public sealed record InstallationsParameters
{
    /// <summary>Generar bandejas portacables perimetrales.</summary>
    public bool IncludeCableTrays { get; init; } = true;

    /// <summary>Ancho de bandeja (m).</summary>
    public double TrayWidth { get; init; } = 0.30;

    /// <summary>Altura de instalación de bandejas desde el piso (m).</summary>
    public double TrayHeight { get; init; } = 2.50;

    /// <summary>Generar conductor de puesta a tierra perimetral.</summary>
    public bool IncludeEarthingRing { get; init; } = true;

    /// <summary>Altura del conductor de tierra desde el piso (m).</summary>
    public double EarthingHeight { get; init; } = 0.30;
}
