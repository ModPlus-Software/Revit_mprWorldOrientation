namespace mprWorldOrientation.Views.Controls;

using System.Windows;
using System.Windows.Input;

/// <summary>
/// Логика взаимодействия для SelectionOptionsButtonControl.xaml
/// </summary>
public partial class SelectionOptionsButtonControl
{
    public static readonly DependencyProperty ExecuteCommandProperty = DependencyProperty.Register(
        "ExecuteCommand", typeof(ICommand), typeof(SelectionOptionsButtonControl), new PropertyMetadata(default(ICommand)));

    public static readonly DependencyProperty ButtonTextProperty = DependencyProperty.Register(
        "ButtonText", typeof(string), typeof(SelectionOptionsButtonControl), new PropertyMetadata(default(string)));

    public SelectionOptionsButtonControl()
    {
        InitializeComponent();

        ModPlusAPI.Windows.Helpers.WindowHelpers.ChangeStyleForResourceDictionary(Resources);
        ModPlusAPI.Language.SetLanguageProviderForResourceDictionary(Resources);
    }

    public ICommand ExecuteCommand
    {
        get => (ICommand)GetValue(ExecuteCommandProperty);
        set => SetValue(ExecuteCommandProperty, value);
    }

    public string ButtonText
    {
        get => (string)GetValue(ButtonTextProperty);
        set => SetValue(ButtonTextProperty, value);
    }

    private void Button_OnClick(object sender, RoutedEventArgs e)
    {
        Popup.IsOpen = !Popup.IsOpen;
    }

    private void ClosePopup_OnClick(object sender, RoutedEventArgs e)
    {
        Popup.IsOpen = false;
    }
}