using System.Text;

namespace CompraProgramada.Api.Services {

    public class ParserServ: IParserServ {

        public ParserServ() {

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        }

        public List<ResultB3> ExtrairCotacoes(string caminhoFicheiro, List<string> tickersDesejados) {

            var cotacoesEncontradas = new List<ResultB3>();

            if (!File.Exists(caminhoFicheiro)) {

                throw new FileNotFoundException($"Erro ao encontrar o arquivo no caminho: {caminhoFicheiro}");
            }

            foreach (var linha in File.ReadLines(caminhoFicheiro, Encoding.GetEncoding("ISO-8859-1"))) {
                
                if (linha.StartsWith("01")) {

                    
                    var bdi = linha.Substring(10, 2);
                    var tipoMercado = linha.Substring(24, 3);
                    
                    // Filtra Lote Padrão e Mercado a Vista
                    if (bdi == "02" && tipoMercado == "010") {

                        var ticker = linha.Substring(12, 12).Trim();

                        
                        if (tickersDesejados.Contains(ticker)) {

                            //separa as infos do que vem do arquivo
                            var precoString = linha.Substring(108, 13);
                            var precoDecimal = decimal.Parse(precoString) / 100m;

                            cotacoesEncontradas.Add(new ResultB3 {
                                Ticker = ticker,
                                PrecoFechamento = precoDecimal
                            });
                        }
                    }
                }
            }

            return cotacoesEncontradas;
        }
    }
}