namespace PRSWebApi.Models
{
    public class RequestStatusUpdateDto
    {
        public string Status { get; set; } = null!;
        public string? ReasonForRejection { get; set; }
    }
}
