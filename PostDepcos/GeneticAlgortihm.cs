using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace PostDepcos
{
    enum crossoverType
    {
        greedy,
        order
    }
    internal class GeneticAlgortihm
    {
        Random random;
        Instance instance;
        public List<Solution> run(Instance inst, int timeLimit = 5, int popSize = 20, int seed = 1, crossoverType crossoverType = crossoverType.greedy, int populationType = 1)
        {
            Stopwatch stopwatch = new Stopwatch();
            stopwatch.Start();
            instance = inst;
            random = new Random(1);
            List<Solution> front = new List<Solution>();
            List<Solution> population = initializePopulation(popSize, populationType);
            List<Solution> childs = new List<Solution>();
            front = checkFront(population, front);
            while (stopwatch.Elapsed.TotalSeconds < timeLimit)
            {
                population = selection(population, (int)Math.Round(Math.Sqrt(popSize)));
                childs = crossover(population, 1.0, crossoverType);
                front = checkFront(childs, front);
                childs = mutation(childs);
                front = checkFront(childs, front);
                population = elite(population, childs, 0.03);
            }
            stopwatch.Stop();
            return front;
        }

        private List<Solution> elite(List<Solution> parents, List<Solution> childs, double ratio = 0.03)
        {
            List<Solution> population = new List<Solution>();
            var ranks_parents = instance.TOPSIS(parents);
            var ranks_childs = instance.TOPSIS(childs);

            int number = (int)Math.Round(parents.Count * ratio);
            for (int i = 0; i < number; i++)
            {
                var max = ranks_parents.Max();
                var idx = ranks_parents.IndexOf(max);
                population.Add(new Solution(parents[idx]));
                parents.RemoveAt(idx);
                ranks_parents.RemoveAt(idx);
            }

            for (int i = 0; i < number; i++)
            {
                var min = ranks_childs.Min();
                var idx = ranks_childs.IndexOf(min);
                childs.RemoveAt(idx);
                ranks_childs.RemoveAt(idx);
            }

            foreach (var child in childs) population.Add(new Solution(child));

            return population;
        }
        private List<Solution> initializePopulation(int size, int type)
        {
            Greedy greedy = new Greedy();
            List<Solution> front_greedy = new List<Solution>();
            if (type != 1 && type != 2)
                front_greedy = greedy.run(instance, Greedy.SortDiffDeadlinesAndArrivalByPiorities);
            List<Solution> population = new List<Solution>();
            
            foreach(Solution s in front_greedy) population.Add(new Solution(s));

            for (int p = front_greedy.Count; p < size; p++)
            {
                List<int> pi;
                if (type == 1 || type == 2) pi = instance.getRandom(p + 1, type);
                else
                {
                    pi = new List<int>(front_greedy[random.Next(front_greedy.Count)].pi);
                    int number = instance.n / 10;
                    for (int k = 0; k < number; k++)
                    {
                        int i = random.Next(1, pi.Count - 1);
                        int j = random.Next(1, pi.Count - 1);
                        (pi[j], pi[j + 1]) = (pi[j + 1], pi[j]);
                        var crit1 = instance.evaluate(pi)[0];
                        if (crit1 == int.MaxValue) (pi[j], pi[j + 1]) = (pi[j + 1], pi[j]);
                    }

                }
                var result = instance.evaluate(pi);
                population.Add(new Solution() { pi = pi, crit1 = result[0], crit2 = result[1] });
            }
            return population;
        }

        private List<Solution> checkFront(List<Solution> population, List<Solution> front)
        {
            foreach(Solution pop in population)
            {
                bool dominated = false;
                
                foreach (var sol in front)
                    if (Instance.dominates(sol.crit1, sol.crit2, pop.crit1, pop.crit2) || (sol.crit1 == pop.crit1 && sol.crit2 == pop.crit2))
                    {
                        dominated = true;
                        break;

                    }
                if (!dominated)
                {
                    for (int k = 0; k < front.Count; k++)
                        if (Instance.dominates(pop.crit1, pop.crit2, front[k].crit1, front[k].crit2))
                        {
                            front.RemoveAt(k);
                            k--;
                        }
                    front.Add(new Solution(pop));
                }
            }

            return new List<Solution>(front);
        }

        private List<Solution> selection(List<Solution> population, int cupSize=4)
        {
            var ranks = instance.TOPSIS(population);
            List<Solution> parents = new List<Solution>();
            for (int i = 0; i < population.Count; i++)
            {
                int idx = random.Next(population.Count);
                double rank = ranks[idx];
                for (int j = 0; j < cupSize -1; j++)
                {
                    int newIdx = random.Next(population.Count);
                    if (ranks[newIdx] > rank)
                    {
                        idx = newIdx;
                        rank = ranks[newIdx];
                    }
                }
                parents.Add(new Solution (population[idx]));
            }

            return parents;
        }
        private List<Solution> crossover(List<Solution> population, double ratio = 1.0, crossoverType type = crossoverType.greedy)
        {
            List<Solution> childs = new List<Solution>();

            foreach (Solution parent in population)
            {
                if (random.NextDouble() < ratio)
                {
                    
                    int i = random.Next(1, parent.pi.Count - 1);
                    int j = random.Next(1, parent.pi.Count - 1);
                    if (i>j) (i, j) = (j, i);

                    List<int> offspring = new List<int>(parent.pi[i..j]);
                    if (offspring.Count == 0 || offspring[0] != -1) offspring.Insert(0, -1);
                    List<int> orders = Enumerable.Range(0, instance.n).ToList();
                    if (type == crossoverType.order) orders = population[random.Next(population.Count)].pi;
                    orders = orders.Except(offspring).ToList();
                    
                    int currentCapacity = 0;
                    int currentTime = 0;
                    int idx = offspring.FindLastIndex(o => o == -1);
                    int last = instance.hub;
                    int dest = -1;
                    for (int l = idx+1; l<offspring.Count-1; l++)
                    {
                        int order = offspring[l];
                        dest = instance.destinations[order];
                        currentTime += instance.travelTimes[last, dest] + instance.serviceTime;
                        currentCapacity += instance.weights[order];
                        last = dest;
                    }
                    int steps = orders.Count;
                    for (int k =0; k< steps; k++)
                    {
                        
                        int min = int.MaxValue;
                        int order = -1;
                        int d = -1;
                        if (type == crossoverType.greedy)
                            foreach (int o in orders)
                            {
                                d = instance.destinations[o];
                                if (min > instance.travelTimes[last, o])
                                    if (min > instance.travelTimes[last, o] * instance.deadlines[o] / instance.priorities[o])
                                    {
                                    min = instance.travelTimes[last, o];
                                    min = instance.travelTimes[last, o] *  instance.deadlines[o]/instance.priorities[o];
                                    dest = d;
                                    order = o;
                                }
                            }
                        else if (type == crossoverType.order)
                        {
                            order = orders[0];
                            dest = instance.destinations[order]; ;
                        }
                        
                        int w = instance.weights[order];
                        int travel = instance.travelTimes[last, dest];
                        int comeback = instance.travelTimes[dest, instance.hub];
                        int approximate = instance.travelTimes[last, dest] + comeback + instance.serviceTime;
                        if (dest != last) approximate += instance.parkingTime;
                        if ((currentCapacity + w <= instance.capacity) && (currentTime + approximate <= instance.timeLimit))
                        {
                            currentCapacity += w;
                            if (dest != last) currentTime += instance.parkingTime;
                            currentTime += instance.travelTimes[last, dest] + instance.serviceTime;
                            offspring.Add(order);
                        }
                        else
                        {
                            offspring.Add(-1);
                            offspring.Add(order);

                            currentCapacity = w;
                            currentTime = instance.travelTimes[instance.hub, dest] + instance.serviceTime + instance.parkingTime;
                            
                        }
                        last = dest;
                        orders.Remove(order);
                    }
                    int vehicle = offspring.Count(of => of == -1);
                    for (int v = vehicle; v < instance.v + 1; v++) offspring.Add(-1);

                    var result = instance.evaluate(offspring);
                    Solution child = new Solution() {pi = offspring, crit1 = result[0], crit2 = result[1] };
                    childs.Add(child);
                }
                else
                    childs.Add(new Solution(parent));
            }    

            return childs;
        }
        private List<Solution> mutation(List<Solution> population, double ratio = 0.15)
        {
            List<Solution> mutants = new List<Solution>();

            foreach (Solution parent in population)
            {
                if (random.NextDouble() < ratio)
                {
                    Solution mutant = new Solution(parent);
                    int i = random.Next(1, mutant.pi.Count - 1);
                    int j = random.Next(1, mutant.pi.Count - 1);
                    (mutant.pi[j], mutant.pi[j + 1]) = (mutant.pi[j + 1], mutant.pi[j]);
                    var result = instance.evaluate(mutant.pi);
                    mutant.crit1 = result[0];
                    mutant.crit2 = result[1];
                    if (mutant.crit1 < int.MaxValue) mutants.Add(mutant);
                    else mutants.Add(new Solution(parent));

                }
                else
                    mutants.Add(new Solution(parent));
            }
            return mutants;
        }
    }
}
