using OnlineStoreApp.Models;

namespace OnlineStoreApp.Services
{
    public class Catalog
    {
        private readonly List<Product> _products = new List<Product>();
        private int _nextId = 1;

        public Product AddProduct(string name, string category, decimal price, int stockQuantity)
        {
            var product = new Product(_nextId++, name, category, price, stockQuantity);
            _products.Add(product);
            return product;
        }

        public void AddProduct(Product product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));
            if (product.Id <= 0) product.Id = _nextId++;
            else if (product.Id >= _nextId) _nextId = product.Id + 1;
            _products.Add(product);
        }

        public Product? FindById(int id) => _products.FirstOrDefault(p => p.Id == id);

        public List<Product> Search(string query)
        {
            if (string.IsNullOrWhiteSpace(query)) return GetAll();
            string q = query.Trim();
            return _products.Where(p =>
                p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                p.Category.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        public bool UpdateProduct(int id, string name, string category, decimal price, int stockQuantity)
        {
            var existing = FindById(id);
            if (existing == null) return false;
            if (price <= 0 || stockQuantity < 0 || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Некорректные параметры товара.");

            existing.Name = name.Trim();
            existing.Category = string.IsNullOrWhiteSpace(category) ? existing.Category : category.Trim();
            existing.Price = price;
            existing.StockQuantity = stockQuantity;
            return true;
        }

        public bool RemoveProduct(int id)
        {
            var existing = FindById(id);
            if (existing == null) return false;
            return _products.Remove(existing);
        }

        public List<Product> GetAll() => _products.ToList();

        public void Print(List<Product>? items = null)
        {
            var list = items ?? _products;
            Console.WriteLine("================================================================================");
            Console.WriteLine(" ID | Наименование                 | Категория       | Цена (руб.) | Остаток");
            Console.WriteLine("--------------------------------------------------------------------------------");
            if (list.Count == 0)
            {
                Console.WriteLine("  Каталог пуст.");
            }
            else
            {
                foreach (var p in list)
                    Console.WriteLine(p);
            }
            Console.WriteLine("================================================================================");
        }
    }
}
