using Data.Models;
using Data.Repositories;
using Shared.DTOs.ActuatorLog;
using Shared.DTOs.Dashboard;
using Shared.DTOs.Dashboard.Statistics;
using Shared.DTOs.SensorReading;

namespace Services
{
    public class DashboardService(ActuatorLogsRepository actuatorLogsRepository, SensorReadingsRepository sensorReadingsRepository)
    {
        public async Task<LatestDataDto> GetLatestAsync()
        {
            List<ActuatorLog> latestActuatorLogs = await actuatorLogsRepository.GetLatestAsync();
            ActuatorLogsDto actuatorLogsDto = new ActuatorLogsDto
            {
                Heater = MapActuatorLogToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "heater")),
                Fan = MapActuatorLogToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "fan")),
                Pump = MapActuatorLogToDto(latestActuatorLogs.FirstOrDefault(l => l.DeviceName.ToLower() == "pump"))
            };

            SensorReading? sensorReadingFromDb = await sensorReadingsRepository.GetLatestAsync();

            SensorReadingReadDto sensorReadingsDto = new SensorReadingReadDto
            {
                Temperature = sensorReadingFromDb != null ? sensorReadingFromDb.Temperature : 0,
                Humidity = sensorReadingFromDb != null ? sensorReadingFromDb.Humidity : 0,
                SoilMoisture = sensorReadingFromDb != null ? sensorReadingFromDb.SoilMoisture : 0,
                CreatedAt = sensorReadingFromDb != null ? sensorReadingFromDb.CreatedAt : DateTime.Now
            };

            LatestDataDto latestDataDto = new LatestDataDto
            {
                LatestActuatorLogs = actuatorLogsDto,
                LatestSensorReading = sensorReadingsDto
            };
            return latestDataDto;
        }


        private ActuatorLogReadDto? MapActuatorLogToDto(ActuatorLog? log)
        {
            if (log is null) return null;

            return new ActuatorLogReadDto
            {
                DeviceName = log.DeviceName,
                State = log.State,
                CreatedAt = log.CreatedAt
            };
        }
        public async Task<StatisticsDto> GetStatisticsAsync(QueryParamsStatisticsDto queryParams)
        {
            DateTime from = queryParams.From;
            DateTime to = queryParams.To;
            List<TemperatureStatisticsDto> temperatureStatisticsDtos = await sensorReadingsRepository.GetTemperatureStatsAsync(from, to);
            List<HumidityStatisticsDto> humidityStatisticsDtos = await sensorReadingsRepository.GetHumidityStatsAsync(from, to);
            List<MoistureStatisticsDto> moistureStatisticsDtos = await sensorReadingsRepository.GetMoistureStatsAsync(from, to);
            List<HeaterStatisticsDto> heaterStatisticsDtos = await actuatorLogsRepository.GetHeaterStatsAsync(from, to);
            StatisticsDto statisticsDto = new StatisticsDto
            {
                Temperature = temperatureStatisticsDtos,
                Humidity = humidityStatisticsDtos,
                Moisture = moistureStatisticsDtos,
                Heater = heaterStatisticsDtos,
            };
            return statisticsDto;
        }
    }
}
