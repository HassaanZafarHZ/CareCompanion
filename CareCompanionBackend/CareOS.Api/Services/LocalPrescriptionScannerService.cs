using System.Text.RegularExpressions;
using System.IO;
using Tesseract;
using CareOS.Api.DTOs;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace CareOS.Api.Services
{
  public class LocalPrescriptionScannerService : ILocalPrescriptionScannerService
    {
        private readonly ILogger<LocalPrescriptionScannerService> _logger;
        private readonly string _tessDataPath;

        // Common medicine names for better detection
        private static readonly string[] CommonMedicines = {
         "Paracetamol", "Aspirin", "Ibuprofen", "Amoxicillin", "Metformin",
            "Omeprazole", "Pantoprazole", "Cetirizine", "Azithromycin", "Ciprofloxacin",
            "Metronidazole", "Diclofenac", "Naproxen", "Ranitidine", "Losartan",
            "Amlodipine", "Atenolol", "Lisinopril", "Simvastatin", "Atorvastatin",
            "Gabapentin", "Tramadol", "Prednisone", "Dexamethasone", "Insulin",
            "Cough", "Syrup", "Tablet", "Capsule", "Vitamin", "Calcium", "Iron",
         "Antacid", "Antibiotic", "Painkiller", "Fever", "Cold", "Flu"
        };

        public LocalPrescriptionScannerService(ILogger<LocalPrescriptionScannerService> logger, IConfiguration configuration)
        {
            _logger = logger;
  _tessDataPath = configuration["TesseractDataPath"] ?? Path.Combine(AppContext.BaseDirectory, "tessdata");
        }

        public async Task<ApiResponse<PrescriptionAnalysisDto>> ScanPrescriptionLocallyAsync(string base64Image)
        {
       try
 {
  if (string.IsNullOrEmpty(base64Image))
               return ApiResponse<PrescriptionAnalysisDto>.ErrorResponse("Image is required");

     _logger.LogInformation("Starting local OCR prescription scan...");

      byte[] imageBytes;
      try
      {
    imageBytes = Convert.FromBase64String(
base64Image.Contains(",") ? base64Image.Split(',')[1] : base64Image
          );
  }
    catch (Exception ex)
        {
            _logger.LogError($"Invalid base64 image: {ex.Message}");
   return ApiResponse<PrescriptionAnalysisDto>.ErrorResponse("Invalid image format");
      }

byte[] processedImageBytes = await Task.Run(() => PreprocessImage(imageBytes));
     string extractedText = await Task.Run(() => ExtractTextFromImageBytes(processedImageBytes));
         
      if (string.IsNullOrEmpty(extractedText))
    {
      _logger.LogWarning("No text extracted from image");
 return ApiResponse<PrescriptionAnalysisDto>.ErrorResponse("Could not extract text. Please use clearer image or try Gemini AI.");
 }

   _logger.LogInformation($"OCR Extracted Text:\n{extractedText}");

         var analysis = ParsePrescriptionText(extractedText);

return ApiResponse<PrescriptionAnalysisDto>.SuccessResponse(
 analysis,
         "? Prescription scanned using OCR"
        );
   }
      catch (Exception ex)
            {
          _logger.LogError($"Local OCR Error: {ex.Message}");
        return ApiResponse<PrescriptionAnalysisDto>.ErrorResponse($"Failed to scan: {ex.Message}");
            }
        }

        private byte[] PreprocessImage(byte[] imageBytes)
        {
      try
     {
           using var image = Image.Load(imageBytes);
      if (image.Width < 200 || image.Height < 200)
image.Mutate(x => x.Resize(image.Width * 2, image.Height * 2));

          var outputStream = new MemoryStream();
       image.SaveAsPng(outputStream);
      outputStream.Position = 0;
       return outputStream.ToArray();
   }
     catch
  {
         return imageBytes;
      }
        }

        private string ExtractTextFromImageBytes(byte[] imageBytes)
    {
            try
       {
    using var engine = new TesseractEngine(_tessDataPath, "eng", EngineMode.Default);
     engine.DefaultPageSegMode = PageSegMode.Auto;

              string tempPath = Path.Combine(Path.GetTempPath(), $"ocr_{Guid.NewGuid()}.png");
       try
    {
                    using var ms = new MemoryStream(imageBytes);
     using var image = Image.Load(ms);
        image.SaveAsPng(tempPath);
  
            using var pix = Pix.LoadFromFile(tempPath);
        using var page = engine.Process(pix);
          return page.GetText()?.Trim() ?? string.Empty;
                }
    finally
     {
     if (File.Exists(tempPath)) File.Delete(tempPath);
           }
     }
      catch (Exception ex)
    {
    _logger.LogError($"OCR Error: {ex.Message}");
                throw;
         }
        }

        private PrescriptionAnalysisDto ParsePrescriptionText(string text)
    {
     return new PrescriptionAnalysisDto
            {
       DoctorName = ExtractDoctorName(text),
 PatientName = ExtractPatientName(text),
       PrescriptionDate = ExtractDate(text),
   RawText = text,
        Medicines = ExtractMedicines(text)
       };
        }

        private string ExtractDoctorName(string text)
        {
     var patterns = new[] {
        @"[Dd]r\.?\s*([A-Za-z]+(?:\s+[A-Za-z]+)*)",
        @"[Dd]octor[:\s]*([A-Za-z]+(?:\s+[A-Za-z]+)*)",
                @"[Pp]hysician[:\s]*([A-Za-z]+)"
          };
 foreach (var p in patterns)
        {
         var m = Regex.Match(text, p);
     if (m.Success && m.Groups[1].Value.Length > 2) return "Dr. " + m.Groups[1].Value.Trim();
            }
return "Doctor";
        }

  private string ExtractPatientName(string text)
        {
         var patterns = new[] {
    @"[Pp]atient[:\s]*([A-Za-z]+(?:\s+[A-Za-z]+)*)",
          @"[Nn]ame[:\s]*([A-Za-z]+(?:\s+[A-Za-z]+)*)",
          @"[Pp]t[:\.\s]*([A-Za-z]+)"
        };
      foreach (var p in patterns)
          {
                var m = Regex.Match(text, p);
         if (m.Success && m.Groups[1].Value.Length > 2) return m.Groups[1].Value.Trim();
  }
            return "Patient";
     }

  private DateTime? ExtractDate(string text)
        {
 var patterns = new[] { @"(\d{1,2})[/-](\d{1,2})[/-](\d{2,4})", @"(\d{4})[/-](\d{1,2})[/-](\d{1,2})" };
        foreach (var p in patterns)
  {
        var m = Regex.Match(text, p);
          if (m.Success && DateTime.TryParse(m.Value, out var date)) return date;
        }
   return DateTime.Now;
        }

        private List<ExtractedMedicine> ExtractMedicines(string text)
      {
            var medicines = new List<ExtractedMedicine>();
            var lines = text.Split('\n', StringSplitOptions.RemoveEmptyEntries);

    // Method 1: Look for common medicine names
   foreach (var commonMed in CommonMedicines)
    {
                if (text.Contains(commonMed, StringComparison.OrdinalIgnoreCase))
          {
   var existingMed = medicines.FirstOrDefault(m => 
             m.MedicineName.Contains(commonMed, StringComparison.OrdinalIgnoreCase));
  
    if (existingMed == null)
     {
       medicines.Add(CreateMedicineFromName(commonMed, text));
     }
      }
            }

            // Method 2: Look for lines with mg, ml, tablet patterns
      foreach (var line in lines)
{
      if (Regex.IsMatch(line, @"\d+\s*(mg|ml|tab|cap)", RegexOptions.IgnoreCase))
         {
    var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
         if (words.Length > 0)
         {
            var medName = words[0];
     if (medName.Length >= 3 && !medicines.Any(m => m.MedicineName.Equals(medName, StringComparison.OrdinalIgnoreCase)))
      {
      medicines.Add(CreateMedicineFromLine(line));
        }
    }
     }
            }

          // Method 3: Look for numbered items (1. Medicine, 2. Medicine)
            var numberedPattern = @"(\d+)[.\)]\s*([A-Za-z]+)";
         var numberedMatches = Regex.Matches(text, numberedPattern);
            foreach (Match m in numberedMatches)
    {
            var name = m.Groups[2].Value;
            if (name.Length >= 4 && !medicines.Any(med => med.MedicineName.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
        medicines.Add(CreateMedicineFromName(name, text));
       }
         }

        // If no medicines found, create placeholder
      if (!medicines.Any())
            {
        _logger.LogWarning("No medicines auto-extracted. Raw text available for manual entry.");
      medicines.Add(new ExtractedMedicine
        {
          MedicineName = "Manual Entry Required",
            Dosage = "Check prescription image",
  Frequency = "As prescribed",
        Duration = "As directed by doctor",
       SuggestedTimes = new List<string> { "08:00 AM", "08:00 PM" },
           Warnings = new List<string> { "?? Please add medicines manually based on prescription image" }
         });
            }

     return medicines;
        }

        private ExtractedMedicine CreateMedicineFromName(string name, string fullText)
        {
      var dosage = ExtractDosageNearMedicine(name, fullText);
    var frequency = ExtractFrequencyNearMedicine(name, fullText);

       return new ExtractedMedicine
   {
     MedicineName = char.ToUpper(name[0]) + name.Substring(1).ToLower(),
    Dosage = dosage,
           Frequency = frequency,
      Duration = "As prescribed",
       SuggestedTimes = GenerateSuggestedTimes(frequency),
                Warnings = GetMedicineWarnings(name)
       };
        }

        private ExtractedMedicine CreateMedicineFromLine(string line)
        {
        var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            var name = words.Length > 0 ? words[0] : "Medicine";
    var dosage = Regex.Match(line, @"\d+\s*(mg|ml|tab|cap)[a-z]*", RegexOptions.IgnoreCase).Value;
          var frequency = "Twice daily";

    if (line.Contains("OD", StringComparison.OrdinalIgnoreCase) || line.Contains("once", StringComparison.OrdinalIgnoreCase))
       frequency = "Once daily";
            else if (line.Contains("BD", StringComparison.OrdinalIgnoreCase) || line.Contains("twice", StringComparison.OrdinalIgnoreCase))
             frequency = "Twice daily";
            else if (line.Contains("TDS", StringComparison.OrdinalIgnoreCase) || line.Contains("three", StringComparison.OrdinalIgnoreCase))
       frequency = "Three times daily";

            return new ExtractedMedicine
      {
        MedicineName = char.ToUpper(name[0]) + name.Substring(1).ToLower(),
      Dosage = string.IsNullOrEmpty(dosage) ? "As prescribed" : dosage,
      Frequency = frequency,
                Duration = "As directed",
   SuggestedTimes = GenerateSuggestedTimes(frequency),
             Warnings = GetMedicineWarnings(name)
          };
        }

        private string ExtractDosageNearMedicine(string medicineName, string text)
        {
         var pattern = $@"{medicineName}\s*[\-:]*\s*(\d+\s*(mg|ml|tablet|tab|caps?|g))";
            var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase);
 return match.Success ? match.Groups[1].Value : "As prescribed";
        }

        private string ExtractFrequencyNearMedicine(string medicineName, string text)
        {
            var idx = text.IndexOf(medicineName, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
   var nearbyText = text.Substring(idx, Math.Min(100, text.Length - idx)).ToLower();
  if (nearbyText.Contains("od") || nearbyText.Contains("once")) return "Once daily";
        if (nearbyText.Contains("bd") || nearbyText.Contains("twice")) return "Twice daily";
             if (nearbyText.Contains("tds") || nearbyText.Contains("three")) return "Three times daily";
      if (nearbyText.Contains("qid") || nearbyText.Contains("four")) return "Four times daily";
     }
        return "Twice daily";
        }

        private List<string> GenerateSuggestedTimes(string frequency)
        {
       return frequency.ToLower() switch
            {
   "once daily" => new List<string> { "09:00 AM" },
   "twice daily" => new List<string> { "08:00 AM", "08:00 PM" },
        "three times daily" => new List<string> { "08:00 AM", "02:00 PM", "08:00 PM" },
    "four times daily" => new List<string> { "08:00 AM", "12:00 PM", "04:00 PM", "08:00 PM" },
                _ => new List<string> { "08:00 AM", "08:00 PM" }
   };
        }

  private List<string> GetMedicineWarnings(string name)
        {
        var n = name.ToLower();
         var warnings = new List<string>();

            if (n.Contains("paracetamol")) { warnings.Add("?? Max 4000mg/day"); warnings.Add("Avoid alcohol"); }
  else if (n.Contains("aspirin")) warnings.Add("?? Take after meals");
            else if (n.Contains("antibiotic") || n.Contains("amoxicillin") || n.Contains("azithromycin")) warnings.Add("? Complete full course");
         else if (n.Contains("metformin")) warnings.Add("?? Take with food");
        else if (n.Contains("ibuprofen") || n.Contains("diclofenac")) warnings.Add("?? Take with food");
            else if (n.Contains("omeprazole") || n.Contains("pantoprazole")) warnings.Add("?? Take before meals");
         
  return warnings.Count > 0 ? warnings : new List<string> { "?? Follow doctor's instructions" };
        }
    }

    public interface ILocalPrescriptionScannerService
    {
        Task<ApiResponse<PrescriptionAnalysisDto>> ScanPrescriptionLocallyAsync(string base64Image);
    }
}