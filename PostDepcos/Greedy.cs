using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PostDepcos
{
    delegate int[] SortFunction(Instance instance);
    internal class Greedy
    {
        public static int af = 0;
        public static int df = 0;
        public static int wf = 0;
        public static int pf = -1;
        public static int dmaf = 1;
        public static int rev = 1;


        public static int[] SortDiffDeadlinesAndArrivalByPiorities(Instance inst)
        {
            double[] array = new double[inst.n];
            for (int i = 0; i < inst.n; ++i) array[i] = (inst.deadlines[i] - inst.arrivals[i])/inst.priorities[i];

            var sorted = array.Select((x, index) => new { x, index }).OrderBy(y => y.x).ToArray();
            int[] orders = Enumerable.Range(0, inst.n).ToArray();
            return sorted.Select(x => orders[x.index]).ToArray();
        }

        public static int[] SortBySilly(Instance inst)
        {
            double[] array = new double[inst.n];
            for (int i = 0; i < inst.n;++i)
            {
                double an = (af == 1 ? inst.arrivals[i] : 1);
                double dn = (df == 1 ? inst.deadlines[i] : 1);
                double wn = (wf == 1 ? inst.weights[i] : 1);
                double pn = (pf == 1 ? inst.priorities[i] : 1);
                double dman = (dmaf == 1 ? (inst.deadlines[i] - inst.arrivals[i]) : 1);

                double ad = (af == -1 ? inst.arrivals[i] : 1);
                double dd = (df == -1 ? inst.deadlines[i] : 1);
                double wd = (wf == -1 ? inst.weights[i] : 1);
                double pd = (pf == -1 ? inst.priorities[i] : 1);
                double dmad = (dmaf == -1 ? (inst.deadlines[i] - inst.arrivals[i]) : 1);

                array[i] = (an * dn * wn * pn * dman) / (ad * dd * wd * pd * dmad) * rev;
                
            }
            var sorted = array.Select((x, index) => new { x, index }).OrderBy(y => y.x).ToArray();
            int[] orders = Enumerable.Range(0, inst.n).ToArray();
            return sorted.Select(x => orders[x.index]).ToArray();
        }

        public static int[] SortByDeadlinesInc(Instance inst)
        {
            var sorted = inst.deadlines.Select((deadline, index) => new { deadline, index }).OrderBy(x => x.deadline).ToArray();
            int[] orders = Enumerable.Range(0, inst.n).ToArray();
            return sorted.Select(x => orders[x.index]).ToArray();
        }

        public static int[] SortByDeadlinesDec(Instance inst)
        {
            var sorted = inst.deadlines.Select((deadline, index) => new { deadline, index }).OrderBy(x => x.deadline).ToArray();
            int[] orders = Enumerable.Range(0, inst.n).ToArray();
            return sorted.Select(x => orders[x.index]).Reverse().ToArray();
        }


        public List<Solution> run(Instance instance, SortFunction sortFunction)
        {

            int[]  orders = sortFunction(instance);
            List<int> pi = Enumerable.Repeat(0, instance.v + 1).Select(x => -1).ToList();
            List<int> F1 = new List<int>();
            List<int> F2 = new List<int>();
            List<Solution> front = new List<Solution>();
            for (int o = 0; o < orders.Length; o++)
            {
                int i = orders[o];
                if (o < orders.Length - 1)
                {
                    F1.Clear();
                    F2.Clear();

                    pi.Insert(1, i);
                    var result = instance.evaluate(pi);
                    F1.Add(result[0]);
                    F2.Add(result[1]);
                    for (int j = 1; j < pi.Count - 2; j++)
                    {
                        (pi[j], pi[j + 1]) = (pi[j + 1], pi[j]);
                        result = instance.evaluate(pi);
                        F1.Add(result[0]);
                        F2.Add(result[1]);
                    }
                    int bestPos = instance.TOPSIS(F1, F2) + 1;

                    //double avg = 0;
                    //for (int j = 0; j < F1.Count; j++) avg += (double)F1[j] / F2[j];
                    //avg /= F1.Count;
                    //double bestVal = F1[0] * avg + F2[0];
                    //int bestPos = 1;
                    //for (int j = 1; j < F1.Count; j++)
                    //    if (F1[j] * avg + F2[j] < bestVal)
                    //    {
                    //        bestVal = F1[j] * avg + F2[j];
                    //        bestPos = j+1;
                    //    }
                    pi.RemoveAt(pi.Count - 2);
                    pi.Insert(bestPos, i);
                }
                else
                {
                    for (int j = 1; j < pi.Count - 2; j++)
                    {
                        (pi[j], pi[j + 1]) = (pi[j + 1], pi[j]);
                        bool dominated = false;
                        var result = instance.evaluate(pi);

                        foreach (var sol in front)
                            if (Instance.dominates(sol.crit1, sol.crit2, result[0], result[1]) || (sol.crit1 == result[0] && sol.crit2 == result[1]))
                            {
                                dominated = true;
                                break;

                            }
                        if (!dominated)
                        {
                            for (int k = 0; k < front.Count; k++)
                                if (Instance.dominates(result[0], result[1], front[k].crit1, front[k].crit2))
                                {
                                    front.RemoveAt(k);
                                    k--;
                                }
                            front.Add(new Solution() { crit1 = result[0], crit2 = result[1], pi = new List<int>(pi) });
                        }
                    }
                }

            }
            return front;
        }

    }
}
