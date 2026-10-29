async function getData() {
    const url = "https://microclimatemanager.runasp.net/api/dashboard/latestData";
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
      }

      const result = await response.json();

      const temperature = document.getElementById("temperature");
      const humidity = document.getElementById("humidity");
      const moisture = document.getElementById("moisture");
      const lastRefreshed = document.getElementById("lastRefreshed");

      if (result.latestSensorReading) {
        temperature.textContent = result.latestSensorReading.temperature;
        humidity.textContent = result.latestSensorReading.humidity;
        moisture.textContent = result.latestSensorReading.soilMoisture;
        
        const readingTime = result.latestSensorReading.createdAt || result.createdAt;
        lastRefreshed.textContent = " " + String(formatTimeAgo(readingTime));
      } else {
        temperature.textContent = "No data";
        humidity.textContent = "No data";
        moisture.textContent = "No data";
        lastRefreshed.textContent = "No data";
      }

      const heater = document.getElementById("heater");
      const fan = document.getElementById("fan");
      const pump = document.getElementById("pump");

      const actuators = result.latestActuatorLogs;
      heater.textContent = formatState(actuators?.heater?.state);
      fan.textContent = formatState(actuators?.fan?.state);
      pump.textContent = formatState(actuators?.pump?.state);

      console.log(result);
    } catch (error) {
      console.error(error.message);
    }
  }

  async function get24HourStatistics() {
    const url = "https://microclimatemanager.runasp.net/api/dashboard/statistics";
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
      }

      const stats = await response.json();

      document.getElementById("tempMin").textContent = stats.temperatureLowest ?? "--";
      document.getElementById("tempAvg").textContent = stats.temperatureAverage ?? "--";
      document.getElementById("tempMax").textContent = stats.temperatureHighest ?? "--";

      document.getElementById("humMin").textContent = stats.humidityLowest ?? "--";
      document.getElementById("humAvg").textContent = stats.humidityAverage ?? "--";
      document.getElementById("humMax").textContent = stats.humidityHighest ?? "--";

      document.getElementById("moistMin").textContent = stats.moistureLowest ?? "--";
      document.getElementById("moistAvg").textContent = stats.moistureAverage ?? "--";
      document.getElementById("moistMax").textContent = stats.moistureHighest ?? "--";

    } catch (error) {
      console.error("Грешка при извличане на 24-часовите статистики:", error.message);
    }
  }

  function formatState(state) {
    if (state === null || state === undefined) return "No data";
    return state === 1 ? "ON" : "OFF";
  }

  function formatTimeAgo(isoString) {
    if (!isoString) return 'No data';

    const date = new Date(isoString); 
    const now = new Date();

    const elapsedSeconds = Math.round((date.getTime() - now.getTime()) / 1000);
    const rtf = new Intl.RelativeTimeFormat('en-US', { numeric: 'auto' });

    const minute = 60;
    const hour = minute * 60;
    const day = hour * 24;
    const month = day * 30;

    const absSeconds = Math.abs(elapsedSeconds);

    if (absSeconds < 45) {
      return 'just now';
    } else if (absSeconds < hour) {
      return rtf.format(Math.round(elapsedSeconds / minute), 'minute');
    } else if (absSeconds < day) {
      return rtf.format(Math.round(elapsedSeconds / hour), 'hour');
    } else if (absSeconds < month) {
      return rtf.format(Math.round(elapsedSeconds / day), 'day');
    } else {
      return rtf.format(Math.round(elapsedSeconds / month), 'month');
    }
  }

  async function startAutoRefresh() {
    await getData();
    await get24HourStatistics();
    setTimeout(startAutoRefresh, 60000); 
  }

  startAutoRefresh();

  let temperatureChartInstance = null;
  let humidityChartInstance = null;
  let moistureChartInstance = null;

  // Инициализация на датите
  const now = new Date();
  const yesterday = new Date(Date.now() - 1 * 24 * 60 * 60 * 1000);

  const dateFromInput = document.getElementById('dateFrom');
  const dateToInput = document.getElementById('dateTo');
  const pointsCountInput = document.getElementById('pointsCount');

  const initialFrom = formatForDateTimeLocal(yesterday);
  const initialTo = formatForDateTimeLocal(now);
  const initialPointsCount = pointsCountInput ? pointsCountInput.value : 500;

  if (dateFromInput) dateFromInput.value = initialFrom;
  if (dateToInput) dateToInput.value = initialTo;

  // Зареждаме графиките с локалното време и избраните точки
  loadDashboardCharts(initialFrom, initialTo, initialPointsCount);

  const applyButton = document.getElementById('applyFilterButton'); 
  applyButton?.addEventListener('click', () => {
    if (dateFromInput.value && dateToInput.value) {
      const fromIso = dateFromInput.value; 
      const toIso = dateToInput.value;
      const pCount = pointsCountInput ? pointsCountInput.value : 500;
      
      loadDashboardCharts(fromIso, toIso, pCount);
    } else {
      alert("Please select both 'From' and 'To' dates.");
    }
  });

  function formatForDateTimeLocal(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    const hours = String(date.getHours()).padStart(2, '0');
    const minutes = String(date.getMinutes()).padStart(2, '0');
    return `${year}-${month}-${day}T${hours}:${minutes}`;
  }

  // Обновена функция - приема pointsCount като параметър
  async function loadDashboardCharts(fromDate, toDate, pointsCount) {
    const url = `https://microclimatemanager.runasp.net/api/dashboard/chartData?from=${encodeURIComponent(fromDate)}&to=${encodeURIComponent(toDate)}&pointsCount=${pointsCount}`;
    console.log(fromDate);
    console.log(toDate);
    try {
      const response = await fetch(url);
      if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
      }

      const data = await response.json();
      console.log(data);

      const rawTempValues = (data.temperature || []).map(stat => ({
        x: stat.dateTime ?? stat.DateTime,
        y: stat.temperature ?? stat.Temperature
      }));
      // Използваме динамичния insertNullGaps
      const tempValues = insertNullGaps(rawTempValues);

      let minTemp = null;
      let maxTemp = null;

      const numericValues = rawTempValues
        .map(item => item.y)
        .filter(val => typeof val === 'number' && !isNaN(val));

      if (numericValues.length > 0) {
        minTemp = Math.min(...numericValues) - 2;
        maxTemp = Math.max(...numericValues) + 2; 
      }

      const rawHumidityValues = (data.humidity || []).map(stat => ({
        x: stat.dateTime ?? stat.DateTime,
        y: stat.humidity ?? stat.Humidity
      }));
      // Използваме динамичния insertNullGaps
      const humidityValues = insertNullGaps(rawHumidityValues);

      renderCombinedChart(
        tempValues, 
        humidityValues, 
        fromDate, 
        toDate, 
        minTemp, 
        maxTemp
      );

      const rawMoistureStats = (data.moisture || []).map(stat => ({
        x: stat.dateTime ?? stat.DateTime,
        y: stat.moisture ?? stat.Moisture
      }));
      const moistureValues = insertNullGaps(rawMoistureStats);

      let minMoisture = 0;
      let maxMoisture = 100;

      renderMoistureChart(moistureValues, fromDate, toDate, minMoisture, maxMoisture);

    } catch (error) {
      console.error("Грешка при извличане на статистиките:", error);
    }
  }

  function formatDateLabel(dateTimeStr) {
    if (!dateTimeStr) return '';
    const date = new Date(dateTimeStr);
    return date.toLocaleString('en-US', {
      day: '2-digit',
      month: 'short',
      hour: '2-digit',
      minute: '2-digit'
    });
  }

  function renderMoistureChart(values, minDate, maxDate, minY, maxY) {
    const ctx = document.getElementById('moistureChart');
    if (!ctx) return;

    if (moistureChartInstance) {
      moistureChartInstance.destroy();
    }

    moistureChartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        datasets: [{
          label: 'Soil Moisture (%)',
          data: values,
          borderColor: '#803434',
          backgroundColor: 'rgba(199, 98, 98, 0.15)',
          borderWidth: 2,
          tension: 0.4,
          fill: true,
          spanGaps: false,
          pointRadius: 0,
          pointHoverRadius: 6
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true, position: 'top' },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: {
            type: 'time',
            time: {
              unit: 'day',
              displayFormats: {
                day: 'MMM dd',
                hour: 'MMM dd, HH:mm'
              }
            },
            min: minDate,
            max: maxDate,
            display: true,
            title: { display: false, text: 'Time' },
            ticks: { maxTicksLimit: 10 }
          },
          y: {
            display: true,
            title: { display: true, text: '%' },
            beginAtZero: false,
            min: minY !== null ? minY : undefined,
            max: maxY !== null ? maxY : undefined
          }
        }
      }
    });
  }

  let combinedChartInstance = null;

  function renderCombinedChart(tempValues, humidityValues, minDate, maxDate, minTemp, maxTemp) {
    const ctx = document.getElementById('tempHumidityChart');
    if (!ctx) return;

    if (combinedChartInstance) {
      combinedChartInstance.destroy();
    }

    const chartCtx = ctx.getContext('2d');

    const tempGradient = chartCtx.createLinearGradient(0, 0, 0, 400);
    tempGradient.addColorStop(0, 'rgba(255, 25, 0, 0.35)');
    tempGradient.addColorStop(1, 'rgba(255, 25, 0, 0.0)');

    const humidityGradient = chartCtx.createLinearGradient(0, 0, 0, 400);
    humidityGradient.addColorStop(0, 'rgba(0, 72, 255, 0.25)');
    humidityGradient.addColorStop(1, 'rgba(0, 72, 255, 0.0)');

    combinedChartInstance = new Chart(ctx, {
      type: 'line',
      data: {
        datasets: [
          {
            label: 'Temperature (°C)',
            data: tempValues,
            borderColor: '#ff1900',
            backgroundColor: tempGradient,
            fill: true,
            spanGaps: false,
            borderWidth: 2,
            tension: 0.4,
            pointRadius: 0,
            pointHoverRadius: 6,
            yAxisID: 'yTemp'
          },
          {
            label: 'Humidity (%)',
            data: humidityValues,
            borderColor: '#0048ff',
            backgroundColor: humidityGradient,
            fill: true,
            spanGaps: false,
            borderWidth: 2,
            tension: 0.4,
            pointRadius: 0,
            pointHoverRadius: 6,
            yAxisID: 'yHumidity'
          }
        ]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: { display: true, position: 'top' },
          tooltip: { mode: 'index', intersect: false }
        },
        scales: {
          x: {
            type: 'time',
            time: {
              unit: 'day',
              displayFormats: {
                day: 'MMM dd',
                hour: 'MMM dd, HH:mm'
              }
            },
            min: minDate,
            max: maxDate,
            display: true,
            title: { display: false, text: 'Time' },
            ticks: { maxTicksLimit: 10 }
          },
          yTemp: {
            type: 'linear',
            display: true,
            position: 'left',
            title: { display: true, text: 'Temperature (°C)' },
            beginAtZero: false,
            min: minTemp !== null ? minTemp : undefined,
            max: maxTemp !== null ? maxTemp : undefined
          },
          yHumidity: {
            type: 'linear',
            display: true,
            position: 'right',
            title: { display: true, text: 'Humidity (%)' },
            beginAtZero: true,
            min: 0,
            max: 100,
            grid: {
              drawOnChartArea: false
            }
          }
        }
      }
    });
  }

  // Динамично изчисляване на прага за празнина (gap)
  function insertNullGaps(dataPoints) {
    if (!dataPoints || dataPoints.length < 2) return dataPoints;

    // 1. Изчисляваме медианния/средния интервал между точките (в ms)
    const intervals = [];
    for (let i = 0; i < dataPoints.length - 1; i++) {
      const t1 = new Date(dataPoints[i].x).getTime();
      const t2 = new Date(dataPoints[i + 1].x).getTime();
      const diff = t2 - t1;
      if (diff > 0) intervals.push(diff);
    }

    if (intervals.length === 0) return dataPoints;

    // Вземаме медианния интервал, за да избегнем влиянието на вече съществуващи дупки
    intervals.sort((a, b) => a - b);
    const medianIntervalMs = intervals[Math.floor(intervals.length / 2)];

    // Ако разликата е 1.8 пъти по-голяма от очаквания интервал, отчитаме прекъсване
    const maxAllowedGapMs = medianIntervalMs * 1.8;

    const result = [];

    for (let i = 0; i < dataPoints.length; i++) {
      const currentPoint = dataPoints[i];
      result.push(currentPoint);

      if (i < dataPoints.length - 1) {
        const nextPoint = dataPoints[i + 1];

        const currentTime = new Date(currentPoint.x).getTime();
        const nextTime = new Date(nextPoint.x).getTime();

        if (nextTime - currentTime > maxAllowedGapMs) {
          const nullTimeString = new Date(currentTime + medianIntervalMs).toISOString();
          result.push({
            x: nullTimeString,
            y: null
          });
        }
      }
    }

    return result;
  }