using ContaBancaria.Application.Enums;
using ContaBancaria.Domain.Entidades;
using ContaBancaria.Domain.Interfaces;
using System.Text;
using System.Text.Json;

namespace ContaBancaria.Application.Services
{
    public class ExtratoExportService : IExtratoExportService
    {
        public string Exportar(
            List<Movimentacao> movimentacoes,
            int numeroConta,
            TipoExportacaoExtrato tipoExportacao)
        {
            if (movimentacoes is null || movimentacoes.Count == 0)
                throw new InvalidOperationException("Não há movimentações para exportar.");

            var pastaExportacao = Path.Combine(Directory.GetCurrentDirectory(), "Extratos");
            Directory.CreateDirectory(pastaExportacao);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

            return tipoExportacao switch
            {
                TipoExportacaoExtrato.Txt => ExportarTxt(movimentacoes, numeroConta, pastaExportacao, timestamp),
                TipoExportacaoExtrato.Json => ExportarJson(movimentacoes, numeroConta, pastaExportacao, timestamp),
                _ => throw new InvalidOperationException("Tipo de exportação inválido.")
            };
        }

        private string ExportarTxt(
            List<Movimentacao> movimentacoes,
            int numeroConta,
            string pastaExportacao,
            string timestamp)
        {
            var caminhoArquivo = Path.Combine(
                pastaExportacao,
                $"extrato_conta_{numeroConta}_{timestamp}.txt");

            var sb = new StringBuilder();

            sb.AppendLine("=== EXTRATO BANCÁRIO ===");
            sb.AppendLine($"Conta: {numeroConta}");
            sb.AppendLine($"Gerado em: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            sb.AppendLine(new string('-', 100));

            foreach (var item in movimentacoes)
            {
                sb.AppendLine(
                    $"{item.Data:dd/MM/yyyy HH:mm:ss} | " +
                    $"Tipo: {item.Tipo} | " +
                    $"Valor: {item.Valor:C} | " +
                    $"Descrição: {item.Descricao} | " +
                    $"Saldo após: {item.SaldoAposMovimentacao:C}");
            }

            sb.AppendLine(new string('-', 100));
            sb.AppendLine("Fim do extrato.");

            File.WriteAllText(caminhoArquivo, sb.ToString(), Encoding.UTF8);

            return caminhoArquivo;
        }

        private string ExportarJson(
            List<Movimentacao> movimentacoes,
            int numeroConta,
            string pastaExportacao,
            string timestamp)
        {
            var caminhoArquivo = Path.Combine(
                pastaExportacao,
                $"extrato_conta_{numeroConta}_{timestamp}.json");

            var objeto = new
            {
                Conta = numeroConta,
                GeradoEm = DateTime.Now,
                Movimentacoes = movimentacoes.Select(m => new
                {
                    m.Data,
                    m.Tipo,
                    m.Valor,
                    m.Descricao,
                    m.SaldoAposMovimentacao
                })
            };

            var json = JsonSerializer.Serialize(objeto, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            File.WriteAllText(caminhoArquivo, json, Encoding.UTF8);

            return caminhoArquivo;
        }
    }
}