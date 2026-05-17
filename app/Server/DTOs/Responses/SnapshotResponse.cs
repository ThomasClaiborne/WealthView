namespace Server.DTOs.Responses;

public class SnapshotResponse
{
    public int      SnapshotId   { get; set; }
    public DateOnly SnapshotDate { get; set; }
    public decimal  TotalValue   { get; set; }
}