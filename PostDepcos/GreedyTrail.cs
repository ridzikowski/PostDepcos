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
            List<string> param = new List<string>();

            var ns = Enumerable.Range(1, 5).Select(x => x * 500).ToList();
            var vs = new List<double>() { 0.05, 0.1, 0.2 };
            var ls = new List<int>() { 75, 150, 300 };
            Random random = new Random(1);
            for (int seed = 0; seed < number; seed++)
            {
                int it = 0;
                int n = ns[random.Next(ns.Count)];
                double vr = vs[random.Next(vs.Count)];
                int v = Math.Round(n * vr);
                int l = ls[random.Next(ls.Count)];

                Instance instance = new Instance(n, v, l, seed);
                List<int> values = new List<int>() { -1, 0, 1 };
                List<int> values2 = new List<int>() { -1, 1 };
                

                List<List<Solution>> fronts = new List<List<Solution>>();
                foreach (int a in values)
                    foreach (int d in values)
                        foreach (int w in values)
                            foreach (int p in values)
                                foreach (int dma in values)
                                    {
                                        Greedy.af = a;
                                        Greedy.df = d;
                                        Greedy.wf = w;
                                        Greedy.pf = p;
                                        Greedy.dmaf = dma;
                                        Greedy greedy = new Greedy();
                                        var front = greedy.run(instance, Greedy.SortBySilly);

                                        fronts.Add(front);
                                        //Console.WriteLine($"it: {it}, af: {a}, df: {d}, wf: {w}, pf: {p}, dmaf: {dma}, rev: {rev}");
                                        if (seed == 0)
                                            param.Add($"{a} {d} {w} {p} {dma}");
                                        it++;
                                        
                                    }
                var array = instance.hvis(fronts);
                var max = array.Max();
                array = array.Select(x =>  x/max).ToList();
                for (int i = 0; i < array.Count; i++) avg[i] += array[i];
                //Console.WriteLine(String.Join("\n", array));
                Console.Write($"{seed} ");
            }
            avg = avg.Select(x => x / number).ToArray();
            //Console.WriteLine(string.Join("\n", avg));
            var maxHVI = avg.Max();
            var idx = Array.IndexOf(avg, maxHVI);
            Console.WriteLine(idx);

            var sorted = avg.Select((value, index) => new { value, index }).OrderBy(x => x.value).ToArray();
            
            var sortedParam = sorted.Select(x => param[x.index]).ToArray();

            for (int i = 0; i < sortedParam.Length; i++)
                Console.WriteLine($"{sorted[i].index} {sorted[i].value} {sortedParam[i]}");


        }

    }
}
