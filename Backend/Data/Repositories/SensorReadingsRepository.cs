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
        public async Task<List<TemperatureStatisticsDto>> GetTemperatureStatsAsync(DateTime from, DateTime to)
        {
            var readings = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return readings.Select(s => new TemperatureStatisticsDto
            {
                DateTime = s.CreatedAt,
                Temperature = s.Temperature
            }).ToList();
        }
        public async Task<List<HumidityStatisticsDto>> GetHumidityStatsAsync(DateTime from, DateTime to)
        {
            var readings = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return readings.Select(s => new HumidityStatisticsDto
            {
                DateTime = s.CreatedAt,
                Humidity = s.Humidity
            }).ToList();
        }

        public async Task<List<MoistureStatisticsDto>> GetMoistureStatsAsync(DateTime from, DateTime to)
        {
            var readings = await db.SensorReadings
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return readings.Select(s => new MoistureStatisticsDto
            {
                DateTime = s.CreatedAt,
                Moisture = s.SoilMoisture
            }).ToList();
        }
    }
}