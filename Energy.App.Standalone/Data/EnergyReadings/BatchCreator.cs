namespace Energy.App.Standalone.Data.EnergyReadings;
public class Batch
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }

    public int BatchNumber { get; set; }
    public int BatchCount { get; set; }
}
public class BatchCreator
{
    public IEnumerable<Batch> GetBatches(DateTime startDate, DateTime endDate)
    {
        if (startDate >= endDate)
            throw new ArgumentException($"StartDate {startDate} cannot in future");

        int numberOfDays = (int)(endDate - startDate).TotalDays;

        int batchCount = numberOfDays / 90 + 1;

        DateTime batchStartDate = startDate;

        for (int batchNumber = 1; batchNumber <= batchCount; batchNumber++)
        {
            DateTime batchEndDate = batchStartDate.AddDays(90);
            if (batchEndDate > endDate)
                batchEndDate = endDate;

            yield return new Batch()
            {
                BatchNumber = batchNumber,
                BatchCount = batchCount,
                StartDate = batchStartDate,
                EndDate = batchEndDate
            };

            batchStartDate = batchEndDate;
        }
    }
}