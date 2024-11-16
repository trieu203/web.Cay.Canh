namespace Cay.Canh.Web.HDT.Data;

public partial class Product
{
    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public int Quantity { get; set; }

    public string? Size { get; set; }

    public decimal? Height { get; set; }

    public string? State { get; set; }

    public int Discount { get; set; }

    public int? CategoryId { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? UpdatedDate { get; set; }

    public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();

    public virtual Category? Category { get; set; }

    public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
}
