using System;

namespace LinqTutorials
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var res = LinqTasks.Task1();

            foreach (var item in res)
            {
                Console.WriteLine(item);
            }
        }
    }
}
