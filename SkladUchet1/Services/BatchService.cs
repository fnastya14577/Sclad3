using System;
using System.Linq;
using SkladUchet1.Data;
using SkladUchet1.Models;

namespace SkladUchet1.Services;

public static class BatchService
{
    public static void WriteOff(int batchId, int amount, int userId, string reason)
    {
        using var db = new AppDbContext();
        var batch = db.Batches.FirstOrDefault(b => b.Id == batchId);

        if (batch == null) throw new Exception("Партия не найдена.");
        if (amount <= 0) throw new Exception("Количество должно быть больше нуля.");
        if (batch.Remaining < amount) throw new Exception($"Недостаточно товара. В наличии: {batch.Remaining}");

        batch.Remaining -= amount;
        batch.CalculateStatus();

        var writeOff = new WriteOff
        {
            BatchId = batch.Id,
            UserId = userId,
            Quantity = amount,
            Date = DateTime.Now,
            Reason = reason
        };


        db.WriteOffs.Add(writeOff);
        db.SaveChanges();
    }
}