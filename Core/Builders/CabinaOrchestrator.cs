using Autodesk.AutoCAD.DatabaseServices;
using CabinaElectrica3D.Core.Builders.Elements;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders;

// ═══════════════════════════════════════════════════════════════════════════════
//  CABINA ORCHESTRATOR
//  Punto de entrada único para la generación.
//  Conoce el orden correcto de construcción, gestiona la transacción
//  y delega todo el dibujo a los builders individuales.
//
//  Para agregar un elemento nuevo al sistema:
//    1. Crear su XxxParameters en Models/CabinaParameters.cs
//    2. Crear su XxxBuilder en Core/Builders/Elements/
//    3. Agregar una línea en BuildPipeline() aquí abajo.
//  No se toca ningún otro archivo.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Orquestador principal de la cabina de transformación 3D.
/// </summary>
public sealed class CabinaOrchestrator
{
    private readonly LayerService _layerService;

    public CabinaOrchestrator(LayerService layerService)
    {
        _layerService = layerService
            ?? throw new ArgumentNullException(nameof(layerService));
    }

    /// <summary>
    /// Genera la cabina completa en la base de datos activa.
    /// Abre y confirma su propia transacción.
    /// </summary>
    /// <param name="db">Base de datos AutoCAD de destino.</param>
    /// <param name="parameters">Configuración completa del conjunto.</param>
    /// <exception cref="ArgumentNullException"/>
    public void Generate(Database db, CabinaParameters parameters)
    {
        ArgumentNullException.ThrowIfNull(db);
        ArgumentNullException.ThrowIfNull(parameters);

        using var tr = db.TransactionManager.StartTransaction();

        _layerService.EnsureAllLayers(tr);

        foreach (var builder in BuildPipeline(parameters))
            builder.Build(tr, db, parameters.InsertionPoint);

        tr.Commit();
    }

    // ── PIPELINE DE CONSTRUCCIÓN ──────────────────────────────────────────────
    // Orden: afuera → adentro / base → techo.
    // yield return es intencional: los builders se instancian perezosamente
    // y no se mantienen en memoria todos a la vez.

    private static IEnumerable<IElementBuilder> BuildPipeline(CabinaParameters p)
    {
        yield return new CasetaBuilder(p.Caseta);

        yield return new CeldaMTBuilder(p.CeldasMT);

        yield return new TransformadorBuilder(p.Transformador, p.Caseta);

        yield return new CuadroBTBuilder(p.CuadroBT, p.Caseta);

        yield return new AcometidaBuilder(p.Acometida);

        yield return new InstallationsBuilder(p.Installations, p.Caseta);
    }
}
