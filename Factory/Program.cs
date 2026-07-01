using Factory;

Console.WriteLine("Hello, World!");
ICalculate calculate = (new CalculateFactory()).GetCalculate("add");
calculate.Calculate(1, 2);


