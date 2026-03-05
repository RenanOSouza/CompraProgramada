namespace CompraProgramada.Api.Services{
    public class DispMotorServ : BackgroundService {
        private readonly IServiceProvider serviceProvider;

        public DispMotorServ (IServiceProvider SPInfo) {
            serviceProvider = SPInfo;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
            
            while (!stoppingToken.IsCancellationRequested) {

                DateTime hoje = DateTime.Today;

                //Confere se é dia 5, 15 ou 25
                if (WorkerServ.ValidaDia(hoje)) {
                    
                    Console.WriteLine("Dia valido");
                    try
                    {
                        using (var scope = serviceProvider.CreateScope()) {

                            var objMotor = scope.ServiceProvider.GetRequiredService<MotorServ>();
                            await objMotor.ExecutarMotorAsync(Path.GetFullPath(Path.Combine(Directory.GetCurrentDirectory(), "..", "cotacoes", "COTAHIST_D27022026.TXT")));
                        }
                    }
                    catch (Exception ex){

                        Console.WriteLine($"[ERRO MOTOR] Falha ao executar as compras: {ex.Message}");

                    }
                } else {
                    Console.WriteLine($"[MOTOR DE COMPRA] Hoje ({hoje:dd/MM/yyyy}) não é dia de compra. Voltando a dormir.");
                }

                await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
            }
        }
    }
}