using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace PostDepcos
{
    internal class Instance
    {
        public int n {  get; set; }
        public int m { get; set; }

        public int hub { get; set; }
        public int[] readyTimes { get; set; }
        public int[] deadlines { get; set; }
        public int[] piorities { get; set; }
        public int[] weights { get; set; }
        public int[] destinations { get; set; }
        public int[,] travelTimes { get; set; }

        public int v { get; set; }
        public int capacity { get; set; }
        public int timeLimit { get; set; }
        public int parkingTime { get; set; }
        public int serviceTime { get; set; }

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
                        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                            travelTimes[i, j] = (int)Math.Round(double.Parse(data[i][j]));
                        else
                            travelTimes[i, j] = (int)Math.Round(double.Parse(data[i][j].Replace(".", ",")));

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

        public List<int> evaluate(List<int> pi)
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
                    if (currentCapacity > capacity || currentTime > timeLimit) return new List<int>(){int.MaxValue, int.MaxValue };
                    totalTime += currentTime;

                    for(int i = 0; i < times.Count; i++) 
                    {
                        int T = times[i] + maxR - deadlines[orders[i]];
                        totalPenalty += Math.Max(0, T) * piorities[orders[i]];
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
                    times.Add(currentTime);
                    orders.Add(idx);
                    currentCapacity += weights[idx];
                    
                    last = dest;
                }

            }

            return new List<int>() { totalTime, totalPenalty };
        }

        public List<int> getRandom(int seed)
        {
            List<int> pi = Enumerable.Repeat(0, n + v + 1).Select(x => -1).ToList();
            
            Random random = new Random(seed);
            int[] orders = Enumerable.Range(0, n).ToArray();
            random.Shuffle(orders);
            int currentCapacity = 0;
            int currentTime = 0;
            int last = hub;
            //int maxR = 0;
            int idx = 1;
            foreach (int order in orders)
            {
                //Console.WriteLine(String.Join(" ", pi));
                int w = weights[order];
                int dest = destinations[order];
                int travel = travelTimes[last, dest];
                //if (maxR < readyTimes[order]) maxR = readyTimes[order];
                int comeback = travelTimes[dest, hub];
                int approximate = travelTimes[last, dest] + comeback + serviceTime;
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
                    //maxR = readyTimes[order];
                }
                idx++;
                last = dest;
            }
            return pi;
        }


        public override string ToString()
        {
            string str = string.Empty;
            str += $"{n} x {m}\n";
            str += "r: "+String.Join(" ", readyTimes) + "\n";
            str += "d: " + String.Join(" ", deadlines) + "\n";
            str += "p: " + String.Join(" ", piorities) + "\n";
            str += "w: " + String.Join(" ", weights) + "\n";
            str += "t: " + String.Join(" ", destinations) + "\n";

            return str;
        }

        //does S1 dominates S2
        public static bool dominates(int s1c1, int s1c2, int s2c1, int s2c2)
        {
            return (s1c1 <= s2c1 && s1c2 <= s2c2) && (s1c1 < s2c1 || s1c2 < s2c2);
        }
    }
}
