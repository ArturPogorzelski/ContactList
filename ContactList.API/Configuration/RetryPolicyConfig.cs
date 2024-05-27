namespace ContactList.API.Configuration
{
    public class RetryPolicyConfig
    {
        public int MaxRetries { get; set; } = 3; 
        public int BaseDelay { get; set; } = 1000;
        public bool Ekspon { get; set; } = false;
    }
}
