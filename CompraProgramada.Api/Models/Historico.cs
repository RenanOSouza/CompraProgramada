using System.Text.Json.Serialization;

namespace CompraProgramada.Api.Models {
    public class HistoricoVenda {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        [JsonIgnore]
        public Cliente? Cliente { get; set; }
        public string Ticker { get; set; } = string.Empty;
        public decimal Quantidade { get; set; }
        public decimal PrecoVenda { get; set; }
        public decimal PrecoMedio { get; set; }
        public decimal ValorTotal { get; set; } 
        public decimal Lucro { get; set; } 
        public DateTime Data { get; set; }
    }
}