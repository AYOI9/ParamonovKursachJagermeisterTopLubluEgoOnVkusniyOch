namespace OnlineStoreApp.Models
{
    public class Client : User
    {
        public string Phone { get; set; }
        public string Email { get; set; }
        public bool IsBlacklisted { get; set; }

        public Client(int id, string lastName, string firstName, string middleName,
            string login, string password, string phone, string email, bool isBlacklisted = false)
            : base(id, lastName, firstName, middleName, login, password)
        {
            Phone = phone;
            Email = email;
            IsBlacklisted = isBlacklisted;
        }

        public override string GetRole() => "Клиент";

        public override string ToString()
        {
            string status = IsBlacklisted ? "В черном списке" : "Активен";
            return $"[{Id}] {GetFullName()} | Тел: {Phone} | Email: {Email} | Статус: {status}";
        }
    }
}
