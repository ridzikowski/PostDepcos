namespace PostDepcos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Instance instance = new Instance(20,50,10,1,100,240);
            Console.WriteLine(instance.ToString());
            //int [] pi = {-1,1,-1};
            var pi = instance.getRandom(1);
            var result = instance.evaluate(pi);
            Console.WriteLine(string.Join(" ", pi));
            Console.WriteLine($"f1(pi):{result[0]}, f2(pi):{result[1]}");
            pi = instance.getRandom(1, 2);
            result = instance.evaluate(pi);
            Console.WriteLine(string.Join(" ", pi));
            Console.WriteLine($"f1(pi):{result[0]}, f2(pi):{result[1]}");
            Greedy greedy = new Greedy();
            var front = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);
            Console.WriteLine("Greedy old");
            foreach (var sol in front)
                Console.WriteLine(sol);
            Console.WriteLine();

            front = greedy.run(instance, Greedy.SortArrivalDiffAndPioritiesAndDiffByDeadlinesAndWeightsRev);
            Console.WriteLine("Greedy new");
            foreach (var sol in front)
                Console.WriteLine(sol);
            Console.WriteLine();

            //Console.WriteLine(Instance.dominates(2, 2, 2, 2));

            //var front = new List<Solution>() { 
            //    new Solution() {crit1 = 2, crit2 = 10 }, 
            //    new Solution() { crit1 = 3, crit2 = 7 },
            //    new Solution() {crit1 = 5, crit2 = 5 },
            //    new Solution() {crit1 = 6, crit2 = 4 },
            //    new Solution() {crit1 = 10, crit2 = 3 }
            //};
            //var x = instance.hvi(front, 12, 12);
            //Console.WriteLine(x);

            //var t = instance.TOPSIS(new List<int>() { 2, 3, 5, 6, 10 }, new List<int>() { 10, 7, 5, 4, 3 });
            //Console.WriteLine(t);

            //GreedyTrail trail = new GreedyTrail();
            Console.WriteLine("TS");
            TabuSearch search = new TabuSearch();
            front = search.run(instance, 50);
            foreach (var sol in front)
                Console.WriteLine(sol);

            Console.WriteLine();
            Console.WriteLine("GA GX");
            GeneticAlgortihm geneticAlgortihm = new GeneticAlgortihm();
            front = geneticAlgortihm.run(instance, 50, 20, 1, crossoverType.greedy);
            foreach (var sol in front)
                Console.WriteLine(sol);

            Console.WriteLine();
            Console.WriteLine("GA OX");
            geneticAlgortihm = new GeneticAlgortihm();
            front = geneticAlgortihm.run(instance, 50, 20, 1, crossoverType.order);
            foreach (var sol in front)
                Console.WriteLine(sol);

            Console.WriteLine("GA GX new rng");
            geneticAlgortihm = new GeneticAlgortihm();
            front = geneticAlgortihm.run(instance, 50, 20, 1, crossoverType.greedy, 2);
            foreach (var sol in front)
                Console.WriteLine(sol);

            Console.WriteLine();
            Console.WriteLine("GA OX new rng");
            geneticAlgortihm = new GeneticAlgortihm();
            front = geneticAlgortihm.run(instance, 50, 20, 1, crossoverType.order, 2);
            foreach (var sol in front)
                Console.WriteLine(sol);
        }
    }
}
