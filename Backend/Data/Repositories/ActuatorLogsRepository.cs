using Data.Models;
using Microsoft.EntityFrameworkCore;
using Shared.DTOs.Dashboard.Statistics;

namespace Data.Repositories
{
    public class ActuatorLogsRepository
    {
        protected readonly AppDbContext db;

        public ActuatorLogsRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task AddAsync(ActuatorLog actuatorLog)
        {
            await db.ActuatorLogs.AddAsync(actuatorLog);
            await db.SaveChangesAsync();
        }

        public async Task<List<ActuatorLog>> GetAllAsync()
        {
            IQueryable<ActuatorLog> query = db.ActuatorLogs.AsQueryable();

            List<ActuatorLog> actuatorLogs = await query.ToListAsync();

            return actuatorLogs;
        }

        public async Task<List<ActuatorLog>> GetLatestAsync()
        {
            List<ActuatorLog> latestLogs = await db.ActuatorLogs
                .GroupBy(a => a.DeviceName)
                .Select(g => g.OrderByDescending(a => a.CreatedAt).FirstOrDefault()!)
                .ToListAsync();

            return latestLogs;
        }

        public async Task<List<HeaterStatisticsDto>> GetHeaterStatsAsync(DateTime from, DateTime to)
        {
            var readings = await db.ActuatorLogs
                .Where(s => s.CreatedAt >= from && s.CreatedAt <= to)
                .Where(s => s.DeviceName == "heater")
                .OrderBy(s => s.CreatedAt)
                .ToListAsync();

            return readings.Select(s => new HeaterStatisticsDto
            {
                DateTime = s.CreatedAt,
                Status = Convert.ToBoolean(s.State)
            }).ToList();
        }
    }
}
