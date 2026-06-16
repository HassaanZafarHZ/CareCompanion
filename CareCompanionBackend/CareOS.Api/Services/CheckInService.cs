using CareOS.Api.Models;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareOS.Api.Services
{
    public class CheckInService : ICheckInService
    {
        private readonly IMongoCollection<CheckIn> _checkIns;
        public CheckInService(Data.MongoDbContext context)
        {
            _checkIns = context.GetCollection<CheckIn>("CheckIns");
        }

        public async Task<CheckIn> SubmitCheckInAsync(string userId, string status)
        {
            var today = DateTime.UtcNow.Date;
            var checkIn = await _checkIns.Find(c => c.UserId == userId && c.Date == today).FirstOrDefaultAsync();
            if (checkIn == null)
            {
                checkIn = new CheckIn { UserId = userId, Date = today, Status = status, Timestamp = DateTime.UtcNow };
                await _checkIns.InsertOneAsync(checkIn);
            }
            else
            {
                checkIn.Status = status;
                checkIn.Timestamp = DateTime.UtcNow;
                await _checkIns.ReplaceOneAsync(c => c.Id == checkIn.Id, checkIn);
            }
            return checkIn;
        }

        public async Task<CheckIn> GetTodayCheckInAsync(string userId)
        {
            var today = DateTime.UtcNow.Date;
            return await _checkIns.Find(c => c.UserId == userId && c.Date == today).FirstOrDefaultAsync();
        }

        public async Task<List<CheckIn>> GetCheckInHistoryAsync(string userId)
        {
            return await _checkIns.Find(c => c.UserId == userId).SortByDescending(c => c.Date).ToListAsync();
        }
    }
}