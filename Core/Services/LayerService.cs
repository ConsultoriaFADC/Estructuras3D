using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;

namespace CabinaElectrica3D.Core.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//  LAYER SERVICE
//  Único responsable de crear y garantizar la existencia de capas AutoCAD.
//  Idempotente: llamar varias veces no duplica capas.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Definición inmutable de una capa AutoCAD.
/// Sirve como descriptor, no como objeto de base de datos.
/// </summary>
public sealed record LayerDefinition(
    string Name,
    Color  Color,
    LineWeight LineWeight = LineWeight.LineWeight035,
    string Description = "");

/// <summary>
/// Servicio de gestión de capas.
/// Proporciona un catálogo estático <see cref="Layers"/> como
/// único punto de verdad para todos los nombres de capa del proyecto.
/// </summary>
public sealed class LayerService
{
    // ── CATÁLOGO ─────────────────────────────────────────────────────────────
    /// <summary>Catálogo de capas del proyecto.</summary>
    public static class Layers
    {
        public static readonly LayerDefinition Caseta =
            new("CAB-CASETA",
                Color.FromRgb(74, 144, 217),
                LineWeight.LineWeight050,
                "Estructura de la caseta prefabricada");

        public static readonly LayerDefinition Transformador =
            new("CAB-TRAFO",
                Color.FromRgb(232, 160, 0),
                LineWeight.LineWeight035,
                "Transformador de potencia");

        public static readonly LayerDefinition CeldasMT =
            new("CAB-CELDAS-MT",
                Color.FromRgb(45, 168, 155),
                LineWeight.LineWeight035,
                "Celdas de media tensión");

        public static readonly LayerDefinition CuadroBT =
            new("CAB-CUADRO-BT",
                Color.FromRgb(139, 111, 212),
                LineWeight.LineWeight035,
                "Cuadro de distribución BT");

        public static readonly LayerDefinition Acometida =
            new("CAB-ACOMETIDA",
                Color.FromRgb(217, 93, 58),
                LineWeight.LineWeight050,
                "Acometida de media tensión");

        public static readonly LayerDefinition Bandejas =
            new("CAB-BANDEJAS",
                Color.FromRgb(90, 112, 144),
                LineWeight.LineWeight025,
                "Bandejas portacables");

        public static readonly LayerDefinition Tierra =
            new("CAB-TIERRA",
                Color.FromRgb(61, 170, 107),
                LineWeight.LineWeight025,
                "Red de puesta a tierra");

        public static readonly LayerDefinition Referencia =
            new("CAB-REF",
                Color.FromRgb(40, 60, 80),
                LineWeight.LineWeight000,
                "Geometría de referencia (no imprime)");

        /// <summary>Todas las capas del proyecto para iteración.</summary>
        public static IEnumerable<LayerDefinition> All =>
        [
            Caseta, Transformador, CeldasMT, CuadroBT,
            Acometida, Bandejas, Tierra, Referencia
        ];
    }

    // ── IMPLEMENTACIÓN ───────────────────────────────────────────────────────
    private readonly Database _db;

    public LayerService(Database database) =>
        _db = database ?? throw new ArgumentNullException(nameof(database));

    /// <summary>
    /// Crea en la base de datos todas las capas del catálogo.
    /// Las capas que ya existen no se modifican.
    /// </summary>
    /// <param name="tr">Transacción AutoCAD activa.</param>
    public void EnsureAllLayers(Transaction tr)
    {
        foreach (var definition in Layers.All)
            EnsureLayer(tr, definition);
    }

    /// <summary>
    /// Garantiza que existe una capa específica con las propiedades indicadas.
    /// Si ya existe, no hace ninguna modificación.
    /// </summary>
    public void EnsureLayer(Transaction tr, LayerDefinition definition)
    {
        ArgumentNullException.ThrowIfNull(tr);
        ArgumentNullException.ThrowIfNull(definition);

        var table = (LayerTable)tr.GetObject(_db.LayerTableId, OpenMode.ForRead);

        if (table.Has(definition.Name))
            return; // idempotente — no sobreescribir capas existentes

        table.UpgradeOpen();

        var record = new LayerTableRecord
        {
            Name       = definition.Name,
            Color      = definition.Color,
            LineWeight = definition.LineWeight,
        };

        table.Add(record);
        tr.AddNewlyCreatedDBObject(record, true);
    }
}
