using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace CabinaElectrica3D.Core.Services;

// ═══════════════════════════════════════════════════════════════════════════════
//  SOLID FACTORY
//  Fábrica estática de sólidos 3D y extensiones de conveniencia.
//  Toda creación de primitivas pasa por aquí — nunca se llama directamente
//  a CreateBox / CreateFrustum desde los builders.
// ═══════════════════════════════════════════════════════════════════════════════

/// <summary>
/// Crea y transforma sólidos 3D de AutoCAD.
/// Todos los métodos devuelven el sólido posicionado y listo para agregar
/// al espacio de modelo; el llamador asume la propiedad del objeto.
/// </summary>
public static class SolidFactory
{
    // ── PRIMITIVAS ────────────────────────────────────────────────────────────

    /// <summary>
    /// Crea un prisma rectangular posicionado por su esquina
    /// inferior-izquierda-frontal (corner mínimo XYZ).
    /// </summary>
    /// <param name="width">Dimensión en X (m).</param>
    /// <param name="depth">Dimensión en Y (m).</param>
    /// <param name="height">Dimensión en Z (m).</param>
    /// <param name="origin">Corner mínimo XYZ. Por defecto Point3d.Origin.</param>
    public static Solid3d Box(
        double  width,
        double  depth,
        double  height,
        Point3d origin = default)
    {
        Guard.Positive(width,  nameof(width));
        Guard.Positive(depth,  nameof(depth));
        Guard.Positive(height, nameof(height));

        var solid = new Solid3d();
        solid.CreateBox(width, depth, height);

        // CreateBox centra en el origen — trasladamos al corner inferior.
        solid.TransformBy(Matrix3d.Displacement(
            origin.GetAsVector() + new Vector3d(width / 2, depth / 2, height / 2)));

        return solid;
    }

    /// <summary>
    /// Crea un cilindro vertical con base en <paramref name="baseCenter"/>.
    /// </summary>
    /// <param name="radius">Radio en metros.</param>
    /// <param name="height">Altura en metros.</param>
    /// <param name="baseCenter">Centro de la cara inferior.</param>
    public static Solid3d Cylinder(
        double  radius,
        double  height,
        Point3d baseCenter = default)
    {
        Guard.Positive(radius, nameof(radius));
        Guard.Positive(height, nameof(height));

        var solid = new Solid3d();
        solid.CreateFrustum(height, radius, radius, radius);

        // CreateFrustum centra en el origen — trasladamos para que la base
        // quede en baseCenter.
        solid.TransformBy(Matrix3d.Displacement(
            baseCenter.GetAsVector() + new Vector3d(0, 0, height / 2)));

        return solid;
    }

    /// <summary>
    /// Crea un cilindro horizontal orientado a lo largo del eje Y,
    /// cuyo extremo inicial (Y mínimo) queda en <paramref name="startPoint"/>.
    /// </summary>
    public static Solid3d CylinderAlongY(
        double  radius,
        double  length,
        Point3d startPoint)
    {
        Guard.Positive(radius, nameof(radius));
        Guard.Positive(length, nameof(length));

        // Creamos vertical y luego rotamos 90° alrededor de X.
        var solid = Cylinder(radius, length, startPoint);

        solid.TransformBy(Matrix3d.Rotation(
            angle:  Math.PI / 2,
            axis:   Vector3d.XAxis,
            center: startPoint));

        // Ajustar desplazamiento Y tras la rotación
        solid.TransformBy(Matrix3d.Displacement(new Vector3d(0, length / 2, -radius)));

        return solid;
    }

    // ── OPERACIONES BOOLEANAS ─────────────────────────────────────────────────

    /// <summary>
    /// Sustrae <paramref name="tool"/> de <paramref name="target"/>.
    /// La herramienta se descarta al finalizar.
    /// Devuelve <paramref name="target"/> para encadenamiento fluido.
    /// </summary>
    public static Solid3d Subtract(Solid3d target, Solid3d tool)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(tool);

        target.BooleanOperation(BooleanOperationType.BoolSubtract, tool);
        tool.Dispose();
        return target;
    }

    /// <summary>
    /// Une <paramref name="tool"/> a <paramref name="target"/>.
    /// La herramienta se descarta al finalizar.
    /// Devuelve <paramref name="target"/> para encadenamiento fluido.
    /// </summary>
    public static Solid3d Unite(Solid3d target, Solid3d tool)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(tool);

        target.BooleanOperation(BooleanOperationType.BoolUnite, tool);
        tool.Dispose();
        return target;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  EXTENSIONES DE ENTIDAD — métodos fluidos para configurar y persistir entidades
// ═══════════════════════════════════════════════════════════════════════════════

public static class EntityExtensions
{
    /// <summary>
    /// Asigna una capa a la entidad y establece color ByLayer.
    /// Retorna la misma entidad para encadenamiento fluido.
    /// </summary>
    public static T OnLayer<T>(this T entity, string layerName) where T : Entity
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(layerName);

        entity.Layer = layerName;
        entity.Color = Color.FromColorIndex(ColorMethod.ByLayer, 256);
        return entity;
    }

    /// <summary>
    /// Agrega la entidad al espacio de modelo y la registra en la transacción.
    /// </summary>
    /// <returns>ObjectId asignado por AutoCAD.</returns>
    public static ObjectId AddToModelSpace(this Entity entity, Transaction tr, Database db)
    {
        ArgumentNullException.ThrowIfNull(entity);
        ArgumentNullException.ThrowIfNull(tr);
        ArgumentNullException.ThrowIfNull(db);

        var ms = (BlockTableRecord)tr.GetObject(
            SymbolUtilityServices.GetBlockModelSpaceId(db),
            OpenMode.ForWrite);

        var id = ms.AppendEntity(entity);
        tr.AddNewlyCreatedDBObject(entity, true);
        return id;
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
//  GUARD — validaciones defensivas reutilizables
// ═══════════════════════════════════════════════════════════════════════════════

internal static class Guard
{
    /// <summary>Lanza <see cref="ArgumentOutOfRangeException"/> si value ≤ 0.</summary>
    public static void Positive(double value, string paramName)
    {
        if (value <= 0)
            throw new ArgumentOutOfRangeException(paramName,
                $"{paramName} debe ser mayor que cero. Valor recibido: {value}");
    }
}
