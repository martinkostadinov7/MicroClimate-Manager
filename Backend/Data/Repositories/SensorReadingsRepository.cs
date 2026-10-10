using Data.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
