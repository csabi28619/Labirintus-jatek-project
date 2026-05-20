using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace labirintus_generáló
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        bool isEnglish = false;
        Window Window => Application.Current.MainWindow;
        /// <summary>
        /// Megadja, hogy hány termet tartamaz a térkép
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>Termek száma</returns>
        static int GetRoomNumber(char[,] map)
        {
            return -1;
        }
        static int GetPassagesNumber(char[,] map)
        {
            return -1;
        }
        static int GetEmptySpaceNumber(char[,] map)
        {
            return -1;
        }
        /// <summary>
        /// A kapott térkép széleit végignézve megállapítja, hogy hány kijárat van.
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>Az alkalmas kijáratok száma</returns>
        static int GetSuitableEntrance(char[,] map)
        {
            return -1;
        }
        /// <summary>
        /// Megnézi, hogy van-e a térképen meg nem engedett karakter?
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>true - A térkép tartalmaz szabálytalan karaktert, false - nincs benne ilyen</returns>
        static bool IsInvalidElement(char[,] map)
        {
            return true;
        }
        /// <summary>
        /// Visszaadja azoknak a járatkaraktereknek a pozícióját, amelyekhez egyetlen szomszéd pozícióból sem lehet eljutni.
        /// </summary>
        /// <param name="map">Labirintus mátrixa</param>
        /// <returns>A pozíciók "sor_index:oszlop_index" formátumban szerepelnek a lista elemeiként
        static List<string> GetUnavailableElements(char[,] map)
        {
            List<string> unavailables = new List<string>();
            // ?
            // pld: string poz = "4:12"; 
            return unavailables;
        }
        /// <summary>
        /// Labiritust generál a kapott pozíciókat tartalmazó lista alapján. A lista elemei egymáshoz kapcsolódó járatok pozíciói.
        /// </summary>
        /// <param name="positionsList">"sor_index:oszlop_index" formátumban az egymáshoz kapcsolódó járatok pozícióit tartalmazó lista </param>
        /// <returns>A létrehozott labirintus térképe</returns>
        static char[,] GenerateLabyrinth(List<string> positionsList)
        {
            return null;
        }

        private void HeightSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbHeight.Content = $"{lbHeight.Content.ToString().Split(':')[0]}: {HeightSlider.Value}";
        }

        private void WidthSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            lbWidth.Content = $"{lbWidth.Content.ToString().Split(':')[0]}: {WidthSlider.Value}";
        }

        private void GenerateButton_Click(object sender, RoutedEventArgs e)
        {
            //TODO: A gomb megnyomására a labirintus generálása történjen meg, majd a kapott térkép alapján a többi információ is frissüljön.
            lbGeneratedRoomNumber.Content = $"{lbGeneratedRoomNumber.Content.ToString().Split(':')[0]}: {GetRoomNumber(null)}";
            lbGeneratedPassages.Content = $"{lbGeneratedPassages.Content.ToString().Split(':')[0]}: {GetPassagesNumber(null)}";
            lbGeneratedEmptySpace.Content = $"{lbGeneratedEmptySpace.Content.ToString().Split(':')[0]}: {GetEmptySpaceNumber(null)}";
            GenerateLabyrinth(null);
        }

        private void btnLanguageChange_Click(object sender, RoutedEventArgs e)
        {
            if (isEnglish)
            {
                Window.Title = "Labirintus generáló 2000";
                btnLanguageChange.Content = "Angol nyelvre váltás";
                lbHeight.Content = $"Labirintus magassága: {HeightSlider.Value}";
                lbWidth.Content = $"Labirintus szélessége: {WidthSlider.Value}";
                btnGenerate.Content = "Labirintus generálása";
                lbGeneratedRoomNumber.Content = $"Generált termek száma: {GetRoomNumber(null)}";
                lbGeneratedPassages.Content = $"Generált járatok száma: {GetPassagesNumber(null)}";
                lbGeneratedEmptySpace.Content = $"Generált üres terek száma: {GetEmptySpaceNumber(null)}";
                isEnglish = false;                
            }
            else
            {
                Window.Title = "Labyrinth generator 2000";
                btnLanguageChange.Content = "Change language to hungarian";
                lbHeight.Content = $"Labirynth height: {HeightSlider.Value}";
                lbWidth.Content = $"Labirynth width: {WidthSlider.Value}";
                btnGenerate.Content = "Generate labirynth";
                lbGeneratedRoomNumber.Content = $"Number of generated rooms: {GetRoomNumber(null)}";
                lbGeneratedPassages.Content = $"Number of generated passages: {GetPassagesNumber(null)}";
                lbGeneratedEmptySpace.Content = $"Number of generated empty spaces: {GetEmptySpaceNumber(null)}";
                isEnglish = true;
            }
        }
    }
}