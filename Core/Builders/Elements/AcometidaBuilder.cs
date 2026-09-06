using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  ACOMETIDA BUILDER
//  Dos estrategias intercambiables sin tocar el orquestador:
//    • Subterránea: zanja de tierra + cables modelados individualmente
//    • Aérea:       cables suspendidos a altura normativa
//
//  Los cables se crean con CylinderAlongY para que corran en sentido Y.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye la acometida de media tensión (subterránea o aérea).
/// </summary>
public sealed class AcometidaBuilder : IElementBuilder
{
    private readonly AcometidaParameters _p;
    private readonly string              _layer;

    public AcometidaBuilder(AcometidaParameters parameters)
    {
        _p     = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _layer = LayerService.Layers.Acometida.Name;
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        if (_p.IsUnderground)
            BuildUndergroundEntry(tr, db, origin);
        else
            BuildAerialEntry(tr, db, origin);
    }

    // ── ACOMETIDA SUBTERRÁNEA ─────────────────────────────────────────────────

    private void BuildUndergroundEntry(Transaction tr, Database db, Point3d origin)
    {
        // La zanja arranca desde la pared frontal (Y = origin.Y) y sale hacia afuera
        double trenchStartY = origin.Y - _p.Length;
        double trenchZ      = origin.Z - _p.TrenchDepth;

        // Cuerpo de la zanja (tierra excavada — representación simbólica)
        SolidFactory
            .Box(
                width:  _p.TrenchWidth,
                depth:  _p.Length,
                height: _p.TrenchDepth,
                origin: new Point3d(
                    origin.X + (_p.TrenchWidth > 0 ? 0.50 : 0),
                    trenchStartY,
                    trenchZ))
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);

        // Cables dentro de la zanja
        double cableSpacing = _p.TrenchWidth / (_p.CableCount + 1);
        double cableRadius  = _p.CableDiameter / 2;

        for (int i = 0; i < _p.CableCount; i++)
        {
            double cableX = origin.X + 0.50 + cableSpacing * (i + 1);
            double cableZ = trenchZ + cableRadius + 0.05;  // sobre el fondo de la zanja

            SolidFactory
                .CylinderAlongY(
                    radius:     cableRadius,
                    length:     _p.Length + 0.30,           // +30 cm penetra en caseta
                    startPoint: new Point3d(cableX, trenchStartY, cableZ))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }

    // ── ACOMETIDA AÉREA ───────────────────────────────────────────────────────

    private void BuildAerialEntry(Transaction tr, Database db, Point3d origin)
    {
        // Cables a 3.5 m sobre el nivel de piso terminado
        const double aerialHeight = 3.50;
        const double phaseSpacing = 0.40;

        double cableRadius = _p.CableDiameter / 2;

        for (int i = 0; i < _p.CableCount; i++)
        {
            double cableX = origin.X + 0.50 + i * phaseSpacing;
            double startY = origin.Y - _p.Length;

            SolidFactory
                .CylinderAlongY(
                    radius:     cableRadius,
                    length:     _p.Length,
                    startPoint: new Point3d(cableX, startY, origin.Z + aerialHeight))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }
}
