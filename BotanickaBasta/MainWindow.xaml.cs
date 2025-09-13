using System.Collections.ObjectModel;
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

namespace BotanickaBasta
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private List<Biljka> biljka;
        private List<Bastovan> bastovan;

        public ObservableCollection<Bastovan> Bastovani { get; } = new();

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;   // da {Binding Bastovani} “vidi” kolekciju

            // test podaci
            var g1 = new Bastovan("Marko", "Petrović", 1, "0601234567");
            var g2 = new Bastovan("Jelena", "Jovanović", 2, "0619876543");

            g1.ZaduzeneBiljke.Add(new Biljka("Ruža"));
            g2.ZaduzeneBiljke.Add(new Biljka("Lala"));
            g2.ZaduzeneBiljke.Add(new Biljka("Zumbul"));

            Bastovani.Add(g1);
            Bastovani.Add(g2);

        }

    }
}