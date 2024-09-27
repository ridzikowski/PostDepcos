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

        public Solution() {
            pi = new List<int>();
            crit1 = 0;
            crit2 = 0;
        }

        public Solution(Solution solution) {
            pi = new List<int>(solution.pi);
            crit1 = solution.crit1;
            crit2 = solution.crit2;
        }

        public override string ToString()
        {
            return $"F1: {crit1} F2: {crit2} pi: " + String.Join(" ", pi);
        }
    }
}
