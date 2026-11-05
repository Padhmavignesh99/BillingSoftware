namespace MasterBilling.API.DTOs.Requests
{
    public class ImportResult
    {
        public int Success { get; set; }
        public int Failed { get; set; }
        public List<string> Errors { get; set; } = new();
        public List<string> Warnings { get; set; } = new();
    }

    public class ExportRequest
    {
        public DateTime? From { get; set; }
        public DateTime? To { get; set; }
        public string Format { get; set; } = "CSV"; // CSV, EXCEL
    }
}

