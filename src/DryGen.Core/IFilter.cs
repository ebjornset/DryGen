namespace DryGen.Core;

public interface IFilter<in T>
{
    bool Accepts(T value);
}
