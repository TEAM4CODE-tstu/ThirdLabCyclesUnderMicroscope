using System.Windows;

namespace GoodsLogistics.Views;

public partial class ConditionsWindow : Window
{
    public ConditionsWindow()
    {
        InitializeComponent();
    }

    public ConditionsWindow(object dataContext) : this()
    {
        // Забираем DataContext главной вкладки, чтобы привязка CorrectnessConditions работала
        DataContext = dataContext;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
