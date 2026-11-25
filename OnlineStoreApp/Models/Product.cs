namespace OnlineStoreApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public Product(int id, string name, string category, decimal price, int stockQuantity)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Наименование товара не может быть пустым.");
            if (price <= 0)
                throw new ArgumentException("Цена товара должна быть положительной.");
            if (stockQuantity < 0)
                throw new ArgumentException("Количество на складе не может быть отрицательным.");

            Id = id;
            Name = name.Trim();
            Category = string.IsNullOrWhiteSpace(category) ? "Общее" : category.Trim();
            Price = price;
            StockQuantity = stockQuantity;
        }

        public override string ToString()
        {
            return $"{Id,3} | {Name,-28} | {Category,-15} | {Price,10:N2} | {StockQuantity,5} шт.";
        }
    }
}
