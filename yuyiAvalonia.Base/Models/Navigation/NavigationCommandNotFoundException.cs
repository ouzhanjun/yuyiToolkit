namespace yuyiAvalonia.Base.Models.Navigation
{
    [Serializable]
    public class NavigationCommandNotFoundException : ApplicationException
    {
        public NavigationCommandNotFoundException():base("Navigation command not found.")
        {
        }

        public NavigationCommandNotFoundException(string message) : base(message)
        {
        }

        public NavigationCommandNotFoundException(string message, Exception innerException) : 
            base(message, innerException)
        {
        }
    }
}