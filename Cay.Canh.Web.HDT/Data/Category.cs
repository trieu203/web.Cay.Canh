namespace Cay.Canh.Web.HDT.Data;

public partial class Category
{
    public int CategoryId { get; set; }

    public string? CategoryName { get; set; }

    public string? Description { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? UpdatedDate { get; set; }

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
