using System.ComponentModel.DataAnnotations;

namespace Cay.Canh.Web.HDT.Data;

public partial class User
{
    public int UserId { get; set; }

    public string UserName { get; set; } = null!;

    public string Password { get; set; } = null!;

    [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
    public string Email { get; set; } = null!;

    public string? FullName { get; set; }

    public string? Role { get; set; }
    public string? Sdt { get; set; }

    public string? Address { get; set; }

    public string? ImageUrl { get; set; }
    public DateTime? NgaySinh { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public virtual ICollection<Cart> Carts { get; set; } = new List<Cart>();

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
