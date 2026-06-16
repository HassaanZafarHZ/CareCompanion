using CareOS.Api.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CareOS.Api.Services
{
    public interface ICheckInService
    {
        Task<CheckIn> SubmitCheckInAsync(string userId, string status);
        Task<CheckIn> GetTodayCheckInAsync(string userId);
        Task<List<CheckIn>> GetCheckInHistoryAsync(string userId);
    }
}