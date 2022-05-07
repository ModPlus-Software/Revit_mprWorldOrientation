namespace mprWorldOrientation.Views;

using mprWorldOrientation.ViewModels;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="mainContext">Основной контекст</param>
    public MainWindow(MainContext mainContext)
    {
        DataContext = mainContext;
        InitializeComponent();

        Title = ModPlusAPI.Language.GetPluginLocalName(ModPlusConnector.Instance);
    }
}
