namespace mprWorldOrientation.ViewModels
{
    using System.Collections.ObjectModel;
    using System.Windows.Input;
    using Autodesk.Revit.DB;
    using ModPlus_Revit;
    using ModPlus_Revit.Enums;
    using ModPlus_Revit.Models;
    using ModPlus_Revit.Utils;
    using ModPlusAPI;
    using ModPlusAPI.Mvvm;
    using ModPlusAPI.Services;
    using ModPlusStyle.Controls;
    using mprWorldOrientation.Models;
    using mprWorldOrientation.Services;

    /// <summary>
    /// Основной контекст
    /// </summary>
    public class MainContext
    {
        private readonly Document _doc;
        private readonly UserSettingsService _userSettingsService;
        private readonly ResultService _resultService;
        private readonly RoomManagerService _roomManagerService;

        /// <summary>
        /// ctor
        /// </summary>
        public MainContext()
        {
            _resultService = new ResultService();
            _doc = ModPlus.UiApplication.ActiveUIDocument.Document;
            _roomManagerService = new RoomManagerService(_resultService);
            _userSettingsService = new UserSettingsService(nameof(mprWorldOrientation));
            ElementApplyFilterForElements = _userSettingsService
                .Get<ElementApplyFilter>(nameof(ElementApplyFilterForElements));
            ElementApplyFilterForElements.SourceCategoriesOverride = PluginSettings.ElementCategories;
            ElementApplyFilterForRooms = _userSettingsService
                .Get<ElementApplyFilter>(nameof(ElementApplyFilterForRooms));
            ElementApplyFilterForRooms.Categories = new ObservableCollection<RevitBuiltInCategory>()
            {
                 new RevitBuiltInCategory(BuiltInCategory.OST_Rooms)
                    {
                        IsSelected = true
                    }
            };
        }

        /// <summary>
        /// Фильтры выбора для элементов
        /// </summary>
        public ElementApplyFilter ElementApplyFilterForElements { get; }

        /// <summary>
        /// Фильтры выбора для помещений
        /// </summary>
        public ElementApplyFilter ElementApplyFilterForRooms { get; }

        /// <summary>
        /// Имя параметра для заполнения
        /// </summary>
        public string SetParameterName
        {
            get => UserConfigFile.GetValue(nameof(mprWorldOrientation), nameof(SetParameterName));
            set => UserConfigFile.SetValue(nameof(mprWorldOrientation), nameof(SetParameterName), value, true);
        }

        /// <summary>
        /// Выбор фильтров по комнатам
        /// </summary>
        public ElementApplyFilterControlScope ScopeForRoomFilter => ElementApplyFilterControlScope.Parameters;

        /// <summary>
        /// Выполнить
        /// </summary>
        public ICommand ExecuteCommand => new RelayCommand<ModPlusWindow>(Execute);

        /// <summary>
        /// Закрытие
        /// </summary>
        public ICommand OnClosingCommand => new RelayCommandWithoutParameter(() =>
            SafeExecute.Execute(() => 
            {
                _userSettingsService.Set(ElementApplyFilterForElements, nameof(ElementApplyFilterForElements));
                _userSettingsService.Set(ElementApplyFilterForRooms, nameof(ElementApplyFilterForRooms));
            }));

        private void Execute(ModPlusWindow win)
        {
            win.Hide();
            var transaction = new Transaction(_doc, "Определение направление проемов");
            transaction.Start();
            SafeExecute.Execute(
                () =>
                {
                    _roomManagerService.SetRoomParameters(ElementApplyFilterForElements, ElementApplyFilterForRooms, SetParameterName);
                    transaction.Commit();
                },
                () => transaction.RollBack(),
                () => win.Close());
        }
    }
}
