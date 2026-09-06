using System.Windows.Controls;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using CabinaElectrica3D.Core.Builders;
using CabinaElectrica3D.Core.Services;
using CabinaElectrica3D.UI.ViewModels;

namespace CabinaElectrica3D.UI;

/// <summary>
/// Code-behind de la paleta. Responsabilidad única: conectar el botón
/// de la UI con el orquestador de dominio. No contiene lógica de negocio.
/// </summary>
public partial class CabinaPalette : UserControl
{
    private readonly CabinaViewModel _viewModel;

    public CabinaPalette()
    {
        InitializeComponent();
        _viewModel = new CabinaViewModel();
        DataContext = _viewModel;
    }

    private void BtnGenerate_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var doc = Application.DocumentManager.MdiActiveDocument;
        if (doc is null) return;

        var editor = doc.Editor;

        // Pedir punto de inserción al usuario en AutoCAD
        var ppr = editor.GetPoint(new PromptPointOptions(
            "\nPunto de inserción de la cabina: "));

        if (ppr.Status != PromptStatus.OK) return;

        var insertionPoint = ppr.Value;

        // Generar en la base de datos activa
        using var docLock = doc.LockDocument();
        var db         = doc.Database;
        var layerSvc   = new LayerService(db);
        var orchestrator = new CabinaOrchestrator(layerSvc);
        var parameters = _viewModel.ToParameters(insertionPoint);

        try
        {
            orchestrator.Generate(db, parameters);
            editor.WriteMessage("\n✔ Cabina generada correctamente.");
        }
        catch (Exception ex)
        {
            editor.WriteMessage($"\n✘ Error al generar la cabina: {ex.Message}");
        }
    }
}
