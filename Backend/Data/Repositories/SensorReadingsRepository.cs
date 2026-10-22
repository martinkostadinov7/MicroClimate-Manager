using Data.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Dashboard.Statistics;

namespace Data.Repositories
{
    public class SensorReadingsRepository
    {
        protected readonly AppDbContext db;

        public SensorReadingsRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task AddAsync(SensorReading sensorReading)
        {
            await db.SensorReadings.AddAsync(sensorReading);
            await db.SaveChangesAsync();
        }

        public async Task<List<SensorReading>> GetAllAsync()
        {
            IQueryable<SensorReading> query = db.SensorReadings.AsQueryable();

            List<SensorReading> sensorReadings = await query.ToListAsync();

            return sensorReadings;
        }


        public async Task<SensorReading?> GetLatestAsync()
        {
            IQueryable<SensorReading> query = db.SensorReadings.AsQueryable();

            query = query.OrderByDescending(s => s.CreatedAt);

            SensorReading? latestSensorReading = await query.FirstOrDefaultAsync();

            return latestSensorReading;
        }
        public async Task<List<TemperatureRecordDto>> GetTemperatureRecordsAsync(DateTime from, DateTime to, int targetPointsCount = 1300)
        {
            var totalDuration = to - from;

            if (totalDuration.TotalSeconds <= 0)
                return new List<TemperatureRecordDto>();

            double windowSeconds = totalDuration.TotalSeconds / targetPointsCount;
           
            if (windowSeconds < 1)
            {
                return await db.SensorReadings
                    .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                    .OrderBy(s => s.CreatedAt)
                    .Select(s => new TemperatureRecordDto
                    {
                        DateTime = s.CreatedAt,
                        Temperature = s.Temperature
                    })
                    .ToListAsync();
            }

            var result = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)EF.Functions.DateDiffSecond(from, s.CreatedAt) / (int)windowSeconds)
                .Select(g => new TemperatureRecordDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Temperature = g.Average(s => s.Temperature)
                })
                .OrderBy(s => s.DateTime)
                .ToListAsync();

            return result;
        }
        public async Task<List<HumidityRecordDto>> GetHumidityRecordsAsync(DateTime from, DateTime to, int targetPointsCount = 1300)
        {
            var totalDuration = to - from;

            if (totalDuration.TotalSeconds <= 0)
                return new List<HumidityRecordDto>();

            double windowSeconds = totalDuration.TotalSeconds / targetPointsCount;

            if (windowSeconds < 1)
            {
                return await db.SensorReadings
                    .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                    .OrderBy(s => s.CreatedAt)
                    .Select(s => new HumidityRecordDto
                    {
                        DateTime = s.CreatedAt,
                        Humidity = s.Humidity
                    })
                    .ToListAsync();
            }

            var result = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)EF.Functions.DateDiffSecond(from, s.CreatedAt) / (int)windowSeconds)
                .Select(g => new HumidityRecordDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Humidity = g.Average(s => s.Humidity)
                })
                .OrderBy(s => s.DateTime)
                .ToListAsync();

            return result;
        }

        public async Task<List<MoistureRecordDto>> GetMoistureRecordsAsync(DateTime from, DateTime to, int targetPointsCount = 1300)
        {
            var totalDuration = to - from;

            if (totalDuration.TotalSeconds <= 0)
                return new List<MoistureRecordDto>();

            double windowSeconds = totalDuration.TotalSeconds / targetPointsCount;

            if (windowSeconds < 1)
            {
                return await db.SensorReadings
                    .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                    .OrderBy(s => s.CreatedAt)
                    .Select(s => new MoistureRecordDto
                    {
                        DateTime = s.CreatedAt,
                        Moisture = s.SoilMoisture
                    })
                    .ToListAsync();
            }

            var result = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)EF.Functions.DateDiffSecond(from, s.CreatedAt) / (int)windowSeconds)
                .Select(g => new MoistureRecordDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Moisture = g.Average(s => s.SoilMoisture)
                })
                .OrderBy(s => s.DateTime)
                .ToListAsync();

            return result;
        }
    }
}