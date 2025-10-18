using Service.Contracts;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Service
{
    public class BirthdayScheduledService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BirthdayScheduledService> _logger;

        public BirthdayScheduledService(
            IServiceProvider serviceProvider,
            ILogger<BirthdayScheduledService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var now = DateTime.Now;
                    
                    // Gửi email sinh nhật vào ngày đầu tiên của mỗi tháng lúc 8:00 AM
                    if (now.Day == 1 && now.Hour == 8 && now.Minute < 5)
                    {
                        _logger.LogInformation("Starting birthday notification check at {Time}", now);

                        using var scope = _serviceProvider.CreateScope();
                        var cauHinhService = scope.ServiceProvider.GetRequiredService<ICauHinhThongBaoService>();
                        
                        var sent = await cauHinhService.RunBirthdayCheckAndSendAsync();
                        
                        _logger.LogInformation("Birthday notification completed. Sent {Count} notifications at {Time}", sent, now);
                        
                        // Chờ 1 giờ để tránh gửi lại trong cùng 1 ngày
                        await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                    }
                    else
                    {
                        // Kiểm tra mỗi 5 phút
                        await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in birthday scheduled service");
                    // Chờ 1 giờ trước khi thử lại nếu có lỗi
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }
        }
    }
}