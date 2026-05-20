using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System;

namespace LabirintusJatek
{
    public partial class MainWindow : Window
    {
        char[,] maze;

        int rows;
        int cols;

        int playerX = 1;
        int playerY = 1;

        string currentLanguage = "hu";
        string currentMapFile = "maze.txt";

        public MainWindow()
        {
            InitializeComponent();

            LanguageBox.SelectedIndex = 0;
        }

        private void LoadMaze_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text files|*.txt";

            if (ofd.ShowDialog() == true)
            {
                currentMapFile = ofd.FileName;
                LoadMaze(ofd.FileName);
            }
        }

        void LoadMaze(string path)
        {
            string[] lines = File.ReadAllLines(path);

            rows = lines.Length;
            cols = lines[0].Length;

            maze = new char[rows, cols];

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    maze[i, j] = lines[i][j];
                }
            }

            playerX = 1;
            playerY = 1;

            DrawMaze();
        }

        void DrawMaze()
        {
            MazeGrid.Children.Clear();

            MazeGrid.Rows = rows;
            MazeGrid.Columns = cols;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                {
                    Border cell = new Border();
                    cell.Width = 30;
                    cell.Height = 30;
                    cell.BorderBrush = Brushes.Black;
                    cell.BorderThickness = new Thickness(0.5);

                    if (i == playerY && j == playerX)
                    {
                        cell.Background = Brushes.Red;
                    }
                    else
                    {
                        switch (maze[i, j])
                        {
                            
                            case '║':
                            case '═':
                            case '—':
                            case '╔':
                            case '╗':
                            case '╚':
                            case '╝':
                            case '╦':
                            case '╩':
                            case '╠':
                            case '╣':
                            case '╬':
                                cell.Background = Brushes.DarkSlateGray;
                                break;

                            case '█':
                                cell.Background = Brushes.Green;
                                break;

                            default:
                                cell.Background = Brushes.White;
                                break;
                        }
                    }

                    MazeGrid.Children.Add(cell);
                }
            }
        }

        bool IsWall(char c)
        {
            return c == '█' ||
                   c == '║' ||
                   c == '═' ||
                   c == '—' ||
                   c == '╔' ||
                   c == '╗' ||
                   c == '╚' ||
                   c == '╝' ||
                   c == '╦' ||
                   c == '╩' ||
                   c == '╠' ||
                   c == '╣' ||
                   c == '╬';
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            int newX = playerX;
            int newY = playerY;

            if (e.Key == Key.W || e.Key == Key.Up)
                newY--;

            if (e.Key == Key.S || e.Key == Key.Down)
                newY++;

            if (e.Key == Key.A || e.Key == Key.Left)
                newX--;

            if (e.Key == Key.D || e.Key == Key.Right)
                newX++;

            if (newX >= 0 && newY >= 0 && newX < cols && newY < rows)
            {
                if (!IsWall(maze[newY, newX]))
                {
                    playerX = newX;
                    playerY = newY;

                    if (maze[playerY, playerX] == '█')
                    {
                        MessageBox.Show(currentLanguage == "hu"
                            ? "Nyertél!"
                            : "You won!");
                    }

                    DrawMaze();
                }
            }
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            string saveFile = Path.ChangeExtension(currentMapFile, ".txt");

            using (StreamWriter sw = new StreamWriter(saveFile))
            {
                sw.WriteLine(playerX);
                sw.WriteLine(playerY);
            }

            MessageBox.Show(currentLanguage == "hu"
                ? "Játék elmentve"
                : "Game saved");
        }

        private void LoadSave_Click(object sender, RoutedEventArgs e)
        {
            string saveFile = Path.ChangeExtension(currentMapFile, ".txt");

            if (File.Exists(saveFile))
            {
                string[] data = File.ReadAllLines(saveFile);

                playerX = int.Parse(data[0]);
                playerY = int.Parse(data[1]);

                DrawMaze();
            }
        }

        private void LanguageBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageBox.SelectedIndex == 0)
            {
                currentLanguage = "hu";
                InfoText.Text = "Mozgás: WASD vagy nyilak";
            }
            else
            {
                currentLanguage = "en";
                InfoText.Text = "Move: WASD or arrows";
            }
        }
    }
}
