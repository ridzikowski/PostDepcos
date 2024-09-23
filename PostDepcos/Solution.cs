using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    internal class Solution
    {
        public int crit1 {  get; set; }
        public int crit2 { get; set; }
        public List<int> pi {  get; set; }

        public override string ToString()
        {
            return $"F1: {crit1} F2: {crit2}" + String.Join(" ", pi);
        }
    }
}
