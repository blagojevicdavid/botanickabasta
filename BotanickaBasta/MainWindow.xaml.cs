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
using System;
using System.Text.RegularExpressions;


namespace BotanickaBasta
{

    public partial class MainWindow : Window
    {

        // Desni panel – bastovani
        public ObservableCollection<Bastovan> Bastovani { get; } = new();
        private const string BASTOVANI = @"..\..\..\podaci\bastovani.txt";
        private Bastovan? _editingTarget = null; // null=Dodaj, !=null=Izmena



        // Leva mapa – sekcije i markeri
        public ObservableCollection<MapSekcija> MapSekcije { get; } = new();
        private const string SEKCIJE = @"..\..\..\podaci\sekcija.txt";
        private MapSekcija? _editingSekcija = null;


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
            ucitajBastovane(BASTOVANI);
            LoadSekcijeFromFile(SEKCIJE);

            // ===== Test podaci: sekcije + markeri na mapi =====
            //var s1 = new Sekcija("Staklenik", 30, "Staklena bašta");
            //var s2 = new Sekcija("Alpinetum", 20, "Planinske vrste");
            //var s3 = new Sekcija("Rosarium", 50, "Ružičnjak");
            //var s4 = new Sekcija("Arboretum", 80, "Drvenaste vrste");

            // Oblasti (X,Y,Š,V) + Marker (Labela, X, Y) — u koordinatama platna (npr. 800x600)
            //MapSekcije.Add(new MapSekcija(s1, 20, 20, 260, 160, new Marker("S1", 20 + 120, 20 + 70)));
            //MapSekcije.Add(new MapSekcija(s2, 320, 20, 420, 120, new Marker("A", 320 + 200, 20 + 50)));
            //MapSekcije.Add(new MapSekcija(s3, 20, 220, 260, 160, new Marker("R", 20 + 115, 220 + 70)));
            //MapSekcije.Add(new MapSekcija(s4, 300, 220, 480, 240, new Marker("AR", 300 + 220, 220 + 95)));
            #endregion

            #region TAB 1
            ucitajFajlove(ULAZ);
            foreach (var b in biljke)
                b.PropertyChanged += Biljka_PropertyChanged;

            viewSource.Source = biljke;
            viewSource.Filter += FilterStranice;
            biljkeDG.ItemsSource = viewSource.View;
            PrikaziStranicu(1);
            #endregion

            #region TAB 2



            #endregion

        }



        #region TAB1

        //-------------------------------------------------------------------------
        const string ULAZ = @"..\..\..\podaci\ulaz.txt";
        const string IZLAZ = @"..\..\..\podaci\izlaz.csv";
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
                using (StreamWriter sw = new StreamWriter(IZLAZ, false, Encoding.UTF8))
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
        #endregion


        #region TAB2
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
            // skini highlight sa prethodne
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

            // postavi novu selekciju
            _selektovana = s;

            if (s != null)
            {
                // upali highlight na novoj
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

            // ⬇️ KLJUČNO: odmah osveži detalje i dugmad na PRVI klik
            UpdateSekcijaDetails(_selektovana);
            UpdateButtons();

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

       
        private void BastovaniGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
            Edit.IsEnabled = true;
            Delete.IsEnabled = true;
        }

        private void BiljkeListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateButtons();
        }

        private void IzmeniSekciju_Click(object sender, RoutedEventArgs e)
        {
            if (_selektovana == null) return;
            MessageBox.Show($"Izmeni sekciju: {_selektovana.Sekcija?.Naziv} – nije još implementirano.");
        }

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

        
        private void UpdateButtons()
        {
            bool hasSekcija = _selektovana != null;
            bool hasBiljka = (BiljkeListBox?.SelectedItem as Biljka) != null;

            if (btnEditSekcija != null) btnEditSekcija.IsEnabled = (_selektovana != null);

            if (btnAssign != null) btnAssign.IsEnabled = hasSekcija && hasBiljka;
            if (btnRemoveFromSekcija != null) btnRemoveFromSekcija.IsEnabled = hasSekcija;
            if (btnUndo != null) btnUndo.IsEnabled = false;

            if (btnEditSekcija != null) btnEditSekcija.IsEnabled = hasSekcija;

        }

        private void UpdateSekcijaDetails(MapSekcija s)
        {
            if (SekcijaNazivVal == null) return; 

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
        private void btnEditSekcija_Click(object sender, RoutedEventArgs e)
        {
            if (_selektovana == null)
            {
                MessageBox.Show("Najpre izaberite sekciju na mapi.");
                return;
            }

            _editingSekcija = _selektovana;
            PopupSekcijaTitle.Text = $"Izmeni sekciju: {_editingSekcija.Sekcija?.Naziv}";

            txtSekcijaNaziv.Text = _editingSekcija.Sekcija?.Naziv ?? "";
            txtSekcijaOpis.Text = _editingSekcija.Sekcija?.Opis ?? "";
            txtKapacitet.Text = (_editingSekcija.Sekcija?.KapacitetMax ?? 0).ToString();

            popupSekcija.IsOpen = true;
        }

        private static string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input)) return input;
            var norm = input.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder(input.Length);
            foreach (var ch in norm)
                if (CharUnicodeInfo.GetUnicodeCategory(ch) != UnicodeCategory.NonSpacingMark)
                    sb.Append(ch);
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }

        private static string GetMarkerLabel(string naziv)
        {
            if (string.IsNullOrWhiteSpace(naziv)) return "?";
            if (string.IsNullOrWhiteSpace(naziv)) return "?";
            var letters = new string(RemoveDiacritics(naziv).Where(char.IsLetter).ToArray());
            if (letters.Length == 0) return "?";
            if (letters.Length == 1) return letters.ToUpperInvariant();
            return letters.Substring(0, 2).ToUpperInvariant();
        }


        private void LoadSekcijeFromFile(string putanja)
        {
            MapSekcije.Clear();
            StreamReader sr = null;
            try
            {
                sr = new StreamReader(putanja, Encoding.UTF8);
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var p = line.Split(',');
                    if (p.Length < 10) continue;

                    string naziv = p[0].Trim();
                    if (!int.TryParse(p[1], out int kap)) continue;
                    string opis = p[2].Trim();

                    if (!int.TryParse(p[3], out int x)) continue;
                    if (!int.TryParse(p[4], out int y)) continue;
                    if (!int.TryParse(p[5], out int w)) continue;
                    if (!int.TryParse(p[6], out int h)) continue;

                    // p[7] = marker label iz fajla (ignorišemo ga, generisaćemo)
                    if (!int.TryParse(p[8], out int mx)) continue;
                    if (!int.TryParse(p[9], out int my)) continue;

                    var s = new Sekcija(naziv, kap, opis);
                    var label = GetMarkerLabel(naziv); // uvek iz naziva
                    var ms = new MapSekcija(s, x, y, w, h, new Marker(label, mx, my));
                    MapSekcije.Add(ms);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri učitavanju sekcija: " + ex.Message);
            }
            finally
            {
                sr?.Close();
            }
        }

        private void SaveSekcijeToFile()
        {
            try
            {
                using var sw = new StreamWriter(SEKCIJE, false, Encoding.UTF8);
                for (int i = 0; i < MapSekcije.Count; i++)
                {
                    var ms = MapSekcije[i];
                    var s = ms.Sekcija;
                    var label = GetMarkerLabel(s.Naziv);
                    sw.Write($"{s.Naziv},{s.KapacitetMax},{s.Opis},{(int)ms.X},{(int)ms.Y},{(int)ms.Sirina},{(int)ms.Visina},{label},{(int)ms.Marker.X},{(int)ms.Marker.Y}");
                    if (i < MapSekcije.Count - 1) sw.WriteLine();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri upisu sekcija: " + ex.Message);
            }
        }
        private void popupSekcija_Opened(object? sender, EventArgs e)
        {
            txtSekcijaNaziv.Focus();
        }
        private void SekcijaBtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (_editingSekcija == null) { popupSekcija.IsOpen = false; return; }

            if (string.IsNullOrWhiteSpace(txtSekcijaNaziv.Text) ||
                string.IsNullOrWhiteSpace(txtKapacitet.Text))
            {
                MessageBox.Show("Popunite Naziv i Kapacitet.");
                return;
            }

            if (!int.TryParse(txtKapacitet.Text.Trim(), out int kap) || kap <= 0)
            {
                MessageBox.Show("Kapacitet mora biti ceo broj > 0.");
                return;
            }

            _editingSekcija.Sekcija.Naziv = txtSekcijaNaziv.Text.Trim();
            _editingSekcija.Sekcija.Opis = txtSekcijaOpis.Text.Trim();
            _editingSekcija.Sekcija.KapacitetMax = kap;

            _editingSekcija.Marker.Labela = GetMarkerLabel(_editingSekcija.Sekcija.Naziv);

            SekcijeItems.Items.Refresh();
            MarkeriItems.Items.Refresh();
            UpdateSekcijaDetails(_editingSekcija);

            SaveSekcijeToFile();

            popupSekcija.IsOpen = false;
        }
        private void SekcijaBtnCancel_Click(object sender, RoutedEventArgs e)
        {
            popupSekcija.IsOpen = false;
            _editingSekcija = null;
        }




        #endregion

        #region POPUP za izmenu bastovana

        private void Add_Click(object sender, RoutedEventArgs e)
        {
            _editingTarget = null;
            PopupTitle.Text = "Dodaj baštovana";
            txtIme.Text = "";
            txtPrezime.Text = "";
            txtZaposlenickiId.Text = "";
            txtTelefon.Text = "";

            popupBastovan.IsOpen = true;

        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            if (BastovaniGrid.SelectedItem is not Bastovan sel)
            {
                MessageBox.Show("Selektujte baštovana.");
                return;
            }

            _editingTarget = sel;

            PopupTitle.Text = "Izmeni baštovana";

            txtIme.Text = sel.Ime;
            txtPrezime.Text = sel.Prezime;
            txtZaposlenickiId.Text = sel.Id.ToString();
            txtTelefon.Text = sel.Telefon;

            popupBastovan.IsOpen = true;

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (BastovaniGrid.SelectedItem is not Bastovan sel)
            {
                MessageBox.Show("Selektujte baštovana.");
                return;
            }

            var potvrda = MessageBox.Show(
                $"Obrisati baštovana: {sel.Ime} {sel.Prezime} (ID: {sel.Id})?",
                "Potvrda brisanja", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (potvrda != MessageBoxResult.Yes) return;

            Bastovani.Remove(sel);
            SaveBastovaniToFile();
        }

        private void popupBastovan_Opened(object? sender, EventArgs e)
        {
            txtIme.Focus();

        }
        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            popupBastovan.IsOpen = false;
        }
        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtIme.Text) ||
                string.IsNullOrWhiteSpace(txtPrezime.Text) ||
                string.IsNullOrWhiteSpace(txtZaposlenickiId.Text) ||
                string.IsNullOrWhiteSpace(txtTelefon.Text))
            {
                MessageBox.Show("Popunite sva obavezna polja (Ime, Prezime, ID, Telefon).",
                                "Provera", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }


            if (!int.TryParse(txtZaposlenickiId.Text.Trim(), out int idVal) || idVal <= 0)
            {
                MessageBox.Show("ID mora biti ceo broj veći od nule.",
                                "Provera", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtZaposlenickiId.Focus();
                return;
            }

            if (IsDuplicateId(idVal))
            {
                MessageBox.Show("Ne može postojati baštovan sa istim ID-om.",
                                "Provera", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtZaposlenickiId.Focus();
                return;
            }

            if (!IsPhoneValid(txtTelefon.Text))
            {
                MessageBox.Show("Unesite validan broj telefona (dozvoljeni +, cifre, razmaci, crtice, zagrade; 6–15 cifara).",
                                "Provera", MessageBoxButton.OK, MessageBoxImage.Warning);
                txtTelefon.Focus();
                return;
            }

            if (_editingTarget != null)
            {
                // izmena postojećeg
                _editingTarget.Ime = txtIme.Text.Trim();
                _editingTarget.Prezime = txtPrezime.Text.Trim();
                _editingTarget.Id = idVal;
                _editingTarget.Telefon = txtTelefon.Text.Trim();
            }
            else
            {
                // dodavanje novog
                Bastovani.Add(new Bastovan(
                    ime: txtIme.Text.Trim(),
                    prezime: txtPrezime.Text.Trim(),
                    id: idVal,
                    telefon: txtTelefon.Text.Trim()
                ));
            }

            BastovaniGrid.Items.Refresh();

            SaveBastovaniToFile();

            popupBastovan.IsOpen = false;
        }

        private void ucitajBastovane(string putanja)
        {
            StreamReader sr = null;
            try
            {
                string linija;
                sr = new StreamReader(putanja);
                while ((linija = sr.ReadLine()) != null)
                { 
                    string[] delovi = linija.Split(',');//string ime, string prezime, int id, string telefon
                    string ime = delovi[0];
                    string prezime = delovi[1]; //Petar, Peric, 123, 064646232
                    int id = int.Parse(delovi[2]);
                    string telefon = delovi[3];

                    Bastovan bastovan = new Bastovan(ime, prezime, id, telefon);
                    Bastovani.Add(bastovan);
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


        private static bool IsPhoneValid(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return false;
            if (!Regex.IsMatch(input, @"^\+?[0-9\s\-\(\)]+$"))
                return false;

            int digits = input.Count(char.IsDigit);
            return digits >= 6 && digits <= 15;
        }

        private bool IsDuplicateId(int id)
        {
            return Bastovani.Any(b => b.Id == id && !ReferenceEquals(b, _editingTarget));
        }
        private void SaveBastovaniToFile()
        {
            try
            {
                using var sw = new StreamWriter(BASTOVANI, false, Encoding.UTF8);
                for (int i = 0; i < Bastovani.Count; i++)
                {
                    sw.Write(Bastovani[i].ToString()); // mora biti Ime,Prezime,Id,Telefon
                    if (i < Bastovani.Count - 1) sw.WriteLine();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Greška pri upisu baštovana: " + ex.Message);
            }
        }




        #endregion

    }


    #endregion

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
