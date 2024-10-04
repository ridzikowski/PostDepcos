using System.Diagnostics;
using System.IO;
using static System.Net.Mime.MediaTypeNames;

namespace PostDepcos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            MainExperiment();
        }

        private static void MainExperiment()
        {
            if (!Directory.Exists("outputs-half-time"))
            {
                Directory.CreateDirectory("outputs-half-time");
            }
            var ns = Enumerable.Range(1, 7).Select(x => x * 500).ToList();
            var vs = new List<double>() { 0.05, 0.1, 0.2 };
            var ls = new List<int>() { 75, 150, 300 };
            int number = 10;
            double time = 0.05;
            int seed = 0;
            int id = 0;
            List<TestResult> results = new List<TestResult>();
            foreach (int n in ns)
                foreach (var vr in vs)
                    foreach (var l in ls)
                        for (int i = 0; i < number; i++)
                        {
                            seed++;
                            int v = (int)Math.Round(n * vr);
                            results.Add(new TestResult() { id = id++, n = n, l = l, v = v, seed = seed, path = $"outputs-half-time/n{n}v{v}l{l}s{seed}.txt" });
                        }
            Console.WriteLine($" {Environment.ProcessorCount} cores");
            int numthreads = Math.Min(100, Environment.ProcessorCount);
            ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = numthreads };
            Parallel.For(0, numthreads, opt, idx =>
            {
                for (int v = idx; v < results.Count; v += numthreads)
                {
                    if (File.Exists(results[v].path))
                        continue;
                    int runTimeLimit = (int)Math.Round(results[v].n* time);
                    Instance instance = new Instance(results[v].n, results[v].v, results[v].l, results[v].seed);
                    Greedy greedy = new Greedy();
                    var front_G = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);
                    if (front_G != null && front_G[0].crit1 < int.MaxValue)
                    {
                        List<List<Solution>> fronts = new List<List<Solution>>();
                        fronts.Add(front_G);
                        TabuSearch search = new TabuSearch();
                        var front_TS = search.run(instance, runTimeLimit);
                        fronts.Add(front_TS);
                        results[v].front = front_TS;
                        
                        GeneticAlgortihm geneticAlgortihm = new GeneticAlgortihm();
                        var front_GA = geneticAlgortihm.run(instance, runTimeLimit, 100, 1, crossoverType.order, 3);
                        fronts.Add(front_GA);
                        results[v].front = front_GA;

                        var h = instance.hvis(fronts);
                        results[v].hvi_ratio_G_TS = h[1] / h[0];
                        results[v].hvi_ratio_G_GA = h[2] / h[0];
                        results[v].hvi_G = h[0];
                        results[v].hvi_TS = h[1];
                        results[v].hvi_GA = h[2];
                    }
                    Console.WriteLine(results[v]);
                    File.WriteAllText(results[v].path, results[v].ToString());
                }
            }
            );

        }

        
    }
}
