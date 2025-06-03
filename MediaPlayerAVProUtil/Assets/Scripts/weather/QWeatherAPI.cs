using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class WeatherData
{
    public string code;
    public string updateTime;
    public string fxLink;
    public DailyWeather[] daily;

    [System.Serializable]
    public class DailyWeather
    {
        public string fxDate;        // 预报日期
        public string sunrise;       // 日出时间
        public string sunset;        // 日落时间
        public string moonrise;      // 月升时间
        public string moonset;       // 月落时间
        public string moonPhase;     // 月相名称
        public string tempMax;       // 预报当天最高温度
        public string tempMin;       // 预报当天最低温度
        public string iconDay;       // 预报白天天气状况图标代码
        public string textDay;       // 预报白天天气状况文字描述
        public string iconNight;     // 预报夜间天气状况图标代码
        public string textNight;     // 预报夜间天气状况文字描述
        public string wind360Day;    // 预报白天风向360角度
        public string windDirDay;    // 预报白天风向
        public string windScaleDay;  // 预报白天风力等级
        public string windSpeedDay;  // 预报白天风速，公里/小时
        public string wind360Night;  // 预报夜间风向360角度
        public string windDirNight;  // 预报夜间风向
        public string windScaleNight;// 预报夜间风力等级
        public string windSpeedNight;// 预报夜间风速，公里/小时
        public string humidity;      // 相对湿度，百分比数值
        public string precip;        // 预报当天总降水量，毫米
        public string pressure;      // 大气压强，百帕
        public string vis;           // 能见度，公里
        public string cloud;         // 云量，百分比数值
        public string uvIndex;       // 紫外线强度指数
    }
}

[System.Serializable]
public class CurrentWeatherData
{
    public string code;
    public string updateTime;
    public string fxLink;
    public CurrentWeather now;

    [System.Serializable]
    public class CurrentWeather
    {
        public string obsTime;    // 数据观测时间
        public string temp;       // 温度，摄氏度
        public string feelsLike;  // 体感温度，摄氏度
        public string icon;       // 天气状况图标代码
        public string text;       // 天气状况的文字描述
        public string wind360;    // 风向360角度
        public string windDir;    // 风向
        public string windScale;  // 风力等级
        public string windSpeed;  // 风速，公里/小时
        public string humidity;   // 相对湿度，百分比数值
        public string precip;     // 当前小时累计降水量，毫米
        public string pressure;   // 大气压强，百帕
        public string vis;        // 能见度，公里
        public string cloud;      // 云量，百分比数值
        public string dew;        // 露点温度
    }
}

public class QWeatherAPI : MonoBehaviour
{
    [Header("API配置")]
    public string apiKey = "YOUR_API_KEY_HERE";

    [Header("城市配置")]
    public string cityId = "101040100"; // 重庆城市ID
    public string cityName = "重庆";

    [Header("API URLs")]
    private const string BASE_URL = "https://devapi.qweather.com/v7/weather/";
    private const string CURRENT_WEATHER_URL = BASE_URL + "now";
    private const string FORECAST_3D_URL = BASE_URL + "3d";
    private const string FORECAST_7D_URL = BASE_URL + "7d";

    [Header("UI显示")]
    public TMP_Text weatherDisplayText;
    public UnityEngine.UI.Button refreshButton;

    [Header("调试信息")]
    public bool showDebugInfo = true;

    // 缓存的天气数据
    private CurrentWeatherData currentWeather;
    private WeatherData forecastWeather;

    void Start()
    {
        // 绑定刷新按钮
        if (refreshButton != null)
        {
            refreshButton.onClick.AddListener(RefreshWeatherData);
        }

        // 启动时自动获取天气数据
        RefreshWeatherData();

        // 每30分钟自动更新一次
        InvokeRepeating(nameof(RefreshWeatherData), 1800f, 1800f);
    }

    public void RefreshWeatherData()
    {
        if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_API_KEY_HERE")
        {
            Debug.LogError("请设置有效的和风天气API Key！");
            UpdateWeatherDisplay("错误：未设置API Key");
            return;
        }

        // 同时获取当前天气和预报
        StartCoroutine(GetCurrentWeather());
        StartCoroutine(GetWeatherForecast(7)); // 获取7天预报
    }

    private IEnumerator GetCurrentWeather()
    {
        string url = $"{CURRENT_WEATHER_URL}?location={cityId}&key={apiKey}";

        if (showDebugInfo)
        {
            Debug.Log($"请求当前天气URL: {url}");
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // 设置请求头
            request.SetRequestHeader("User-Agent", "Unity-QWeather-Client/1.0");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                if (showDebugInfo)
                {
                    Debug.Log($"当前天气API响应: {jsonResponse}");
                }

                try
                {
                    currentWeather = JsonUtility.FromJson<CurrentWeatherData>(jsonResponse);

                    if (currentWeather.code == "200")
                    {
                        Debug.Log("当前天气数据获取成功！");
                        UpdateCurrentWeatherDisplay();
                    }
                    else
                    {
                        Debug.LogError($"API返回错误代码: {currentWeather.code}");
                        UpdateWeatherDisplay($"API错误: 代码 {currentWeather.code}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析当前天气JSON失败: {e.Message}");
                    UpdateWeatherDisplay("数据解析失败");
                }
            }
            else
            {
                Debug.LogError($"当前天气API请求失败: {request.error}");
                UpdateWeatherDisplay($"网络错误: {request.error}");
            }
        }
    }

    private IEnumerator GetWeatherForecast(int days = 3)
    {
        string forecastUrl = days <= 3 ? FORECAST_3D_URL : FORECAST_7D_URL;
        string url = $"{forecastUrl}?location={cityId}&key={apiKey}";

        if (showDebugInfo)
        {
            Debug.Log($"请求天气预报URL: {url}");
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("User-Agent", "Unity-QWeather-Client/1.0");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;

                if (showDebugInfo)
                {
                    Debug.Log($"天气预报API响应: {jsonResponse}");
                }

                try
                {
                    forecastWeather = JsonUtility.FromJson<WeatherData>(jsonResponse);

                    if (forecastWeather.code == "200")
                    {
                        Debug.Log("天气预报数据获取成功！");
                        UpdateForecastWeatherDisplay();
                    }
                    else
                    {
                        Debug.LogError($"预报API返回错误代码: {forecastWeather.code}");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"解析天气预报JSON失败: {e.Message}");
                }
            }
            else
            {
                Debug.LogError($"天气预报API请求失败: {request.error}");
            }
        }
    }

    private void UpdateCurrentWeatherDisplay()
    {
        if (currentWeather?.now == null) return;

        var now = currentWeather.now;
        string displayText = $"🌍 {cityName} 当前天气\n\n";
        displayText += $"🌡️ 温度: {now.temp}°C (体感 {now.feelsLike}°C)\n";
        displayText += $"☁️ 天气: {now.text}\n";
        displayText += $"💨 风向: {now.windDir} {now.windScale}级 ({now.windSpeed}km/h)\n";
        displayText += $"💧 湿度: {now.humidity}%\n";
        displayText += $"👁️ 能见度: {now.vis}km\n";
        displayText += $"📊 气压: {now.pressure}hPa\n";
        displayText += $"🕐 更新时间: {now.obsTime}\n";

        UpdateWeatherDisplay(displayText);
    }

    private void UpdateForecastWeatherDisplay()
    {
        if (forecastWeather?.daily == null || forecastWeather.daily.Length == 0) return;

        string forecastText = "\n\n📅 未来天气预报\n";
        forecastText += "═══════════════════\n";

        for (int i = 0; i < Math.Min(forecastWeather.daily.Length, 5); i++)
        {
            var day = forecastWeather.daily[i];
            string date = DateTime.Parse(day.fxDate).ToString("MM/dd");
            string dayName = i == 0 ? "今天" : DateTime.Parse(day.fxDate).ToString("dddd");

            forecastText += $"📆 {date} ({dayName})\n";
            forecastText += $"   🌡️ {day.tempMin}°C ~ {day.tempMax}°C\n";
            forecastText += $"   🌞 白天: {day.textDay}\n";
            forecastText += $"   🌙 夜间: {day.textNight}\n";
            forecastText += $"   💧 湿度: {day.humidity}% | 降水: {day.precip}mm\n";
            if (i < Math.Min(forecastWeather.daily.Length, 5) - 1)
            {
                forecastText += "   ─────────────────\n";
            }
        }

        // 将预报信息追加到当前天气显示
        if (weatherDisplayText != null && currentWeather?.now != null)
        {
            weatherDisplayText.text += forecastText;
        }
    }

    private void UpdateWeatherDisplay(string text)
    {
        if (weatherDisplayText != null)
        {
            weatherDisplayText.text = text;
        }

        Debug.Log($"天气信息更新: {text}");
    }

    // 公共方法，供其他脚本调用
    public CurrentWeatherData GetCurrentWeatherData()
    {
        return currentWeather;
    }

    public WeatherData GetForecastWeatherData()
    {
        return forecastWeather;
    }

    // 获取特定城市的天气（可扩展功能）
    public void GetWeatherForCity(string newCityId, string newCityName)
    {
        cityId = newCityId;
        cityName = newCityName;
        RefreshWeatherData();
    }

    // 检查API配置是否有效
    public bool IsAPIConfigured()
    {
        return !string.IsNullOrEmpty(apiKey) && apiKey != "YOUR_API_KEY_HERE";
    }
}