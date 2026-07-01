public class BankAccount
{
    // 1. Hide the raw data from the outside world
    private decimal _balance;

    // 2. Control access through a Property (Read-Only from outside)
    public decimal Balance
    {
        get { return _balance; }
    }

    // 3. Control modification through a validated Method
    public void Deposit(decimal amount)
    {
        if (amount > 0) // Guard clause: validates the action
        {
            _balance += amount;
        }
    }
}