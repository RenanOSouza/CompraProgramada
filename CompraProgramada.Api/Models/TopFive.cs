namespace CompraProgramada.Api.Models {
    
    public class TopFive {
        
        public long Id {get; set;}
        public DateTime DataCriacao {get; set;} = DateTime.UtcNow;
        public bool Ativo {get; set;} = true;
        public DateTime? DataDesativacao { get; set; }
        public ICollection<ItemCesta> Itens { get; set; } = new List<ItemCesta>();

    }

    public class ItemCesta {
        
        public long Id { get; set; }
        public long CestaId { get; set; }
        required public string Ticker { get; set; }
        public decimal PercentualPeso { get; set; }
        public TopFive? Cesta { get; set; }
    }

    
}