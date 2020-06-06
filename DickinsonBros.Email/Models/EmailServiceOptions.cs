using System.Diagnostics.CodeAnalysis;

namespace DickinsonBros.Email.Models
{
    [ExcludeFromCodeCoverage]
    public class EmailServiceOptions
    {
        public int SmtpTimeoutSeconds { get; set; }
        public string Password { get; set; }
        public string SaveDirectory { get; set; }
        public bool SaveEmail { get; set; }
        public bool SendSmtp { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
    }
}
