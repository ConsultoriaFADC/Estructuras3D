using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  CASETA BUILDER
//  Responsabilidades: losa de piso, muros (con vanos de puertas), cubierta
//  y rejillas de ventilación laterales.
//
//  Estrategia: se crea una caja exterior sólida, se le resta el hueco interior
//  (operación booleana Subtract) y luego se restan los vanos de puertas.
//  Esto garantiza muros paramétricos con espesor correcto sin dibujar cada muro
//  por separado.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye la caseta prefabricada: losa de piso, paredes,
/// cubierta, vanos de puerta y ventilación lateral.
/// </summary>
public sealed class CasetaBuilder : IElementBuilder
{
    private readonly CasetaParameters _p;
    private readonly string _layer;

    public CasetaBuilder(CasetaParameters parameters)
    {
        _p     = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _layer = LayerService.Layers.Caseta.Name;
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        BuildFloorSlab(tr, db, origin);
        BuildWallShell(tr, db, origin);
        BuildRoofSlab(tr, db, origin);

        if (_p.IncludeVentilation)
            BuildVentilationGrilles(tr, db, origin);
    }

    // ── LOSA DE PISO ─────────────────────────────────────────────────────────

    private void BuildFloorSlab(Transaction tr, Database db, Point3d origin)
    {
        double wt = _p.WallThickness;

        var slab = SolidFactory
            .Box(
                width:  _p.Width  + 2 * wt,
                depth:  _p.Depth  + 2 * wt,
                height: _p.SlabThickness,
                origin: new Point3d(origin.X - wt, origin.Y - wt, origin.Z - _p.SlabThickness))
            .OnLayer(_layer);

        slab.AddToModelSpace(tr, db);
    }

    // ── MUROS (caja exterior menos hueco interior menos vanos) ───────────────

    private void BuildWallShell(Transaction tr, Database db, Point3d origin)
    {
        double wt  = _p.WallThickness;
        double extW = _p.Width  + 2 * wt;
        double extD = _p.Depth  + 2 * wt;

        // 1. Caja exterior completa
        var shell = SolidFactory.Box(extW, extD, _p.Height,
            new Point3d(origin.X - wt, origin.Y - wt, origin.Z));

        // 2. Restar hueco interior — genera el espesor de muro
        var interior = SolidFactory.Box(_p.Width, _p.Depth, _p.Height, origin);
        shell = SolidFactory.Subtract(shell, interior);

        // 3. Restar vanos de puerta en fachada frontal (cara Y = origin.Y - wt)
        shell = SubtractDoorVoids(shell, origin, wt);

        shell.OnLayer(_layer).AddToModelSpace(tr, db);
    }

    private Solid3d SubtractDoorVoids(Solid3d shell, Point3d origin, double wt)
    {
        double spacing = _p.Width / (_p.DoorCount + 1);

        for (int i = 0; i < _p.DoorCount; i++)
        {
            double doorX = origin.X + spacing * (i + 1) - _p.DoorWidth / 2;

            var doorVoid = SolidFactory.Box(
                width:  _p.DoorWidth,
                depth:  wt + 0.01,          // +1 cm para asegurar corte limpio
                height: _p.DoorHeight,
                origin: new Point3d(doorX, origin.Y - wt, origin.Z));

            shell = SolidFactory.Subtract(shell, doorVoid);
        }

        return shell;
    }

    // ── CUBIERTA ─────────────────────────────────────────────────────────────

    private void BuildRoofSlab(Transaction tr, Database db, Point3d origin)
    {
        double wt = _p.WallThickness;

        // Cubierta con vuelo igual al espesor de muro en los cuatro lados
        var roof = SolidFactory
            .Box(
                width:  _p.Width  + 2 * wt,
                depth:  _p.Depth  + 2 * wt,
                height: _p.SlabThickness,
                origin: new Point3d(origin.X - wt, origin.Y - wt, origin.Z + _p.Height))
            .OnLayer(_layer);

        roof.AddToModelSpace(tr, db);
    }

    // ── REJILLAS DE VENTILACIÓN ───────────────────────────────────────────────

    private void BuildVentilationGrilles(Transaction tr, Database db, Point3d origin)
    {
        const double ventW = 0.40;
        const double ventH = 0.20;
        const double ventZOffset = 0.30;        // altura desde el piso interior
        const double ventZOffsetHigh = _p_Height() - 0.50; // cerca del techo

        BuildGrille(tr, db,
            x: origin.X - _p.WallThickness,
            y: origin.Y + (_p.Depth - ventW) / 2,
            z: origin.Z + ventZOffset,
            widthDim: _p.WallThickness,
            depthDim: ventW,
            heightDim: ventH);

        BuildGrille(tr, db,
            x: origin.X + _p.Width,
            y: origin.Y + (_p.Depth - ventW) / 2,
            z: origin.Z + ventZOffset,
            widthDim: _p.WallThickness,
            depthDim: ventW,
            heightDim: ventH);
    }

    private void BuildGrille(Transaction tr, Database db,
        double x, double y, double z,
        double widthDim, double depthDim, double heightDim)
    {
        var grille = SolidFactory
            .Box(widthDim, depthDim, heightDim, new Point3d(x, y, z))
            .OnLayer(_layer);

        grille.AddToModelSpace(tr, db);
    }

    // Acceso a _p.Height sin capturar en lambda (C# limitation workaround)
    private double _p_Height() => _p.Height;
}
