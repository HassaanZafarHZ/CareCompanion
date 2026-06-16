using CareOS.Api.Data;
using CareOS.Api.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace CareOS.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DatabaseTestController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public DatabaseTestController(MongoDbContext context)
        {
            _context = context;
        }

        // GET: api/DatabaseTest/check-connection
        [HttpGet("check-connection")]
        public async Task<IActionResult> CheckConnection()
        {
            try
            {
                // Test connection by counting documents
                var collection = _context.GetCollection<User>("Users");
                var count = await collection.CountDocumentsAsync(FilterDefinition<User>.Empty);

                return Ok(new
                {
                    success = true,
                    message = "✅ Local MongoDB connection successful!",
                    connectionString = "mongodb://localhost:27017",
                    database = "CareOsDb",
                    collection = "Users",
                    documentCount = count,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    message = "❌ MongoDB connection failed!",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    hint = "Make sure MongoDB service is running (check Windows Services)"
                });
            }
        }

        // POST: api/DatabaseTest/create-test-user (Test ke liye)
        [HttpPost("create-test-user")]
        public async Task<IActionResult> CreateTestUser()
        {
            try
            {
                var collection = _context.GetCollection<User>("Users");

                var testUser = new User
                {
                    FullName = "Test Elder",
                    Email = "test@careos.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Test@123"),
                    Pin = "9999",
                    Role = "ELDER",
                    PhoneNumber = "+92-300-0000000",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                await collection.InsertOneAsync(testUser);

                return Ok(new
                {
                    success = true,
                    message = "✅ Test user created successfully!",
                    userId = testUser.Id,
                    userName = testUser.FullName
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // GET: api/DatabaseTest/list-users
        [HttpGet("list-users")]
        public async Task<IActionResult> ListUsers()
        {
            try
            {
                var collection = _context.GetCollection<User>("Users");
                var users = await collection.Find(FilterDefinition<User>.Empty).ToListAsync();

                return Ok(new
                {
                    success = true,
                    count = users.Count,
                    users = users.Select(u => new
                    {
                        id = u.Id,
                        name = u.FullName,
                        email = u.Email,
                        role = u.Role,
                        pin = u.Pin
                    })
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }

        // DELETE: api/DatabaseTest/clear-database (Database reset ke liye)
        [HttpDelete("clear-database")]
        public async Task<IActionResult> ClearDatabase()
        {
            try
            {
                var usersCollection = _context.GetCollection<User>("Users");
                var assignmentsCollection = _context.GetCollection<Models.ElderCaretakerAssignment>("Assignments");

                await usersCollection.DeleteManyAsync(FilterDefinition<User>.Empty);
                await assignmentsCollection.DeleteManyAsync(FilterDefinition<Models.ElderCaretakerAssignment>.Empty);

                return Ok(new
                {
                    success = true,
                    message = "✅ Database cleared successfully!"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    success = false,
                    error = ex.Message
                });
            }
        }
    }
}