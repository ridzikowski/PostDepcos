using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace PostDepcos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //MainExperiment();
            //PreExperiment(10);
            GreedySpeed();
        }

        private static void GreedySpeed()
        {
            var ns = Enumerable.Range(1, 5).Select(x => x *1000).ToList();
            var vs = new List<double>() { 0.05, 0.1, 0.2 };
            foreach(var n in ns) 
                foreach(var v in vs)
                {
                    Instance instance = new Instance(n, (int)Math.Round(n *v),50, 1);
                    Greedy greedy = new Greedy();
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    var front = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);
                    stopwatch.Stop();
                    Console.WriteLine($"n: {n}, v: {v} time: {stopwatch.Elapsed.TotalSeconds} s");
                }
        }

        private static void MainExperiment()
        {
            
        }

        private static void PreExperiment(int runTimeLimit = 5)
        {
            List<int> ns = new List<int>() {1000};
            List<int> vs = new List<int>() {50, 100, 200, 500 };
            int number = 5;
            int seed = 0;
            int id = 0;
            List<TestResult> results = new List<TestResult>();
            foreach(int n in ns) 
                    for(int i = 0; i < number; i++)
                    {
                        seed++;
                        foreach (var v in vs)
                        {
                            results.Add(new TestResult() {id = id++, n = n, v = v, seed = seed, algorithm = Algorithm.TS });
                            results.Add(new TestResult() { id = id++, n = n, v = v, seed = seed, algorithm = Algorithm.GA });
                    }
                    }
            int numthreads = 10;
            ParallelOptions opt = new ParallelOptions() { MaxDegreeOfParallelism = numthreads };
            Parallel.For(0, numthreads, opt, idx =>
            {
                for (int v = idx; v < results.Count; v += numthreads)
                {
                    Instance instance = new Instance(results[v].n, results[v].v, 50, results[v].seed);
                    Greedy greedy = new Greedy();
                    var front_G = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);
                    if (front_G != null && front_G[0].crit1 < int.MaxValue)
                        {
                        List<List<Solution>> fronts = new List<List<Solution>>();
                        fronts.Add(front_G);
                        if (results[v].algorithm == Algorithm.TS)
                        {
                            TabuSearch search = new TabuSearch();
                            var front_TS = search.run(instance, runTimeLimit);
                            fronts.Add(front_TS);
                            results[v].front = front_TS;
                        } else if (results[v].algorithm == Algorithm.GA)
                        {
                            GeneticAlgortihm geneticAlgortihm = new GeneticAlgortihm();
                            var front_GA = geneticAlgortihm.run(instance, runTimeLimit, 100, 1, crossoverType.order, 3);
                            fronts.Add(front_GA);
                            results[v].front = front_GA;
                        }
                        
                        var h = instance.hvis(fronts);
                        results[v].hvi = h[1]/h[0];
                    }
                    Console.WriteLine(results[v]);
                }
            }
            );

            Console.WriteLine();
            foreach ( var result in results ) Console.WriteLine(result);
        }
    }
}
