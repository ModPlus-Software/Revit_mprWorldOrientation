namespace mprWorldOrientation.Models
{
    using ModPlusAPI;

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
        /// "C"
        public static string Nord => Language.GetItem("s1");

        /// <summary>
        /// Имя направления юга
        /// </summary>
        /// "Ю"
        public static string South => Language.GetItem("s2");

        /// <summary>
        /// Имя направления запада
        /// </summary>
        /// "З"
        public static string West => Language.GetItem("s3");

        /// <summary>
        /// Имя направления востока
        /// </summary>
        /// "В"
        public static string East => Language.GetItem("s4");

        /// <summary>
        /// Имя направления северо-запада
        /// </summary>
        /// "СЗ"
        public static string NordWest => Language.GetItem("s5");

        /// <summary>
        /// Имя направления юго-запада
        /// </summary>
        /// "ЮЗ"
        public static string SouthWest => Language.GetItem("s6");

        /// <summary>
        /// Имя направления северо-востока
        /// </summary>
        /// "СВ"
        public static string NordEast => Language.GetItem("s7");

        /// <summary>
        /// Имя направления юго-востока
        /// </summary>
        /// "ЮВ"
        public static string SouthEast => Language.GetItem("s8");
    }
}
