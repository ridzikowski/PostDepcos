using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    delegate int[] SortFunction(Instance instance);
    internal class Greedy
    {
        int[] orders;
        

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


        public Greedy(Instance instance, SortFunction sortFunction) {

            orders = sortFunction(instance);
            //Console.WriteLine(String.Join(" ", orders));
            //foreach (var order in orders) Console.Write(instance.deadlines[order] + " ");
            //Console.WriteLine();

            List<int> pi = Enumerable.Repeat(0, instance.v + 1).Select(x => -1).ToList();
            List<int> F1 = new List<int>();
            List<int> F2 = new List<int>();
            List<Solution> front = new List<Solution>();
            for (int o =0; o<orders.Length; o++)
            {
                int i = orders[o];
                if (o < orders.Length - 1)
                {
                    F1.Clear();
                    F2.Clear();

                    pi.Insert(1, i);
                    //Console.WriteLine(String.Join(" ", pi));
                    var result = instance.evaluate(pi);
                    F1.Add(result[0]);
                    F2.Add(result[1]);
                    for (int j = 1; j < pi.Count-2; j++)
                    {
                        (pi[j], pi[j+1]) = (pi[j+1], pi[j]);
                        //Console.WriteLine(String.Join(" ", pi));
                        result = instance.evaluate(pi);
                        F1.Add(result[0]);
                        F2.Add(result[1]);
                    }
                
                    double avg = 0;
                    for (int j = 0; j < F1.Count; j++) avg += (double)F1[j] / F2[j];
                    avg /= F1.Count;
                    double bestVal = F1[0] * avg + F2[0];
                    int bestPos = 1;
                    for (int j = 1; j < F1.Count; j++)
                        if (F1[j] * avg + F2[j] < bestVal)
                        {
                            bestVal = F1[j] * avg + F2[j];
                            bestPos = j+1;
                        }
                    //(pi[pi.Count - 2], pi[bestPos]) = (pi[bestPos], pi[pi.Count - 2]);
                    pi.RemoveAt(pi.Count - 2);
                    pi.Insert(bestPos, i);
                    //Console.WriteLine(String.Join(" ", pi));
                }
                else
                {
                    for (int j = 1; j < pi.Count - 2; j++)
                    {
                        (pi[j], pi[j + 1]) = (pi[j + 1], pi[j]);
                        //Console.WriteLine(String.Join(" ", pi));
                        bool dominated = false;
                        var result = instance.evaluate(pi);

                        foreach (var sol in front)
                            if (Instance.dominates(sol.crit1, sol.crit2, result[0], result[1]))
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
            foreach (var sol in front)
                Console.WriteLine(sol);

        }

    }
}
