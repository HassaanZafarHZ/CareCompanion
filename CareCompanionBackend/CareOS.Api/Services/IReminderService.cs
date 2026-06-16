using CareOS.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareOS.Api.Services
{
    public interface IReminderService
    {
        Task<List<Reminder>> GetRemindersByUserIdAsync(string userId);
        Task<Reminder> CreateReminderAsync(Reminder reminder);
        Task<Reminder> UpdateReminderAsync(string id, Reminder reminder);
        Task<bool> DeleteReminderAsync(string id);
    }
}