using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  CELDA MT BUILDER
//  Genera N módulos idénticos de celdas de media tensión alineados
//  sobre la pared izquierda de la caseta.
//  Cada módulo = cuerpo metálico + panel frontal con manija.
//  Opcional: barras de embarrado trifásico en la parte trasera.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye el conjunto de celdas de media tensión.
/// </summary>
public sealed class CeldaMTBuilder : IElementBuilder
{
    private readonly CeldasMTParameters _p;
    private readonly string             _layer;

    public CeldaMTBuilder(CeldasMTParameters parameters)
    {
        _p     = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _layer = LayerService.Layers.CeldasMT.Name;
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        for (int i = 0; i < _p.ModuleCount; i++)
            BuildModule(tr, db, origin, i);

        if (_p.IncludeBusbars && _p.ModuleCount > 1)
            BuildBusbars(tr, db, origin);
    }

    // ── MÓDULO INDIVIDUAL ─────────────────────────────────────────────────────

    private void BuildModule(Transaction tr, Database db, Point3d cabinaOrigin, int index)
    {
        double x = cabinaOrigin.X + _p.OffsetFromLeftWall + index * _p.ModuleWidth;
        double y = cabinaOrigin.Y;
        double z = cabinaOrigin.Z;

        var moduleOrigin = new Point3d(x, y, z);

        BuildModuleBody(tr, db, moduleOrigin);
        BuildModuleFrontPanel(tr, db, moduleOrigin);
        BuildModuleHandle(tr, db, moduleOrigin);
    }

    private void BuildModuleBody(Transaction tr, Database db, Point3d origin)
    {
        // Cuerpo principal con 2 mm de separación lateral para distinguir módulos
        const double sideClearance = 0.01;

        SolidFactory
            .Box(
                width:  _p.ModuleWidth - sideClearance * 2,
                depth:  _p.ModuleDepth,
                height: _p.ModuleHeight,
                origin: new Point3d(origin.X + sideClearance, origin.Y, origin.Z))
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);
    }

    private void BuildModuleFrontPanel(Transaction tr, Database db, Point3d origin)
    {
        // Panel frontal sobresaliente — ventanas de inspección y placa de maniobra
        const double panelDepth  = 0.02;
        const double panelInsetH = 0.40;    // altura del panel central desde el suelo
        const double panelHeight = 1.40;

        SolidFactory
            .Box(
                width:  _p.ModuleWidth * 0.85,
                depth:  panelDepth,
                height: panelHeight,
                origin: new Point3d(
                    origin.X + _p.ModuleWidth * 0.075,
                    origin.Y - panelDepth,
                    origin.Z + panelInsetH))
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);
    }

    private void BuildModuleHandle(Transaction tr, Database db, Point3d origin)
    {
        const double handleW = 0.08;
        const double handleH = 0.06;
        const double handleD = 0.03;

        SolidFactory
            .Box(
                width:  handleW,
                depth:  handleD,
                height: handleH,
                origin: new Point3d(
                    origin.X + (_p.ModuleWidth - handleW) / 2,
                    origin.Y - handleD,
                    origin.Z + _p.ModuleHeight * 0.50))
            .OnLayer(_layer)
            .AddToModelSpace(tr, db);
    }

    // ── BARRAS DE EMBARRADO ───────────────────────────────────────────────────

    private void BuildBusbars(Transaction tr, Database db, Point3d cabinaOrigin)
    {
        // Tres barras trifásicas horizontales corriendo por la parte trasera del conjunto
        ReadOnlySpan<double> phaseYOffsets = [0.10, 0.20, 0.30];

        double busX      = cabinaOrigin.X + _p.OffsetFromLeftWall;
        double totalW    = _p.ModuleCount * _p.ModuleWidth;
        double backY     = cabinaOrigin.Y + _p.ModuleDepth;
        double busZ      = cabinaOrigin.Z + _p.ModuleHeight - 0.15;

        const double busSection = 0.04;     // sección cuadrada 40x40 mm

        foreach (double yOff in phaseYOffsets)
        {
            SolidFactory
                .Box(
                    width:  totalW,
                    depth:  busSection,
                    height: busSection,
                    origin: new Point3d(busX, backY - yOff - busSection, busZ))
                .OnLayer(_layer)
                .AddToModelSpace(tr, db);
        }
    }
}
