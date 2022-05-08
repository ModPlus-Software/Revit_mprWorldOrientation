namespace mprWorldOrientation.Models;

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;

/// <summary>
/// Обертка над помещением
/// </summary>
public class RoomWrapper
{
    private readonly Lazy<Solid> _solid;

    /// <summary>
    ///  Обертка над помещением
    /// </summary>
    /// <param name="room">Помещение</param>
    public RoomWrapper(Room room)
    {
        RevitElement = room;
        _solid = new Lazy<Solid>(() => GetSolid(room));
    }

    /// <summary>
    /// Элемент в представлении Ревит
    /// </summary>
    public Room RevitElement { get; }

    /// <summary>
    /// Солид
    /// </summary>
    public Solid Solid => _solid.Value;

    /// <summary>
    /// Явялется ли комната контурной
    /// </summary>
    public bool IsCounturRoom => DependentElements.Any(el => el.IsContourElement);

    /// <summary>
    /// Список зависимых элементов
    /// </summary>
    public List<ElementWrapper> DependentElements { get; set; } = new ();

    private Solid GetSolid(Room room)
    {
        var solids = new List<Solid>();
        var opt = new Options
        {
            DetailLevel = ViewDetailLevel.Fine
        };

        var geometry = room
            .get_Geometry(opt)
            .GetTransformed(Transform.Identity);

        foreach (var geometryElement in geometry)
        {
            if (geometryElement is Solid { Volume: > 0 } solid)
            {
                solids.Add(solid);
            }
        }

        if (!solids.Any())
            return null;

        return solids.OrderBy(i => i.Volume).Last();
    }
}