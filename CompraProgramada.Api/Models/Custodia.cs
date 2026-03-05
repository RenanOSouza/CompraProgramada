namespace CompraProgramada.Api.Models {
    
    public class Custodia {
        public long Id { get; set; }
        required public long ContaGraficaId { get; set; }
        required public string Ticker { get; set; }
        required public int Quantidade { get; set; }
        required public decimal PrecoMedio { get; set; }
        public DateTime DataUltimaAtualizacao { get; set; }
        public ContaGrafica? ContaGrafica { get; set; }
    }

}
