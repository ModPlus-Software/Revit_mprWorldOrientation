namespace mprWorldOrientation.Services;

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using ModPlus_Revit;
using Models;
using Enums;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

/// <summary>
/// Сервис по получению элементов
/// </summary>
public class GetElementService
{
    private readonly Document _doc;
    private readonly UIDocument _uiDoc;
    private readonly GeometryService _geometryService;

    /// <summary>
    /// ctor
    /// </summary>
    public GetElementService()
    {
        _doc = ModPlus.UiApplication.ActiveUIDocument.Document;
        _uiDoc = ModPlus.UiApplication.ActiveUIDocument;
        _geometryService = new GeometryService();
    }

    /// <summary>
    /// Получает обертки над помещениями
    /// </summary>
    /// <param name="settings">Данные с настройками</param>
    /// <param name="scopeType">Параметры для выбора элементов</param>
    /// <returns>Список элементов</returns>
    public List<RoomWrapper> GetRoomWrapper(SettingsData settings, ScopeType scopeType)
    {
        var roomWrappers = GetScopedCollector(scopeType).WhereElementIsNotElementType()
            .Where(i => settings.ElementApplyFilterForRooms.Filter.EqualityParameters.Any() ?
                        settings.ElementApplyFilterForRooms.Filter.IsMatch(i) : true)
            .OfType<Room>()
            .Select(i => new RoomWrapper(i))
            .ToList();
        roomWrappers.ForEach(i => i.DependentElements = GetRoomDependentElements(i, settings));
        return roomWrappers;
    }

    /// <summary>
    /// Получает все помещения в модели
    /// </summary>
    /// <returns>Список помещений</returns>
    public List<RoomWrapper> GetAllRooms()
    {
        return new FilteredElementCollector(_doc)
            .WhereElementIsNotElementType()
            .OfClass(typeof(SpatialElement))
            .OfType<Room>()
            .Select(i => new RoomWrapper(i))
            .ToList();
    }

    private List<ElementWrapper> GetRoomDependentElements(RoomWrapper roomWrapper, SettingsData settings)
    {
        var allWalls = roomWrapper.RevitElement.GetBoundarySegments(
                new SpatialElementBoundaryOptions
                {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center
                })
            .SelectMany(i => i.Select(w => _doc.GetElement(w.ElementId))).OfType<Wall>().ToList();

        var glassWindow = settings.ElementApplyFilterForGlassWalls.IsEnabled 
            ? allWalls.Where(i => i.CurtainGrid != null)
                .Where(i => !settings.ElementApplyFilterForGlassWalls.Filter.EqualityParameters.Any() || 
                            settings.ElementApplyFilterForGlassWalls.Filter.IsMatch(i)) 
            : new List<Wall>();

        var elements = allWalls.Where(i => i.CurtainGrid == null)
            .SelectMany(i => i.GetDependentElements(null))
            .Select(id => _doc.GetElement(id)).ToList();
        
        var doors = settings.ElementApplyFilterForDoors.IsEnabled 
            ? elements.Where(i => (BuiltInCategory)i.Category.Id.IntegerValue == settings.ElementApplyFilterForDoors.Category.BuiltInCategory)
                .Where(i => !settings.ElementApplyFilterForDoors.Filter.EqualityParameters.Any() || 
                            settings.ElementApplyFilterForDoors.Filter.IsMatch(i)) 
            : new List<Element>();

        var windows = settings.ElementApplyFilterForWindows.IsEnabled 
            ? elements
                .Where(i => (BuiltInCategory)i.Category.Id.IntegerValue == settings.ElementApplyFilterForWindows.Category.BuiltInCategory)
                .Where(i => !settings.ElementApplyFilterForWindows.Filter.EqualityParameters.Any() ||
                            settings.ElementApplyFilterForWindows.Filter.IsMatch(i)) 
            : new List<Element>();

        return doors.Concat(glassWindow).Concat(windows).Distinct().Select(i => new ElementWrapper(i))
            .Where(i => i.Vectors.Any(l => _geometryService.HasLineSolidIntersection(l, roomWrapper.Solid)))
            .ToList();
    }

    private FilteredElementCollector GetScopedCollector(ScopeType scope)
    {
        switch (scope)
        {
            case ScopeType.InProject:
                return new FilteredElementCollector(_doc);

            case ScopeType.InActiveView:
                return new FilteredElementCollector(_doc, _doc.ActiveView.Id);

            case ScopeType.SelectedElement:
                var selectedElementIds = _uiDoc.Selection.GetElementIds();

                if (!selectedElementIds.Any())
                {
                    selectedElementIds = _uiDoc.Selection
                    .PickObjects(ObjectType.Element)
                    .Select(e => _doc.GetElement(e).Id)
                    .ToList();
                }
                
                return selectedElementIds.Any()
                    ? new FilteredElementCollector(_doc, selectedElementIds)
                    : new FilteredElementCollector(_doc, new List<ElementId> { ElementId.InvalidElementId });
        }

        return new FilteredElementCollector(_doc);
    }
}