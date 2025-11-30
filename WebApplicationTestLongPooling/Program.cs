using LognPoolingLib;

namespace WebApplicationTestLongPooling
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.OutputFormatters.Add(new PlainTextOutputFormatter());
            });

            var messageQueuePoolOptions = new MessageQueuePoolOptions()
            {
                LiveTime = TimeSpan.FromMinutes(1),
            };
            builder.Services.AddSingleton(messageQueuePoolOptions);
            builder.Services.AddSingleton<MessageQueuePool>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseAuthorization();


            app.MapControllers();
            app.Run();
        }
    }
}
