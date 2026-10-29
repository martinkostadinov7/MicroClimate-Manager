document.addEventListener('DOMContentLoaded', loadSettings);

async function loadSettings() {
  const apiUrl = 'https://microclimatemanager.runasp.net/api/settings';

  const settingToInputId = {
    'TemperatureMin': 'tempMin',
    'TemperatureMax': 'tempMax',
    'HumidityMin': 'humMin',
    'HumidityMax': 'humMax',
    'MoistureMin': 'moistMin',
    'MoistureMax': 'moistMax'
  };

  try {
    const response = await fetch(apiUrl);
    
    if (!response.ok) {
      throw new Error(`Error: ${response.status}`);
    }

    const settings = await response.json();

    settings.forEach(setting => {
      const inputId = settingToInputId[setting.settingName];
      
      if (inputId) {
        const inputElement = document.getElementById(inputId);
        if (inputElement) {
          inputElement.value = setting.value;
        }
      }
    });

  } catch (error) {
    console.error('Error while getting the setting from server:', error);
  }
}

async function saveSettings() {
  const apiUrl = 'https://microclimatemanager.runasp.net/api/settings';
  const saveButton = document.getElementById('saveButton');

  const tempMinVal = document.getElementById('tempMin').value;
  const tempMaxVal = document.getElementById('tempMax').value;
  const humMinVal = document.getElementById('humMin').value;
  const humMaxVal = document.getElementById('humMax').value;
  const moistMinVal = document.getElementById('moistMin').value;
  const moistMaxVal = document.getElementById('moistMax').value;

  if (!tempMinVal || !tempMaxVal || !humMinVal || !humMaxVal || !moistMinVal || !moistMaxVal) {
    alert('Please fill in all input fields!');
    return;
  }

  const tempMin = parseFloat(tempMinVal);
  const tempMax = parseFloat(tempMaxVal);
  const humMin = parseFloat(humMinVal);
  const humMax = parseFloat(humMaxVal);
  const moistMin = parseFloat(moistMinVal);
  const moistMax = parseFloat(moistMaxVal);

  if (tempMin >= tempMax) {
    alert('Minimum temperature cannot be greater than or equal to maximum temperature!');
    return;
  }

  if (humMin >= humMax) {
    alert('Minimum humidity cannot be greater than or equal to maximum humidity!');
    return;
  }

  if (moistMin >= moistMax) {
    alert('Minimum soil moisture cannot be greater than or equal to maximum soil moisture!');
    return;
  }

  const settingsDto = [
    { settingName: 'TemperatureMin', value: tempMinVal },
    { settingName: 'TemperatureMax', value: tempMaxVal },
    { settingName: 'HumidityMin', value: humMinVal },
    { settingName: 'HumidityMax', value: humMaxVal },
    { settingName: 'MoistureMin', value: moistMinVal },
    { settingName: 'MoistureMax', value: moistMaxVal }
  ];

  try {
    saveButton.disabled = true;
    saveButton.innerText = 'Saving...';

    const response = await fetch(apiUrl, {
      method: 'PUT',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(settingsDto)
    });

    if (!response.ok) {
      throw new Error(`Failed to save settings. Status code: ${response.status}`);
    }

    alert('Settings updated successfully!');

  } catch (error) {
    console.error('Error updating settings:', error);
    alert('An error occurred while saving settings.');
  } finally {
    saveButton.disabled = false;
    saveButton.innerText = 'Save';
  }
}
