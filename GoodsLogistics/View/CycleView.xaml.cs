using System.Windows;
using System.Windows.Controls;

namespace GoodsLogistics.Views;

public partial class CycleView : UserControl
{
    public CycleView()
    {
        InitializeComponent();
    }

    private void ShowConditions_Click(object sender, RoutedEventArgs e)
    {
        var window = new ConditionsWindow(DataContext)
        {
            Owner = Window.GetWindow(this)
        };
        window.ShowDialog();
    }
}
