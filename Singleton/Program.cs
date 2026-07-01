using Singleton;

Console.WriteLine("Hello, World!");
Logger obj1 = Logger.Instance;
Logger obj2 = Logger.Instance;

Console.WriteLine(obj1.Counter());
Console.WriteLine(obj2.Counter());
