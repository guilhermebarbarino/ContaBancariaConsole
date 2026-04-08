using ContaBancaria.Domain.Entidades;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ContaBancaria.Domain.Interfaces
{
    public interface IMovimentacaoRepository
    {
        void Adicionar(Movimentacao movimentacao);
        List<Movimentacao> ObterPorConta(int numeroConta);
    }
}
