using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using LabyrinthEditor.Models;
using LabyrinthEditor.Services;
using LabyrinthEditor.ViewModels;
using ValidationResult = LabyrinthEditor.Services.ValidationResult;

namespace LabyrinthEditor.Views
{
    public partial class MainWindow : Window
    {
        private readonly MainViewModel   _vm   = new MainViewModel();
        private readonly LanguageService _lang = LanguageService.Instance;

        public MainWindow()
        {
            InitializeComponent();
            TheMapCanvas.SetViewModel(_vm);

            _vm.MapChanged += UpdateInfoPanel;
            _vm.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(MainViewModel.TitleText))
                    Title = _vm.TitleText;
                if (e.PropertyName == nameof(MainViewModel.StatusText))
                    StatusBar.Text = _vm.StatusText;
            };

            BuildPalettePanel();
            UpdateLanguage();
            UpdateInfoPanel();
            HighlightActiveTool();

            KeyDown += (s, e) =>
            {
                if (e.Key == System.Windows.Input.Key.Z &&
                    System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    _vm.UndoCommand.Execute(null);
                if (e.Key == System.Windows.Input.Key.Y &&
                    System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    _vm.RedoCommand.Execute(null);
                if (e.Key == System.Windows.Input.Key.S &&
                    System.Windows.Input.Keyboard.Modifiers == System.Windows.Input.ModifierKeys.Control)
                    SaveMap();
            };
        }

        private static readonly char[] CorridorChars =
            { '╬','═','║','╔','╗','╚','╝','╦','╩','╠','╣' };

        private void BuildPalettePanel()
        {
            PalettePanel.Children.Clear();
            foreach (char c in CorridorChars)
            {
                var preview = new TilePreviewCanvas
                {
                    TileChar         = c,
                    Width            = 32,
                    Height           = 32,
                    IsHitTestVisible = false
                };
                var btn = new Button
                {
                    Content         = preview,
                    Width           = 40,
                    Height          = 40,
                    Margin          = new Thickness(2),
                    Tag             = c,
                    Background      = new SolidColorBrush(Color.FromRgb(30, 30, 46)),
                    BorderBrush     = new SolidColorBrush(Color.FromRgb(42, 42, 58)),
                    BorderThickness = new Thickness(1),
                    Padding         = new Thickness(3),
                    ToolTip         = c.ToString(),
                    Cursor          = System.Windows.Input.Cursors.Hand,
                    Template        = BuildButtonTemplate()
                };
                btn.Click += PaletteTile_Click;
                PalettePanel.Children.Add(btn);
            }
            RefreshPaletteHighlight();
        }

        private static ControlTemplate BuildButtonTemplate()
        {
            var template = new ControlTemplate(typeof(Button));
            var border   = new FrameworkElementFactory(typeof(Border));
            border.SetBinding(Border.BackgroundProperty,
                new System.Windows.Data.Binding("Background")
                { RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetBinding(Border.BorderBrushProperty,
                new System.Windows.Data.Binding("BorderBrush")
                { RelativeSource = new System.Windows.Data.RelativeSource(
                    System.Windows.Data.RelativeSourceMode.TemplatedParent) });
            border.SetValue(Border.BorderThicknessProperty, new Thickness(1));
            border.SetValue(Border.CornerRadiusProperty,    new CornerRadius(3));
            border.SetValue(Border.PaddingProperty,         new Thickness(3));
            var cp = new FrameworkElementFactory(typeof(ContentPresenter));
            cp.SetValue(ContentPresenter.HorizontalAlignmentProperty, HorizontalAlignment.Center);
            cp.SetValue(ContentPresenter.VerticalAlignmentProperty,   VerticalAlignment.Center);
            border.AppendChild(cp);
            template.VisualTree = border;
            return template;
        }

        private void PaletteTile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn && btn.Tag is char c)
            {
                _vm.SelectedCorridorChar = c;
                _vm.ActiveTool           = EditorTool.Corridor;
                HighlightActiveTool();
                RefreshPaletteHighlight();
            }
        }

        private void RefreshPaletteHighlight()
        {
            foreach (Button btn in PalettePanel.Children)
            {
                if (btn.Tag is char c)
                {
                    bool selected = (c == _vm.SelectedCorridorChar &&
                                     _vm.ActiveTool == EditorTool.Corridor);
                    btn.BorderBrush = selected
                        ? new SolidColorBrush(Color.FromRgb(200, 160, 20))
                        : new SolidColorBrush(Color.FromRgb(42,  42,  58));
                    btn.Background  = selected
                        ? new SolidColorBrush(Color.FromRgb(50, 40, 10))
                        : new SolidColorBrush(Color.FromRgb(30, 30, 46));
                }
            }
        }

        private void UpdateInfoPanel()
        {
            if (_vm.Map == null) return;
            var L    = _lang;
            var grid = _vm.Map.GetGridCopy();
            InfoName.Text    = $"{L.Get("info_name")}:     {_vm.Map.MapName}";
            InfoSize.Text    = $"{L.Get("info_size")}:    {_vm.Map.Cols} × {_vm.Map.Rows}";
            InfoRooms.Text   = $"{L.Get("info_rooms")}:   {ValidationService.GetRoomNumber(grid)}";
            InfoExits.Text   = $"{L.Get("info_exits")}:   {ValidationService.GetSuitableEntrance(grid)}";
            InfoEnemies.Text = $"{L.Get("info_enemies")}: {_vm.Map.TotalEnemies()}";
            Title = _vm.TitleText;
        }

        private void UpdateValidationPanel(ValidationResult result)
        {
            ValidationList.ItemsSource = result.Issues
                .Select(i => $"{SeverityIcon(i.Severity)} {i.Message}")
                .ToList();
        }

        private static string SeverityIcon(ValidationSeverity s) => s switch
        {
            ValidationSeverity.Error   => "✗",
            ValidationSeverity.Warning => "⚠",
            _                          => "✓"
        };

        private void BtnLanguage_Click(object sender, RoutedEventArgs e)
        {
            _lang.Toggle();
            UpdateLanguage();
            UpdateInfoPanel();
        }

        private void UpdateLanguage()
        {
            var L = _lang;

            MenuFile.Header     = L.Get("menu_file");
            MenuNew.Header      = L.Get("file_new");
            MenuOpen.Header     = L.Get("file_open");
            MenuSave.Header     = L.Get("file_save");
            MenuSaveAs.Header   = L.Get("file_save_as");
            MenuExit.Header     = L.Get("file_exit");
            MenuEdit.Header     = L.Get("menu_edit");
            MenuUndo.Header     = $"{L.Get("edit_undo")}\tCtrl+Z";
            MenuRedo.Header     = $"{L.Get("edit_redo")}\tCtrl+Y";
            MenuView.Header     = L.Get("menu_view");
            MenuGrid.Header     = L.Get("view_grid");
            MenuHighlights.Header = L.Get("view_highlights");
            MenuReset.Header    = L.Get("view_reset");
            MenuValidate.Header = L.Get("menu_validate");

            BtnNew.Content      = L.Get("toolbar_new");
            BtnOpen.Content     = L.Get("toolbar_open");
            BtnSave.Content     = L.Get("toolbar_save");
            BtnUndo.Content     = L.Get("toolbar_undo");
            BtnRedo.Content     = L.Get("toolbar_redo");
            BtnValidate.Content = L.Get("toolbar_validate");
            BtnLanguage.Content = L.Get("lang_toggle");

            LblSectionTool.Text      = L.Get("section_tool");
            BtnToolCorridor.Content  = L.Get("tool_corridor");
            BtnToolRoom.Content      = L.Get("tool_room");
            BtnToolPlayerStart.Content = L.Get("tool_player");
            BtnToolEnemy.Content     = L.Get("tool_enemy");
            BtnToolVoid.Content      = L.Get("tool_void");

            LblEnemyAdd.Text    = L.Get("enemy_hint_add");
            LblEnemyRemove.Text = L.Get("enemy_hint_remove");
            LblEnemyMax.Text    = L.Get("enemy_hint_max");

            LblSectionCorridor.Text = L.Get("section_corridor");
            LblSectionZoom.Text     = L.Get("section_zoom");
            BtnZoomReset.Content    = L.Get("zoom_reset");

            LblSectionMapInfo.Text   = L.Get("section_mapinfo");
            LblSectionValidate.Text  = L.Get("section_validate");
            BtnRunValidation.Content = L.Get("validate_run");

            LblLegendTitle.Text    = L.Get("section_legend");
            LblLegendInvalid.Text  = L.Get("legend_invalid");
            LblLegendIsolated.Text = L.Get("legend_isolated");
            LblLegendExit.Text     = L.Get("legend_exit");

            LblSectionControls.Text = L.Get("section_controls");
            LblControlsText.Text    = L.Get("controls_text").Replace("\\n", "\n");
        }

        private void MenuNew_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;
            var dlg = new NewMapDialog(_lang) { Owner = this };
            if (dlg.ShowDialog() == true)
            {
                _vm.NewMap(dlg.Rows, dlg.Cols);
                UpdateInfoPanel();
                ValidationList.ItemsSource = null;
            }
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;
            var ofd = new OpenFileDialog
            {
                Title  = _lang.Get("fdlg_open_title"),
                Filter = _lang.Get("fdlg_open_filter")
            };
            if (ofd.ShowDialog() == true)
            {
                try
                {
                    _vm.LoadMap(ofd.FileName);
                    UpdateInfoPanel();
                    ValidationList.ItemsSource = null;
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show(
                        $"{_lang.Get("error_load_msg")}\n{ex.Message}",
                        _lang.Get("error_load_title"),
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)   => SaveMap();
        private void MenuSaveAs_Click(object sender, RoutedEventArgs e) => SaveMapAs();

        private void SaveMap()
        {
            if (string.IsNullOrEmpty(_vm.Map?.FilePath)) SaveMapAs();
            else TrySave(_vm.Map.FilePath);
        }

        private void SaveMapAs()
        {
            var result = _vm.RunValidation();
            if (!result.IsValid)
            {
                UpdateValidationPanel(result);
                var errors = result.Issues
                    .Where(i => i.Severity == ValidationSeverity.Error)
                    .Select(i => i.Message);
                MessageBox.Show(
                    $"{_lang.Get("dialog_invalid_msg")}\n\n{string.Join("\n", errors)}",
                    _lang.Get("dialog_invalid_title"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            var sfd = new SaveFileDialog
            {
                Title    = _lang.Get("fdlg_save_title"),
                Filter   = _lang.Get("fdlg_save_filter"),
                FileName = _vm.Map?.MapName ?? "untitled"
            };
            if (sfd.ShowDialog() == true) TrySave(sfd.FileName);
        }

        private void TrySave(string path)
        {
            try
            {
                _vm.SaveMap(path);
                UpdateInfoPanel();
                MessageBox.Show(
                    _lang.Get("dialog_saved"),
                    _lang.Get("dialog_saved_title"),
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(
                    $"{_lang.Get("error_save_msg")}\n{ex.Message}",
                    _lang.Get("error_save_title"),
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            if (ConfirmDiscardChanges()) Application.Current.Shutdown();
        }

        private void MenuUndo_Click(object sender, RoutedEventArgs e)
        {
            _vm.UndoCommand.Execute(null);
            UpdateInfoPanel();
        }

        private void MenuRedo_Click(object sender, RoutedEventArgs e)
        {
            _vm.RedoCommand.Execute(null);
            UpdateInfoPanel();
        }

        private void MenuGrid_Click(object sender, RoutedEventArgs e) =>
            _vm.ShowGrid = MenuGrid.IsChecked;

        private void MenuHighlights_Click(object sender, RoutedEventArgs e)
        {
            _vm.ShowHighlights = MenuHighlights.IsChecked;
            _vm.RefreshHighlights();
        }

        private void MenuReset_Click(object sender, RoutedEventArgs e) =>
            TheMapCanvas.ResetView();

        private void MenuValidate_Click(object sender, RoutedEventArgs e)
        {
            UpdateValidationPanel(_vm.RunValidation());
            UpdateInfoPanel();
        }

        private void BtnToolCorridor_Click(object sender, RoutedEventArgs e)
        {
            _vm.ActiveTool = EditorTool.Corridor;
            HighlightActiveTool(); RefreshPaletteHighlight();
        }

        private void BtnToolRoom_Click(object sender, RoutedEventArgs e)
        {
            _vm.ActiveTool = EditorTool.Room;
            HighlightActiveTool(); RefreshPaletteHighlight();
        }

        private void BtnToolPlayerStart_Click(object sender, RoutedEventArgs e)
        {
            _vm.ActiveTool = EditorTool.PlayerStart;
            HighlightActiveTool(); RefreshPaletteHighlight();
        }

        private void BtnToolEnemy_Click(object sender, RoutedEventArgs e)
        {
            _vm.ActiveTool = EditorTool.Enemy;
            HighlightActiveTool(); RefreshPaletteHighlight();
        }

        private void BtnToolVoid_Click(object sender, RoutedEventArgs e)
        {
            _vm.ActiveTool = EditorTool.Void;
            HighlightActiveTool(); RefreshPaletteHighlight();
        }

        private static readonly Color ColorInactive = Color.FromRgb(30,  30,  46);
        private static readonly Color ColorActive   = Color.FromRgb(180, 40,  40);
        private static readonly Color ColorPlayer   = Color.FromRgb(20,  140, 60);
        private static readonly Color ColorEnemy    = Color.FromRgb(160, 30,  30);

        private void HighlightActiveTool()
        {
            BtnToolCorridor.Background    = new SolidColorBrush(ColorInactive);
            BtnToolRoom.Background        = new SolidColorBrush(ColorInactive);
            BtnToolPlayerStart.Background = new SolidColorBrush(ColorInactive);
            BtnToolEnemy.Background       = new SolidColorBrush(ColorInactive);
            BtnToolVoid.Background        = new SolidColorBrush(ColorInactive);

            switch (_vm.ActiveTool)
            {
                case EditorTool.Corridor:    BtnToolCorridor.Background    = new SolidColorBrush(ColorActive);  break;
                case EditorTool.Room:        BtnToolRoom.Background        = new SolidColorBrush(ColorActive);  break;
                case EditorTool.PlayerStart: BtnToolPlayerStart.Background = new SolidColorBrush(ColorPlayer);  break;
                case EditorTool.Enemy:       BtnToolEnemy.Background       = new SolidColorBrush(ColorEnemy);   break;
                case EditorTool.Void:        BtnToolVoid.Background        = new SolidColorBrush(ColorActive);  break;
            }

            EnemyHintPanel.Visibility =
                _vm.ActiveTool == EditorTool.Enemy ? Visibility.Visible : Visibility.Collapsed;
        }

        private void BtnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            _vm.CellSize *= 1.25;
            TxtZoom.Text  = $"{(int)(_vm.CellSize / MapRenderer.DefaultCellSize * 100)}%";
        }

        private void BtnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            _vm.CellSize /= 1.25;
            TxtZoom.Text  = $"{(int)(_vm.CellSize / MapRenderer.DefaultCellSize * 100)}%";
        }

        private bool ConfirmDiscardChanges()
        {
            if (_vm.Map == null || !_vm.Map.IsDirty) return true;
            var res = MessageBox.Show(
                _lang.Get("dialog_save_before_close"),
                _lang.Get("dialog_unsaved_title"),
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Question);
            if (res == MessageBoxResult.Yes) { SaveMap(); return true; }
            if (res == MessageBoxResult.No)  return true;
            return false;
        }

        protected override void OnClosing(System.ComponentModel.CancelEventArgs e)
        {
            if (_vm.Map?.IsDirty == true && !ConfirmDiscardChanges())
                e.Cancel = true;
            base.OnClosing(e);
        }
    }
}
