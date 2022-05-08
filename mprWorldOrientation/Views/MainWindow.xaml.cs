namespace mprWorldOrientation.Views;

/// <summary>
/// Логика взаимодействия для MainWindow.xaml
/// </summary>
public partial class MainWindow
{
    /// <summary>
    /// ctor
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        Title = ModPlusAPI.Language.GetPluginLocalName(ModPlusConnector.Instance);
    }
}
