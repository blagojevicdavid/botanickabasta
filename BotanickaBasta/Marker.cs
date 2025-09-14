using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BotanickaBasta
{
    public class Marker
    {
        private string labela;      // npr. "1", "A", "R1"
        private double x;           // Canvas.Left
        private double y;           // Canvas.Top
        private string ikonicaRel;  // relativna putanja do male ikonice (opciono)

        public Marker()
        {
            this.labela = "";
            this.x = 0;
            this.y = 0;
            this.ikonicaRel = "";   // prazno = bez ikonice
        }

        public Marker(string labela, double x, double y, string ikonicaRel = "")
        {
            this.labela = labela;
            this.x = x;
            this.y = y;
            this.ikonicaRel = ikonicaRel ?? "";
        }

        public string Labela { get { return labela; } set { labela = value; } }
        public double X { get { return x; } set { x = value; } }
        public double Y { get { return y; } set { y = value; } }
        public string IkonicaRel { get { return ikonicaRel; } set { ikonicaRel = value; } }
    }
}

