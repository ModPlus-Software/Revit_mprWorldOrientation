namespace mprWorldOrientation.Extensions
{
    using Autodesk.Revit.DB;

    /// <summary>
    /// Расширения для параметра элемента Revit
    /// </summary>
    public static class ParameterExtensions
    {
        /// <summary>
        /// Получает параметр из экземпляра или типа элемента
        /// </summary>
        /// <param name="elem">Element</param>
        /// <param name="parameterName">Имя параметра</param>
        public static Parameter GetParameterFromInstanceOrType(this Element elem, string parameterName)
        {
            if (!elem.IsValidObject)
                return null;

            var param = elem.LookupParameter(parameterName);
            if (param != null)
                return param;

            var typeId = elem.GetTypeId();
            if (typeId == null)
                return null;

            var type = elem.Document?.GetElement(typeId);

            param = type?.LookupParameter(parameterName);
            return param;
        }

        /// <summary>
        /// Задать значение параметру
        /// </summary>
        /// <param name="parameter">Параметр</param>
        /// <param name="value">Значение</param>
        /// <returns>true - значение задано, иначе - false</returns>
        public static bool SetParameterValue(
            this Parameter parameter,
            object value)
        {
            if (parameter == null
                || parameter.IsReadOnly)
                return false;

            switch (parameter.StorageType)
            {
                case StorageType.String:
                    return parameter.Set(value.ToString());

                case StorageType.Integer:
                    if (!(value is int iValue))
                        return false;
                    return parameter.Set(iValue);

                case StorageType.Double:
                    if (!(value is double dValue))
                        return false;

#if R2020 || R2019 || R2018 || R2017
                    return parameter.Set(UnitUtils.ConvertToInternalUnits(dValue, parameter.DisplayUnitType));
#else
                    return parameter.Set(UnitUtils.ConvertToInternalUnits(dValue, parameter.GetUnitTypeId()));
#endif
                case StorageType.ElementId:
                    if (!(value is ElementId idValue))
                        return false;
                    return parameter.Set(idValue);
            }

            return false;
        }
    }
}