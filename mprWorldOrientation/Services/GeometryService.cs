namespace mprWorldOrientation.Services;

using Autodesk.Revit.DB;
using Models;
using ModPlus_Revit;
using ModPlus_Revit.Utils;
using ModPlusAPI;
using ModPlusAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Сервис по работе с ориентацией
/// </summary>
public class GeometryService
{
    private readonly Lazy<Transform> _transform;
    private Lazy<Dictionary<string, (XYZ, XYZ)>> _diapazones;
    private ResultService _resultService;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="resultService">Логер</param>
    public GeometryService(ResultService resultService)
    {
        _resultService = resultService;
        var doc = ModPlus.UiApplication.ActiveUIDocument.Document;
        _transform = new Lazy<Transform>(() => GetTransform(doc));
        _diapazones = new Lazy<Dictionary<string, (XYZ, XYZ)>>(() => GetDiapasones());
    }

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
    /// <param name="vector">Вектор направления отверстия наружу</param>
    /// <param name="element">Обертка над элементом</param>
    /// <returns>Сторона света</returns>
    public string WorldDirectionByVector(XYZ vector, ElementWrapper element)
    {
        var basic = _diapazones;
        vector = _transform.Value.OfVector(vector);
        foreach (var diapasone in _diapazones.Value)
        {
            var conectionLine = Line.CreateBound(diapasone.Value.Item1, diapasone.Value.Item2);
            var vectorLine = Line.CreateBound(XYZ.Zero, vector);
            var intersection = vectorLine.Intersect(conectionLine);
            if (intersection == SetComparisonResult.Overlap)
                return diapasone.Key;
        }

        _resultService.Add(
            string.Format(Language.GetItem("t5"), element.RevitElement.Id.IntegerValue),
            ModPlusAPI.Enums.ResultItemType.Error);

        return string.Empty;
    }

    private XYZ Rotate(XYZ vector, double angle)
    {
        var x = (vector.X * Math.Cos(angle)) - (vector.Y * Math.Sin(angle));
        var y = (vector.X * Math.Sin(angle)) + (vector.Y * Math.Cos(angle));
        return new XYZ(x, y, 0).Normalize();
    }

    private Dictionary<string, (XYZ, XYZ)> GetDiapasones()
    {
        var result = new Dictionary<string, (XYZ, XYZ)>();
        var basicVector = XYZ.BasisY;
        var startAngle = 45 / 2;
        var lastVector = Rotate(basicVector, -startAngle.DegreeToRadian());
        foreach (var dir in PluginSettings.DirectionOrderList)
        {
            var localLastVector = Rotate(lastVector, -45.DegreeToRadian());
            result.Add(dir, (lastVector, localLastVector));
            lastVector = localLastVector;
        }

        return result;
    }

    private Transform GetTransform(Document doc)
    {
        var basePoint = new FilteredElementCollector(doc)
            .OfCategory(BuiltInCategory.OST_ProjectBasePoint)
            .Cast<BasePoint>()
            .FirstOrDefault();
        if (basePoint == null)
            return Transform.Identity;
        var angle = basePoint.get_Parameter(BuiltInParameter.BASEPOINT_ANGLETON_PARAM).AsDouble();
        return Transform.CreateRotation(XYZ.BasisZ, -angle);
    }
}