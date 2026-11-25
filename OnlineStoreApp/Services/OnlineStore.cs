using OnlineStoreApp.Models;

namespace OnlineStoreApp.Services
{
    public class OnlineStore
    {
        public Catalog Catalog { get; } = new Catalog();
        public List<Client> Clients { get; } = new List<Client>();
        public List<Administrator> Administrators { get; } = new List<Administrator>();
        public List<Order> Orders { get; } = new List<Order>();
        public List<Payment> Payments { get; } = new List<Payment>();
        public List<BlacklistEntry> Blacklist { get; } = new List<BlacklistEntry>();

        private int _nextUserId = 101;
        private int _nextOrderId = 1001;
        private int _nextPaymentId = 5001;

        public void SeedDemoData()
        {
            Catalog.AddProduct("Ноутбук ASUS VivoBook 15", "Ноутбуки", 54990m, 8);
            Catalog.AddProduct("Смартфон Samsung Galaxy A55", "Смартфоны", 32990m, 14);
            Catalog.AddProduct("Наушники Sony WH-1000XM5", "Аудио", 24500m, 5);
            Catalog.AddProduct("Клавиатура Keychron K2", "Периферия", 8900m, 12);
            Catalog.AddProduct("Монитор LG UltraGear 27\"", "Мониторы", 28400m, 6);

            var client1 = new Client(_nextUserId++, "Иванов", "Иван", "Иванович", "ivanov", "1234", "+7-911-111-22-33", "ivanov@mail.ru");
            var client2 = new Client(_nextUserId++, "Петров", "Петр", "Петрович", "petrov", "1234", "+7-921-222-33-44", "petrov@mail.ru");
            var client3 = new Client(_nextUserId++, "Сидорова", "Анна", "Сергеевна", "sidorova", "1234", "+7-905-333-44-55", "sidorova@mail.ru");
            Clients.AddRange(new[] { client1, client2, client3 });

            var admin = new Administrator(_nextUserId++, "Парамонов", "Дмитрий", "Дмитриевич", "admin", "admin", "Главный администратор");
            Administrators.Add(admin);

            // Предзаполненный неоплаченный просроченный заказ для демонстрации работы с неплательщиками
            var overdueOrder = new Order(_nextOrderId++, client2, DateTime.Now.AddDays(-5))
            {
                Status = OrderStatus.UnpaidOverdue
            };
            var prod = Catalog.FindById(2);
            if (prod != null)
            {
                overdueOrder.AddItem(prod, 1);
                prod.StockQuantity -= 1;
            }
            Orders.Add(overdueOrder);

            var paidOrder = new Order(_nextOrderId++, client3, DateTime.Now.AddDays(-2))
            {
                Status = OrderStatus.Paid
            };
            var prod2 = Catalog.FindById(4);
            if (prod2 != null)
            {
                paidOrder.AddItem(prod2, 2);
                prod2.StockQuantity -= 2;
            }
            Orders.Add(paidOrder);
            var payment = new Payment(_nextPaymentId++, paidOrder.Id, paidOrder.CalculateTotal(), "Банковская карта");
            payment.Process();
            Payments.Add(payment);
        }

        public User? Authenticate(string login, string password)
        {
            var admin = Administrators.FirstOrDefault(a =>
                a.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && a.CheckPassword(password));
            if (admin != null) return admin;

            return Clients.FirstOrDefault(c =>
                c.Login.Equals(login, StringComparison.OrdinalIgnoreCase) && c.CheckPassword(password));
        }

        public Client RegisterClient(string lastName, string firstName, string middleName,
            string login, string password, string phone, string email)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(password))
                throw new ArgumentException("Логин и пароль обязательны для заполнения.");
            if (Clients.Any(c => c.Login.Equals(login, StringComparison.OrdinalIgnoreCase)) ||
                Administrators.Any(a => a.Login.Equals(login, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Пользователь с таким логином уже существует.");

            var client = new Client(_nextUserId++, lastName.Trim(), firstName.Trim(), middleName.Trim(),
                login.Trim(), password, phone.Trim(), email.Trim());
            Clients.Add(client);
            return client;
        }

        public Order PlaceOrder(Client client, Dictionary<int, int> cartItems)
        {
            if (client.IsBlacklisted)
                throw new InvalidOperationException("Оформление заказа отклонено: клиент находится в «черном списке» за неоплату.");
            if (cartItems == null || cartItems.Count == 0)
                throw new InvalidOperationException("Корзина пуста. Добавьте хотя бы один товар.");

            // Проверяем наличие по всем позициям до списания остатков, чтобы не оставить заказ частично списанным
            var resolved = new List<(Product product, int qty)>();
            foreach (var kv in cartItems)
            {
                var product = Catalog.FindById(kv.Key)
                    ?? throw new KeyNotFoundException($"Товар с ID={kv.Key} не найден.");
                if (kv.Value <= 0)
                    throw new ArgumentException($"Некорректное количество для товара «{product.Name}».");
                if (product.StockQuantity < kv.Value)
                    throw new InvalidOperationException($"Недостаточно товара «{product.Name}» на складе (доступно: {product.StockQuantity}).");
                resolved.Add((product, kv.Value));
            }

            var order = new Order(_nextOrderId++, client);
            foreach (var (product, qty) in resolved)
            {
                product.StockQuantity -= qty;
                order.AddItem(product, qty);
            }

            Orders.Add(order);
            return order;
        }

        public Payment PayOrder(int orderId, string method)
        {
            var order = Orders.FirstOrDefault(o => o.Id == orderId)
                ?? throw new KeyNotFoundException($"Заказ #{orderId} не найден.");

            if (order.Status == OrderStatus.Paid)
                throw new InvalidOperationException($"Заказ #{orderId} уже оплачен.");
            if (order.Status == OrderStatus.Cancelled)
                throw new InvalidOperationException($"Заказ #{orderId} отменен и не может быть оплачен.");

            var payment = new Payment(_nextPaymentId++, order.Id, order.CalculateTotal(), method);
            if (!payment.Process())
                throw new InvalidOperationException("Платеж отклонен платежной системой.");

            order.Status = OrderStatus.Paid;
            Payments.Add(payment);
            return payment;
        }

        public List<Order> GetUnpaidOrders()
        {
            return Orders.Where(o => o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.UnpaidOverdue).ToList();
        }

        public BlacklistEntry BlacklistClient(int clientId, string reason, Administrator admin)
        {
            var client = Clients.FirstOrDefault(c => c.Id == clientId)
                ?? throw new KeyNotFoundException($"Клиент с ID={clientId} не найден.");

            if (client.IsBlacklisted)
                throw new InvalidOperationException($"Клиент {client.GetFullName()} уже находится в «черном списке».");

            client.IsBlacklisted = true;
            var entry = new BlacklistEntry(client.Id, client.GetFullName(), reason, admin.GetFullName());
            Blacklist.Add(entry);
            return entry;
        }

        public bool RemoveFromBlacklist(int clientId)
        {
            var client = Clients.FirstOrDefault(c => c.Id == clientId);
            if (client == null || !client.IsBlacklisted) return false;
            client.IsBlacklisted = false;
            Blacklist.RemoveAll(b => b.ClientId == clientId);
            return true;
        }
    }
}
