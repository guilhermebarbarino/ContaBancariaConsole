using System.Globalization;
using System.Text;
using System.Text.Json;
using ContaBancaria.Application.Services;
using ContaBancaria.Domain.Interfaces;
using ContaBancaria.Infrastructure.Persistence;
using ContaBancaria.Infrastructure.Repositories;

internal class Program
{
    private static int Main()
    {
        CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
        CultureInfo.CurrentUICulture = CultureInfo.CurrentCulture;
        Console.OutputEncoding = Encoding.UTF8;

        try
        {
            Executar();
            return 0;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or JsonException
                                   or ArgumentException or InvalidOperationException)
        {
            Console.Error.WriteLine($"Não foi possível abrir a base de dados: {ex.Message}");
            return 1;
        }
    }

    private static void Executar()
    {
        var jsonDatabase = new JsonDatabase("database.json");
        var context = new InMemoryDatabaseContext(jsonDatabase);

        IClienteRepository clienteRepository = new ClienteRepository(context);
        IContaRepository contaRepository = new ContaRepository(context);
        IMovimentacaoRepository movimentacaoRepository = new MovimentacaoRepository(context);
        IExtratoExportService extratoExportService = new ExtratoExportService();

        var clienteService = new ClienteService(clienteRepository, contaRepository, context);
        var contaService = new ContaService(contaRepository, clienteRepository, movimentacaoRepository, context);

        while (true)
        {
            Console.WriteLine("\n=== MENU ===");
            Console.WriteLine("1 - Cadastrar cliente");
            Console.WriteLine("2 - Listar clientes");
            Console.WriteLine("3 - Listar contas por CPF");
            Console.WriteLine("4 - Depositar");
            Console.WriteLine("5 - Sacar");
            Console.WriteLine("6 - Transferir");
            Console.WriteLine("7 - Consultar saldo");
            Console.WriteLine("8 - Consultar extrato");
            Console.WriteLine("9 - Exportar extrato");
            Console.WriteLine("0 - Sair");

            var opcao = Console.ReadLine()?.Trim();
            if (opcao is null)
                return;

            try
            {
                switch (opcao)
                {
                    case "1":
                        Console.Write("CPF: ");
                        var cpfCadastro = Console.ReadLine() ?? string.Empty;

                        if (clienteService.CpfJaCadastrado(cpfCadastro))
                        {
                            Console.WriteLine("Já existe cliente cadastrado com esse CPF.");
                            break;
                        }

                        Console.Write("Nome: ");
                        var nome = Console.ReadLine() ?? string.Empty;

                        Console.Write("Data de nascimento (yyyy-MM-dd): ");
                        if (!DateTime.TryParseExact(Console.ReadLine(), "yyyy-MM-dd", CultureInfo.InvariantCulture,
                            DateTimeStyles.None, out var dataNascimento))
                        {
                            Console.WriteLine("Data inválida.");
                            break;
                        }

                        var cliente = clienteService.Cadastrar(nome, cpfCadastro, dataNascimento);
                        var contasCriadas = contaService.ListarContasPorCpf(cpfCadastro);
                        var contaCriada = contasCriadas.First();

                        Console.WriteLine($"Cliente {cliente.Nome} cadastrado com sucesso.");
                        Console.WriteLine("Conta criada automaticamente pelo nosso sistema.");
                        Console.WriteLine($"Agência: {contaCriada.Agencia} | Conta: {contaCriada.Numero} | DAC: {contaCriada.Dac}");
                        break;

                    case "2":
                        var clientes = clienteService.Listar();

                        if (!clientes.Any())
                        {
                            Console.WriteLine("Nenhum cliente cadastrado.");
                            break;
                        }

                        foreach (var c in clientes)
                        {
                            Console.WriteLine($"Nome: {c.Nome} | CPF: {c.ObterCpfMascarado()} | Idade: {c.ObterIdade()}");
                        }
                        break;

                    case "3":
                        Console.Write("Informe o CPF do cliente: ");
                        var cpfConsulta = Console.ReadLine() ?? string.Empty;

                        var contasDoCliente = contaService.ListarContasPorCpf(cpfConsulta);

                        if (!contasDoCliente.Any())
                        {
                            Console.WriteLine("Nenhuma conta encontrada para esse CPF.");
                            break;
                        }

                        foreach (var contas in contasDoCliente)
                        {
                            Console.WriteLine($"Conta: {contas.Numero} | Saldo: {contas.Saldo:C}");
                        }
                        break;

                    case "4":
                        var contaDeposito = LerNumeroConta("Conta: ");

                        var valorDeposito = LerValor();

                        contaService.Depositar(contaDeposito, valorDeposito);
                        Console.WriteLine("Depósito realizado com sucesso.");
                        break;

                    case "5":
                        var contaSaque = LerNumeroConta("Conta: ");

                        var valorSaque = LerValor();

                        contaService.Sacar(contaSaque, valorSaque);
                        Console.WriteLine("Saque realizado com sucesso.");
                        break;

                    case "6":
                        var origem = LerNumeroConta("Conta origem: ");
                        var destino = LerNumeroConta("Conta destino: ");
                        var valorTransferencia = LerValor();

                        contaService.Transferir(origem, destino, valorTransferencia);
                        Console.WriteLine("Transferência realizada com sucesso.");
                        break;

                    case "7":
                        var contaConsulta = LerNumeroConta("Conta: ");

                        var contaSaldo = contaService.ConsultarSaldo(contaConsulta);
                        Console.WriteLine($"Saldo atual: {contaSaldo.Saldo:C}");
                        break;

                    case "8":
                        var numeroContaExtrato = LerNumeroConta("Informe o número da conta: ");

                        var extrato = contaService.ConsultarExtrato(numeroContaExtrato);

                        if (!extrato.Any())
                        {
                            Console.WriteLine("Nenhuma movimentação encontrada para esta conta.");
                            break;
                        }

                        Console.WriteLine("\n=== EXTRATO ===");
                        foreach (var item in extrato)
                        {
                            Console.WriteLine(
                                $"{item.Data:dd/MM/yyyy HH:mm:ss} | {item.Tipo} | Valor: {item.Valor:C} | {item.Descricao} | Saldo após: {item.SaldoAposMovimentacao:C}");
                        }
                        break;

                    case "9":
                        var numeroContaExportacao = LerNumeroConta("Informe o número da conta: ");

                        var movimentacoes = contaService.ConsultarExtrato(numeroContaExportacao);

                        if (!movimentacoes.Any())
                        {
                            Console.WriteLine("Nenhuma movimentação encontrada para esta conta.");
                            break;
                        }

                        Console.WriteLine("Escolha o formato de exportação:");
                        Console.WriteLine("1 - TXT");
                        Console.WriteLine("2 - JSON");
                        Console.Write("Opção: ");

                        var opcaoExportacao = Console.ReadLine();

                        string caminhoArquivo;

                        switch (opcaoExportacao)
                        {
                            case "1":
                                caminhoArquivo = extratoExportService.Exportar(
                                    movimentacoes,
                                    numeroContaExportacao,
                                    ContaBancaria.Application.Enums.TipoExportacaoExtrato.Txt);

                                Console.WriteLine($"Extrato exportado com sucesso em TXT.");
                                Console.WriteLine($"Arquivo: {caminhoArquivo}");
                                break;

                            case "2":
                                caminhoArquivo = extratoExportService.Exportar(
                                    movimentacoes,
                                    numeroContaExportacao,
                                     ContaBancaria.Application.Enums.TipoExportacaoExtrato.Json);

                                Console.WriteLine($"Extrato exportado com sucesso em JSON.");
                                Console.WriteLine($"Arquivo: {caminhoArquivo}");
                                break;

                            default:
                                Console.WriteLine("Formato de exportação inválido.");
                                break;
                        }

                        break;
                    case "0":
                        return;

                    default:
                        Console.WriteLine("Opção inválida.");
                        break;
                }
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException
                                       or IOException or UnauthorizedAccessException or OverflowException)
            {
                Console.WriteLine($"Erro: {ex.Message}");
            }
        }
    }

    private static int LerNumeroConta(string mensagem)
    {
        Console.Write(mensagem);
        if (!int.TryParse(Console.ReadLine(), out var numero) || numero <= 0)
            throw new ArgumentException("Número da conta inválido. Informe um inteiro positivo.");

        return numero;
    }

    private static decimal LerValor()
    {
        Console.Write("Valor (ex.: 100,50, sem separador de milhar): ");
        const NumberStyles formato = NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;
        if (!decimal.TryParse(Console.ReadLine()?.Trim(), formato, CultureInfo.CurrentCulture, out var valor))
            throw new ArgumentException("Valor inválido. Use vírgula para os centavos, por exemplo: 100,50.");

        return valor;
    }
}
