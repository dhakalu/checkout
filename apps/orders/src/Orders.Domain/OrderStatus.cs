namespace Orders.Domain;


public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returning,
    Returned
}