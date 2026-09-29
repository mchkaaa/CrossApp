namespace Core.Domain;

public class Customer
{
    public string Id { get; }
    public int TotalPurchases { get; private set; }

    public Customer(string id)
    {
        Id = id;
    }

    public void RecordPurchase() => TotalPurchases++;
}