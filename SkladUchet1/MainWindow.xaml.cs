using System.Windows;
using SkladUchet1.ViewModels;

namespace SkladUchet1;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        DataContext = new MainViewModel();
    }

    private void BtnAdd_Click(object sender, RoutedEventArgs e)
    {
        var addWindow = new AddBatchWindow();
        if (addWindow.ShowDialog() == true)
        {
            if (DataContext is MainViewModel vm)
                vm.RefreshCommand.Execute(null);
        }
    }
}