using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using CabinaElectrica3D.Core.Builders.Interfaces;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.Models;

namespace CabinaElectrica3D.Core.Builders.Elements;

// ═══════════════════════════════════════════════════════════════════════════════
//  INSTALLATIONS BUILDER
//  Gestiona bandejas portacables y conductor de puesta a tierra perimetral.
//  Ambos recorren el perímetro interior de la caseta.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Construye instalaciones auxiliares: bandejas portacables y red de tierra.
/// </summary>
public sealed class InstallationsBuilder : IElementBuilder
{
    private readonly InstallationsParameters _p;
    private readonly CasetaParameters        _caseta;

    public InstallationsBuilder(InstallationsParameters parameters, CasetaParameters caseta)
    {
        _p      = parameters ?? throw new ArgumentNullException(nameof(parameters));
        _caseta = caseta     ?? throw new ArgumentNullException(nameof(caseta));
    }

    public void Build(Transaction tr, Database db, Point3d origin)
    {
        if (_p.IncludeCableTrays)
            BuildCableTrays(tr, db, origin);

        if (_p.IncludeEarthingRing)
            BuildEarthingConductor(tr, db, origin);
    }

    // ── BANDEJAS PORTACABLES ─────────────────────────────────────────────────

    private void BuildCableTrays(Transaction tr, Database db, Point3d origin)
    {
        const double trayHeight = 0.04;     // altura de perfil de bandeja
        string trayLayer = LayerService.Layers.Bandejas.Name;

        double z    = origin.Z + _p.TrayHeight;
        double w    = _p.TrayWidth;
        double lenX = _caseta.Width;
        double lenY = _caseta.Depth;

        // Tramo frontal (Y = origin.Y, corriendo en X)
        SolidFactory
            .Box(lenX, w, trayHeight, new Point3d(origin.X, origin.Y, z))
            .OnLayer(trayLayer)
            .AddToModelSpace(tr, db);

        // Tramo trasero
        SolidFactory
            .Box(lenX, w, trayHeight,
                 new Point3d(origin.X, origin.Y + lenY - w, z))
            .OnLayer(trayLayer)
            .AddToModelSpace(tr, db);

        // Tramo lateral izquierdo (corriendo en Y)
        SolidFactory
            .Box(w, lenY, trayHeight, new Point3d(origin.X, origin.Y, z))
            .OnLayer(trayLayer)
            .AddToModelSpace(tr, db);

        // Tramo lateral derecho
        SolidFactory
            .Box(w, lenY, trayHeight,
                 new Point3d(origin.X + lenX - w, origin.Y, z))
            .OnLayer(trayLayer)
            .AddToModelSpace(tr, db);
    }

    // ── CONDUCTOR DE PUESTA A TIERRA ─────────────────────────────────────────

    private void BuildEarthingConductor(Transaction tr, Database db, Point3d origin)
    {
        // Conductor perimetral — sección 50 mm² representada como sólido rectangular
        const double conductorW = 0.008;    // 8 mm de ancho (referencia visual)
        const double conductorH = 0.004;    // 4 mm de alto
        string earthLayer = LayerService.Layers.Tierra.Name;

        double z      = origin.Z + _p.EarthingHeight;
        double offset = 0.10;               // separación desde la pared

        double innerX = origin.X + offset;
        double innerY = origin.Y + offset;
        double lenX   = _caseta.Width  - offset * 2;
        double lenY   = _caseta.Depth  - offset * 2;

        // Los cuatro tramos perimetrales
        SolidFactory
            .Box(lenX, conductorW, conductorH,
                 new Point3d(innerX, innerY, z))
            .OnLayer(earthLayer)
            .AddToModelSpace(tr, db);

        SolidFactory
            .Box(lenX, conductorW, conductorH,
                 new Point3d(innerX, innerY + lenY, z))
            .OnLayer(earthLayer)
            .AddToModelSpace(tr, db);

        SolidFactory
            .Box(conductorW, lenY, conductorH,
                 new Point3d(innerX, innerY, z))
            .OnLayer(earthLayer)
            .AddToModelSpace(tr, db);

        SolidFactory
            .Box(conductorW, lenY, conductorH,
                 new Point3d(innerX + lenX, innerY, z))
            .OnLayer(earthLayer)
            .AddToModelSpace(tr, db);
    }
}
