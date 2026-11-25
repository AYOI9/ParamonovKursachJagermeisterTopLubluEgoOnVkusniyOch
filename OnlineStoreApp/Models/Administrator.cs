namespace OnlineStoreApp.Models
{
    public class Administrator : User
    {
        public string Position { get; set; }

        public Administrator(int id, string lastName, string firstName, string middleName,
            string login, string password, string position)
            : base(id, lastName, firstName, middleName, login, password)
        {
            Position = position;
        }

        public override string GetRole() => "Администратор";

        public override string ToString()
        {
            return $"[{Id}] {GetFullName()} ({Position})";
        }
    }
}
