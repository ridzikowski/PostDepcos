namespace PostDepcos
{
    internal class Program
    {
        static void Main(string[] args)
        {
            
            Instance instance = new Instance(20,102,3,1,100,240);
            
            var pi = instance.getRandom(2);
            var result = instance.evalute(pi);
            Console.WriteLine($"f1(pi):{result[0]}, f2(pi):{result[1]}");
        }
    }
}
