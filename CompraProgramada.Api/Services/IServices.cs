namespace CompraProgramada.Api.Services {
    public interface IParserServ {
        List<ResultB3> ExtrairCotacoes(string caminhoFicheiro, List<string> tickersDesejados);
    }
    
    public interface IKafkaServ {
        Task PublicarIrDedoDuroAsync(long contaFilhoteId, string ticker, decimal valorOperacao);
        Task PublicarIrVendaAsync(object eventoDarf);
    }
}