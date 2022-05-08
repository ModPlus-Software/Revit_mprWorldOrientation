namespace mprWorldOrientation.Services;

using Autodesk.Revit.DB;
using mprWorldOrientation.Models;

/// <summary>
/// Сервис по работе с ориентацией
/// </summary>
public class GeometryService
{
    /// <summary>
    /// Имеется ли пересечение между линией и солидом
    /// </summary>
    /// <param name="line">Линия</param>
    /// <param name="solid">Солид</param>
    public bool HasLineSolidIntersection(Line line, Solid solid)
    {
        if (line == null || solid == null)
            return false;

        var intersectionResult = solid.IntersectWithCurve(line, new SolidCurveIntersectionOptions()
        {
            ResultType = SolidCurveIntersectionMode.CurveSegmentsInside
        });

        if (intersectionResult.SegmentCount == 0)
            return false;

        var commonLength = 0d;
        foreach (var segment in intersectionResult)
        {
            commonLength += segment.Length;
        }

        // в случаях с дуговыми объектами, предполагаем что если пересечение и найдено,
        // но сумма длин сегментов меньше 70 процентов линии, то линия смотри наружу объекта и 
        return line.ApproximateLength * 0.7 < commonLength;
    }

    /// <summary>
    /// Определить сторону света по вектору
    /// </summary>
    /// <param name="vector">Вектор напраления отверстия наружу</param>
    /// <returns>Сторона света</returns>
    public string WorldDirectionByVector(XYZ vector)
    {
        if (vector.IsAlmostEqualTo(XYZ.BasisY, PluginSettings.Tolerance))
            return PluginSettings.North;
        if (vector.IsAlmostEqualTo(-XYZ.BasisY, PluginSettings.Tolerance))
            return PluginSettings.South;
        if (vector.IsAlmostEqualTo(XYZ.BasisX, PluginSettings.Tolerance))
            return PluginSettings.East;
        if (vector.IsAlmostEqualTo(-XYZ.BasisX, PluginSettings.Tolerance))
            return PluginSettings.West;
        if (vector.X < 0)
        {
            if (vector.Y > 0)
                return PluginSettings.NorthWest;
            if (vector.Y < 0)
                return PluginSettings.SouthWest;
        }
        else
        {
            if (vector.Y > 0)
                return PluginSettings.NorthEast;
            if (vector.Y < 0)
                return PluginSettings.SouthEast;
        }

        return string.Empty;
    }
}