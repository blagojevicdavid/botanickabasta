using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;

namespace BotanickaBasta
{
    internal class Biljka : INotifyPropertyChanged
    {
        private int sifra;
        private string naucniNaziv, uobicajeniNaziv, porodica, datumNabavke, lokacija, status, slikaPath;
        private BitmapImage slika;

        public event PropertyChangedEventHandler? PropertyChanged;

        public Biljka(int sifra, string naucniNaziv, string uobicajeniNaziv, string porodica, string datumNabavke, string lokacija, string status, string slikaPath = null)
        {
            this.sifra = sifra;
            this.naucniNaziv = naucniNaziv;
            this.uobicajeniNaziv = uobicajeniNaziv;
            this.porodica = porodica;
            this.datumNabavke = datumNabavke;
            this.lokacija = lokacija;
            this.status = status;
            this.slikaPath = slikaPath;

            if (!string.IsNullOrEmpty(slikaPath))
            {
                // Kreiranje apsolutnog puta iz relativnog
                string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                string absolutePath = Path.Combine(appFolder, slikaPath);
                if (File.Exists(absolutePath))
                    this.slika = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
            }
        }

        public int Sifra
        {
            get { return sifra; }
            set { if (this.sifra != value) { this.sifra = value; NotifyProperyChanged("Sifra"); } }
        }
        public string NaucniNaziv
        {
            get { return naucniNaziv; }
            set { if (this.naucniNaziv != value) { this.naucniNaziv = value; NotifyProperyChanged("NaucniNaziv"); } }
        }
        public string UobicajeniNaziv
        {
            get { return uobicajeniNaziv; }
            set { if (this.uobicajeniNaziv != value) { this.uobicajeniNaziv = value; NotifyProperyChanged("UobicajeniNaziv"); } }
        }
        public string Porodica
        {
            get { return porodica; }
            set { if (this.porodica != value) { this.porodica = value; NotifyProperyChanged("Porodica"); } }
        }
        public string DatumNabavke
        {
            get { return datumNabavke; }
            set { if (this.datumNabavke != value) { this.datumNabavke = value; NotifyProperyChanged("DatumNabavke"); } }
        }
        public string Lokacija
        {
            get { return lokacija; }
            set { if (this.lokacija != value) { this.lokacija = value; NotifyProperyChanged("Lokacija"); } }
        }
        public string Status
        {
            get { return status; }
            set { if (this.status != value) { this.status = value; NotifyProperyChanged("Status"); } }
        }

        public BitmapImage Slika
        {
            get { return slika; }
            set { if (this.slika != value) { this.slika = value; NotifyProperyChanged("Slika"); } }
        }

        public string SlikaPath
        {
            get { return slikaPath; }
            set
            {
                if (this.slikaPath != value)
                {
                    this.slikaPath = value;
                    // Ažuriranje BitmapImage kada se promeni putanja
                    if (!string.IsNullOrEmpty(slikaPath))
                    {
                        string appFolder = AppDomain.CurrentDomain.BaseDirectory;
                        string absolutePath = Path.Combine(appFolder, slikaPath);
                        if (File.Exists(absolutePath))
                            this.slika = new BitmapImage(new Uri(absolutePath, UriKind.Absolute));
                    }
                    NotifyProperyChanged("SlikaPath");
                    NotifyProperyChanged("Slika");
                }
            }
        }

        private void NotifyProperyChanged(string v)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(v));
        }

        public override string ToString()
        {
            return sifra + "," + naucniNaziv + "," + uobicajeniNaziv + "," + porodica + "," + datumNabavke + "," + lokacija + "," + status + "," + slikaPath;
        }
    }
}
