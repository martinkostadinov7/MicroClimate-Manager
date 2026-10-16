using Data.Models;
using Data.Repositories;
using Shared.DTOs.Setting;

namespace Services
{
    public class SettingsService(SettingsRepository settingsRepository)
    {
        public async Task EditSettingsAsync(List<SettingDto> dtos)
        {
            List<Setting> settingsFromDb = await settingsRepository.GetAllAsync();

            foreach (var dto in dtos)
            {
                var setting = settingsFromDb.FirstOrDefault(s => s.SettingName == dto.SettingName);
                if (setting != null)
                {
                    setting.Value = dto.Value;
                    setting.Applied = false;
                }
            }

            await settingsRepository.UpdateAsync(settingsFromDb);
        }

        public async Task<List<SettingDto>> GetAllUnappliedAsync()
        {
            List<Setting> settingsFromDb = await settingsRepository.GetAllAsync();

            List<SettingDto> settings = new List<SettingDto>();
            foreach (var setting in settingsFromDb)
            {
                if (!setting.Applied)
                {
                    settings.Add(
                        new SettingDto
                        {
                            SettingName = setting.SettingName!,
                            Value = setting.Value!
                        });
                }
            }
            return settings;
        }

        public async Task<List<SettingDto>> GetAllAsync()
        {
            List<Setting> settingsFromDb = await settingsRepository.GetAllAsync();

            List<SettingDto> settings = new List<SettingDto>();
            foreach (var setting in settingsFromDb)
            {
                settings.Add(
                    new SettingDto
                    {
                        SettingName = setting.SettingName!,
                        Value = setting.Value!
                    });
            }
            return settings;
        }

        public async Task ApplySettingsAsync(List<SettingDto> settingsToApply)
        {
            List<string> settingNames = new List<string>();
            foreach (var setting in settingsToApply)
            {
                settingNames.Add(setting.SettingName);
            }
            await settingsRepository.ApplySettingsByNamesAsync(settingNames);
        }
    }
}
