using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public class Dodela
    {
        private Biljka biljka;
        private Sekcija sekcija;

        public Dodela() { }

        public Dodela(Biljka biljka, Sekcija sekcija)
        {
            this.biljka = biljka;
            this.sekcija = sekcija;
        }

        public Biljka Biljka { get { return biljka; } set { biljka = value; } }
        public Sekcija Sekcija { get { return sekcija; } set { sekcija = value; } }
    }
}

