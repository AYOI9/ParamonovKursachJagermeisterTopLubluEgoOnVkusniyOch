namespace OnlineStoreApp.Models
{
    public class Order
    {
        public int Id { get; set; }
        public Client Client { get; set; }
        public DateTime CreatedAt { get; set; }
        public OrderStatus Status { get; set; }
        public List<OrderItem> Items { get; private set; }

        public Order(int id, Client client, DateTime? createdAt = null)
        {
            Id = id;
            Client = client ?? throw new ArgumentNullException(nameof(client));
            CreatedAt = createdAt ?? DateTime.Now;
            Status = OrderStatus.PendingPayment;
            Items = new List<OrderItem>();
        }

        public void AddItem(Product product, int quantity)
        {
            var existing = Items.FirstOrDefault(i => i.Product.Id == product.Id);
            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                Items.Add(new OrderItem(product, quantity));
            }
        }

        public decimal CalculateTotal()
        {
            return Items.Sum(i => i.GetSubtotal());
        }

        public string GetStatusRu() => Status switch
        {
            OrderStatus.New => "Новый",
            OrderStatus.PendingPayment => "Ожидает оплаты",
            OrderStatus.Paid => "Оплачен",
            OrderStatus.UnpaidOverdue => "Просрочен (не оплачен)",
            OrderStatus.Cancelled => "Отменен",
            _ => Status.ToString()
        };

        public void Print()
        {
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine($"Заказ #{Id} | Дата: {CreatedAt:dd.MM.yyyy HH:mm} | Статус: {GetStatusRu()}");
            Console.WriteLine($"Клиент: {Client.GetFullName()} (ID: {Client.Id})");
            Console.WriteLine("--------------------------------------------------------------------------");
            int idx = 1;
            foreach (var item in Items)
            {
                Console.WriteLine($"{idx,2}. {item}");
                idx++;
            }
            Console.WriteLine("--------------------------------------------------------------------------");
            Console.WriteLine($"ИТОГО К ОПЛАТЕ: {CalculateTotal():N2} руб.");
            Console.WriteLine("--------------------------------------------------------------------------");
        }
    }
}
