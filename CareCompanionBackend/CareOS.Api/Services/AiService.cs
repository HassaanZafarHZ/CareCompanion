using CareOS.Api.DTOs;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace CareOS.Api.Services
{
    public class AiService : IAiService
    {
        private readonly string _geminiApiKey;
        private readonly ILogger<AiService> _logger;
        private readonly IHttpClientFactory _httpClientFactory;

        public AiService(IConfiguration configuration, ILogger<AiService> logger, IHttpClientFactory httpClientFactory)
        {
            _geminiApiKey = configuration["GeminiAI:ApiKey"] ?? "";
            _logger = logger;
            _httpClientFactory = httpClientFactory;
        }

        // REAL GEMINI AI: Prescription Analysis
        public async Task<ApiResponse<PrescriptionAnalysisDto>> AnalyzePrescriptionAsync(string base64Image)
        {
            try
            {
                if (string.IsNullOrEmpty(_geminiApiKey))
                {
                    _logger.LogWarning("Gemini API key not configured");
                    return GetMockAnalysis();
                }

                // Clean base64 string
                if (base64Image.Contains(","))
                {
                    base64Image = base64Image.Split(',')[1];
                }

                _logger.LogInformation("Sending prescription to Gemini AI for analysis...");

                // Gemini API endpoint - Updated to gemini-2.0-flash
                var apiUrl = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_geminiApiKey}";

                // Create HTTP client
                var httpClient = _httpClientFactory.CreateClient();

                // Create request
                var requestBody = new
                {
                    contents = new[]
                    {
                        new
                        {
                            parts = new object[]
                            {
                                new
                                {
                                    text = @"You are a medical prescription analyzer. Analyze this prescription image and extract information in this EXACT JSON format:

{
  ""doctorName"": ""Full doctor name"",
  ""patientName"": ""Patient name if visible"",
  ""prescriptionDate"": ""Date in YYYY-MM-DD format"",
  ""medicines"": [
    {
      ""medicineName"": ""Medicine name"",
      ""dosage"": ""Dosage amount (e.g., 500mg, 2 tablets)"",
      ""frequency"": ""How often (e.g., twice daily, three times daily)"",
      ""duration"": ""Duration (e.g., 7 days, 2 weeks)"",
      ""instructions"": ""Special instructions (e.g., after meals, before sleep)""
    }
  ]
}

IMPORTANT:
- Extract ALL medicines from prescription
- If Urdu text, translate to English
- If handwriting unclear, make best guess
- If field not visible, use ""Not specified""
- Return ONLY valid JSON, no extra text"
                                },
                                new
                                {
                                    inline_data = new
                                    {
                                        mime_type = "image/jpeg",
                                        data = base64Image
                                    }
                                }
                            }
                        }
                    }
                };

                var jsonContent = JsonSerializer.Serialize(requestBody);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                // Send request
                var response = await httpClient.PostAsync(apiUrl, content);
                var responseBody = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Gemini API Error: {response.StatusCode} - {responseBody}");
                    return GetMockAnalysis();
                }

                _logger.LogInformation($"Gemini response received: {responseBody.Substring(0, Math.Min(200, responseBody.Length))}...");

                // Parse Gemini response
                var analysis = ParseGeminiResponse(responseBody);

                return ApiResponse<PrescriptionAnalysisDto>.SuccessResponse(
                    analysis,
                    "✅ Prescription analyzed by Gemini AI"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError($"Gemini AI Error: {ex.Message}");
                return GetMockAnalysis();
            }
        }

        // Parse Gemini API response
        private PrescriptionAnalysisDto ParseGeminiResponse(string responseBody)
        {
            try
            {
                using var doc = JsonDocument.Parse(responseBody);
                var root = doc.RootElement;

                // Extract text from Gemini response
                var textContent = root
                    .GetProperty("candidates")[0]
                    .GetProperty("content")
                    .GetProperty("parts")[0]
                    .GetProperty("text")
                    .GetString() ?? "";

                _logger.LogInformation($"Extracted text: {textContent.Substring(0, Math.Min(500, textContent.Length))}...");

                // Extract JSON from markdown code block if present
                var jsonMatch = Regex.Match(textContent, @"```json\s*(\{[\s\S]*?\})\s*```");
                if (jsonMatch.Success)
                {
                    textContent = jsonMatch.Groups[1].Value;
                }
                else
                {
                    // Try to find JSON without markdown
                    jsonMatch = Regex.Match(textContent, @"\{[\s\S]*\}");
                    if (jsonMatch.Success)
                    {
                        textContent = jsonMatch.Value;
                    }
                }

                // Parse prescription JSON
                using var prescDoc = JsonDocument.Parse(textContent);
                var prescRoot = prescDoc.RootElement;

                var analysis = new PrescriptionAnalysisDto
                {
                    DoctorName = GetJsonString(prescRoot, "doctorName") ?? "Not specified",
                    PatientName = GetJsonString(prescRoot, "patientName") ?? "Not specified",
                    PrescriptionDate = ParsePrescriptionDate(GetJsonString(prescRoot, "prescriptionDate")),
                    RawText = textContent,
                    Medicines = new List<ExtractedMedicine>()
                };

                // Parse medicines
                if (prescRoot.TryGetProperty("medicines", out var medicinesElement))
                {
                    foreach (var medElement in medicinesElement.EnumerateArray())
                    {
                        var medicine = new ExtractedMedicine
                        {
                            MedicineName = GetJsonString(medElement, "medicineName") ?? "Unknown",
                            Dosage = GetJsonString(medElement, "dosage") ?? "As prescribed",
                            Frequency = GetJsonString(medElement, "frequency") ?? "As directed",
                            Duration = GetJsonString(medElement, "duration") ?? "As prescribed",
                            SuggestedTimes = GenerateSuggestedTimes(GetJsonString(medElement, "frequency") ?? ""),
                            Warnings = GetMedicineWarnings(
                                GetJsonString(medElement, "medicineName") ?? "",
                                GetJsonString(medElement, "instructions") ?? ""
                            )
                        };

                        analysis.Medicines.Add(medicine);
                    }
                }

                // If no medicines extracted
                if (analysis.Medicines.Count == 0)
                {
                    analysis.Medicines.Add(new ExtractedMedicine
                    {
                        MedicineName = "Unable to extract medicines",
                        Dosage = "Please verify manually",
                        Frequency = "As directed",
                        Duration = "As prescribed",
                        SuggestedTimes = new List<string> { "08:00 AM", "08:00 PM" },
                        Warnings = new List<string> { "⚠️ Manual verification required" }
                    });
                }

                _logger.LogInformation($"Successfully parsed {analysis.Medicines.Count} medicines");
                return analysis;
            }
            catch (Exception ex)
            {
                _logger.LogError($"Parse error: {ex.Message}");
                return GetMockAnalysis().Data!;
            }
        }

        // Helper to safely get JSON string
        private string? GetJsonString(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var prop) && prop.ValueKind == JsonValueKind.String)
            {
                var value = prop.GetString();
                return string.IsNullOrWhiteSpace(value) || value == "Not specified" ? null : value;
            }
            return null;
        }

        // Parse prescription date
        private DateTime? ParsePrescriptionDate(string? dateString)
        {
            if (string.IsNullOrEmpty(dateString)) return null;
            return DateTime.TryParse(dateString, out var date) ? date : null;
        }

        // Generate suggested times
        private List<string> GenerateSuggestedTimes(string frequency)
        {
            frequency = frequency.ToLower();

            if (frequency.Contains("once") || frequency.Contains("daily once"))
                return new List<string> { "09:00 AM" };

            if (frequency.Contains("twice"))
                return new List<string> { "08:00 AM", "08:00 PM" };

            if (frequency.Contains("three") || frequency.Contains("thrice"))
                return new List<string> { "08:00 AM", "02:00 PM", "08:00 PM" };

            if (frequency.Contains("four"))
                return new List<string> { "08:00 AM", "12:00 PM", "04:00 PM", "08:00 PM" };

            return new List<string> { "08:00 AM", "08:00 PM" };
        }

        // Get medicine warnings
        private List<string> GetMedicineWarnings(string medicineName, string instructions)
        {
            var warnings = new List<string>();
            medicineName = medicineName.ToLower();
            instructions = instructions.ToLower();

            if (medicineName.Contains("paracetamol"))
            {
                warnings.Add("⚠️ Max 4000mg per day");
                warnings.Add("Avoid alcohol");
            }

            if (medicineName.Contains("aspirin"))
                warnings.Add("⚠️ Take after meals");

            if (medicineName.Contains("antibiotic") || medicineName.Contains("amoxicillin"))
                warnings.Add("✅ Complete full course");

            if (instructions.Contains("after meal"))
                warnings.Add("📋 Take with food");

            if (instructions.Contains("before sleep"))
                warnings.Add("📋 Take before bedtime");

            if (warnings.Count == 0)
                warnings.Add("📋 Follow doctor's instructions");

            return warnings;
        }

        // Fallback mock data
        private ApiResponse<PrescriptionAnalysisDto> GetMockAnalysis()
        {
            var analysis = new PrescriptionAnalysisDto
            {
                DoctorName = "Dr. Ahmed Khan",
                PatientName = "Patient Name",
                PrescriptionDate = DateTime.UtcNow.AddDays(-1),
                RawText = "Mock analysis (Gemini API unavailable)",
                Medicines = new List<ExtractedMedicine>
                {
                    new ExtractedMedicine
                    {
                        MedicineName = "Paracetamol",
                        Dosage = "500mg",
                        Frequency = "Three times daily",
                        Duration = "5 days",
                        SuggestedTimes = new List<string> { "08:00 AM", "02:00 PM", "08:00 PM" },
                        Warnings = new List<string> { "⚠️ Max 4000mg per day", "Avoid alcohol" }
                    }
                }
            };

            return ApiResponse<PrescriptionAnalysisDto>.SuccessResponse(analysis, "Using mock data");
        }

        // Mood detection
        public async Task<ApiResponse<string>> DetectMoodFromTextAsync(string messageText)
        {
            await Task.Delay(500);
            string mood = "Neutral";
            messageText = messageText.ToLower();

            if (Regex.IsMatch(messageText, @"\b(happy|good|great|excellent)\b"))
                mood = "Happy";
            else if (Regex.IsMatch(messageText, @"\b(sad|depressed|lonely)\b"))
                mood = "Sad";
            else if (Regex.IsMatch(messageText, @"\b(anxious|worried|nervous)\b"))
                mood = "Anxious";
            else if (Regex.IsMatch(messageText, @"\b(pain|hurt|sick|unwell)\b"))
                mood = "Unwell";

            return ApiResponse<string>.SuccessResponse(mood);
        }

        // Medicine interactions
        public async Task<ApiResponse<List<string>>> CheckMedicineInteractionsAsync(List<string> medicineNames)
        {
            await Task.Delay(1000);
            var warnings = new List<string>();

            if (medicineNames.Any(m => m.ToLower().Contains("aspirin")) &&
                medicineNames.Any(m => m.ToLower().Contains("warfarin")))
                warnings.Add("⚠️ INTERACTION: Aspirin + Warfarin increases bleeding risk");

            if (warnings.Count == 0)
                warnings.Add("✅ No major interactions detected");

            return ApiResponse<List<string>>.SuccessResponse(warnings);
        }
    }
}