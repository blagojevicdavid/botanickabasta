using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
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
        // (možeš ukloniti ako ne koristiš)
        private List<Biljka> biljka;
        private List<Bastovan> bastovan;

        // Desni panel – baštovani
        public ObservableCollection<Bastovan> Bastovani { get; } = new();

        // Leva mapa – sekcije i markeri
        public ObservableCollection<MapSekcija> MapSekcije { get; } = new();

        // Trenutno selektovana sekcija na mapi
        private MapSekcija _selektovana;

        public MainWindow()
        {
            InitializeComponent();

            // Centriraj mapu kad se prozor učita (ako koristiš ZoomPanBehavior)
            Loaded += (_, __) => MapZoom.Center();

            // DataContext da XAML Binding radi (Bastovani, MapSekcije)
            DataContext = this;

            // ===== Test podaci: baštovani =====
            var g1 = new Bastovan("Marko", "Petrović", 1, "0601234567");
            var g2 = new Bastovan("Jelena", "Jovanović", 2, "0619876543");

            g1.ZaduzeneBiljke.Add(new Biljka("Ruža"));
            g2.ZaduzeneBiljke.Add(new Biljka("Lala"));
            g2.ZaduzeneBiljke.Add(new Biljka("Zumbul"));

            Bastovani.Add(g1);
            Bastovani.Add(g2);

            // ===== Test podaci: sekcije + markeri na mapi =====
            var s1 = new Sekcija("Staklenik", 30, "Staklena bašta");
            var s2 = new Sekcija("Alpinetum", 20, "Planinske vrste");
            var s3 = new Sekcija("Rosarium", 50, "Ružičnjak");
            var s4 = new Sekcija("Arboretum", 80, "Drvenaste vrste");

            // Oblasti (X,Y,Š,V) + Marker (Labela, X, Y) — u koordinatama platna (npr. 800x600)
            MapSekcije.Add(new MapSekcija(s1, 20, 20, 260, 160, new Marker("S1", 20 + 120, 20 + 70)));
            MapSekcije.Add(new MapSekcija(s2, 320, 20, 420, 120, new Marker("A", 320 + 200, 20 + 50)));
            MapSekcije.Add(new MapSekcija(s3, 20, 220, 260, 160, new Marker("R", 20 + 115, 220 + 70)));
            MapSekcije.Add(new MapSekcija(s4, 300, 220, 480, 240, new Marker("AR", 300 + 220, 220 + 95)));
        }

        // ===================== MAPA: HANDLERI =====================

        /// <summary> Klik na oblast sekcije (pravougaonik ili natpis u sekciji). </summary>
        private void SekcijaArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = (sender as FrameworkElement)?.DataContext as MapSekcija;
            if (s != null) SelectSekcija(s);
            e.Handled = true;
        }

        /// <summary> Klik na marker (bedž). </summary>
        private void Marker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = (sender as FrameworkElement)?.DataContext as MapSekcija;
            if (s != null) SelectSekcija(s);
            e.Handled = true;
        }

        /// <summary> Zajednički selektor: gasi stari highlight, pali novi (i na oblasti i na markeru). </summary>
        private void SelectSekcija(MapSekcija s)
        {
            // 1) skini highlight sa PRETHODNE selekcije
            if (_selektovana != null)
            {
                var oldAreaCP = SekcijeItems.ItemContainerGenerator.ContainerFromItem(_selektovana) as ContentPresenter;
                var oldRect = FindChild<Rectangle>(oldAreaCP, "AreaRect");
                if (oldRect != null)
                {
                    oldRect.Stroke = (Brush)new BrushConverter().ConvertFromString("#5B9BD5");
                    oldRect.StrokeThickness = 2;
                }

                var oldMarkCP = MarkeriItems.ItemContainerGenerator.ContainerFromItem(_selektovana) as ContentPresenter;
                var oldBadge = FindChild<Border>(oldMarkCP, "MarkerBadge");
                if (oldBadge != null)
                    oldBadge.Background = (Brush)new BrushConverter().ConvertFromString("#2D89EF");
            }

            // 2) postavi novu selekciju
            _selektovana = s;

            if (s == null) return;

            // 3) upali highlight na NOVOJ selekciji
            var areaCP = SekcijeItems.ItemContainerGenerator.ContainerFromItem(_selektovana) as ContentPresenter;
            var rect = FindChild<Rectangle>(areaCP, "AreaRect");
            if (rect != null)
            {
                rect.Stroke = Brushes.OrangeRed;
                rect.StrokeThickness = 3;
            }

            var markCP = MarkeriItems.ItemContainerGenerator.ContainerFromItem(_selektovana) as ContentPresenter;
            var badge = FindChild<Border>(markCP, "MarkerBadge");
            if (badge != null)
                badge.Background = Brushes.OrangeRed;

            // (opciono) ažuriraj desni panel detalja ovde…
        }

        /// <summary> Helper: nađi element po imenu unutar DataTemplate visual-tree-a. </summary>
        private T FindChild<T>(DependencyObject parent, string name) where T : FrameworkElement
        {
            if (parent == null) return null;
            int n = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < n; i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T fe && (string.IsNullOrEmpty(name) || fe.Name == name))
                    return fe;
                var hit = FindChild<T>(child, name);
                if (hit != null) return hit;
            }
            return null;
        }
        private void Mapa_ClearSelection(object sender, MouseButtonEventArgs e) //handler za prazan klik na kanvas
        {
            SelectSekcija(null);
            
        }




    }
}
