namespace mprWorldOrientation.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using Autodesk.Revit.DB;
    using ModPlusAPI;
    using ModPlusAPI.Services;
    using mprWorldOrientation.Extensions;
    using mprWorldOrientation.Models;

    /// <summary>
    /// Сервис по работе с помещениями
    /// </summary>
    public class RoomManagerService
    {
        private readonly GeometryService _geometryService;
        private readonly GetElementService _getElementService;
        private readonly ResultService _resultService;

        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="resultService">Логер</param>
        public RoomManagerService(ResultService resultService)
        {
            _geometryService = new GeometryService();
            _getElementService = new GetElementService();
            _resultService = resultService;
        }

        /// <summary>
        /// Записывает значение парамера в зависимости от проемов
        /// </summary>
        /// <param name="settings">Данные с настройками</param>
        public void SetRoomParameters(SettingsData settings)
        {
            var rooms = _getElementService.GetRoomWrapper(settings);

            AnalyzeRoomPositions(rooms);
            var counturRooms = rooms.Where(i => i.IsCounturRoom).ToList();
            
            foreach (var room in counturRooms)
            {
                var uniqueOrintationValues = GetWorldSideString(
                    room.DependentElements.Where(i => i.IsConturElement));

                if (settings.ElementApplyFilterForRooms.IsSetParamForElements 
                    && !string.IsNullOrEmpty(settings.ElementApplyFilterForRooms.SetParameterName))
                {
                    SetParameter(
                        room.RevitElement, 
                        settings.ElementApplyFilterForRooms.SetParameterName.Trim(), 
                        uniqueOrintationValues);
                }

                foreach (var dependentElement in room.DependentElements)
                {
                    var equivalentSettingModel = settings.ElementsModels
                        .FirstOrDefault(i => i.Filter.Categories.First().BuiltInCategory 
                        == (BuiltInCategory)dependentElement.RevitElement.Category.Id.IntegerValue);

                    if (equivalentSettingModel == null)
                        continue;

                    if (equivalentSettingModel.IsSetParamForElements
                    && !string.IsNullOrEmpty(equivalentSettingModel.SetParameterName))
                    {
                        SetParameter(
                        dependentElement.RevitElement,
                        equivalentSettingModel.SetParameterName.Trim(),
                        uniqueOrintationValues);
                    }
                }
            }
        }

        private void SetParameter(Element element, string parameterName, object paramValue)
        {
            var param = element.GetParameterFromInstanceOrType(parameterName);
            if (param == null)
            {
                /*
                 * t1 У элементов помещений нет параметрама 
                 * t2 поэтому не возможно заполнить результаты анализа
                 */
                _resultService.Add($"{Language.GetItem("t1")}{parameterName}, {Language.GetItem("t2")}");
                return;
            }

            if (!param.SetParameterValue(paramValue))
            {
                /*
                 * t3 Для элемента  
                 * t4 не удалось заполнить парам. 
                 * t5  значениеим 
                 * t6 Проверьте тип запоняемого параметра. Он должен быть строковым
                 */
                _resultService.Add($"{Language.GetItem("t3")}{element.Id.IntegerValue} " +
                    $"{Language.GetItem("t3")}\"{parameterName}\"{Language.GetItem("t5")}\"{paramValue}\". " +
                    $"{Language.GetItem("t6")}");
            }
        }

        /// <summary>
        /// Определяет являетли ли комната внутренней и не имеет стен, которые являются контуром
        /// </summary>
        /// <param name="rooms">Список оберток стен</param>
        private void AnalyzeRoomPositions(List<RoomWrapper> rooms)
        {
            foreach (var room in rooms)
            {
                var solids = rooms.Select(i => i.Solid);

                foreach (var depElement in room.DependentElements)
                {
                    depElement.HasFirstVectorIntersectedRoom = solids
                        .Any(s => _geometryService.HasLineSolidIntersection(
                            depElement.FirstVector, s));
                    depElement.HasSecondVectorIntersectionRoom = solids
                        .Any(s => _geometryService.HasLineSolidIntersection(
                            depElement.SecondVector, s));
                }
            }
        }

        private string GetWorldSideString(IEnumerable<ElementWrapper> elementWrappers)
        {
            var values = new List<string>();
            foreach (var eleementWrapper in elementWrappers)
            {
                var outSideVector = eleementWrapper.HasFirstVectorIntersectedRoom 
                    ? eleementWrapper.SecondVector 
                    : eleementWrapper.FirstVector;

                var wordSide = _geometryService.WorldDirectionByVector(outSideVector.Direction.Normalize());
                if (!string.IsNullOrEmpty(wordSide))
                    values.Add(wordSide);
            }

            return string.Join("+", values.Distinct()).Trim(new char[] { '+' });
        }
    }
}
