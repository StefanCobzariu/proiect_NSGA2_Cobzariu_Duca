using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProiectIA_NSGA2_final
{
    public class Car
    {

        public double price { get; set; }
        public double range { get; set; }
        public double[] objectives { get; set; } = new double[2];
        public double crowdingDistance { get; set; }

        public int rank { get; set; }

        public List<Car> dominatedBy { get; set; }


        public override string ToString()
        {
            return $"Pret: {price}, Autonomie masina : {range}, DistCrowding: {crowdingDistance:F0}";
        }

    }
}
