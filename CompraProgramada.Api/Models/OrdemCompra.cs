namespace CompraProgramada.Api.Models {
    
    public class OrdemCompra {
        public long Id { get; set; }
        public DateTime DataOperacao { get; set; }
        required public string Ticker { get; set; }
        public int QuantidadeTotal { get; set; }
        public decimal PrecoUnitario { get; set; }
        public decimal ValorTotalOperacao { get; set; }
        public string TipoMercado { get; set; } = "LOTE_PADRAO";
        public ICollection<Distribuicao> Distribuicoes { get; set; } = new List<Distribuicao>();
        public int Residuo { get; set; }
    }


    public class HistoricoValorMensal {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        public decimal ValorAntigo { get; set; }
        public decimal ValorNovo { get; set; }
        public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
        public Cliente? Cliente { get; set; }
    }
}