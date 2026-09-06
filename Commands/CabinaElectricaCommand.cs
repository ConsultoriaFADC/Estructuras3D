using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using CabinaElectrica3D.UI;

// Declarar el ensamblado como plugin AutoCAD
[assembly: ExtensionApplication(typeof(CabinaElectrica3D.Commands.CabinaElectricaApp))]
[assembly: CommandClass(typeof(CabinaElectrica3D.Commands.CabinaElectricaCommand))]

namespace CabinaElectrica3D.Commands;

// ═══════════════════════════════════════════════════════════════════════════════
//  EXTENSION APPLICATION — ciclo de vida del plugin
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Punto de entrada del plugin. AutoCAD llama a Initialize() al cargar
/// la DLL y a Terminate() al cerrar el documento.
/// </summary>
public sealed class CabinaElectricaApp : IExtensionApplication
{
    private static PaletteSet? _paletteSet;

    public void Initialize()
    {
        // Nada en Initialize — la paleta se crea al ejecutar el comando
        // para no consumir recursos si el usuario no la abre.
    }

    public void Terminate()
    {
        _paletteSet?.Dispose();
        _paletteSet = null;
    }

    // ── ACCESO GLOBAL A LA PALETA ─────────────────────────────────────────────

    internal static PaletteSet GetOrCreatePaletteSet()
    {
        if (_paletteSet is not null)
            return _paletteSet;

        _paletteSet = new PaletteSet("Cabina Eléctrica 3D")
        {
            MinimumSize = new System.Drawing.Size(260, 500),
            Style       = PaletteSetStyles.ShowAutoHideButton
                        | PaletteSetStyles.ShowCloseButton
                        | PaletteSetStyles.Snappable
        };

        // Agregar el UserControl WPF como paleta
        _paletteSet.AddVisual("Configuración", new CabinaPalette());

        return _paletteSet;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  COMANDOS AUTOCAD
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Comandos disponibles en AutoCAD.
/// <list type="bullet">
///   <item><c>CABINA</c> — abre/muestra la paleta de configuración</item>
/// </list>
/// </summary>
public sealed class CabinaElectricaCommand
{
    /// <summary>
    /// Abre la paleta de configuración de la cabina eléctrica 3D.
    /// Escribir <c>CABINA</c> en la línea de comandos de AutoCAD.
    /// </summary>
    [CommandMethod("CABINA", CommandFlags.Modal)]
    public void OpenCabinaPalette()
    {
        var palette = CabinaElectricaApp.GetOrCreatePaletteSet();

        if (!palette.Visible)
            palette.Visible = true;

        palette.Activate(0);
    }
}
