namespace OnlineStoreApp.Models
{
    public enum OrderStatus
    {
        New,
        PendingPayment,
        Paid,
        UnpaidOverdue,
        Cancelled
    }
}
