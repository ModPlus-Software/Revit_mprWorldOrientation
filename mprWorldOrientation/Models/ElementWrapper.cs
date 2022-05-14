namespace mprWorldOrientation.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using ModPlus_Revit.Utils;

/// <summary>
/// Обертка над элементом
/// </summary>
public class ElementWrapper
{
    private readonly double _rayLength = 1000d.MmToFt();
    private readonly Lazy<List<Line>> _lines;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="element">Элемент</param>
    /// <param name="revitLinkInstance">Связанный файл</param>
    public ElementWrapper(Element element, RevitLinkInstance revitLinkInstance)
    {
        RevitElement = element;
        _lines = new Lazy<List<Line>>(() => GetRayVectors(element));
        Doc = revitLinkInstance?.GetLinkDocument() ?? element.Document;
        RevitLink = revitLinkInstance;
    }

    /// <summary>
    /// Связанный файл
    /// </summary>
    public RevitLinkInstance RevitLink { get; }

    /// <summary>
    /// Документ которому принадлежит элемент
    /// </summary>
    public Document Doc { get; }

    /// <summary>
    /// Элемент в представлении Ревит
    /// </summary>
    public Element RevitElement { get; }

    /// <summary>
    /// Первый вектор
    /// </summary>
    public Line FirstVector => _lines.Value.First();

    /// <summary>
    /// Второй вектор
    /// </summary>
    public Line SecondVector => _lines.Value.Last();

    /// <summary>
    /// Все векторы для пересечения
    /// </summary>
    public List<Line> Vectors => _lines.Value;

    /// <summary>
    /// Имеет ли первый вектор пересечение
    /// </summary>
    public bool HasFirstVectorIntersectedRoom { get; set; }

    /// <summary>
    /// Имеет ли второй вектор пересечение
    /// </summary>
    public bool HasSecondVectorIntersectionRoom { get; set; }

    /// <summary>
    /// Является ли элемент не контурным
    /// </summary>
    public bool IsContourElement => HasFirstVectorIntersectedRoom != HasSecondVectorIntersectionRoom;

    private List<Line> GetRayVectors(Element element)
    {
        Line firstLine;
        Line secondLine;
        var transform = RevitLink == null ? Transform.Identity : RevitLink.GetTotalTransform();
        switch (element)
        {
            case Wall glassWall:
            {
                var locationLine = ((LocationCurve)glassWall.Location).Curve;
                var locationDir = (locationLine.GetEndPoint(1) - locationLine.GetEndPoint(0)).Normalize();
                var centralPoint = locationLine.Evaluate(0.5, true);
                var centralPointUp = centralPoint + (XYZ.BasisZ * 1000.MmToFt());
                var centralPointUpTr = transform.OfPoint(centralPointUp);
                var firstDir = locationDir.CrossProduct(XYZ.BasisZ);
                var firstDirTr = transform.OfVector(firstDir).Normalize();
                firstLine = Line.CreateBound(centralPointUpTr, centralPointUpTr + (firstDirTr * _rayLength));
                secondLine = Line.CreateBound(centralPointUpTr, centralPointUpTr + (-firstDirTr * _rayLength));
                break;
            }

            case FamilyInstance familyInstance:
            {
                var locationPoint = ((LocationPoint)familyInstance.Location).Point;
                var fistDirection = familyInstance.FacingOrientation;
                var firstDirTr = transform.OfVector(fistDirection).Normalize();
                var upLocPoint = locationPoint + (XYZ.BasisZ * 1000.MmToFt());
                var upLocPointTr = transform.OfPoint(upLocPoint);
                firstLine = Line.CreateBound(upLocPointTr, upLocPointTr + (firstDirTr * _rayLength));
                secondLine = Line.CreateBound(upLocPointTr, upLocPointTr - (firstDirTr * _rayLength));
                break;
            }

            default:
                throw new ArgumentOutOfRangeException($"Not supported type {element.GetType()}");
        }

        return new List<Line> { firstLine, secondLine };
    }
}