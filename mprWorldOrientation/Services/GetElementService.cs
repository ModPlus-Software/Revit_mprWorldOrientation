namespace mprWorldOrientation.Services;

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Architecture;
using ModPlus_Revit;
using Models;

/// <summary>
/// Сервис по получению элементов
/// </summary>
public class GetElementService
{
    private readonly Document _doc;
    private readonly GeometryService _geometryService;

    /// <summary>
    /// ctor
    /// </summary>
    public GetElementService()
    {
        _doc = ModPlus.UiApplication.ActiveUIDocument.Document;
        _geometryService = new GeometryService();
    }

    /// <summary>
    /// Получает обертки над помещениями
    /// </summary>
    /// <param name="settings">Данные с настройками</param>
    /// <returns>Список элементов</returns>
    public List<RoomWrapper> GetRoomWrapper(SettingsData settings)
    {
        var multiCategoryFilter = new ElementMulticategoryFilter(
            settings.ElementApplyFilterForRooms.Filter.Categories.Select(i => i.BuiltInCategory)
                .ToList());
        var roomWrappers = new FilteredElementCollector(_doc).WhereElementIsNotElementType()
            .WherePasses(multiCategoryFilter)
            .Where(i => !settings.ElementApplyFilterForRooms.Filter.EqualityParameters.Any() || 
                        settings.ElementApplyFilterForRooms.Filter.IsMatch(i))
            .OfType<Room>()
            .Select(i => new RoomWrapper(i))
            .ToList();
        roomWrappers.ForEach(i => i.DependentElements = GetRoomDependentElements(i, settings));
        return roomWrappers;
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
            ? elements.Where(i => settings.ElementApplyFilterForDoors.Filter.Categories
                    .Select(c => c.BuiltInCategory).Contains((BuiltInCategory)i.Category.Id.IntegerValue))
                .Where(i => !settings.ElementApplyFilterForDoors.Filter.EqualityParameters.Any() || 
                            settings.ElementApplyFilterForDoors.Filter.IsMatch(i)) 
            : new List<Element>();

        var windows = settings.ElementApplyFilterForDoors.IsEnabled 
            ? elements
                .Where(i => settings.ElementApplyFilterForWindows.Filter.Categories
                    .Select(c => c.BuiltInCategory).Contains((BuiltInCategory)i.Category.Id.IntegerValue))
                .Where(i => !settings.ElementApplyFilterForWindows.Filter.EqualityParameters.Any() ||
                            settings.ElementApplyFilterForWindows.Filter.IsMatch(i)) 
            : new List<Element>();

        return doors.Concat(glassWindow).Concat(windows).Distinct().Select(i => new ElementWrapper(i))
            .Where(i => i.Vectors.Any(l => _geometryService.HasLineSolidIntersection(l, roomWrapper.Solid)))
            .ToList();
    }
}