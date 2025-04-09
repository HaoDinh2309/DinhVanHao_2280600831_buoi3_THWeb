namespace DinhVanHao_2280600831_buoi3_THWeb.Models
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public string ImageUrl { get; set; } // Thêm trường ImageUrl để lưu URL ảnh của sản phẩm
    }



}
