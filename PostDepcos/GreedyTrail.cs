using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PostDepcos
{
    internal class GreedyTrail
    {

        public GreedyTrail() {

            int number = 1000;
            double[] avg = new double[486];
            for (int seed = 0; seed < number; seed++)
            {
                int it = 0;
                Instance instance = new Instance(20, 50, 20, 1, 10, 120);
                List<int> values = new List<int>() { -1, 0, 1 };
                List<int> values2 = new List<int>() { -1, 1 };

                List<List<Solution>> fronts = new List<List<Solution>>();
                foreach (int a in values)
                    foreach (int d in values)
                        foreach (int w in values)
                            foreach (int p in values)
                                foreach (int dma in values)
                                    foreach (int rev in values2)
                                    {
                                        Greedy.af = a;
                                        Greedy.df = d;
                                        Greedy.wf = w;
                                        Greedy.pf = p;
                                        Greedy.dmaf = dma;
                                        Greedy.rev = rev;
                                        Greedy greedy = new Greedy();
                                        var front = greedy.run(instance, Greedy.SortBySilly);

                                        fronts.Add(front);
                                        Console.WriteLine($"it: {it}, af: {a}, df: {d}, wf: {w}, pf: {p}, dmaf: {dma}, rev: {rev}");
                                        it++;
                                    }
                var array = instance.hvis(fronts);
                var max = array.Max();
                array = array.Select(x =>  x/max).ToList();
                for (int i = 0; i < array.Count; i++) avg[i] += array[i];
                //Console.WriteLine(String.Join("\n", array));
            }
            avg = avg.Select(x => x / number).ToArray();
            //Console.WriteLine(string.Join("\n", avg));
            var maxHVI = avg.Max();
            var idx = Array.IndexOf(avg, maxHVI);
            Console.WriteLine(idx);
        }

    }
}
