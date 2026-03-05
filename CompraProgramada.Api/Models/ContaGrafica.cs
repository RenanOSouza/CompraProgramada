namespace CompraProgramada.Api.Models {
    public class ContaGrafica {
        public long Id { get; set; }
        public long ClienteId { get; set; }
        required public string NumeroConta { get; set; }
        public string Tipo { get; set; } = "FILHOTE"; 
        public DateTime DataCriacao { get; set; }
        public decimal SaldoFinanceiro { get; set; } = 0m;
        public Cliente? Cliente { get; set; }
        public ICollection<Custodia> Custodias { get; set; } = new List<Custodia>();
    }
}