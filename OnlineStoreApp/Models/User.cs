namespace OnlineStoreApp.Models
{
    public abstract class User
    {
        public int Id { get; protected set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Login { get; protected set; }
        protected string Password { get; set; }

        protected User(int id, string lastName, string firstName, string middleName, string login, string password)
        {
            Id = id;
            LastName = lastName;
            FirstName = firstName;
            MiddleName = middleName;
            Login = login;
            Password = password;
        }

        public string GetFullName()
        {
            return $"{LastName} {FirstName} {MiddleName}".Trim();
        }

        public bool CheckPassword(string input)
        {
            return Password == input;
        }

        public abstract string GetRole();
    }
}
