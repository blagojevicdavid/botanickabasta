using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public enum TipAkcije
    {
        Dodavanje,   // biljka -> sekcija
        Uklanjanje,  // iz sekcije
        Premestanje  // iz sekcije A u sekciju B
    }

    public class UndoAkcija
    {
        private TipAkcije tip;
        private Biljka biljka;
        private Sekcija izSekcije;  // može biti null za Dodavanje
        private Sekcija uSekciju;   // može biti null za Uklanjanje

        public UndoAkcija() { }

        public UndoAkcija(TipAkcije tip, Biljka biljka, Sekcija izSekcije, Sekcija uSekciju)
        {
            this.tip = tip;
            this.biljka = biljka;
            this.izSekcije = izSekcije;
            this.uSekciju = uSekciju;
        }

        public TipAkcije Tip { get { return tip; } set { tip = value; } }
        public Biljka Biljka { get { return biljka; } set { biljka = value; } }
        public Sekcija IzSekcije { get { return izSekcije; } set { izSekcije = value; } }
        public Sekcija USekciju { get { return uSekciju; } set { uSekciju = value; } }
    }
}

