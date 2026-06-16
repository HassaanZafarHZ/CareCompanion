using CareOS.Api.Hubs;
using CareOS.Api.Data;
using CareOS.Api.Helpers;
using CareOS.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// Register background scheduler for daily task assignment
builder.Services.AddHostedService<CareOS.Api.Background.DailyTaskScheduler>();

// ============================================
// 1. Configuration Settings (appsettings.json se load)
// ============================================
var mongoSettings = builder.Configuration.GetSection("MongoDbSettings").Get<MongoDbSettings>();
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();

// ============================================
// 2. MongoDB Connection
// ============================================
builder.Services.AddSingleton(mongoSettings!);
builder.Services.AddSingleton<MongoDbContext>();
builder.Services.AddHttpClient();
// ============================================
// 3. JWT Configuration
// ============================================
builder.Services.AddSingleton(jwtSettings!);
builder.Services.AddSingleton<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAssignmentService, AssignmentService>();
builder.Services.AddScoped<IMedicationService, MedicationService>();
builder.Services.AddScoped<IEmergencyService, EmergencyService>();
builder.Services.AddScoped<IDailyCheckInService, DailyCheckInService>();
builder.Services.AddScoped<IDietService, DietService>();
builder.Services.AddSingleton<IAiService, AiService>();
builder.Services.AddScoped<IPrescriptionService, PrescriptionService>();
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IHealthService, HealthService>();
builder.Services.AddScoped<ITaskService, TaskService>();
builder.Services.AddScoped<ICallService, CallService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IReminderService, ReminderService>();
builder.Services.AddScoped<ICheckInService, CheckInService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IFileUploadService, FileUploadService>();
builder.Services.AddScoped<IActivityService, ActivityService>();
var tesseractPath = builder.Configuration["TesseractPath"];
builder.Services.AddScoped<ILocalPrescriptionScannerService, LocalPrescriptionScannerService>();
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings!.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
    };
});

// ============================================
// 4. CORS Policy (Frontend se connect hone ke liye)
// ============================================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy => policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// ============================================
// 5. Controllers
// ============================================
builder.Services.AddControllers();

// ============================================
// 6. Swagger (API Testing UI)
// ============================================
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CareOS API",
        Version = "v1",
        Description = "Healthcare Management System for Elders & Caretakers"
    });

    // JWT Authorization in Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// ============================================
// 7. SignalR (Real-time Communication)
// ============================================
builder.Services.AddSignalR();


var app = builder.Build();

// ============================================
// 8. Middleware Pipeline
// ============================================
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CareOS API v1");
    });
}

app.UseHttpsRedirection();
app.UseCors("AllowFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// SignalR Hub mapping (baad mein use karenge)
app.MapHub<EmergencyHub>("/hubs/emergency").RequireCors("AllowFrontend");
app.MapHub<ChatHub>("/hubs/chat").RequireCors("AllowFrontend");
app.MapHub<CallHub>("/hubs/call").RequireCors("AllowFrontend");
app.MapHub<NotificationHub>("/hubs/notification").RequireCors("AllowFrontend");
// app.MapHub<EmergencyHub>("/hubs/emergency");

app.Run();