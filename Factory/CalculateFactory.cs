using System;
using System.Collections.Generic;
using System.Text;

namespace Factory
{
    internal class CalculateFactory
    {
        public ICalculate GetCalculate(string type)
        {
            ICalculate calculate = null;
            if (type.ToLower().Equals("add"))
            {
                calculate = new Add();
            }
            else if (type.ToLower().Equals("divide"))
            {
                calculate = new Divide();
            }

            return calculate;
        }
    }
}
