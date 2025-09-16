using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public class Biljka1
    {
        private string naziv;
        public Biljka1(string naziv) 
        {
            this.naziv = naziv;
        }
        public string Naziv { get { return naziv; } }
    }
}
