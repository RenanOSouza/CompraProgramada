# 📈 Compra Programada (Top Five) - Robo-Advisor

Um sistema completo de automatização de investimentos (*Robo-Advisor*) baseado no modelo de **Target Allocation**. O projeto gerencia a captação de clientes, o agendamento de compras mensais fracionadas (*Dollar Cost Averaging*) e o rebalanceamento dinâmico de carteiras baseado em uma cesta de ações recomendadas (Top Five).

## 🚀 Arquitetura e Tecnologias

O projeto foi desenhado para ser robusto e escalável, utilizando as seguintes tecnologias:

* **Backend:** C# com .NET Core (ASP.NET Core Web API)
* **Banco de Dados:** MySQL com Entity Framework Core (Code-First Migrations)
* **Mensageria:** Apache Kafka (Confluent.Kafka) executado via Docker
* **Frontend:** Angular com TypeScript, HTML e SCSS
* **Processamento em Background:** .NET Hosted Services (`BackgroundService`)
* **Leitura de Dados B3:** Parser customizado para extração de preços de fechamento via arquivo posicional `COTAHIST.TXT`

## ✨ Principais Funcionalidades

### 1. Motor de Compras Automatizado (Worker Service)
* **Execução Agendada:** Um robô em background roda diariamente e identifica se o dia atual é data de aporte (dias 5, 15 e 25).
* **Ajuste de Dias Úteis:** Transfere automaticamente a execução para a próxima segunda-feira se a data de compra cair em um fim de semana.
* **Rateio Financeiro:** Divide o aporte mensal do cliente em 3 parcelas exatas, mitigando o risco de volatilidade do mercado.

### 2. Rebalanceamento de Carteira (Target Allocation)
* **Ajuste Dinâmico:** Quando o Gestor altera a composição da Cesta "Top Five", o motor recalcula o alvo financeiro de todos os clientes ativos.
* **Venda de Excedentes:** Identifica e vende automaticamente ativos que saíram da cesta ou que ultrapassaram o peso percentual alvo.
* **Compra de Déficits:** Utiliza o capital liberado pelas vendas para adquirir os novos ativos da cesta.
* **Prevenção de Frações:** Converte cálculos financeiros quebrados em lotes inteiros de ações `(int)`, mantendo o saldo excedente como saldo em conta.

### 3. Apuração de Imposto de Renda e Mensageria (Kafka)
* **Cálculo Mensal Integrado:** O motor agrega todo o histórico de vendas do cliente dentro do mês corrente.
* **Regra de Isenção Fiscal:** Verifica a regra da Receita Federal de isenção para vendas de até R$ 20.000,00 mensais.
* **Geração de DARF:** Se o volume ultrapassar o teto e houver lucro apurado na operação, o sistema calcula a alíquota de 20% sobre o lucro líquido.
* **Eventos em Tempo Real:** Publica o evento fiscal detalhado em formato JSON no tópico `ir-venda-rebalanceamento` do Kafka para consumo de outros microsserviços.

### 4. Dashboard do Cliente (Angular)
* **Visão de Patrimônio:** Exibe o valor total aplicado, lucro/prejuízo atual e o *Share* percentual da composição da carteira em um gráfico interativo.
* **Lote Padrão vs. Fracionário:** O frontend identifica dinamicamente se a quantidade de ações é múltipla de 100, dividindo a visualização de forma fluida entre mercado à vista (ex: `PETR4`) e fracionário (ex: `PETR4F`).
* **Histórico de Vendas:** Tabela transparente que exibe as operações de rebalanceamento realizadas pelo robô, detalhando preços, volumes operados e lucro/prejuízo apurado.
* **Gestão de Adesão:** Permite alterar o valor do aporte mensal ou encerrar definitivamente a adesão ao produto, desativando a conta para as próximas rodadas do motor.

## 🛠️ Pré-requisitos e Como Executar

Você vai precisar das seguintes ferramentas instaladas:
* Git
* Docker e Docker Compose
* .NET SDK
* Node.js e npm
* Visual Studio ou Visual Studio Code

**1. Subindo a infraestrutura (Kafka e MySQL)**
```bash
docker-compose up -d
```

**2. Configurando o Backend (.NET Core)**
Navegue até a pasta da API, aplique as migrations para criar o esquema do banco de dados e inicie o servidor:
```bash
cd CompraProgramada.Api
dotnet ef database update
dotnet run
```

**3. Configurando o Frontend (Angular)**
Em um novo terminal, navegue até a pasta do painel web, instale as dependências e inicie o servidor de desenvolvimento:
```bash
cd compra-programada-app
npm install
ng serve
```

**4. Arquivo da B3 (COTAHIST)**
Certifique-se de que o arquivo de cotações `COTAHIST_D27022026.TXT` está devidamente posicionado na pasta `/cotacoes` na raiz do projeto (um nível acima da API), para que o `ParserServ` possa extrair os preços de fechamento do mercado corretamente.

## 📄 Estrutura de Dados Principal (Entity Framework)

* `Cliente`: Entidade central com dados cadastrais e controle de status de adesão (`Ativo`).
* `ContaGrafica` / `Custodia`: Representação fiel da carteira do cliente, controlando o estoque atual (quantidade) e o Preço Médio ponderado das ações.
* `TopFive` / `CestaItem`: Histórico versionado das cestas recomendadas pelo gestor.
* `HistoricoVenda`: Tabela de auditoria contendo cada operação de venda executada pelo motor, fundamental para o cálculo mensal de evolução patrimonial e Imposto de Renda.
