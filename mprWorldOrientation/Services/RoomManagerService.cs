namespace mprWorldOrientation.Services;

using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Extensions;
using Models;
using ModPlusAPI;
using ModPlusAPI.Enums;
using ModPlusAPI.Services;
using Enums;

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
    /// Записывает значение параметра в зависимости от проемов
    /// </summary>
    /// <param name="settings">Данные с настройками</param>
    /// <param name="scopeType">Параметры для выбора элементов</param>
    public void SetRoomParameters(SettingsData settings, ScopeType scopeType)
    {
        var models = new List<UserSettingsModel>(settings.ElementsModels);
        models.Add(settings.ElementApplyFilterForRooms);
        models.ForEach(i => i.SetElementCount = 0);
        var rooms = _getElementService.GetRoomWrapper(settings, scopeType);
        var a = Language.GetItem("t3");
        _resultService.Add(string.Format(Language.GetItem("t3"), rooms.Count.ToString()), ResultItemType.Success);

        AnalyzeRoomPositions(rooms);

        var contourRooms = rooms.Where(i => i.IsCounturRoom).ToList();

        foreach (var room in contourRooms)
        {
            var uniqueOrientationValues = GetWorldSideString(room.DependentElements.Where(i => i.IsContourElement));

            if (settings.ElementApplyFilterForRooms.IsSetParamForElements 
                && !string.IsNullOrEmpty(settings.ElementApplyFilterForRooms.SetParameterName))
            {
                if (SetParameter(
                    room.RevitElement,
                    settings.ElementApplyFilterForRooms.SetParameterName.Trim(),
                    uniqueOrientationValues))
                    settings.ElementApplyFilterForRooms.SetElementCount++;
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
                    if (SetParameter(
                        dependentElement.RevitElement,
                        equivalentSettingModel.SetParameterName.Trim(),
                        uniqueOrientationValues))
                    {
                        equivalentSettingModel.SetElementCount++;
                    }
                }
            }
        }

        foreach (var model in models)
        {
            if (model.IsSetParamForElements && !string.IsNullOrEmpty(model.SetParameterName))
            {
                _resultService.Add(
                    string.Format(
                        Language.GetItem("t4"),
                        model.Category.DisplayName,
                        model.SetElementCount.ToString()),
                    ResultItemType.Success);
            }
            else if (model.IsSetParamForElements && string.IsNullOrEmpty(model.SetParameterName))
            {
                _resultService.Add(
                    string.Format(
                        Language.GetItem("t6"),
                        model.Category.DisplayName),
                    ResultItemType.Warning);
            }
        }
    }

    private bool SetParameter(Element element, string parameterName, object paramValue)
    {
        var param = element.GetParameterFromInstanceOrType(parameterName);
        if (param == null)
        {
            // У элементов помещений нет параметра "{0}", поэтому не возможно заполнить результаты анализа
            _resultService.Add(string.Format(Language.GetItem("t1"), parameterName), ResultItemType.Error);
            return false;
        }

        if (!param.SetParameterValue(paramValue))
        {
            // Не удалось задать значение параметру "{0}". Проверьте тип заполняемого параметра - он должен быть строковым
            _resultService.Add(string.Format(Language.GetItem("t2"), parameterName), element.Id.ToString(), ResultItemType.Error);
            return false;
        }

        return true;
    }

    /// <summary>
    /// Определяет является ли комната внутренней и не имеет стен, которые являются контуром
    /// </summary>
    /// <param name="rooms">Список оберток стен</param>
    private void AnalyzeRoomPositions(List<RoomWrapper> rooms)
    {
        var solids = _getElementService.GetAllRooms().Select(i => i.Solid).ToList();
        foreach (var room in rooms)
        {
            foreach (var depElement in room.DependentElements)
            {
                depElement.HasFirstVectorIntersectedRoom = 
                    solids.Any(s => _geometryService.HasLineSolidIntersection(depElement.FirstVector, s));
                depElement.HasSecondVectorIntersectionRoom = 
                    solids.Any(s => _geometryService.HasLineSolidIntersection(depElement.SecondVector, s));
            }
        }
    }

    private string GetWorldSideString(IEnumerable<ElementWrapper> elementWrappers)
    {
        var values = new List<string>();
        foreach (var elementWrapper in elementWrappers)
        {
            var outSideVector = elementWrapper.HasFirstVectorIntersectedRoom 
                ? elementWrapper.SecondVector 
                : elementWrapper.FirstVector;

            var wordSide = _geometryService.WorldDirectionByVector(outSideVector.Direction.Normalize());
            if (!string.IsNullOrEmpty(wordSide))
                values.Add(wordSide);
        }

        return string.Join("+", values.Distinct()).Trim('+');
    }
}