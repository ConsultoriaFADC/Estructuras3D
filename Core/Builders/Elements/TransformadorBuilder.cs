using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  TRANSFORMADOR BUILDER
//  Genera: cuba principal, aletas de radiador (izquierda y derecha),
//  bornes de MT y bornes de BT en la tapa superior.
//
//  Se posiciona automáticamente a partir de OffsetFromLeftWall y centrado
//  en la profundidad de la caseta.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye el transformador de potencia: cuba, radiadores y bornes.
/// </summary>
public sealed class TransformadorBuilder : IElementBuilder
{
    private readonly TransformadorParameters _p;
    private readonly CasetaParameters        _caseta;
    private readonly string                  _layer;

    public TransformadorBuilder(TransformadorParameters parameters, CasetaParameters caseta)
    {
        _p      = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _caseta = caseta     ?? throw new ArgumentNullException(nameof(caseta));
        _layer  = LayerService.Layers.Transformador.Name;
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        var trafOrigin = ComputeTransformerOrigin(origin);

        BuildTank(tr, db, trafOrigin);
        BuildRadiators(tr, db, trafOrigin);

        if (_p.IncludeTerminals)
            BuildTerminals(tr, db, trafOrigin);
    }

    // ── POSICIONAMIENTO ───────────────────────────────────────────────────────

    /// <summary>
    /// Calcula el punto de origen del transformador (corner inferior izquierdo frontal).
    /// Centrado en la profundidad de la caseta con clearance mínimo de pared.
    /// </summary>
    private Point3d ComputeTransformerOrigin(Point3d cabinaOrigin)
    {
        double x = cabinaOrigin.X + _p.OffsetFromLeftWall;
        double y = cabinaOrigin.Y + (_caseta.Depth - _p.TankDepth) / 2;
        double z = cabinaOrigin.Z;  // apoyado en el piso

        return new Point3d(x, y, z);
    }

    // ── CUBA ─────────────────────────────────────────────────────────────────

    private void BuildTank(Transaction tr, Database db, Point3d origin)
    {
        SolidFactory
            .Box(_p.TankWidth, _p.TankDepth, _p.TankHeight, origin)
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);
    }

    // ── RADIADORES ────────────────────────────────────────────────────────────

    private void BuildRadiators(Transaction tr, Database db, Point3d trafOrigin)
    {
        const double finThickness = 0.015;  // espesor de aleta (m)
        const double finOverhang  = 0.060;  // vuelo lateral (m)
        const double finHeight    = 0.28;   // altura de cada aleta (m)
        const double marginZ      = 0.10;   // margen desde la base

        double usableHeight = _p.TankHeight - marginZ * 2;
        double spacing      = _p.RadiatorColumns > 1
            ? usableHeight / (_p.RadiatorColumns - 1)
            : usableHeight;

        for (int i = 0; i < _p.RadiatorColumns; i++)
        {
            double z = trafOrigin.Z + marginZ + i * spacing;

            // Aleta lado izquierdo de la cuba
            SolidFactory
                .Box(
                    width:  finThickness,
                    depth:  _p.TankDepth + finOverhang * 2,
                    height: finHeight,
                    origin: new Point3d(
                        trafOrigin.X - finThickness,
                        trafOrigin.Y - finOverhang,
                        z - finHeight / 2))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);

            // Aleta lado derecho de la cuba
            SolidFactory
                .Box(
                    width:  finThickness,
                    depth:  _p.TankDepth + finOverhang * 2,
                    height: finHeight,
                    origin: new Point3d(
                        trafOrigin.X + _p.TankWidth,
                        trafOrigin.Y - finOverhang,
                        z - finHeight / 2))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }

    // ── BORNES ────────────────────────────────────────────────────────────────

    private void BuildTerminals(Transaction tr, Database db, Point3d trafOrigin)
    {
        double topZ = trafOrigin.Z + _p.TankHeight;

        BuildTerminalRow(tr, db,
            count:   3,
            radius:  0.040,
            height:  0.22,
            baseX:   trafOrigin.X,
            width:   _p.TankWidth,
            fixedY:  trafOrigin.Y + _p.TankDepth * 0.70,   // lado MT (trasero)
            baseZ:   topZ);

        BuildTerminalRow(tr, db,
            count:   4,
            radius:  0.035,
            height:  0.18,
            baseX:   trafOrigin.X,
            width:   _p.TankWidth,
            fixedY:  trafOrigin.Y + _p.TankDepth * 0.30,   // lado BT (frontal)
            baseZ:   topZ);
    }

    private void BuildTerminalRow(Transaction tr, Database db,
        int count, double radius, double height,
        double baseX, double width, double fixedY, double baseZ)
    {
        double spacing = width / (count + 1);

        for (int i = 0; i < count; i++)
        {
            SolidFactory
                .Cylinder(
                    radius:     radius,
                    height:     height,
                    baseCenter: new Point3d(baseX + spacing * (i + 1), fixedY, baseZ))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }
}
