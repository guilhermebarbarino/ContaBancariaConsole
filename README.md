# Conta Bancária Console — C# / .NET 8

Aplicação de console para estudar modelagem de domínio, regras de negócio,
separação de responsabilidades e persistência local em JSON.

## Funcionalidades

- Cadastro de clientes a partir de 18 anos, com conta criada automaticamente.
- CPF único, normalizado com ou sem pontuação e mascarado na listagem.
- Depósito, saque, transferência, consulta de saldo e histórico de movimentações.
- Validação de valores positivos, saldo suficiente e contas de origem e destino.
- Exportação de extratos em TXT e JSON, com nomes únicos para evitar sobrescrita.
- Preservação de IDs, vínculos, saldos e datas ao reabrir a aplicação.

A validação de CPF verifica formato e unicidade; não calcula os dígitos verificadores.

## Organização

| Pasta | Responsabilidade |
| --- | --- |
| `ContaBancaria/Domain` | Entidades, regras de negócio e contratos de persistência |
| `ContaBancaria/Application` | Casos de uso e exportação de extratos |
| `ContaBancaria/Infrastructure` | Repositórios em memória e persistência JSON |
| `ContaBancaria/Program.cs` | Composição dos serviços e interação pelo console |
| `ContaBancaria.Tests` | Testes de regras de negócio, persistência e falhas |

## Executar

Instale o SDK do .NET 8 e execute os comandos na raiz do repositório:

```bash
dotnet restore ContaBancaria/ContaBancaria.csproj
dotnet run --project ContaBancaria/ContaBancaria.csproj
```

No menu:

- Informe datas no formato `yyyy-MM-dd`, por exemplo `1990-05-20`.
- Informe valores com vírgula decimal e sem separador de milhar, por exemplo `1500,50`.
- Informe CPF com 11 dígitos, com ou sem pontos e hífen.
- Digite `0` para sair. O encerramento da entrada também termina o programa.

## Persistência

O arquivo `database.json` e a pasta `Extratos` são criados no diretório de onde
a aplicação é executada. Para continuar usando a mesma base, execute sempre
a partir desse diretório ou mova o arquivo existente para ele antes de iniciar.

Os serviços utilizam uma unidade de trabalho: cadastro de cliente e conta,
ou alteração de saldos e registro de movimentações, são salvos juntos. Se uma
operação ou gravação falhar, o contexto em memória retorna ao estado anterior.

A gravação usa um arquivo temporário no mesmo diretório e depois substitui o
arquivo de dados. Um JSON vazio, inválido ou com coleções nulas gera um erro
na inicialização, preservando o arquivo para correção ou restauração de backup.

A persistência é destinada a uma única instância da aplicação por arquivo.
Não há controle de concorrência entre processos. O projeto é uma simulação
educacional e não implementa autenticação ou criptografia dos dados locais.

Os arquivos locais de dados, extratos, IDE e compilação são ignorados pelo Git.

## Integração contínua

O GitHub Actions executa restore, build e testes de regressão no .NET 8 em cada push e pull request para `main`. A execução manual também está disponível na aba **Actions**.

## Testes

```bash
dotnet test ContaBancaria.Tests/ContaBancaria.Tests.csproj --configuration Release
```

A suíte usa diretórios temporários isolados e verifica:

- Reabertura da base sem perder IDs, vínculos, saldos ou histórico.
- CPF duplicado com formatações diferentes e validação da idade mínima.
- Valores inválidos, saldo insuficiente, mesma conta e conta inexistente.
- Transferência sem débito parcial quando o saldo do destino excede `decimal`.
- Restauração do estado após falhas no registro de movimentações ou na gravação.
- Rejeição de arquivos JSON inválidos e exportações sem sobrescrita.

## Autor

Guilherme Barbarino
