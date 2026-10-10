using Data.Models;
using Data.Repositories;
using Shared.DTOs.SensorReading;

namespace Services
{
    public class SensorReadingsService(SensorReadingsRepository sensorReadingsRepository)
    {
        public async Task AddAsync(SensorReadingCreateDto dto)
        {
            SensorReading sensorReading = new SensorReading
            {
                Temperature = dto.Temperature,
                Humidity = dto.Humidity,
                SoilMoisture = dto.SoilMoisture,
                CreatedAt = DateTime.Now
            };

            await sensorReadingsRepository.AddAsync(sensorReading);
        }

        public async Task<List<SensorReadingReadDto>> GetAllAsync()
        {
            List<SensorReading> sensorReadingsFromDb = await sensorReadingsRepository.GetAllAsync();

            List<SensorReadingReadDto> sensorReadingDtos = new List<SensorReadingReadDto>();
            foreach (var sensorReading in sensorReadingsFromDb)
            {
                sensorReadingDtos.Add(
                    new SensorReadingReadDto
                    {
                        Temperature = sensorReading.Temperature,
                        Humidity = sensorReading.Humidity,
                        SoilMoisture = sensorReading.SoilMoisture,
                        CreatedAt = sensorReading.CreatedAt
                    });
            }
            return sensorReadingDtos;
        }
    }
}
