using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    internal class TabuSearch
    {
        public List<Solution> run(Instance instance, int timeLimit = 5)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            Greedy greedy = new Greedy();
            List<Solution> front = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);

            List<int> F1 = new List<int>(), F2 = new List<int>();
            List<int> I = new List<int>(), J = new List<int>();
            foreach (var sol in front)
            {
                F1.Add(sol.crit1);
                F2.Add(sol.crit2);
            }
            Solution curr = front[instance.TOPSIS(F1, F2)];

            int[,] tabuList = new int[curr.pi.Count, curr.pi.Count];
            //for (int i = 0; i < curr.pi.Count; ++i)
            //{
            //    for (int j = 0; j < curr.pi.Count; ++j)
            //    {
            //        tabuList[i, j] = 0;
            //    }
            //}

            int cadence = (int)(Math.Sqrt(curr.pi.Count));
           
            int iter = 0;
            while (stopwatch.Elapsed.TotalSeconds < timeLimit)
            {
                F1.Clear();
                F2.Clear();
                I.Clear();
                J.Clear();
                for (int i = 1; i < curr.pi.Count - 1; ++i)
                {
                    for (int j = i + 1; j < curr.pi.Count - 1; ++j)
                    {
                        (curr.pi[i], curr.pi[j]) = (curr.pi[j], curr.pi[i]);
                        var result = instance.evaluate(curr.pi);

                        if (result[0] < int.MaxValue && iter >= tabuList[i, j])
                        {
                            F1.Add(result[0]);
                            F2.Add(result[1]);
                            I.Add(i);
                            J.Add(j);
                        }
                        (curr.pi[i], curr.pi[j]) = (curr.pi[j], curr.pi[i]);
                    }
                }
                int pos = instance.TOPSIS(F1, F2);
                //Console.WriteLine($"{I[pos]} {J[pos]}");
                (curr.pi[I[pos]], curr.pi[J[pos]]) = (curr.pi[J[pos]], curr.pi[I[pos]]);
                tabuList[I[pos], J[pos]] = iter + cadence;

                bool dominated = false;
                foreach (var sol in front)
                    if (Instance.dominates(sol.crit1, sol.crit2, F1[pos], F2[pos]) || (sol.crit1 == F1[pos] && sol.crit2 == F2[pos]))
                    {
                        dominated = true;
                        break;

                    }
                if (!dominated)
                {
                    for (int k = 0; k < front.Count; k++)
                        if (Instance.dominates(F1[pos], F2[pos], front[k].crit1, front[k].crit2))
                        {
                            front.RemoveAt(k);
                            k--;
                        }
                    front.Add(new Solution() { crit1 = F1[pos], crit2 = F2[pos], pi = new List<int>(curr.pi) });
                }
                iter++;
            }
            stopwatch.Stop();
            //Console.WriteLine($"Runtime: {stopwatch.Elapsed.TotalSeconds}");
            return front;
        }
    }
}
