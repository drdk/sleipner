namespace DR.Sleipner.Caching.Contract
{
    public interface ILookupHandler<T> where T : class
    {
        TResult Handle<TResult>(LookupContext<T, TResult> lookupContext);
    }
}
