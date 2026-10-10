using Data.Models;
using Data.Repositories;
using Shared.DTOs.Setting;

namespace Services
{
    public class SettingsService(SettingsRepository settingsRepository)
    {
        public async Task EditAsync(SettingDto dto) 
        {
            Setting setting = await settingsRepository.GetByNameAsync(dto.SettingName) ?? throw new NullReferenceException("Cannot found setting.");
            setting.Value = dto.Value;
            await settingsRepository.UpdateAsync(setting);
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
