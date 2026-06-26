namespace BuildingBlocks.CQRS.Behaviours
{
    public interface ICacheable
    {
        string CacheKey { get; }
        TimeSpan? Expiration => null;
    }
}
