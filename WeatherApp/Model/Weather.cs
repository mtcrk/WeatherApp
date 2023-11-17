

namespace WeatherApp.Model
{
    public class Weather
	{
        public int Id { get; set; }
        public string location { get; set; }
        public double temperature { get; set; }
        public double windSpeed { get; set; }
        public double cloud { get; set; }
        public DateTime Date { get; set; }
    }
}

