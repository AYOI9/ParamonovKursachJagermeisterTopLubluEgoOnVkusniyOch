using System.Text;
using OnlineStoreApp.Models;
using OnlineStoreApp.Services;

namespace OnlineStoreApp
{
    public class Program
    {
        static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.InputEncoding = Encoding.UTF8;

            var store = new OnlineStore();
            store.SeedDemoData();

            bool exitProgram = false;
            while (!exitProgram)
            {
                Console.WriteLine();
                Console.WriteLine("================================================================================");
                Console.WriteLine("                   ИНФОРМАЦИОННАЯ СИСТЕМА «ИНТЕРНЕТ-МАГАЗИН»");
                Console.WriteLine("================================================================================");
                Console.WriteLine("1 - Войти в систему (Клиент / Администратор)");
                Console.WriteLine("2 - Просмотреть каталог товаров (гостевой режим)");
                Console.WriteLine("3 - Зарегистрироваться как новый клиент");
                Console.WriteLine("0 - Выход из программы");
                Console.WriteLine("--------------------------------------------------------------------------------");

                int choice = ReadIntInRange("Выберите действие: ", 0, 3);
                switch (choice)
                {
                    case 1:
                        LoginFlow(store);
                        break;
                    case 2:
                        store.Catalog.Print();
                        Pause();
                        break;
                    case 3:
                        RegisterFlow(store);
                        break;
                    case 0:
                        exitProgram = true;
                        Console.WriteLine("Работа программы завершена.");
                        break;
                }
            }
        }

        static void LoginFlow(OnlineStore store)
        {
            Console.WriteLine("\n--- Вход в систему (демо: ivanov/1234, petrov/1234, admin/admin) ---");
            Console.Write("Логин: ");
            string login = Console.ReadLine() ?? "";
            Console.Write("Пароль: ");
            string password = Console.ReadLine() ?? "";

            var user = store.Authenticate(login.Trim(), password);
            if (user == null)
            {
                Console.WriteLine("[Ошибка] Неверный логин или пароль.");
                Pause();
                return;
            }

            Console.WriteLine($"\nДобро пожаловать, {user.GetFullName()}! Роль: {user.GetRole()}.");
            if (user is Client client)
                ClientMenu(store, client);
            else if (user is Administrator admin)
                AdminMenu(store, admin);
        }

        static void RegisterFlow(OnlineStore store)
        {
            try
            {
                Console.WriteLine("\n--- Регистрация нового клиента ---");
                string lastName = ReadNonEmpty("Фамилия: ");
                string firstName = ReadNonEmpty("Имя: ");
                Console.Write("Отчество (необязательно): ");
                string middleName = Console.ReadLine() ?? "";
                string phone = ReadNonEmpty("Телефон: ");
                string email = ReadNonEmpty("Email: ");
                string login = ReadNonEmpty("Придумайте логин: ");
                string password = ReadNonEmpty("Придумайте пароль: ");

                var client = store.RegisterClient(lastName, firstName, middleName, login, password, phone, email);
                Console.WriteLine($"[ОК] Клиент зарегистрирован: {client}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка регистрации] {ex.Message}");
            }
            Pause();
        }

        static void ClientMenu(OnlineStore store, Client client)
        {
            var cart = new Dictionary<int, int>();
            int selectItem = -1;

            while (selectItem != 0)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------------------------------------------------");
                string statusText = client.IsBlacklisted ? "ЗАБЛОКИРОВАН (в черном списке)" : "Активен";
                Console.WriteLine($"КАБИНЕТ КЛИЕНТА: {client.GetFullName()} | Статус: {statusText} | В корзине позиций: {cart.Count}");
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("1 - Просмотр каталога товаров");
                Console.WriteLine("2 - Поиск товара по названию или категории");
                Console.WriteLine("3 - Добавить товар в корзину");
                Console.WriteLine("4 - Просмотреть корзину");
                Console.WriteLine("5 - Оформить заказ");
                Console.WriteLine("6 - Оплатить заказ");
                Console.WriteLine("7 - Мои заказы");
                Console.WriteLine("0 - Выйти из аккаунта");
                Console.WriteLine("--------------------------------------------------------------------------------");

                selectItem = ReadIntInRange("Выберите пункт меню: ", 0, 7);
                switch (selectItem)
                {
                    case 1:
                        store.Catalog.Print();
                        Pause();
                        break;
                    case 2:
                        Console.Write("Введите строку для поиска: ");
                        string query = Console.ReadLine() ?? "";
                        store.Catalog.Print(store.Catalog.Search(query));
                        Pause();
                        break;
                    case 3:
                        AddToCartFlow(store, cart);
                        break;
                    case 4:
                        PrintCart(store, cart);
                        Pause();
                        break;
                    case 5:
                        PlaceOrderFlow(store, client, cart);
                        break;
                    case 6:
                        PayOrderFlow(store, client);
                        break;
                    case 7:
                        PrintClientOrders(store, client);
                        Pause();
                        break;
                    case 0:
                        Console.WriteLine("Выход из аккаунта клиента.");
                        break;
                }
            }
        }

        static void AddToCartFlow(OnlineStore store, Dictionary<int, int> cart)
        {
            store.Catalog.Print();
            int id = ReadIntMin("Введите ID товара (0 - отмена): ", 0);
            if (id == 0) return;

            var product = store.Catalog.FindById(id);
            if (product == null)
            {
                Console.WriteLine("[Ошибка] Товар с указанным ID не найден.");
                Pause();
                return;
            }

            int qty = ReadIntMin($"Введите количество (доступно {product.StockQuantity} шт.): ", 1);
            int currentQty = cart.TryGetValue(id, out int existing) ? existing : 0;
            if (currentQty + qty > product.StockQuantity)
            {
                Console.WriteLine($"[Ошибка] Недостаточно товара на складе (доступно: {product.StockQuantity}).");
                Pause();
                return;
            }

            cart[id] = currentQty + qty;
            Console.WriteLine($"[ОК] Товар «{product.Name}» ({qty} шт.) добавлен в корзину.");
            Pause();
        }

        static void PrintCart(OnlineStore store, Dictionary<int, int> cart)
        {
            Console.WriteLine("\n--- Текущая корзина ---");
            if (cart.Count == 0)
            {
                Console.WriteLine("Корзина пуста.");
                return;
            }

            decimal total = 0;
            foreach (var kv in cart)
            {
                var p = store.Catalog.FindById(kv.Key);
                if (p == null) continue;
                decimal sub = p.Price * kv.Value;
                total += sub;
                Console.WriteLine($"ID {p.Id}: {p.Name} — {kv.Value} шт. x {p.Price:N2} = {sub:N2} руб.");
            }
            Console.WriteLine($"Итого в корзине: {total:N2} руб.");
        }

        static void PlaceOrderFlow(OnlineStore store, Client client, Dictionary<int, int> cart)
        {
            try
            {
                var order = store.PlaceOrder(client, cart);
                cart.Clear();
                Console.WriteLine("\n[ОК] Заказ успешно оформлен!");
                order.Print();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Ошибка оформления заказа] {ex.Message}");
            }
            Pause();
        }

        static void PayOrderFlow(OnlineStore store, Client client)
        {
            var unpaid = store.Orders.Where(o => o.Client.Id == client.Id &&
                (o.Status == OrderStatus.PendingPayment || o.Status == OrderStatus.UnpaidOverdue)).ToList();

            if (unpaid.Count == 0)
            {
                Console.WriteLine("\nУ вас нет заказов, ожидающих оплаты.");
                Pause();
                return;
            }

            Console.WriteLine("\n--- Заказы, ожидающие оплаты ---");
            foreach (var o in unpaid)
                Console.WriteLine($"Заказ #{o.Id} | Сумма: {o.CalculateTotal():N2} руб. | Статус: {o.GetStatusRu()}");

            int orderId = ReadIntMin("Введите номер заказа для оплаты (0 - отмена): ", 0);
            if (orderId == 0) return;

            if (!unpaid.Any(o => o.Id == orderId))
            {
                Console.WriteLine("[Ошибка] Заказ не найден среди ваших неоплаченных заказов.");
                Pause();
                return;
            }

            Console.WriteLine("Способ оплаты: 1 - Банковская карта, 2 - Электронный кошелек");
            int methodChoice = ReadIntInRange("Выберите способ: ", 1, 2);
            string method = methodChoice == 1 ? "Банковская карта" : "Электронный кошелек";

            try
            {
                var payment = store.PayOrder(orderId, method);
                Console.WriteLine($"\n[ОК] Оплата проведена! {payment}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"\n[Ошибка оплаты] {ex.Message}");
            }
            Pause();
        }

        static void PrintClientOrders(OnlineStore store, Client client)
        {
            var orders = store.Orders.Where(o => o.Client.Id == client.Id).ToList();
            if (orders.Count == 0)
            {
                Console.WriteLine("\nИстория заказов пуста.");
                return;
            }
            foreach (var o in orders) o.Print();
        }

        static void AdminMenu(OnlineStore store, Administrator admin)
        {
            int selectItem = -1;
            while (selectItem != 0)
            {
                Console.WriteLine();
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine($"ПАНЕЛЬ АДМИНИСТРАТОРА: {admin.GetFullName()} ({admin.Position})");
                Console.WriteLine("--------------------------------------------------------------------------------");
                Console.WriteLine("1 - Просмотр каталога товаров");
                Console.WriteLine("2 - Добавить товар в каталог");
                Console.WriteLine("3 - Редактировать товар в каталоге");
                Console.WriteLine("4 - Удалить товар из каталога");
                Console.WriteLine("5 - Просмотр всех заказов");
                Console.WriteLine("6 - Просмотр неоплаченных заказов (неплательщиков)");
                Console.WriteLine("7 - Занести неплательщика в «черный список»");
                Console.WriteLine("8 - Просмотр и снятие блокировки «черного списка»");
                Console.WriteLine("0 - Выйти из аккаунта");
                Console.WriteLine("--------------------------------------------------------------------------------");

                selectItem = ReadIntInRange("Выберите пункт меню: ", 0, 8);
                switch (selectItem)
                {
                    case 1:
                        store.Catalog.Print();
                        Pause();
                        break;
                    case 2:
                        AdminAddProduct(store);
                        break;
                    case 3:
                        AdminUpdateProduct(store);
                        break;
                    case 4:
                        AdminDeleteProduct(store);
                        break;
                    case 5:
                        if (store.Orders.Count == 0) Console.WriteLine("Заказов нет.");
                        else foreach (var o in store.Orders) o.Print();
                        Pause();
                        break;
                    case 6:
                        AdminViewUnpaid(store);
                        Pause();
                        break;
                    case 7:
                        AdminBlacklistClient(store, admin);
                        break;
                    case 8:
                        AdminManageBlacklist(store);
                        break;
                    case 0:
                        Console.WriteLine("Выход из панели администратора.");
                        break;
                }
            }
        }

        static void AdminAddProduct(OnlineStore store)
        {
            try
            {
                Console.WriteLine("\n--- Добавление товара ---");
                string name = ReadNonEmpty("Наименование: ");
                string category = ReadNonEmpty("Категория: ");
                decimal price = ReadDecimalMin("Цена (руб.): ", 0.01m);
                int stock = ReadIntMin("Количество на складе: ", 0);
                var p = store.Catalog.AddProduct(name, category, price, stock);
                Console.WriteLine($"[ОК] Товар добавлен: {p}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            Pause();
        }

        static void AdminUpdateProduct(OnlineStore store)
        {
            store.Catalog.Print();
            int id = ReadIntMin("Введите ID товара для редактирования (0 - отмена): ", 0);
            if (id == 0) return;

            var existing = store.Catalog.FindById(id);
            if (existing == null)
            {
                Console.WriteLine("[Ошибка] Товар не найден.");
                Pause();
                return;
            }

            try
            {
                Console.WriteLine($"Текущее наименование: {existing.Name}");
                string name = ReadNonEmpty("Новое наименование: ");
                string category = ReadNonEmpty("Новая категория: ");
                decimal price = ReadDecimalMin("Новая цена (руб.): ", 0.01m);
                int stock = ReadIntMin("Новый остаток на складе: ", 0);
                store.Catalog.UpdateProduct(id, name, category, price, stock);
                Console.WriteLine("[ОК] Данные товара обновлены.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            Pause();
        }

        static void AdminDeleteProduct(OnlineStore store)
        {
            store.Catalog.Print();
            int id = ReadIntMin("Введите ID товара для удаления (0 - отмена): ", 0);
            if (id == 0) return;

            Console.WriteLine(store.Catalog.RemoveProduct(id)
                ? $"[ОК] Товар #{id} удален из каталога."
                : $"[Ошибка] Товар #{id} не найден.");
            Pause();
        }

        static void AdminViewUnpaid(OnlineStore store)
        {
            var unpaid = store.GetUnpaidOrders();
            Console.WriteLine("\n--- Неоплаченные заказы ---");
            if (unpaid.Count == 0)
            {
                Console.WriteLine("Неоплаченных заказов нет.");
                return;
            }
            foreach (var o in unpaid)
            {
                string bl = o.Client.IsBlacklisted ? " [УЖЕ В ЧС]" : "";
                Console.WriteLine($"Заказ #{o.Id} | Клиент ID={o.Client.Id} ({o.Client.GetFullName()}){bl} | Сумма: {o.CalculateTotal():N2} руб. | Статус: {o.GetStatusRu()}");
            }
        }

        static void AdminBlacklistClient(OnlineStore store, Administrator admin)
        {
            AdminViewUnpaid(store);
            Console.WriteLine("\nСписок всех клиентов:");
            foreach (var c in store.Clients) Console.WriteLine("  " + c);

            int clientId = ReadIntMin("Введите ID клиента для занесения в «черный список» (0 - отмена): ", 0);
            if (clientId == 0) return;

            Console.Write("Укажите причину (Enter для причины по умолчанию): ");
            string reason = Console.ReadLine() ?? "";
            try
            {
                var entry = store.BlacklistClient(clientId, reason, admin);
                Console.WriteLine($"[ОК] Клиент добавлен в «черный список»: {entry}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Ошибка] {ex.Message}");
            }
            Pause();
        }

        static void AdminManageBlacklist(OnlineStore store)
        {
            Console.WriteLine("\n--- Черный список неплательщиков ---");
            if (store.Blacklist.Count == 0)
            {
                Console.WriteLine("Черный список пуст.");
                Pause();
                return;
            }

            foreach (var entry in store.Blacklist)
                Console.WriteLine("  " + entry);

            int id = ReadIntMin("Введите ID клиента для исключения из ЧС (0 - назад): ", 0);
            if (id == 0) return;

            Console.WriteLine(store.RemoveFromBlacklist(id)
                ? $"[ОК] Клиент ID={id} исключен из «черного списка»."
                : $"[Ошибка] Клиент ID={id} не найден в «черном списке».");
            Pause();
        }

        static int ReadIntInRange(string prompt, int min, int max)
        {
            int val;
            bool ok;
            do
            {
                Console.Write(prompt);
                ok = int.TryParse(Console.ReadLine(), out val) && val >= min && val <= max;
                if (!ok) Console.WriteLine($"Ошибка ввода! Введите целое число от {min} до {max}.");
            } while (!ok);
            return val;
        }

        static int ReadIntMin(string prompt, int min)
        {
            int val;
            bool ok;
            do
            {
                Console.Write(prompt);
                ok = int.TryParse(Console.ReadLine(), out val) && val >= min;
                if (!ok) Console.WriteLine($"Ошибка ввода! Введите целое число >= {min}.");
            } while (!ok);
            return val;
        }

        static decimal ReadDecimalMin(string prompt, decimal min)
        {
            decimal val;
            bool ok;
            do
            {
                Console.Write(prompt);
                string raw = (Console.ReadLine() ?? "").Replace(',', '.');
                ok = decimal.TryParse(raw, System.Globalization.NumberStyles.Number,
                    System.Globalization.CultureInfo.InvariantCulture, out val) && val >= min;
                if (!ok) Console.WriteLine($"Ошибка ввода! Введите число >= {min}.");
            } while (!ok);
            return val;
        }

        static string ReadNonEmpty(string prompt)
        {
            string input;
            do
            {
                Console.Write(prompt);
                input = (Console.ReadLine() ?? "").Trim();
                if (string.IsNullOrEmpty(input)) Console.WriteLine("Поле не может быть пустым!");
            } while (string.IsNullOrEmpty(input));
            return input;
        }

        static void Pause()
        {
            Console.Write("\nНажмите Enter для продолжения...");
            Console.ReadLine();
        }
    }
}
