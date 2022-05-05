namespace mprWorldOrientation;

using Autodesk.Revit.UI;
using ModPlus_Revit;
using ModPlusAPI.Mvvm;

/// <summary>
/// Main context
/// </summary>
public class MainContext : ObservableObject
{
    private readonly UIApplication _uiApplication;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainContext"/> class.
    /// </summary>
    public MainContext()
    {
        _uiApplication = ModPlus.UiApplication;
    }
}