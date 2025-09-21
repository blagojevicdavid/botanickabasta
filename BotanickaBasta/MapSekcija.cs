using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public class MapSekcija
    {
        private Sekcija sekcija;     // model po PDF-u
        private double x;            // Canvas.Left (položaj oblasti)
        private double y;            // Canvas.Top
        private double sirina;
        private double visina;
        private Marker marker;       // oznaka/marker sekcije

        public MapSekcija()
        {
            this.sekcija = new Sekcija();
            this.x = 0;
            this.y = 0;
            this.sirina = 0;
            this.visina = 0;
            this.marker = new Marker();
        }
        public ObservableCollection<Biljka> BiljkeUSekciji { get; } = new ObservableCollection<Biljka>();

        public MapSekcija(Sekcija sekcija, double x, double y, double sirina, double visina, Marker marker)
        {
            this.sekcija = sekcija;
            this.x = x;
            this.y = y;
            this.sirina = sirina;
            this.visina = visina;
            this.marker = marker;
        }

        public Sekcija Sekcija { get { return sekcija; } set { sekcija = value; } }
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public double Sirina { get { return sirina; } set { sirina = value; } }
        public double Visina { get { return visina; } set { visina = value; } }
        public Marker Marker { get { return marker; } set { marker = value; } }
    }
}

