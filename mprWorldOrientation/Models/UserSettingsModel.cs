namespace mprWorldOrientation.Models;

using ModPlus_Revit.Models;
using ModPlus_Revit.Utils;
using ModPlusAPI.Mvvm;
using System;

/// <summary>
/// Модель пользовательских настроек
/// </summary>
public class UserSettingsModel : ObservableObject
{
    private string _setParamName;
    private bool _isEnable;
    private bool _isSetParamValue;

    /// <summary>
    /// ctor
    /// </summary>
    public UserSettingsModel()
    {
        Filter = new ElementApplyFilter();
    }

    /// <summary>
    /// Событие возникаемое при изменении доступности
    /// </summary>
    public event Action EnableChanged;

    /// <summary>
    /// Фильтры для элементов
    /// </summary>
    public ElementApplyFilter Filter { get; set; }

    /// <summary>
    /// Категория
    /// </summary>
    public RevitBuiltInCategory Category { get; set; }

    /// <summary>
    /// Имя устанавливаемого параметра
    /// </summary>
    public string SetParameterName
    {
        get => _setParamName;
        set
        {
            _setParamName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Применен ли данный фильтр
    /// </summary>
    public bool IsEnabled
    {
        get => _isEnable;
        set
        {
            _isEnable = value;
            EnableChanged?.Invoke();
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Записывать ли в элементы результаты анализа
    /// </summary>
    public bool IsSetParamForElements
    {
        get => _isSetParamValue;
        set
        {
            _isSetParamValue = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Кол-во элементов которым удалось установить параметры
    /// </summary>
    public int SetElementCount { get; set; }
}