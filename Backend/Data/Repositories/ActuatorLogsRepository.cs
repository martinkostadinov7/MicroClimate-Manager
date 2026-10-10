using Data.Models;
using Microsoft.EntityFrameworkCore;

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
    }
}
