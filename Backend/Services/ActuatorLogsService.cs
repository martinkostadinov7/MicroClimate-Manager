using Data.Models;
using Data.Repositories;
using Shared.DTOs.ActuatorLog;
using Shared.DTOs.SensorReading;

namespace Services
{
    public class ActuatorLogsService(ActuatorLogsRepository actuatorLogsRepository)
    {
        public async Task AddAsync(ActuatorLogCreateDto dto)
        {
            ActuatorLog actuatorLog = new ActuatorLog
            {
                DeviceName = dto.DeviceName,
                State = dto.State,
                CreatedAt = DateTime.Now
            };

            await actuatorLogsRepository.AddAsync(actuatorLog);
        }

        public async Task<List<ActuatorLogReadDto?>> GetAllAsync()
        {
            List<ActuatorLog> actuatorLogsFromDb = await actuatorLogsRepository.GetAllAsync();

            List<ActuatorLogReadDto?> actuatorLogsDtos = new List<ActuatorLogReadDto?>();
            foreach (var actuatorLog in actuatorLogsFromDb)
            {
                actuatorLogsDtos.Add(MapToDto(actuatorLog));
            }
            return actuatorLogsDtos;
        }

        public async Task<ActuatorLogsDto> GetLatestAsync()
        {
            List<ActuatorLog> latestActuatorLogs = await actuatorLogsRepository.GetLatestAsync();
            var dto = new ActuatorLogsDto
            {
                Heater = MapToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "heater")),
                Fan = MapToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "fan")),
                Pump = MapToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "pump"))
            };

            return dto;
        }

        private ActuatorLogReadDto? MapToDto(ActuatorLog? log)
        {
            if (log is null) return null;

            return new ActuatorLogReadDto
            {
                DeviceName = log.DeviceName,
                State = log.State,
                CreatedAt = log.CreatedAt
            };
        }
    }
}
