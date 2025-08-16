namespace MILANO.Server.Web.Extensions
{
	/// <summary>
	/// Extension methods for configuring the request pipeline.
	/// </summary>
	public static class WebApplicationExtensions
	{
		public static void ConfigureMilanoApp(this WebApplication app)
		{
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.UseMilanoMiddleware();
			app.MapMilanoEndpoints();
		}
	}
}
