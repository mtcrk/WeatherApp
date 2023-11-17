
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using WeatherApp.Context;
using WeatherApp.Model;

namespace WeatherApp.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherController : ControllerBase
    {
        private readonly WeatherDbContext dbContext;

        public WeatherController(WeatherDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        [EnableCors("CorsPolicy")]
        [HttpPost("GetCurrentWeatherAndSave/{city}", Name = "GetCurrentWeatherAndSave")]
        public async Task<IActionResult> GetCurrentWeather(string city)
        {
            try
            {
                dynamic weatherData = await GetWeatherData(city);

                double? temperatureC = weatherData?.current?.temp_c != null ? Convert.ToDouble(weatherData.current.temp_c) : (double?)null;
                double? windSpeedMph = weatherData?.current?.wind_mph != null ? Convert.ToDouble(weatherData.current.wind_mph) : (double?)null;
                double? cloudiness = weatherData?.current?.cloud != null ? Convert.ToDouble(weatherData.current.cloud) : (double?)null;

                var weather = new Weather
                {
                    location = weatherData?.location?.name?.ToString(),
                    temperature = temperatureC ?? 0,
                    windSpeed = windSpeedMph ?? 0,
                    cloud = cloudiness ?? 0,
                    Date = DateTime.UtcNow
                };

                dbContext.Weathers.Add(weather);
                await dbContext.SaveChangesAsync();

                return Ok(new { Weather = weather, Message = "Başarılı" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Veri çekilemedi");
            }
        }


        private async Task<dynamic> GetWeatherData(string city)
        {
            using (HttpClient client = new HttpClient())
            {
                string apiUrl = "https://weatherapi-com.p.rapidapi.com/current.json?q="+city+"&days=3";
                client.DefaultRequestHeaders.Add("X-RapidAPI-Host", "weatherapi-com.p.rapidapi.com");
                client.DefaultRequestHeaders.Add("X-RapidAPI-Key", "3fe880755amshd9bb1e733be08fcp1c0a1bjsn26b2ab5a61c6");

                HttpResponseMessage response = await client.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    return JObject.Parse(json);
                }
                else
                {
                    throw new Exception($"API'den veri alınamadı. Durum kodu: {response.StatusCode}");
                }
            }
        }

        [EnableCors("CorsPolicy")]
        [HttpGet("LastRecordedWeather", Name = "LastRecordedWeather")]
        public IActionResult GetLatestWeather()
        {
            try
            {
                var latestWeather = dbContext.Weathers.OrderByDescending(w => w.Date).FirstOrDefault();

                if (latestWeather == null)
                {
                    return NotFound("data bulunamadı");
                }

                return Ok(latestWeather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Son veri çekilemedi");
            }
        }

        [HttpGet("{id}", Name = "GetWeatherById")]
        public IActionResult GetWeatherById(int id)
        {
            try
            {
                var weather = dbContext.Weathers.FirstOrDefault(w => w.Id == id);

                if (weather == null)
                {
                    return NotFound($"{id} data bulunamadı");
                }

                return Ok(weather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Veri çekilemedi");
            }
        }

        [HttpPost("createWeather", Name = "createWeather")]
        public async Task<IActionResult> CreateWeather([FromBody] WeatherDto newWeatherDto)
        {
            try
            {
                var newWeather = new Weather
                {
                    location = newWeatherDto.location,
                    temperature = newWeatherDto.temperature,
                    windSpeed = newWeatherDto.windSpeed,
                    cloud = newWeatherDto.cloud,
                    Date = newWeatherDto.Date
                };

                dbContext.Weathers.Add(newWeather);
                await dbContext.SaveChangesAsync();

                return CreatedAtAction("GetCurrentWeather", newWeather);
            }
            catch (Exception e)
            {
                return StatusCode(500, "Yeni kayıt eklenemedi");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeather(int id, [FromBody] WeatherDto updatedWeather)
        {
            try
            {
                var weather = dbContext.Weathers.FirstOrDefault(w => w.Id == id);

                if (weather == null)
                {
                    return NotFound($"{id} data bulunamadı");
                }
                weather.location = updatedWeather.location;
                weather.temperature = updatedWeather.temperature;
                weather.windSpeed = updatedWeather.windSpeed;
                weather.cloud = updatedWeather.cloud;
                weather.Date = updatedWeather.Date;

                await dbContext.SaveChangesAsync();

                return Ok(weather);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Güncelleme Hatası");
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeather(int id)
        {
            try
            {
                var weather = dbContext.Weathers.FirstOrDefault(w => w.Id == id);

                if (weather == null)
                {
                    return NotFound($"{id} id ile kayıt bulunamadı ");
                }

                dbContext.Weathers.Remove(weather);
                await dbContext.SaveChangesAsync();

                return Ok($"{id} id li kayıt silindi");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Kayıt silinemedi");
            }
        }

    }
}
