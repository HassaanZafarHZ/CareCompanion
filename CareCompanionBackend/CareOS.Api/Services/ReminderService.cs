using CareOS.Api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareOS.Api.Services
{
    public class ReminderService : IReminderService
    {
        private readonly IMongoCollection<Reminder> _reminders;
        public ReminderService(Data.MongoDbContext context)
        {
            _reminders = context.GetCollection<Reminder>("Reminders");
        }

        public async Task<List<Reminder>> GetRemindersByUserIdAsync(string userId)
        {
            return await _reminders.Find(r => r.UserId == userId && r.IsActive).ToListAsync();
        }

        public async Task<Reminder> CreateReminderAsync(Reminder reminder)
        {
            await _reminders.InsertOneAsync(reminder);
            return reminder;
        }

        public async Task<Reminder> UpdateReminderAsync(string id, Reminder reminder)
        {
            await _reminders.ReplaceOneAsync(r => r.Id == id, reminder);
            return reminder;
        }

        public async Task<bool> DeleteReminderAsync(string id)
        {
            var result = await _reminders.DeleteOneAsync(r => r.Id == id);
            return result.DeletedCount > 0;
        }
    }
}