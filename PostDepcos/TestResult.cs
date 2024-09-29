using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    enum Algorithm
    {
        G,
        TS,
        GA
    }
    internal class TestResult
    {
        public int id {  get; set; }
        public int n {  get; set; }
        public int l { get; set; }
        public int v { get; set; }
        public int seed { get; set; }
        public Algorithm? algorithm { get; set; }
        public List<Solution>? front { get; set; }
        public double? hvi_ratio_G_TS { get; set; }
        public double? hvi_ratio_G_GA { get; set; }
        public double? hvi_G { get; set; }
        public double? hvi_TS { get; set; }
        public double? hvi_GA { get; set; }
        public string path { get; set; }

        public override string ToString()
        {
            return $"{n} {v} {l} {seed} {hvi_ratio_G_TS} {hvi_ratio_G_GA} {hvi_G} {hvi_TS} {hvi_GA}";
        }
    }

    
}
