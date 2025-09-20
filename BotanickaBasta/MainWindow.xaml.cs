using Microsoft.Win32;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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

    public partial class MainWindow : Window
    {

        // Desni panel – bastovani
        public ObservableCollection<Bastovan> Bastovani { get; } = new();

        // Leva mapa – sekcije i markeri
        public ObservableCollection<MapSekcija> MapSekcije { get; } = new();

        // Trenutno selektovana sekcija na mapi
        private MapSekcija _selektovana;

        // BILJKE ---------------------------------------------------------------------------- BILJKE
        private ObservableCollection<Biljka> biljke = new ObservableCollection<Biljka>();
        private CollectionViewSource viewSource = new CollectionViewSource();
        private int trenutnaStranica = 1;
        private int stavkiPoStranici = 16;
        private int ukupnoStranica => (int)Math.Ceiling((double)biljke.Count / stavkiPoStranici);
        // BILJKE ---------------------------------------------------------------------------- BILJKE
        public MainWindow()
        {
            InitializeComponent();

            // Binding
            DataContext = this;

            //Test podaci
            #region testpodaci
            // ===== Test podaci: baštovani =====
            var g1 = new Bastovan("Marko", "Petrović", 1, "0601234567");
            var g2 = new Bastovan("Jelena", "Jovanović", 2, "0619876543");

            g1.ZaduzeneBiljke.Add(new Biljka1("Ruža"));
            g2.ZaduzeneBiljke.Add(new Biljka1("Lala"));
            g2.ZaduzeneBiljke.Add(new Biljka1("Zumbul"));

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
            #endregion

            ucitajFajlove(ULAZ);
            foreach (var b in biljke)
                b.PropertyChanged += Biljka_PropertyChanged;

            viewSource.Source = biljke;
            viewSource.Filter += FilterStranice;
            biljkeDG.ItemsSource = viewSource.View;

            PrikaziStranicu(1);
        }

        // ===================== MAPA HANDLERI ====================
        // Klik na oblast sekcije 
        private void SekcijaArea_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = (sender as FrameworkElement)?.DataContext as MapSekcija;
            if (s != null) SelectSekcija(s);
            e.Handled = true;
        }

        // Klik na marker
        private void Marker_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var s = (sender as FrameworkElement)?.DataContext as MapSekcija;
            if (s != null) SelectSekcija(s);
            e.Handled = true;
        }

        // Zajednički selektor: gasi stari highlight, pali novi (i na oblasti i na markeru).
        private void SelectSekcija(MapSekcija s)
        {
            /// skini highlight sa prethodne selekcije
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

            /// postavi novu selekciju
            _selektovana = s;

            /// prazan klik
            if (s == null) return;

            /// upali highlight na NOVOJ selekciji
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


        }


        // nađi element po imenu unutar DataTemplate visual-tree-a.
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

        // handler za prazan klik na kanvas
        private void Mapa_ClearSelection(object sender, MouseButtonEventArgs e)
        {
            SelectSekcija(null);

        }

        #region Desni panel: handleri i pomoćne metode

        // --- SelectionChanged (DataGrid baštovana / ListBox biljaka) ---
        private void BastovaniGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void BiljkeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        // --- Dugmad: Sekcije ---

        private void IzmeniSekciju_Click(object sender, RoutedEventArgs e)
        {
            if (_selektovana == null) return;
            MessageBox.Show($"Izmeni sekciju: {_selektovana.Sekcija?.Naziv} – nije još implementirano.");
        }


        // --- Dugmad: Raspored biljaka ---
        private void DodeliBiljkuUSekciju_Click(object sender, RoutedEventArgs e)
        {
            var b = BiljkeListBox?.SelectedItem as Biljka1;
            if (_selektovana == null || b == null) return;

            MessageBox.Show($"Dodeli biljku '{b.Naziv}' u sekciju '{_selektovana.Sekcija?.Naziv}' – nije još implementirano.");
            UpdateButtons();
        }

        private void UkloniBiljkuIzSekcije_Click(object sender, RoutedEventArgs e)
        {
            if (_selektovana == null) return;

            MessageBox.Show($"Ukloni biljku iz sekcije '{_selektovana.Sekcija?.Naziv}' – nije još implementirano.");
            UpdateButtons();
        }

        private void PonistiAkciju_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Poništi (Undo) – nije još implementirano.");
            UpdateButtons();
        }

        // --- Pomoćno: enable/disable dugmadi prema selekciji ---
        private void UpdateButtons()
        {
            bool hasSekcija = _selektovana != null;
            bool hasBiljka = (BiljkeListBox?.SelectedItem as Biljka1) != null;

            if (btnAssign != null) btnAssign.IsEnabled = hasSekcija && hasBiljka;
            if (btnRemoveFromSekcija != null) btnRemoveFromSekcija.IsEnabled = hasSekcija;
            if (btnUndo != null) btnUndo.IsEnabled = false;
        }

        // --- Pomoćno: osvežavanje panela "Detalji sekcije" ---
        private void UpdateSekcijaDetails(MapSekcija s)
        {
            if (SekcijaNazivVal == null) return; // XAML još nije spreman?

            if (s == null)
            {
                SekcijaNazivVal.Text = "—";
                SekcijaOpisVal.Text = "—";
                SekcijaKapacitetVal.Text = "—";
            }
            else
            {
                SekcijaNazivVal.Text = s.Sekcija?.Naziv ?? "—";
                SekcijaOpisVal.Text = s.Sekcija?.Opis ?? "—";
                SekcijaKapacitetVal.Text = (s.Sekcija != null) ? s.Sekcija.KapacitetMax.ToString() : "—";
            }
        }

        #endregion



        //-------------------------------------------------------------------------
        const string ULAZ = @"..\..\..\podaci\ulaz.txt";
        private void Biljka_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            SacuvajUFajl();
        }

        private void SacuvajUFajl()
        {
            using (StreamWriter sw = new StreamWriter(ULAZ, false))
            {
                for (int i = 0; i < biljke.Count; i++)
                {
                    sw.Write(biljke[i].ToString());
                    if (i < biljke.Count - 1)
                        sw.WriteLine();
                }
            }
        }

        private void FilterStranice(object sender, FilterEventArgs e)
        {
            Biljka b = e.Item as Biljka;
            if (b != null)
            {
                int index = biljke.IndexOf(b);
                int start = (trenutnaStranica - 1) * stavkiPoStranici;
                int end = start + stavkiPoStranici;
                e.Accepted = index >= start && index < end;
            }
        }
        private void PrikaziStranicu(int stranica)
        {
            if (stranica < 1) stranica = 1;
            if (stranica > ukupnoStranica) stranica = ukupnoStranica == 0 ? 1 : ukupnoStranica;

            trenutnaStranica = stranica;
            viewSource.View.Refresh();
            labelStranica.Content = $"Stranica {trenutnaStranica} / {ukupnoStranica}";
        }

        private void btnPrethodna_Click(object sender, RoutedEventArgs e)
        {
            PrikaziStranicu(trenutnaStranica - 1);
        }

        private void btnSledeca_Click(object sender, RoutedEventArgs e)
        {
            PrikaziStranicu(trenutnaStranica + 1);
        }

        private void ucitajFajlove(string putanja)
        {
            StreamReader sr = null;
            try
            {
                string linija;
                sr = new StreamReader(putanja);
                while ((linija = sr.ReadLine()) != null)
                {
                    string[] delovi = linija.Split(',');
                    int sifra = int.Parse(delovi[0]);
                    //int sifra = biljke.Count;
                    string naucniNaziv = delovi[1];
                    string uobicajeniNaziv = delovi[2];
                    string porodica = delovi[3];
                    string datumNabavke = delovi[4];
                    string lokacija = delovi[5];
                    string status = delovi[6];
                    string slika = delovi[7];

                    Biljka biljka = new Biljka(sifra, naucniNaziv, uobicajeniNaziv, porodica, datumNabavke, lokacija, status, slika);
                    biljke.Add(biljka);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                sr?.Close();
            }
        }

        private void ObrisiBTN_Click(object sender, RoutedEventArgs e)
        {
            if (biljkeDG.SelectedItem is Biljka b)
            {
                for (int i = 0; i < biljke.Count; i++)
                {
                    if (biljke[i].Sifra == b.Sifra)
                    {
                        biljke.RemoveAt(i);
                        break;
                    }
                }
                PrikaziStranicu(trenutnaStranica);
                //SacuvajUFajl();
                using (StreamWriter sw = new StreamWriter(ULAZ, false))
                {
                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Selektujte biljku!");
            }
        }

        private void biljkeDG_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.DataContext = biljkeDG.SelectedItem as Biljka;
        }

        private void promeniSliku_Click(object sender, RoutedEventArgs e)
        {
            if (biljkeDG.SelectedItem is Biljka b)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    string absolutePath = fileDialog.FileName;
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    string relativePath = System.IO.Path.GetRelativePath(appFolder, absolutePath);

                    b.Slika = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
                    b.SlikaPath = relativePath;
                }
                PrikaziStranicu(trenutnaStranica);
                //SacuvajUFajl();
                using (StreamWriter sw = new StreamWriter(ULAZ, false))
                {
                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
            }
            else
            {
                MessageBox.Show("Selektujte biljku!");
            }
        }

        private void DodajBTN_Click(object sender, RoutedEventArgs e)
        {
            int moze = 1;
            int sifra = -1;
            string path = "";
            DateTime d = DateTime.Today;
            string[] razdeljenDatum;

            if (string.IsNullOrWhiteSpace(sifraUnos.Text) || string.IsNullOrWhiteSpace(naucniUnos.Text) ||
                string.IsNullOrWhiteSpace(uobicajeniUnos.Text) || string.IsNullOrWhiteSpace(porodicaUnos.Text) ||
                string.IsNullOrWhiteSpace(datumUnos.Text) || string.IsNullOrWhiteSpace(lokacijaUnos.Text) ||
                string.IsNullOrWhiteSpace(statusUnos.Text))
            {
                moze = 0;
                MessageBox.Show("Popunite sva polja!");
            }

            if (moze == 1)
            {
                if (!int.TryParse(sifraUnos.Text, out sifra))
                {
                    moze = 0;
                    MessageBox.Show("Sifra moze da sadrzi samo brojeve!");
                }
            }

            if (moze == 1)
            {
                foreach (Biljka biljka in biljke)
                {
                    if (biljka.Sifra == sifra)
                    {
                        MessageBox.Show("Postoji biljka sa sifrom!");
                        moze = 0;
                        break;
                    }
                }
            }

            if (moze == 1)
            {
                try
                {
                    string separator = datumUnos.Text.Contains('.') ? "." :
                                       datumUnos.Text.Contains('/') ? "/" : " ";
                    razdeljenDatum = datumUnos.Text.Split(separator.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    DateTime d1 = new DateTime(int.Parse(razdeljenDatum[2]), int.Parse(razdeljenDatum[1]), int.Parse(razdeljenDatum[0]));
                    if (d1 > DateTime.Today)
                    {
                        moze = 0;
                        MessageBox.Show("Ne mozete uneti datum iz buducnosti!");
                    }
                    else
                    {
                        d = d1;
                    }
                }
                catch (Exception ex)
                {
                    moze = 0;
                    MessageBox.Show("Nevalidan datum: " + ex.Message);
                }
            }

            if (moze == 1)
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "Slike|*.jpg;*.jpeg;*.png;*.bmp;*.gif";
                bool? success = fileDialog.ShowDialog();
                if (success == true)
                {
                    string absolutePath = fileDialog.FileName;
                    string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                    path = System.IO.Path.GetRelativePath(appFolder, absolutePath);
                }

                Biljka b = new Biljka(sifra, naucniUnos.Text, uobicajeniUnos.Text, porodicaUnos.Text, d.ToString("dd/MM/yyyy"), lokacijaUnos.Text, statusUnos.Text, path);
                biljke.Add(b);
                b.PropertyChanged += Biljka_PropertyChanged;
                PrikaziStranicu(ukupnoStranica);
                sifraUnos.Text = naucniUnos.Text = uobicajeniUnos.Text = porodicaUnos.Text = datumUnos.Text = lokacijaUnos.Text = statusUnos.Text = "";

                try
                {
                    using (StreamWriter sw = new StreamWriter(ULAZ, false))
                    {
                        for (int i = 0; i < biljke.Count; i++)
                        {
                            sw.Write(biljke[i].ToString());
                            if (i < biljke.Count - 1)
                                sw.WriteLine();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
        }

        private void eksportBTN_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter("izlaz.csv", false, Encoding.UTF8))
                {
                    sw.WriteLine("Sifra,NaucniNaziv,UobicajeniNaziv,Porodica,DatumNabavke,Lokacija,Status,SlikaPath");

                    foreach (Biljka biljka in biljke)
                    {
                        sw.WriteLine(biljka.ToString());
                    }
                }
                MessageBox.Show("Eksport uspešan!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška prilikom eksportovanja: " + ex.Message);
            }
        }

        private void sifraUnos_LostFocus(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(sifraUnos.Text, out int novaSifra))
            {
                if (biljke.Any(b => b != biljkeDG.SelectedItem && b.Sifra == novaSifra))
                {
                    MessageBox.Show("Već postoji biljka sa ovom šifrom!");
                    sifraUnos.Text = "";
                    sifraUnos.Focus();
                }
            }
        }

        private void biljkeDG_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // Dohvati element na koji je kliknuto
            var dep = (DependencyObject)e.OriginalSource;

            // Penji se kroz vizualno stablo dok ne nađeš DataGridRow ili null
            while (dep != null && !(dep is DataGridRow))
            {
                dep = VisualTreeHelper.GetParent(dep);
            }

            // Ako nije DataGridRow, znači da je kliknuto u prazno
            if (dep == null)
            {
                biljkeDG.UnselectAll();
                biljkeDG.SelectedItem = null;
            }
        }

    }

    public class DatumValidationRule : ValidationRule
    {
        public override ValidationResult Validate(object value, CultureInfo cultureInfo)
        {
            string s = value as string;
            if (string.IsNullOrWhiteSpace(s))
                return new ValidationResult(false, "Datum je obavezan");

            DateTime temp;
            string[] formati = new[]
            {
        "d.M.yyyy",
        "dd.MM.yyyy",
        "d/M/yyyy",
        "dd/MM/yyyy",
        "d M yyyy",
        "dd MM yyyy"
    };

            if (!DateTime.TryParseExact(s, formati, cultureInfo, DateTimeStyles.None, out temp))
                return new ValidationResult(false, "Nevalidan format datuma");

            if (temp > DateTime.Today)
                return new ValidationResult(false, "Datum ne može biti u budućnosti");

            return ValidationResult.ValidResult;
        }
    }

}
