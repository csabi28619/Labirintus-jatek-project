using System.Windows;
using System.Windows.Controls;
using LabyrinthEditor.Services;

namespace LabyrinthEditor.Views
{
    public class NewMapDialog : Window
    {
        private readonly TextBox _tbRows;
        private readonly TextBox _tbCols;
        private readonly LanguageService _lang;

        public int Rows { get; private set; } = 15;
        public int Cols { get; private set; } = 20;

        public NewMapDialog(LanguageService lang)
        {
            _lang = lang;

            Title  = lang.Get("dialog_new_title");
            Width  = 280; Height = 180;
            ResizeMode = ResizeMode.NoResize;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            Background = System.Windows.Media.Brushes.Black;
            Foreground = System.Windows.Media.Brushes.White;

            var grid = new Grid { Margin = new Thickness(16) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(120) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            var lblRows = MakeLabel(lang.Get("dialog_new_rows") + ":");
            _tbRows     = MakeTextBox("15");
            Grid.SetRow(lblRows, 0); Grid.SetColumn(lblRows, 0);
            Grid.SetRow(_tbRows, 0); Grid.SetColumn(_tbRows, 1);

            var lblCols = MakeLabel(lang.Get("dialog_new_cols") + ":");
            _tbCols     = MakeTextBox("20");
            Grid.SetRow(lblCols, 1); Grid.SetColumn(lblCols, 0);
            Grid.SetRow(_tbCols, 1); Grid.SetColumn(_tbCols, 1);

            var btnPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            var btnOk     = new Button { Content = lang.Get("dialog_ok"),     Width = 70, Margin = new Thickness(4,0,0,0), IsDefault = true };
            var btnCancel = new Button { Content = lang.Get("dialog_cancel"),  Width = 70, Margin = new Thickness(4,0,0,0), IsCancel  = true };
            btnOk.Click     += BtnOk_Click;
            btnCancel.Click += (s, e) => DialogResult = false;
            btnPanel.Children.Add(btnOk);
            btnPanel.Children.Add(btnCancel);
            Grid.SetRow(btnPanel, 2); Grid.SetColumnSpan(btnPanel, 2);

            grid.Children.Add(lblRows); grid.Children.Add(_tbRows);
            grid.Children.Add(lblCols); grid.Children.Add(_tbCols);
            grid.Children.Add(btnPanel);
            Content = grid;
        }

        private void BtnOk_Click(object sender, RoutedEventArgs e)
        {
            if (!int.TryParse(_tbRows.Text, out int r) || r < 3 || r > 100)
            {
                MessageBox.Show("3 – 100", _lang.Get("dialog_new_rows"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (!int.TryParse(_tbCols.Text, out int c) || c < 3 || c > 200)
            {
                MessageBox.Show("3 – 200", _lang.Get("dialog_new_cols"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            Rows = r; Cols = c;
            DialogResult = true;
        }

        private static TextBlock MakeLabel(string text) => new TextBlock
        {
            Text      = text,
            Foreground = System.Windows.Media.Brushes.LightGray,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            FontSize   = 12,
            VerticalAlignment = VerticalAlignment.Center,
            Margin    = new Thickness(0, 6, 8, 6)
        };

        private static TextBox MakeTextBox(string text) => new TextBox
        {
            Text       = text,
            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
            Background = System.Windows.Media.Brushes.DarkSlateGray,
            Foreground = System.Windows.Media.Brushes.White,
            BorderBrush = System.Windows.Media.Brushes.DimGray,
            Margin  = new Thickness(0, 6, 0, 6),
            Padding = new Thickness(4, 2, 4, 2)
        };
    }
}
