using CareOS.Api.DTOs;
using CareOS.Api.Models;

namespace CareOS.Api.Services
{
    public interface ICallService
    {
        Task<ApiResponse<Call>> InitiateCallAsync(InitiateCallDto request);
        Task<ApiResponse<Call>> AcceptCallAsync(string callId);
        Task<ApiResponse<Call>> DeclineCallAsync(string callId);
        Task<ApiResponse<Call>> EndCallAsync(EndCallDto request);
        Task<ApiResponse<List<Call>>> GetCallHistoryAsync(string userId);
        Task<ApiResponse<Call>> GetActiveCallAsync(string userId);
    }
}