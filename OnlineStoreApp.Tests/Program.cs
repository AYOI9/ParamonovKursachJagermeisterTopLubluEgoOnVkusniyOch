using OnlineStoreApp.Models;
using OnlineStoreApp.Services;

int passed = 0;
int failed = 0;

void Assert(bool cond, string name)
{
    if (cond)
    {
        passed++;
        Console.WriteLine($"[PASS] {name}");
    }
    else
    {
        failed++;
        Console.WriteLine($"[FAIL] {name}");
    }
}

void AssertThrows<T>(Action act, string name) where T : Exception
{
    try
    {
        act();
        failed++;
        Console.WriteLine($"[FAIL] {name} (expected {typeof(T).Name})");
    }
    catch (T)
    {
        passed++;
        Console.WriteLine($"[PASS] {name}");
    }
}

var store = new OnlineStore();
store.SeedDemoData();

// 1. Authentication tests
var client = store.Authenticate("ivanov", "1234") as Client;
var admin = store.Authenticate("admin", "admin") as Administrator;
Assert(client != null && client.GetRole() == "Клиент", "Auth valid client");
Assert(admin != null && admin.GetRole() == "Администратор", "Auth valid admin");
Assert(store.Authenticate("ivanov", "wrong") == null, "Auth rejects wrong password");

// 2. Catalog CRUD
var added = store.Catalog.AddProduct("Веб-камера Logitech C920", "Периферия", 7500m, 4);
Assert(store.Catalog.FindById(added.Id) != null, "Catalog AddProduct");
Assert(store.Catalog.UpdateProduct(added.Id, "Веб-камера Logitech C920 Pro", "Периферия", 7900m, 6), "Catalog UpdateProduct");
Assert(store.Catalog.FindById(added.Id)?.Price == 7900m, "Catalog updated price persisted");
Assert(store.Catalog.Search("Logitech").Count == 1, "Catalog Search by name");
Assert(store.Catalog.RemoveProduct(added.Id), "Catalog RemoveProduct");
Assert(store.Catalog.FindById(added.Id) == null, "Catalog removed item absent");

// 3. Order & Payment
var product1 = store.Catalog.FindById(1)!;
int stockBefore = product1.StockQuantity;
var order = store.PlaceOrder(client!, new Dictionary<int, int> { { 1, 2 } });
Assert(order.Status == OrderStatus.PendingPayment, "Order initial status PendingPayment");
Assert(order.CalculateTotal() == product1.Price * 2, "Order total calculation");
Assert(product1.StockQuantity == stockBefore - 2, "Order deducts stock");

var payment = store.PayOrder(order.Id, "Банковская карта");
Assert(payment.IsSuccess && order.Status == OrderStatus.Paid, "PayOrder transitions status to Paid");
AssertThrows<InvalidOperationException>(() => store.PayOrder(order.Id, "Карта"), "PayOrder rejects duplicate payment");

// 4. Stock validation
AssertThrows<InvalidOperationException>(() =>
    store.PlaceOrder(client!, new Dictionary<int, int> { { 1, 999 } }), "PlaceOrder rejects insufficient stock");

// 5. Blacklist non-payers
var debtor = store.Clients.First(c => c.Login == "petrov");
Assert(store.GetUnpaidOrders().Any(o => o.Client.Id == debtor.Id), "Seeded unpaid order present");
var entry = store.BlacklistClient(debtor.Id, "Неоплата заказа", admin!);
Assert(debtor.IsBlacklisted && store.Blacklist.Contains(entry), "Admin blacklists non-payer");
AssertThrows<InvalidOperationException>(() =>
    store.PlaceOrder(debtor, new Dictionary<int, int> { { 4, 1 } }), "Blacklisted client blocked from ordering");
Assert(store.RemoveFromBlacklist(debtor.Id) && !debtor.IsBlacklisted, "Admin removes client from blacklist");

// 6. Registration
var reg = store.RegisterClient("Новиков", "Олег", "Игоревич", "novikov", "pass1", "+7-900-000-00-00", "nov@mail.ru");
Assert(store.Authenticate("novikov", "pass1") == reg, "Newly registered client can authenticate");
AssertThrows<InvalidOperationException>(() =>
    store.RegisterClient("Дубль", "Тест", "", "ivanov", "123", "1", "a@b.c"), "Duplicate login rejected");

Console.WriteLine($"\nSummary: {passed} passed, {failed} failed.");
return failed == 0 ? 0 : 1;
