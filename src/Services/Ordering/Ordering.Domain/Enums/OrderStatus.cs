namespace Ordering.Domain.Enums
{
    public enum OrderStatus
    {
        //Unassigned = 0,  // The sentinel
        Draft = 1,
        Pending = 2,
        Completed = 3,
        Cancelled = 4
    }
}
