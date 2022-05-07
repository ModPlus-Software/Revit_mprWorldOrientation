namespace mprWorldOrientation.Services
{
    using System.Collections.Generic;
    using System.Linq;
    using ModPlus_Revit.Utils;
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
        /// <param name="elementApplyFilterForElements">Фильтр по элементам</param>
        /// <param name="elementApplyFilterForRooms">Фильтр по помещениям</param>
        /// <param name="setParamName">Заполняемый параметр</param>
        public void SetRoomParameters(
            ElementApplyFilter elementApplyFilterForElements, ElementApplyFilter elementApplyFilterForRooms, string setParamName)
        {
            var rooms = _getElementService.GetRoomWrapper(elementApplyFilterForRooms, elementApplyFilterForElements);

            AnalyzeRoomPositions(rooms);
            var counturRooms = rooms.Where(i => i.IsCounturRoom).ToList();
            
            foreach (var room in counturRooms)
            {
                var uniqueOrintationValues = GetWorldSideString(
                    room.DependentElements.Where(i => i.IsConturElement));

                var param = room.RevitElement.GetParameterFromInstanceOrType(setParamName);
                if (param == null)
                {
                    _resultService.Add($"У элементов помещений нет параметрама {setParamName}, " +
                        $"поэтому не возможно заполнить результаты анализа");
                    return;
                }

                if (!param.SetParameterValue(uniqueOrintationValues))
                {
                    _resultService.Add($"Для элемента {room.RevitElement.Id.IntegerValue} " +
                        $"не удалось заполнить парам. \"{setParamName}\" значениеим \"{uniqueOrintationValues}\". " +
                        $"Проверьте тип запоняемого параметра. Он должен быть строковым");
                }
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
