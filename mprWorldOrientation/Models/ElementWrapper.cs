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
    public ElementWrapper(Element element)
    {
        RevitElement = element;
        _lines = new Lazy<List<Line>>(() => GetRayVectors(element));
    }

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
        Line firstLine = null;
        Line secondLine = null;
        switch (element)
        {
            case Wall glassWall:
            {
                var locationLine = ((LocationCurve)glassWall.Location).Curve;
                var locationDir = (locationLine.GetEndPoint(1) - locationLine.GetEndPoint(0)).Normalize();
                var centralPoint = locationLine.GetEndPoint(0) + (locationDir * locationLine.ApproximateLength / 2);
                var centralPointUp = centralPoint + (XYZ.BasisZ * 500.MmToFt());
                var firstDir = locationDir.CrossProduct(XYZ.BasisZ);
                var secondDir = -firstDir;
                firstLine = Line.CreateBound(centralPointUp, centralPointUp + (firstDir * _rayLength));
                secondLine = Line.CreateBound(centralPointUp, centralPointUp + (secondDir * _rayLength));
                break;
            }

            case FamilyInstance familyInstance:
            {
                var locationPoint = ((LocationPoint)familyInstance.Location).Point;
                var fistDirection = familyInstance.FacingOrientation;
                var upLocPoint = locationPoint + (XYZ.BasisZ * 500.MmToFt());
                firstLine = Line.CreateBound(upLocPoint, upLocPoint + (fistDirection * _rayLength));
                secondLine = Line.CreateBound(upLocPoint, upLocPoint - (fistDirection * _rayLength));
                break;
            }
        }

        return new List<Line> { firstLine, secondLine };
    }
}