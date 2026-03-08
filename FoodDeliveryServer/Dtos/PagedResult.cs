namespace FoodDeliveryServer.Dtos
{
    // <T> means: this box can hold Orders, or FoodItems, whatever you decide
    public class PagedResult<T>
    {
        public List<T> Items { get; set; } = new(); // Data for this page (e.g., 10 orders)
        public int TotalCount { get; set; }         // Total number of records in database (e.g., 1000 records)
        public int PageNumber { get; set; }         // Current page number
        public int PageSize { get; set; }           // Number of records per page

        // 👇 An automatically calculated property: Total pages = Total count / Page size (rounded up)
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}