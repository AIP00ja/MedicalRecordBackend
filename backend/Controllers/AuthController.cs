using backend.Data;
using backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;

        public AuthController(AppDbContext db)
        {
            _db = db;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupRequest request)
        {
            if (_db.Users.Any(u => u.Email == request.Email))
                return BadRequest("Email already registered.");

            var user = new User
            {
                FullName = request.FullName,
                Email = request.Email,
                Gender = request.Gender,
                PhoneNumber = request.PhoneNumber,
                PasswordHash = HashPassword(request.Password)
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Ok(new { message = "Signup successful" });
        }

        private string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null)
                return Unauthorized("Invalid email or password");

            var hashedInputPassword = HashPassword(request.Password);

            if (user.PasswordHash != hashedInputPassword)
                return Unauthorized("Invalid email or password");

            return Ok(new { message = "Login successful" });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Path.GetFileName(file.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var fileUrl = $"{Request.Scheme}://{Request.Host}/uploads/{Uri.EscapeDataString(fileName)}";
            return Ok(new { message = "File uploaded successfully", fileUrl });
        }

        [HttpPost("upload-metadata")]
        public async Task<IActionResult> SaveFileMetadata([FromBody] MedicalFile file)
        {
            if (string.IsNullOrEmpty(file.FileName) || string.IsNullOrEmpty(file.FileUrl))
                return BadRequest("Missing required fields.");

            _db.MedicalFiles.Add(file);
            await _db.SaveChangesAsync();

            return Ok(file);
        }
        [HttpGet("files/{email}")]
        public async Task<IActionResult> GetFilesByEmail(string email)
        {
            var files = await _db.MedicalFiles
                .Where(f => f.UploadedByEmail == email)
                .OrderByDescending(f => f.UploadedAt)
                .ToListAsync();

            return Ok(files);
        }

        [HttpDelete("file/{id}")]
        public async Task<IActionResult> DeleteFile(int id)
        {
            var file = await _db.MedicalFiles.FindAsync(id);
            if (file == null) return NotFound("File not found");

            
            var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
            var physicalPath = Path.Combine(uploadsPath, Path.GetFileName(file.FileUrl));
            if (System.IO.File.Exists(physicalPath))
            {
                System.IO.File.Delete(physicalPath);
            }

            _db.MedicalFiles.Remove(file);
            await _db.SaveChangesAsync();

            return Ok(new { message = "File deleted" });
        }

        [HttpPut("update-profile")]
        public async Task<IActionResult> UpdateProfile(
    [FromForm] string email,
    [FromForm] string fullName,
    [FromForm] string gender,
    [FromForm] string phoneNumber,
    IFormFile? profileImage)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound("User not found");

            user.FullName = fullName;
            user.Gender = gender;
            user.PhoneNumber = phoneNumber;

            if (profileImage != null && profileImage.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profiles");
                if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

                var fileName = Guid.NewGuid() + Path.GetExtension(profileImage.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }

                user.ProfileImagePath = $"/profiles/{fileName}";
            }

            await _db.SaveChangesAsync();
            return Ok(user);
        }
        [HttpGet("get-user/{email}")]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null) return NotFound();
            return Ok(user);
        }

    }
}
