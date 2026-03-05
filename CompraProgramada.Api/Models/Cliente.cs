using CompraProgramada.Api.Controllers;

using System.Text.Json.Serialization;

namespace CompraProgramada.Api.Models {
    public class Cliente {
        public long Id { get; set; }
        required public string Nome { get; set; }
        required public string Cpf { get; set; }
        required public string Email { get; set; }
        public decimal ValorMensal { get; set; }
        public bool Ativo { get; set; } = true;
        public DateTime DataAdesao { get; set; }

        public ContaGrafica? ContaGrafica { get; set; } 
        public ICollection<HistoricoValorMensal> HistoricosValor { get; set; } = new List<HistoricoValorMensal>();
        public ICollection<HistoricoVenda> HistoricoVendas { get; set; } = new List<HistoricoVenda>();

    }
}