using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace mprWorldOrientation.Models
{
    /// <summary>
    /// Настройки для плагина
    /// </summary>
    public static class PluginSettings
    {
        /// <summary>
        /// Погрешность
        /// </summary>
        public static double Tolerance => 0.0000006;

        /// <summary>
        /// Имя направления севера
        /// </summary>
        public static string Nord => "С";

        /// <summary>
        /// Имя направления юга
        /// </summary>
        public static string South => "Ю";

        /// <summary>
        /// Имя направления запада
        /// </summary>
        public static string West => "З";

        /// <summary>
        /// Имя направления востока
        /// </summary>
        public static string East => "В";

        /// <summary>
        /// Имя направления северо-запада
        /// </summary>
        public static string NordWest => "СЗ";

        /// <summary>
        /// Имя направления юго-запада
        /// </summary>
        public static string SouthWest => "ЮЗ";

        /// <summary>
        /// Имя направления северо-востока
        /// </summary>
        public static string NordEast => "СВ";

        /// <summary>
        /// Имя направления юго-востока
        /// </summary>
        public static string SouthEast => "ЮВ";

        /// <summary>
        /// Категории доступные для выбора анализа элементов
        /// </summary>
        public static List<BuiltInCategory> ElementCategories => new List<BuiltInCategory>
        {
            BuiltInCategory.OST_Doors,
            BuiltInCategory.OST_Windows,
            BuiltInCategory.OST_Walls
        };
    }
}
