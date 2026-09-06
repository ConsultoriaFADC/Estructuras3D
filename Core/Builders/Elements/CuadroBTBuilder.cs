using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  CUADRO BT BUILDER
//  Posicionado en la pared trasera, lado derecho — opuesto a las celdas MT.
//  Genera: envolvente metálica + cuadrícula de módulos DIN en la puerta frontal.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye el cuadro de distribución en baja tensión.
/// </summary>
public sealed class CuadroBTBuilder : IElementBuilder
{
    private readonly CuadroBTParameters _p;
    private readonly CasetaParameters   _caseta;
    private readonly string             _layer;

    public CuadroBTBuilder(CuadroBTParameters parameters, CasetaParameters caseta)
    {
        _p      = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _caseta = caseta     ?? throw new ArgumentNullException(nameof(caseta));
        _layer  = LayerService.Layers.CuadroBT.Name;
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        var btOrigin = ComputeOrigin(origin);

        BuildEnclosure(tr, db, btOrigin);
        BuildModuleGrid(tr, db, btOrigin);
        BuildTopCableEntry(tr, db, btOrigin);
    }

    // ── POSICIONAMIENTO ───────────────────────────────────────────────────────

    private Point3d ComputeOrigin(Point3d cabinaOrigin) => new(
        x: cabinaOrigin.X + _caseta.Width - _p.OffsetFromRightWall - _p.Width,
        y: cabinaOrigin.Y + _caseta.Depth - _p.Depth,   // pegado a pared trasera
        z: cabinaOrigin.Z);

    // ── ENVOLVENTE ────────────────────────────────────────────────────────────

    private void BuildEnclosure(Transaction tr, Database db, Point3d origin)
    {
        SolidFactory
            .Box(_p.Width, _p.Depth, _p.Height, origin)
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);
    }

    // ── CUADRÍCULA DE MÓDULOS DIN ─────────────────────────────────────────────

    private void BuildModuleGrid(Transaction tr, Database db, Point3d origin)
    {
        const int    rowsPerColumn = 8;
        const double moduleH       = 0.040;
        const double moduleD       = 0.018;    // profundidad (sobresale de la puerta)
        const double rowSpacing    = 0.055;    // paso entre módulos
        const double startZ        = 0.20;     // arranque desde el piso
        const double sideMargin    = 0.05;

        double colWidth = (_p.Width - sideMargin * 2) / _p.ColumnCount;

        for (int col = 0; col < _p.ColumnCount; col++)
        {
            double colX = origin.X + sideMargin + col * colWidth + colWidth * 0.05;
            double modW = colWidth * 0.90;

            for (int row = 0; row < rowsPerColumn; row++)
            {
                double modZ = origin.Z + startZ + row * rowSpacing;

                SolidFactory
                    .Box(modW, moduleD, moduleH,
                         new Point3d(colX, origin.Y - moduleD, modZ))
                    .OnLayer(_layer)
                    .AddToModelSpace(tr, db);
            }
        }
    }

    // ── ENTRADA DE CABLES POR TECHO ───────────────────────────────────────────

    private void BuildTopCableEntry(Transaction tr, Database db, Point3d origin)
    {
        // Prensaestopas: 3 cilindros en la tapa superior representando entradas de cable
        const int    entryCount  = 3;
        const double entryRadius = 0.025;
        const double entryHeight = 0.04;

        double topZ    = origin.Z + _p.Height;
        double spacing = _p.Width / (entryCount + 1);

        for (int i = 0; i < entryCount; i++)
        {
            SolidFactory
                .Cylinder(
                    radius:     entryRadius,
                    height:     entryHeight,
                    baseCenter: new Point3d(origin.X + spacing * (i + 1),
                                           origin.Y + _p.Depth / 2,
                                           topZ))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }
}
