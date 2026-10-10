using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class SettingsRepository
    {
        protected readonly AppDbContext db;

        public SettingsRepository(AppDbContext db)
        {
            this.db = db;
        }

        public async Task<Setting?> GetByNameAsync(string name)
        {
            Setting? setting = await db.Settings.SingleOrDefaultAsync(x => x.SettingName == name);

            return setting;
        }
        public async Task UpdateAsync(Setting setting)
        {
            db.Entry(setting).State = EntityState.Modified;
            await db.SaveChangesAsync();
        }

        public async Task<List<Setting>> GetAllAsync()
        {
            List<Setting> settings = await db.Settings.ToListAsync();
            return settings;
        }

        public async Task ApplySettingsByNamesAsync(List<string> settingNames)
        {
            await db.Settings
                .Where(s => settingNames.Contains(s.SettingName))
                .ExecuteUpdateAsync(s => s.SetProperty(b => b.Applied, true));
        }
    }
}
