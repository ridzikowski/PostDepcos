namespace PostDepcos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Instance instance = new Instance(10,50,10,1,100,240);
            int [] pi = {-1,1,-1};
            // var pi = instance.getRandom(2);
            var result = instance.evaluate(pi);
            // Console.WriteLine($"f1(pi):{result[0]}, f2(pi):{result[1]}");
        }
    }
}
