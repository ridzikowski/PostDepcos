using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    internal class Instance
    {
        int n {  get; set; }
        int m { get; set; }

        int hub { get; set; }
        int[] readyTimes { get; set; }
        int[] deadlines { get; set; }
        int[] piorities { get; set; }
        int[] weights { get; set; }
        int[] destinations { get; set; }
        int[,] travelTimes { get; set; }
        
        int v { get; set; }
        int capacity { get; set; }
        int timeLimit { get; set; }
        int parkingTime { get; set; }
        int serviceTime { get; set; }

        public Instance(int n, int m, int v, int seed, int capacity, int timeLimit, int parkingTime=2,int serviceTime = 2, int maxWeights = 5, int maxReadyTime = 20, double speed = 1.0)
        {
            this.n = n;
            this.m = m;
            this.v = v;
            
            Random rng = new Random(seed);
            this.capacity = capacity;
            this.timeLimit = timeLimit;
            this.parkingTime = parkingTime;
            this.serviceTime = serviceTime;
            var filePath = @"travelMatrixosrm.csv";
            var data = File.ReadLines(filePath).Select(x => x.Split(',')).ToArray();
            
            travelTimes = new int[347, 347];
            if (data != null)
            {
                for (int i = 0; i < 347; i++)
                {
                    for (int j = 0; j < 347; j++)
                    {
                        // Console.WriteLine(data[i][j]);
                        // travelTimes[i, j] = (int)Math.Round(double.Parse(data[i][j].Replace(".",","))); 
                        travelTimes[i, j] = (int)Math.Round(double.Parse(data[i][j]));

                    }
                }
            }
            hub = rng.Next(3);

            List<int> location = new List<int>();
            while (location.Count < m)
            {
                int val = rng.Next(3,347);
                if (!location.Contains(val)) { location.Add(val); }
            }
            location.Sort();
            readyTimes = Enumerable.Repeat(0, n).Select(i => rng.Next(maxReadyTime)).ToArray();
            destinations = new int[n];
            for (int i = 0; i < destinations.Length; i++) destinations[i] = location[rng.Next(0, location.Count)];
            piorities = Enumerable.Repeat(0, n).Select(i => rng.Next(1,10)).ToArray();
            deadlines = Enumerable.Repeat(0, n).Select(i => readyTimes[i] + rng.Next(60, 120)).ToArray();
            weights = Enumerable.Repeat(0, n).Select(i => rng.Next(maxWeights)).ToArray();
            
        }

        public List<int> evaluate(int[] pi)
        {
            int totalTime = 0;
            int currentCapacity = 0;
            int currentTime = 0;
            int totalPenalty = 0;
            int maxR = 0;
            int last = hub;

            List<int> times = new List<int>();
            List<int> orders = new List<int>();

            foreach (int idx in pi)
            {
                if (idx == -1)
                {
                    currentTime += travelTimes[last, hub];
                    if (currentCapacity > capacity || currentTime > timeLimit) return new List<int>(){-1,-1};
                    totalTime += maxR + currentTime;

                    for(int i = 0; i < times.Count; i++) 
                    {
                        int T = times[i] + maxR - deadlines[orders[i]];
                        Console.WriteLine(times[i]);
                        Console.WriteLine(maxR);
                        Console.WriteLine(deadlines[orders[i]]);
                        totalPenalty += Math.Max(0, T) * piorities[orders[i]];
                        Console.WriteLine(piorities[orders[i]]);
                    }
                        


                    //new vehicle
                    times.Clear();
                    orders.Clear();
                    currentCapacity = 0;
                    currentTime = 0;
                    maxR = 0;
                    last = hub;
                }
                else
                {
                    if (readyTimes[idx] > maxR) maxR = readyTimes[idx];
                    int dest = destinations[idx];
                    
                    if (last != dest) currentTime += parkingTime;
                    currentTime += travelTimes[last, dest] + serviceTime;
                    Console.WriteLine($"test travel {travelTimes[0, 14] } ");
                    Console.WriteLine($"last: {last} dest: {destinations[idx]} and parking: {parkingTime} travel: {travelTimes[last, dest] } and service: {serviceTime}");
                    Console.WriteLine($"current time: {currentTime} and idex: {idx}");
                    times.Add(currentTime);
                    orders.Add(idx);
                    currentCapacity += weights[idx];
                    
                    last = dest;
                }

            }

            return new List<int>() { totalTime, totalPenalty };
        }

        public int[] getRandom(int seed)
        {
            int [] pi = Enumerable.Repeat(0, n + v + 1).Select(x => -1).ToArray();
            
            Random random = new Random(seed);
            int[] orders = Enumerable.Range(0, n).ToArray();
            random.Shuffle(orders);
            int currentCapacity = 0;
            int currentTime = 0;
            int last = hub;
            int maxR = 0;
            int idx = 1;
            foreach (int order in orders)
            {
                Console.WriteLine(String.Join(" ", pi));
                int w = weights[order];
                int dest = destinations[order];
                int travel = travelTimes[last, dest];
                if (maxR < readyTimes[order]) maxR = readyTimes[order];
                int comeback = travelTimes[dest, hub];
                int approximate = maxR + travelTimes[last, dest] + comeback + serviceTime;
                if (dest != last) approximate += parkingTime;
                if ((currentCapacity + w <= capacity) && (currentTime + approximate <= timeLimit))
                {
                    currentCapacity += w;
                    if (dest != last) currentTime += parkingTime;
                    currentTime += travelTimes[last, dest] + serviceTime;
                    pi[idx] = order;
                }
                else
                {
                    idx++;
                    pi[idx] = order;
                    
                    currentCapacity = w;
                    currentTime = travelTimes[hub, dest] + serviceTime + parkingTime;
                    maxR = readyTimes[order];
                }
                idx++;
                last = dest;
            }

            return pi;
        }

    }
}
