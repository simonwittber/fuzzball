

public class SignalRangeAttribute : System.Attribute
{
    public float min, max;

    public SignalRangeAttribute(float min, float max)
    {
        this.min = min;
        this.max = max;
    }
}