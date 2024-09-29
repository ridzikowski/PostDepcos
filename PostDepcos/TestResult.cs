﻿using System;
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
        public Algorithm algorithm { get; set; }
        public List<Solution>? front { get; set; }
        public double? hvi { get; set; }
        public double? hvi_full { get; set; }
        public string path { get; set; }

        public override string ToString()
        {
            return $"{n} {v} {l} {seed} {algorithm} {hvi} {hvi_full}";
        }
    }

    
}
