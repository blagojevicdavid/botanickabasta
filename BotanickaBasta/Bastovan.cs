using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public class Bastovan
    {
        private string ime;
        private string prezime;
        private int id;
        private string telefon;
        private List<Biljka> zaduzeneBiljke;

        public Bastovan (string ime, string prezime, int id, string telefon)
        {
            this.ime = ime;
            this.prezime = prezime;
            this.id = id;
            this.telefon = telefon;
            this.zaduzeneBiljke = new List<Biljka> ();
        }

        public string Ime { get { return ime; } set { ime = value; } }
        public string Prezime { get { return prezime; } set { prezime = value; } }
        public int Id { get { return id; } set { id = value; } }    
        public string Telefon { get { return telefon; } set { telefon = value; } }
        public List<Biljka> ZaduzeneBiljke { get { return zaduzeneBiljke; }  set { zaduzeneBiljke = value; } }




    }
}
