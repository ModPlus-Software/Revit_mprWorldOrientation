namespace mprWorldOrientation.Models
{
    using System.Collections.Generic;

    /// <summary>
    /// Класс содержащий модели пользовательских настроек для обработки
    /// </summary>
    public class SettingsData
    {
        /// <summary>
        /// Фильтры выбора для дверей
        /// </summary>
        public UserSettingsModel ElementApplyFilterForDoors { get; set; }

        /// <summary>
        /// Фильтры выбора для окон
        /// </summary>
        public UserSettingsModel ElementApplyFilterForWindows { get; set; }

        /// <summary>
        /// Фильтры выбора для витражей
        /// </summary>
        public UserSettingsModel ElementApplyFilterForGlassWalls { get; set; }

        /// <summary>
        /// Фильтры выбора для помещений
        /// </summary>
        public UserSettingsModel ElementApplyFilterForRooms { get; set; }

        /// <summary>
        /// Список с моделями для элементов
        /// </summary>
        public List<UserSettingsModel> ElementsModels { get; set; }
    }
}
