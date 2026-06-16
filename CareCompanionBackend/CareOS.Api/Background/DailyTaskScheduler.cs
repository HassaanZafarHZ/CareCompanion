using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CareOS.Api.Services;
using CareOS.Api.DTOs;
using MongoDB.Driver;

namespace CareOS.Api.Background
{
    public class DailyTaskScheduler : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        public DailyTaskScheduler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var now = DateTime.Now;
                var nextRun = now.Date.AddDays(1).AddHours(6); // Run at 6 AM daily
                if (now > nextRun)
                    nextRun = nextRun.AddDays(1);
                var delay = nextRun - now;
                await Task.Delay(delay, stoppingToken);
                await AssignDailyTasks();
            }
        }

        private async Task AssignDailyTasks()
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var taskService = scope.ServiceProvider.GetRequiredService<ITaskService>();
                var assignmentService = scope.ServiceProvider.GetRequiredService<IAssignmentService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<CareOS.Api.Data.MongoDbContext>();
                var users = dbContext.GetCollection<CareOS.Api.Models.User>("Users");
                var caretakers = await users.Find(u => u.Role == "CARETAKER" && u.IsActive).ToListAsync();
                foreach (var caretaker in caretakers)
                {
                    var assignmentsResult = await assignmentService.GetAssignmentsByCaretakerIdAsync(caretaker.Id);
                    if (assignmentsResult.Success && assignmentsResult.Data != null)
                    {
                        foreach (var assignment in assignmentsResult.Data)
                        {
                            var taskDto = new CreateTaskDto
                            {
                                ElderId = assignment.ElderId,
                                Title = "Daily Checkup",
                                Description = $"Rozana health checkup for {assignment.ElderName}.",
                                Priority = "High",
                                DueDate = DateTime.UtcNow.Date.AddHours(10) // 10 AM
                            };
                            await taskService.CreateTaskAsync(taskDto, caretaker.Id);
                        }
                    }
                }
            }
        }
    }
}
