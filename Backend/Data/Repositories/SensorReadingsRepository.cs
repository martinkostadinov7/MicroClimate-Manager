using Data.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Dashboard;
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
        public async Task<List<TemperatureChartDataDto>> GetTemperatureChartDataAsync(DateTime from, DateTime to, int pointsCount = 1300)
        {
            if (pointsCount <= 0)
                throw new ArgumentException("Points count must be greater than 0.", nameof(pointsCount));

            var totalDuration = to - from;

            if (totalDuration <= TimeSpan.Zero)
                return new List<TemperatureChartDataDto>();

            var totalSeconds = totalDuration.TotalSeconds;

            var intervalSeconds = totalSeconds / pointsCount;
            if (intervalSeconds < 1) intervalSeconds = 1;

            var chartData = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)(EF.Functions.DateDiffSecond(from, s.CreatedAt) / intervalSeconds))
                .Select(g => new TemperatureChartDataDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Temperature = Math.Round(g.Average(s => s.Temperature), 2)
                })
                .OrderBy(dto => dto.DateTime)
                .ToListAsync();

            return chartData;
        }
        public async Task<List<HumidityChartDataDto>> GetHumidityChartDataAsync(DateTime from, DateTime to, int pointsCount = 1300)
        {
            if (pointsCount <= 0)
                throw new ArgumentException("Points count must be greater than 0.", nameof(pointsCount));

            var totalDuration = to - from;

            if (totalDuration <= TimeSpan.Zero)
                return new List<HumidityChartDataDto>();

            var totalSeconds = totalDuration.TotalSeconds;

            var intervalSeconds = totalSeconds / pointsCount;
            if (intervalSeconds < 1) intervalSeconds = 1;

            var chartData = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)(EF.Functions.DateDiffSecond(from, s.CreatedAt) / intervalSeconds))
                .Select(g => new HumidityChartDataDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Humidity = Math.Round(g.Average(s => s.Humidity), 2)
                })
                .OrderBy(dto => dto.DateTime)
                .ToListAsync();

            return chartData;
        }
        public async Task<List<MoistureChartDataDto>> GetMoistureChartDataAsync(DateTime from, DateTime to, int pointsCount = 1300)
        {
            if (pointsCount <= 0)
                throw new ArgumentException("Points count must be greater than 0.", nameof(pointsCount));

            var totalDuration = to - from;

            if (totalDuration <= TimeSpan.Zero)
                return new List<MoistureChartDataDto>();

            var totalSeconds = totalDuration.TotalSeconds;

            var intervalSeconds = totalSeconds / pointsCount;
            if (intervalSeconds < 1) intervalSeconds = 1;

            var chartData = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .GroupBy(s => (int)(EF.Functions.DateDiffSecond(from, s.CreatedAt) / intervalSeconds))
                .Select(g => new MoistureChartDataDto
                {
                    DateTime = g.Min(s => s.CreatedAt),
                    Moisture = Math.Round(g.Average(s => s.SoilMoisture), 2)
                })
                .OrderBy(dto => dto.DateTime)
                .ToListAsync();

            return chartData;
        }

        public async Task<StatisticsDto> GetStatisticsAsync()
        {
            var past24Hours = DateTime.UtcNow.AddHours(-24);

            var stats = await db.SensorReadings
                .Where(s => s.CreatedAt >= past24Hours)
                .GroupBy(s => 1)
                .Select(g => new StatisticsDto
                {
                    TemperatureLowest = g.Min(s => s.Temperature),
                    TemperatureHighest = g.Max(s => s.Temperature),
                    TemperatureAverage = Math.Round(g.Average(s => s.Temperature), 2),

                    HumidityLowest = g.Min(s => s.Humidity),
                    HumidityHighest = g.Max(s => s.Humidity),
                    HumidityAverage = Math.Round(g.Average(s => s.Humidity), 2),

                    MoistureLowest = g.Min(s => s.SoilMoisture),
                    MoistureHighest = g.Max(s => s.SoilMoisture),
                    MoistureAverage = Math.Round(g.Average(s => s.SoilMoisture), 2)
                })
                .FirstOrDefaultAsync();

            return stats ?? new StatisticsDto();
        }
    }
}