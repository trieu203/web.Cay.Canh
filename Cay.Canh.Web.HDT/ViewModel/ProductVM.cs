namespace Cay.Canh.Web.HDT.ViewModel
{
    public class ProductVM
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public int Quantity { get; set; }
        public string? ImageUrl { get; set; }
        public string? State { get; set; }
        public int CartId { get; set; }
        public string? Description { get; set; }
    }

    public class ProductVMDT
    {
        public int ProductId { get; set; }
        public string? ProductName { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int Discount { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public int CartId { get; set; }
        public decimal? Height { get; set; }
        public string? State { get; set; }
        public string? Size { get; set; }
    }
}
