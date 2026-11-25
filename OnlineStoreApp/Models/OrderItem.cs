namespace OnlineStoreApp.Models
{
    public class OrderItem
    {
        public Product Product { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }

        public OrderItem(Product product, int quantity)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));
            if (quantity <= 0)
                throw new ArgumentException("Количество товара должно быть больше нуля.");

            Product = product;
            Quantity = quantity;
            UnitPrice = product.Price;
        }

        public decimal GetSubtotal() => UnitPrice * Quantity;

        public override string ToString()
        {
            return $"{Product.Name,-28} | {Quantity,3} шт. x {UnitPrice,10:N2} = {GetSubtotal(),11:N2} руб.";
        }
    }
}
