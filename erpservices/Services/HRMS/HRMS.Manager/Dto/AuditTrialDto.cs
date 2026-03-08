using System;

public class AuditTrialDto
{
    public string FileName { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public string ActivityType { get; set; }
    public string ActivityPeriod { get; set; }
    public string ActivityStatus { get; set; }
    public int UserId { get; set; }
}