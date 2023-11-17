using System;
using Microsoft.EntityFrameworkCore;
using WeatherApp.Model;

namespace WeatherApp.Context
{
	public class WeatherDbContext :DbContext
	{
		public WeatherDbContext(DbContextOptions<WeatherDbContext> contextOptions) : base(contextOptions)
		{

		}

		public DbSet<Weather> Weathers { get; set; }
	}
}

