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
        public async Task<ChartDataDto> GetChartDataAsync(QueryParamsChartDataDto queryParams)
        {
            DateTime from = queryParams.From;
            DateTime to = queryParams.To;
            int pointsCount = queryParams.PointsCount;

            List<TemperatureChartDataDto> temperatureChartDataDtos = await sensorReadingsRepository.GetTemperatureChartDataAsync(from, to, pointsCount);
            List<HumidityChartDataDto> humidityChartDataDtos = await sensorReadingsRepository.GetHumidityChartDataAsync(from, to, pointsCount);
            List<MoistureChartDataDto> moistureChartDataDtos = await sensorReadingsRepository.GetMoistureChartDataAsync(from, to, pointsCount);
            ChartDataDto chartDataDto = new()
            {
                Temperature = temperatureChartDataDtos,
                Humidity = humidityChartDataDtos,
                Moisture = moistureChartDataDtos
            };
            return chartDataDto;
        }

        public async Task<StatisticsDto> GetStatisticsAsync()
        {
            StatisticsDto statisticsDto = await sensorReadingsRepository.GetStatisticsAsync();
            return statisticsDto;
        }
    }
}
