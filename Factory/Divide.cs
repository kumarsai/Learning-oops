using System;
using System.Collections.Generic;
using System.Text;

namespace Factory
{
    internal class Divide : ICalculate
    {
        public void Calculate(int a, int b) 
        {
            Console.WriteLine("a/b is {0}", a / b);
        }
    }

    internal class Add : ICalculate
    {
        public void Calculate(int a, int b)
        {
            Console.WriteLine("a+b is {0}", a + b);
        }
    }
}
