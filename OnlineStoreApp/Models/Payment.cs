namespace OnlineStoreApp.Models
{
    public class Payment
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string Method { get; set; }
        public DateTime PaidAt { get; set; }
        public bool IsSuccess { get; private set; }

        public Payment(int id, int orderId, decimal amount, string method)
        {
            Id = id;
            OrderId = orderId;
            Amount = amount;
            Method = string.IsNullOrWhiteSpace(method) ? "Банковская карта" : method.Trim();
            PaidAt = DateTime.Now;
            IsSuccess = false;
        }

        public bool Process()
        {
            if (Amount <= 0)
            {
                IsSuccess = false;
                return false;
            }
            PaidAt = DateTime.Now;
            IsSuccess = true;
            return true;
        }

        public override string ToString()
        {
            string state = IsSuccess ? "Успешно" : "Отклонен";
            return $"Платеж #{Id} по заказу #{OrderId} | {Amount:N2} руб. | {Method} | {PaidAt:dd.MM.yyyy HH:mm} | {state}";
        }
    }
}
