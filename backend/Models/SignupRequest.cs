namespace backend.Models
{
    public class SignupRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Gender { get; set; } = "";
        public string PhoneNumber { get; set; } = "";
        public string Password { get; set; } = "";
    }
}
