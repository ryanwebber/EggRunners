using System;

public class Handle
{
    private static int counter = 0;

    private int id;

    private Handle(int id)
    {
        this.id = id;
    }

    public override bool Equals(object obj)
    {
        return obj is Handle handle &&
               id == handle.id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(id);
    }

    public static Handle CreateUnique()
    {
        return new Handle(++counter);
    }
}
