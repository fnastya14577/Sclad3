using System;

namespace SkladUchet1.Models;

public class WriteOff
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public Batch? Batch { get; set; }
    public int UserId { get; set; }
    public int Quantity { get; set; }
    public DateTime Date { get; set; } = DateTime.Now;
    public string Reason { get; set; } = "";
}