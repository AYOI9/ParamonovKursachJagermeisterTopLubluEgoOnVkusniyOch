namespace OnlineStoreApp.Models
{
    public class BlacklistEntry
    {
        public int ClientId { get; set; }
        public string ClientName { get; set; }
        public string Reason { get; set; }
        public DateTime AddedAt { get; set; }
        public string AddedBy { get; set; }

        public BlacklistEntry(int clientId, string clientName, string reason, string addedBy)
        {
            ClientId = clientId;
            ClientName = clientName;
            Reason = string.IsNullOrWhiteSpace(reason) ? "Нарушение сроков оплаты заказа" : reason.Trim();
            AddedAt = DateTime.Now;
            AddedBy = addedBy;
        }

        public override string ToString()
        {
            return $"ID клиента: {ClientId} | {ClientName} | Причина: {Reason} | Дата: {AddedAt:dd.MM.yyyy HH:mm} | Кем: {AddedBy}";
        }
    }
}
