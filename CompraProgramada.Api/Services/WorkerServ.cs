namespace CompraProgramada.Api.Services
{
    public static class WorkerServ{ 

        public static bool ValidaDia(DateTime dataAtual){

            int ano = dataAtual.Year;
            int mes = dataAtual.Month;

            var dataExecucao1 = AjustarParaDiaUtil(new DateTime(ano, mes, 5));
            var dataExecucao2 = AjustarParaDiaUtil(new DateTime(ano, mes, 15));
            var dataExecucao3 = AjustarParaDiaUtil(new DateTime(ano, mes, 25));

            return dataAtual.Date == dataExecucao1.Date || dataAtual.Date == dataExecucao2.Date || dataAtual.Date == dataExecucao3.Date;
        }
        
        private static DateTime AjustarParaDiaUtil(DateTime data) {

            //Calcula se o dia é sabado ou domingo. Se for, adia para a proxima segunda
            if (data.DayOfWeek == DayOfWeek.Saturday) return data.AddDays(2); 
                
            if (data.DayOfWeek == DayOfWeek.Sunday) return data.AddDays(1);

            return data;
        }
    }
}