using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace CabinaElectrica3D.Core.Builders.Interfaces;

/// <summary>
/// Contrato para todos los constructores de elementos de la cabina.
/// Cada implementación es responsable de un único elemento cohesivo
/// (caseta, transformador, celdas, etc.) y no conoce a los demás.
/// </summary>
public interface IElementBuilder
{
    /// <summary>
    /// Genera las entidades 3D del elemento y las persiste en el espacio de modelo.
    /// </summary>
    /// <param name="tr">Transacción AutoCAD activa. No se abre ni se confirma aquí.</param>
    /// <param name="db">Base de datos de destino.</param>
    /// <param name="origin">Punto de inserción del conjunto completo de la cabina.</param>
    void Build(Transaction tr, Database db, Point3d origin);
}
