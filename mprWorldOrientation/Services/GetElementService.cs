namespace mprWorldOrientation.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using Autodesk.Revit.DB.Architecture;
    using ModPlus_Revit;
    using ModPlus_Revit.Utils;
    using mprWorldOrientation.Models;

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
        /// <param name="roomFilter">Фильтр</param>
        /// <param name="elementsFilter">Фильтры для элементов</param>
        /// <returns>Список элементов</returns>
        public List<RoomWrapper> GetRoomWrapper(ElementApplyFilter roomFilter, ElementApplyFilter elementsFilter)
        {
            var multiCategoryFilter = new ElementMulticategoryFilter(
                    roomFilter.Categories.Select(i => i.BuiltInCategory)
                    .ToList());
            var roomWrappers = new FilteredElementCollector(_doc).WhereElementIsNotElementType()
                .WherePasses(multiCategoryFilter)
                .Where(i => roomFilter.EqualityParameters.Any() ? roomFilter.IsMatch(i) : true)
                .OfType<Room>()
                .Select(i => new RoomWrapper(i))
                .ToList();
            roomWrappers.ForEach(i => i.DependentElements = GetRoomDependentElements(i, elementsFilter));
            return roomWrappers;
        }

        private List<ElementWrapper> GetRoomDependentElements(RoomWrapper roomWrapper, ElementApplyFilter filter)
        {
            var wallBuiltInCategory = BuiltInCategory.OST_Walls;
            var allSelectedCategories = filter.Categories
                .Select(i => i.BuiltInCategory).ToList();
            var multiCategoryFilter = new ElementMulticategoryFilter(allSelectedCategories
                .Where(i => i != wallBuiltInCategory).ToList());

            var allWalls = roomWrapper.RevitElement.GetBoundarySegments(
                new SpatialElementBoundaryOptions
                {
                    SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.Center
                })
            .SelectMany(i => i.Select(w => _doc.GetElement(w.ElementId))).OfType<Wall>().ToList();

            var glassWindow = allSelectedCategories.Contains(wallBuiltInCategory) ? 
                allWalls.Where(i => i.CurtainGrid != null)
                .Where(i => filter.EqualityParameters.Any() ? filter.IsMatch(i) : true) 
                : new List<Wall>();

            var elements = allWalls.Where(i => i.CurtainGrid == null)
            .SelectMany(i => i.GetDependentElements(multiCategoryFilter)).ToList()
            .Select(id => _doc.GetElement(id))
            .Where(i => filter.EqualityParameters.Any() ? filter.IsMatch(i) : true);

            return elements.Concat(glassWindow).Select(i => new ElementWrapper(i))
                .Where(i => i.Vectors.Any(l => _geometryService.HasLineSolidIntersection(l, roomWrapper.Solid)))
                .ToList();
        }
    }
}
