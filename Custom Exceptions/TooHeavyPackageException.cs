namespace et3_challenge.Custom_Exceptions;

public class TooHeavyPackageException : Exception
{
    public Delivery Delivery { get; }

    public TooHeavyPackageException(Delivery delivery)
        : base($"Delivery {delivery.Id} ({delivery.Weight}kg) exceeds the 10kg vehicle capacity.")
    {
        Delivery = delivery;
    }
}