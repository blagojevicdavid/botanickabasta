using System;

namespace BotanickaBasta
{
    public class Sekcija
    {
        private string naziv;
        private string opis;
        private int kapacitetMax;

        public Sekcija()
        {
            this.naziv = string.Empty;
            this.opis = string.Empty;
            this.kapacitetMax = 0;
        }

        public Sekcija(string naziv, int kapacitetMax, string opis = "")
        {
            this.naziv = naziv;
            this.kapacitetMax = kapacitetMax;
            this.opis = opis ?? "";
        }

        public string Naziv { get { return naziv; } set { naziv = value; } }
        public string Opis { get { return opis; } set { opis = value; } }
        public int KapacitetMax { get { return kapacitetMax; } set { kapacitetMax = value; } }
    }
}
