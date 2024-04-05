namespace Common.Commands;

public class RequestPayment
{
    public RequestPayment(string cardNumber, string cvv, int amount)
    {
        CardNumber = cardNumber;
        Cvv = cvv;
        Amount = amount;
    }

    public string CardNumber { get; }

    public string Cvv { get; }

    public int Amount { get; }
}
