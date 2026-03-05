using System.Text.Json;
using Confluent.Kafka;

namespace CompraProgramada.Api.Services {
    public class KafkaServ: IKafkaServ {
        private readonly ProducerConfig config;

        public KafkaServ() {
            
            config = new ProducerConfig {
                BootstrapServers = "localhost:9092" 
            };
        }
        public async Task PublicarIrVendaAsync(object eventoDarf) {
            using var producer = new ProducerBuilder<Null, string>(config).Build();
    
            // Serializa o objeto que monta o rebalance
            string payload = JsonSerializer.Serialize(eventoDarf);
            
            // Publica no tópico específico para a geração da DARF
            await producer.ProduceAsync("ir-venda-rebalanceamento", new Message<Null, string> { Value = payload });
            
            Console.WriteLine($"[KAFKA] Evento publicado no tópico ir-venda-rebalanceamento: {payload}");
        }

        public async Task PublicarIrDedoDuroAsync(long contaFilhoteId, string ticker, decimal valorOperacao) {
            
            // IR Dedo-Duro de 0,005%
            decimal valorIr = Math.Round(valorOperacao * 0.00005m, 2);

            var evento = new {
                ContaGraficaId = contaFilhoteId,
                Ticker = ticker,
                ValorOperacao = valorOperacao,
                ImpostoRetido = valorIr,
                DataCalculo = DateTime.UtcNow
            };

            //Envia o alerta para o Kafka

            using var producer = new ProducerBuilder<Null, string>(config).Build();
            
            string payload = JsonSerializer.Serialize(evento);
            
            await producer.ProduceAsync("ir-dedo-duro", new Message<Null, string> { Value = payload });
            
            Console.WriteLine($"[KAFKA] Evento publicado no tópico ir-dedo-duro: {payload}");
        }
    }
}