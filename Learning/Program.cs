Console.WriteLine("Hello, World!");
var bankAccount = new BankAccount();
Console.WriteLine($"Current Balance: {bankAccount.Balance}");
bankAccount.Deposit(100);
Console.WriteLine($"Current Balance: {bankAccount.Balance}");

DiscountPolicy policy = new PremiumCustomerDiscount();
decimal discount = policy.CalculateDiscount(1000);
Console.WriteLine("Discount for Premium Customer: " + discount);

