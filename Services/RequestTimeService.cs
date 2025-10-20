namespace SmartGear.PM0902.Services;

public sealed class RequestTimeService : IRequestTimeService
{
    public DateTime UtcNow => DateTime.UtcNow;
}