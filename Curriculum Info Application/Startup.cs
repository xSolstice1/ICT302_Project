namespace Curriculum_Info_Application
{
    public class Startup
    {
        public IConfiguration _configuration { get; }

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IConfiguration>(_configuration);
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
        }
    }
}
