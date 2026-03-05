namespace CompraProgramada.Api.Models {
    public class Distribuicao {
        public long Id { get; set; }
        public long OrdemCompraId { get; set; }
        required public long ContaGraficaId { get; set; }
        required public string Ticker { get; set; }
        required public int Quantidade { get; set; }
        required public decimal ValorRateado { get; set; }
        public OrdemCompra? OrdemCompra { get; set; }
        public ContaGrafica? ContaGrafica { get; set; }
    }
}