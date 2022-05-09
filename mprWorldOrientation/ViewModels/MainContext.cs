namespace mprWorldOrientation.ViewModels;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using Autodesk.Revit.DB;
using ModPlus_Revit;
using ModPlus_Revit.Enums;
using ModPlus_Revit.Models;
using ModPlusAPI;
using ModPlusAPI.Mvvm;
using ModPlusAPI.Services;
using ModPlusStyle.Controls;
using Enums;
using Models;
using Services;

/// <summary>
/// Основной контекст
/// </summary>
public class MainContext : ObservableObject
{
    private readonly Document _doc;
    private readonly UserSettingsService _userSettingsService;
    private readonly ResultService _resultService;
    private readonly RoomManagerService _roomManagerService;
    private readonly List<UserSettingsModel> _elementsSettingsModels;
    private readonly ModPlusWindow _window;

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="modPlusWindow">Окно</param>
    public MainContext(ModPlusWindow modPlusWindow)
    {
        _window = modPlusWindow;
        _resultService = new ResultService();
        _doc = ModPlus.UiApplication.ActiveUIDocument.Document;
        _roomManagerService = new RoomManagerService(_resultService);
        _userSettingsService = new UserSettingsService(nameof(mprWorldOrientation));

        ElementApplyFilterForDoors = _userSettingsService
            .Get<UserSettingsModel>(nameof(ElementApplyFilterForDoors));
        ElementApplyFilterForDoors.Filter.Categories = new ObservableCollection<RevitBuiltInCategory>
        {
            new (BuiltInCategory.OST_Doors)
        };

        ElementApplyFilterForWindows = _userSettingsService
            .Get<UserSettingsModel>(nameof(ElementApplyFilterForWindows));
        ElementApplyFilterForWindows.Filter.Categories = new ObservableCollection<RevitBuiltInCategory>
        {
            new (BuiltInCategory.OST_Windows)
        };

        ElementApplyFilterForGlassWalls = _userSettingsService
            .Get<UserSettingsModel>(nameof(ElementApplyFilterForGlassWalls));
        ElementApplyFilterForGlassWalls.Filter.Categories = new ObservableCollection<RevitBuiltInCategory>
        {
            new (BuiltInCategory.OST_Walls)
        };

        ElementApplyFilterForRooms = _userSettingsService
            .Get<UserSettingsModel>(nameof(ElementApplyFilterForRooms));
        ElementApplyFilterForRooms.Filter.Categories = new ObservableCollection<RevitBuiltInCategory>
        {
            new (BuiltInCategory.OST_Rooms)
        };

        _elementsSettingsModels = new List<UserSettingsModel>
        {
            ElementApplyFilterForDoors, ElementApplyFilterForWindows, ElementApplyFilterForGlassWalls
        };

        _elementsSettingsModels.ForEach(i => i.EnableChanged += () =>
        {
            OnPropertyChanged(nameof(IsCommandEnable));
        });
    }

    /// <summary>
    /// Фильтры выбора для дверей
    /// </summary>
    public UserSettingsModel ElementApplyFilterForDoors { get; }

    /// <summary>
    /// Фильтры выбора для окон
    /// </summary>
    public UserSettingsModel ElementApplyFilterForWindows { get; }

    /// <summary>
    /// Фильтры выбора для витражей
    /// </summary>
    public UserSettingsModel ElementApplyFilterForGlassWalls { get; }

    /// <summary>
    /// Фильтры выбора для помещений
    /// </summary>
    public UserSettingsModel ElementApplyFilterForRooms { get; }

    /// <summary>
    /// Выбор фильтров по комнатам
    /// </summary>
    public ElementApplyFilterControlScope FilterScope => ElementApplyFilterControlScope.Parameters;

    /// <summary>
    /// Свойство установки параметра
    /// </summary>
    public SetParameterType SetParameterType
    {
        get => Enum.TryParse(UserConfigFile.GetValue(nameof(mprWorldOrientation), nameof(SetParameterType)), out SetParameterType b) 
            ? b : SetParameterType.ForRoom;
        set 
        {
            switch (value)
            {
                case SetParameterType.ForRoom:
                    ElementApplyFilterForRooms.IsSetParamForElements = true;
                    _elementsSettingsModels.ForEach(i => i.IsSetParamForElements = false);
                    break;
                case SetParameterType.ForElements:
                    ElementApplyFilterForRooms.IsSetParamForElements = false;
                    _elementsSettingsModels.ForEach(i => i.IsSetParamForElements = true);
                    break;
                default:
                    ElementApplyFilterForRooms.IsSetParamForElements = true;
                    _elementsSettingsModels.ForEach(i => i.IsSetParamForElements = true);
                    break;
            }

            UserConfigFile.SetValue(nameof(mprWorldOrientation), nameof(SetParameterType), value.ToString(), true);
        } 
    }

    /// <summary>
    /// Доступна ли команда к выполнению
    /// </summary>
    public bool IsCommandEnable => _elementsSettingsModels.Any(i => i.IsEnabled);

    /// <summary>
    /// Выполнить
    /// </summary>
    public ICommand ExecuteCommand => new RelayCommand<ScopeType>(Execute, _ => IsCommandEnable);

    /// <summary>
    /// Закрытие
    /// </summary>
    public ICommand OnClosingCommand => new RelayCommandWithoutParameter(() =>
        SafeExecute.Execute(() => 
        {
            _userSettingsService.Set(ElementApplyFilterForDoors, nameof(ElementApplyFilterForDoors));
            _userSettingsService.Set(ElementApplyFilterForWindows, nameof(ElementApplyFilterForWindows));
            _userSettingsService.Set(ElementApplyFilterForGlassWalls, nameof(ElementApplyFilterForGlassWalls));
            _userSettingsService.Set(ElementApplyFilterForRooms, nameof(ElementApplyFilterForRooms));
        }));

    private void Execute(ScopeType scopeType)
    {
        _window.Hide();

        // tr1 Определение направление проемов
        var transaction = new Transaction(_doc, Language.GetItem("tr1"));
        transaction.Start();
        SafeExecute.Execute(
            () =>
            {
                var settingsData = new SettingsData
                {
                    ElementApplyFilterForDoors = ElementApplyFilterForDoors,
                    ElementApplyFilterForWindows = ElementApplyFilterForWindows,
                    ElementApplyFilterForGlassWalls = ElementApplyFilterForGlassWalls,
                    ElementApplyFilterForRooms = ElementApplyFilterForRooms,
                    ElementsModels = _elementsSettingsModels
                };
                _roomManagerService.SetRoomParameters(settingsData, scopeType);
                transaction.Commit();
                if (_resultService.Count(ModPlusAPI.Enums.ResultItemType.Info) > 0)
                    _resultService.Show();
            },
            () => transaction.RollBack(),
            () => _window.Close());
    }
}