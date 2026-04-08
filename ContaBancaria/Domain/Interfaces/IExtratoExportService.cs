using ContaBancaria.Application.Enums;
using ContaBancaria.Domain.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaBancaria.Domain.Interfaces
{
    public interface IExtratoExportService
    {
        string Exportar(List<Movimentacao> movimentacoes, int numeroConta, TipoExportacaoExtrato tipoExportacao);
    }
}
