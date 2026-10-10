using Data.Models;

namespace Data
{
    public static class SeedData
    {
        public static void SeedAll(AppDbContext db)
        {
            if (!db.Settings.Any())
            {
                SeedSettings(db);
            }
        }

        public static void SeedSettings(AppDbContext db)
        {
            List<Setting> settings =
            [
                new Setting { SettingName = "TemperatureMin", Value = "18", Applied = false},
                new Setting { SettingName = "TemperatureMax", Value = "24", Applied = false},
                new Setting { SettingName = "HumidityMin", Value = "50", Applied = false},
                new Setting { SettingName = "HumidityMax", Value = "70", Applied = false},
                new Setting { SettingName = "MoistureMin", Value = "60", Applied = false},
                new Setting { SettingName = "MoistureMax", Value = "80", Applied = false},
            ];
            db.Settings.AddRange(settings);
            db.SaveChanges();
        }
    }
}
