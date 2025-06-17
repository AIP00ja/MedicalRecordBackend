namespace backend.Models
{
    public class MedicalFile
    {
        public int Id { get; set; }
        public string FileName { get; set; } = "";
        public string FileType { get; set; } = "";
        public string FileUrl { get; set; } = "";
        public string UploadedByEmail { get; set; } = "";
        public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    }
}
