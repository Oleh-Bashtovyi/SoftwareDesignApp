using System.Windows;
using System.Windows.Threading;

namespace SoftwareDesignApp.UI.Windows;

/// <summary>
/// Interaction logic for ResultWindow.xaml
/// </summary>
public partial class ResultWindow : Window
{
    public string DisplayText { get; }

    public ResultWindow(string text)
    {
        InitializeComponent();
        DisplayText = text;
        DataContext = this;

        // Прив'язка прокрутки до слайдера
        slider.ValueChanged += Slider_ValueChanged;

        // Обробка подій після рендерингу для визначення розміру ScrollViewer
        Loaded += (s, e) =>
        {
            Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
            {
                if (scrollViewer.ScrollableHeight == 0)
                    slider.IsEnabled = false;
            }));
        };
    }


    private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (scrollViewer.ScrollableHeight > 0)
        {
            double offset = e.NewValue * scrollViewer.ScrollableHeight;
            scrollViewer.ScrollToVerticalOffset(offset);
        }
    }

    private void OkButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}