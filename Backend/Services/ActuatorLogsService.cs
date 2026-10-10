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

        public async Task<List<ActuatorLogReadDto>> GetAllAsync()
        {
            List<ActuatorLog> actuatorLogsFromDb = await actuatorLogsRepository.GetAllAsync();

            List<ActuatorLogReadDto> actuatorLogsDtos = new List<ActuatorLogReadDto>();
            foreach (var actuatorLog in actuatorLogsFromDb)
            {
                actuatorLogsDtos.Add(
                    new ActuatorLogReadDto
                    {
                        DeviceName = actuatorLog.DeviceName,
                        State = actuatorLog.State,
                        CreatedAt = actuatorLog.CreatedAt
                    });
            }
            return actuatorLogsDtos;
        }
    }
}
