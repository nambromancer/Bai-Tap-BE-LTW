using System;

namespace _18DH110115_LTW.Models.ViewModel
{
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductImage { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string Category { get; set; }
        // Tổng giá cho mỗi sản phẩm
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}