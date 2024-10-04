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
        public int l { get; set; }

        public int hub { get; set; }
        public int[] arrivals { get; set; }
        public int[] deadlines { get; set; }
        public int[] priorities { get; set; }
        public int[] weights { get; set; }
        public int[] destinations { get; set; }
        public int[,] travelTimes { get; set; }

        public int v { get; set; }
        public int capacity { get; set; }
        public int timeLimit { get; set; }
        public int parkingTime { get; set; }
        public int serviceTime { get; set; }

        public Instance(int n, int v,int l, int seed, int capacity=1000, int timeLimit=480, int parkingTime=2,int serviceTime = 2, double speed = 1.0)
        {
            this.n = n;
            this.l = l;
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
            while (location.Count < this.l)
            {
                int val = rng.Next(3,347);
                if (!location.Contains(val)) { location.Add(val); }
            }
            location.Sort();
            arrivals = Enumerable.Repeat(0, n).Select(i => rng.Next(300, 720)).ToArray();
            destinations = new int[n];
            for (int i = 0; i < destinations.Length; i++) destinations[i] = location[rng.Next(0, location.Count)];
            priorities = Enumerable.Repeat(0, n).Select(i => rng.Next(1,10)).ToArray();
            deadlines = Enumerable.Repeat(0, n).Select(i => arrivals[i] + rng.Next(60, 180)).ToArray();
            weights = Enumerable.Repeat(0, n).Select(i => rng.Next(1, 10)).ToArray();
            
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
                        totalPenalty += Math.Max(0, T) * priorities[orders[i]];
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
                    if (arrivals[idx] > maxR) maxR = arrivals[idx];
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

        public List<int> getRandom(int seed, int type = 1)
        {
            int num = n + v + 1;
            if (type == 2) num = v + 1;
            List<int> pi = Enumerable.Repeat(0, num).Select(x => -1).ToList();

            Random random = new Random(seed);
            int[] orders = Enumerable.Range(0, n).ToArray();
            random.Shuffle(orders);
            int currentCapacity = 0;
            int currentTime = 0;
            int last = hub;
            int idx = 1;
            foreach (int order in orders)
            {
                if (type == 1)
                {

                    int w = weights[order];
                    int dest = destinations[order];
                    int travel = travelTimes[last, dest];
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
                    }
                    idx++;
                    last = dest;
                }
                else if (type == 2)
                {
                    do
                    {
                        idx = random.Next(1, pi.Count - 1);
                        pi.Insert(idx, order);
                        var result = evaluate(pi);
                        if (result[0] < int.MaxValue) break;
                        pi.RemoveAt(idx);
                    }while (true);
                    
                }
            }
            return pi;
        }



        public override string ToString()
        {
            string str = string.Empty;
            str += $"{n} x {l}\n";
            str += "r: "+String.Join(" ", arrivals) + "\n";
            str += "d: " + String.Join(" ", deadlines) + "\n";
            str += "p: " + String.Join(" ", priorities) + "\n";
            str += "w: " + String.Join(" ", weights) + "\n";
            str += "t: " + String.Join(" ", destinations) + "\n";

            return str;
        }

        //does S1 dominates S2
        public static bool dominates(int s1c1, int s1c2, int s2c1, int s2c2)
        {
            return (s1c1 <= s2c1 && s1c2 <= s2c2) && (s1c1 < s2c1 || s1c2 < s2c2);
        }


        public double hvi(List<Solution> front, double z1, double z2)
        {
            double volume = 0;

            front = front.OrderBy(solution => solution.crit1).ToList();
            for (int i = 0; i < front.Count - 1; i++)
            {
                volume += (front[i + 1].crit1 - front[i].crit1) * (z2 - front[i].crit2);
            }

            volume += (z1 - front[front.Count-1].crit1) * (z2 - front[front.Count-1].crit2);

            return volume;
        }

        public List<double> hvis(List<List<Solution>> fronts, double multiplier = 1.2)
        {
            List<double> volumes = new List<double>();
            double z1 = -1, z2 = -1;
            foreach (var front in fronts) 
                foreach (var sol in front)
                {
                    if (sol.crit1 > z1) z1 = sol.crit1;
                    if (sol.crit2 > z2) z2 = sol.crit2;
                }
            z1 *= multiplier;
            z2 *= multiplier;
            foreach(var front in fronts) volumes.Add(hvi(front, z1, z2));
            
            return volumes;
        }

        public int TOPSIS(List<int> F1, List<int> F2)
        {
            int m = F1.Count;
            int n = 2;
            double root1=0, root2=0;

            double[,] r = new double[m,n];

            double Aw1 = -1, Ab1 = Double.MaxValue;
            double Aw2 = -1, Ab2 = Double.MaxValue;

            for (int k = 0;k< m; k++)
            {
                root1 += F1[k]* F1[k];
                root2 += F2[k]* F2[k];
            }
            root1 = Math.Sqrt(root1);
            root2 = Math.Sqrt(root2);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    r[i, j] = (j == 0 ? (root1 == 0 ? F1[i] : F1[i] / root1) : (root2 == 0 ? F2[i] : F2[i] / root2)) * 0.5;
                }
                if (r[i, 0] > Aw1) Aw1 = r[i, 0];
                if (r[i, 1] > Aw2) Aw2 = r[i, 1];

                if (r[i, 0] < Ab1) Ab1 = r[i, 0];
                if (r[i, 1] < Ab2) Ab2 = r[i, 1];
            }

            List<double> diw = new List<double>();
            List<double> dib = new List<double>();
            for (int i = 0; i < m; i++)
            {
                diw.Add(Math.Sqrt((r[i,0] - Aw1) * (r[i, 0] - Aw1) + (r[i, 1] - Aw2) * (r[i, 1] - Aw2)));
                dib.Add(Math.Sqrt((r[i,0] - Ab1) * (r[i, 0] - Ab1) + (r[i, 1] - Ab2) * (r[i, 1] - Ab2)));
            }

            List<double> siw = new List<double>();
            for (int i = 0; i < m; i++) siw.Add((diw[i] + dib[i]) == 0 ? diw[i] : diw[i] / (diw[i] + dib[i]));

            var max = siw.Max();
            var idx = siw.IndexOf(max);
            return idx;
        }

        public List<double> TOPSIS(List<Solution> solutions)
        {
            List<int> F1 = new List<int>(), F2 = new List<int>();
            foreach (Solution s in solutions)
            {
                F1.Add(s.crit1);
                F2.Add(s.crit2);
            }
            int m = F1.Count;
            int n = 2;
            double root1 = 0, root2 = 0;

            double[,] r = new double[m, n];

            double Aw1 = -1, Ab1 = Double.MaxValue;
            double Aw2 = -1, Ab2 = Double.MaxValue;

            for (int k = 0; k < m; k++)
            {
                root1 += F1[k] * F1[k];
                root2 += F2[k] * F2[k];
            }
            root1 = Math.Sqrt(root1);
            root2 = Math.Sqrt(root2);

            for (int i = 0; i < m; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    r[i, j] = (j == 0 ? (root1 == 0 ? F1[i] : F1[i] / root1) : (root2 == 0 ? F2[i] : F2[i] / root2)) * 0.5;
                }
                if (r[i, 0] > Aw1) Aw1 = r[i, 0];
                if (r[i, 1] > Aw2) Aw2 = r[i, 1];

                if (r[i, 0] < Ab1) Ab1 = r[i, 0];
                if (r[i, 1] < Ab2) Ab2 = r[i, 1];
            }

            List<double> diw = new List<double>();
            List<double> dib = new List<double>();
            for (int i = 0; i < m; i++)
            {
                diw.Add(Math.Sqrt((r[i, 0] - Aw1) * (r[i, 0] - Aw1) + (r[i, 1] - Aw2) * (r[i, 1] - Aw2)));
                dib.Add(Math.Sqrt((r[i, 0] - Ab1) * (r[i, 0] - Ab1) + (r[i, 1] - Ab2) * (r[i, 1] - Ab2)));
            }

            List<double> siw = new List<double>();
            for (int i = 0; i < m; i++) siw.Add((diw[i] + dib[i]) == 0 ? diw[i] : diw[i] / (diw[i] + dib[i]));

            //var max = siw.Max();
            //var idx = siw.IndexOf(max);
            return siw;
        }

    }
}
