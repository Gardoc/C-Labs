namespace Truant.Simulation;

public class RandomGenerator
{
    private ulong state;

    public long State => unchecked((long)state);

    public RandomGenerator(ulong? state = null)
    {
        this.state = state ?? (ulong)Random.Shared.NextInt64(1, long.MaxValue);

        if (this.state == 0)
        {
            throw new ArgumentException("Состояние генератора не может быть равно нулю.");
        }
    }

    public double NextDouble()
    {
        ulong x = state;

        x ^= x << 13;
        x ^= x >> 7;
        x ^= x << 17;

        state = x;

        return (state >> 11) * (1.0 / (1UL << 53));
    }

    public int Next(int minValue, int maxValue)
    {
        if (minValue >= maxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxValue));
        }

        double value = NextDouble();

        return minValue +
            (int)(value * (maxValue - minValue));
    }
}
