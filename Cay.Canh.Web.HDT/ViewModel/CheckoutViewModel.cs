﻿using System.ComponentModel.DataAnnotations;

namespace Cay.Canh.Web.HDT.ViewModel
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        public string? FullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập email.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [RegularExpression(@"^[0-9]{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? PhoneNumber { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng.")]
        [RegularExpression(@"^ấp\s[\w\s]+,?\s(xã|Xã)\s[\w\s]+,?\s(huyện|Huyện|thị trấn|Thị trấn)\s[\w\s]+,?\s(tỉnh|Tỉnh|thành phố|Thành phố)\s[\w\s]+$",
        ErrorMessage = "Địa chỉ không đúng định dạng. Ví dụ: Ấp Hai Thủ, Xã Long Hòa, Huyện Châu Thành, Tỉnh Trà Vinh.")]
        public string? ShippingAddress { get; set; }



        public string? PaymentMethod { get; set; }
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();
    }
}
