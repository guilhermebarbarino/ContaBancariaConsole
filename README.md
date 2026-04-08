# 💰 Conta Bancária Console - C# (.NET)

Aplicação de console desenvolvida em C# com foco em simulação de um
sistema bancário, aplicando conceitos de **Clean Architecture, SOLID e
boas práticas de engenharia de software**.

------------------------------------------------------------------------

## 📌 Objetivo

Simular operações bancárias reais em um ambiente simples, com foco em:

-   Modelagem de domínio
-   Organização em camadas
-   Regras de negócio
-   Persistência em arquivo
-   Evolução incremental do sistema

------------------------------------------------------------------------

## 🏗️ Arquitetura

O projeto segue uma estrutura inspirada em **Clean Architecture**:

ContaBancariaConsole

├── Domain │ ├── Entities │ ├── Interfaces │ ├── Application │ ├──
Services │ ├── Enums │ ├── Infrastructure │ ├── Persistence │ ├──
Repositories │ └── Program.cs

------------------------------------------------------------------------

## 🧠 Conceitos aplicados

-   SOLID
-   Clean Architecture (adaptada)
-   Encapsulamento de regras de negócio
-   Inversão de dependência
-   Separação de responsabilidades
-   Persistência híbrida (memória + arquivo)

------------------------------------------------------------------------

## ⚙️ Funcionalidades

### 👤 Cliente

-   Cadastro com validação de CPF único
-   Validação de maioridade (18 anos)
-   Verificação de CPF antes do cadastro completo
-   CPF exibido de forma mascarada

------------------------------------------------------------------------

### 🏦 Conta Bancária

-   Conta criada automaticamente ao cadastrar cliente
-   Geração automática de:
    -   Agência
    -   Número da conta
    -   DAC
-   Conta vinculada ao CPF do cliente

------------------------------------------------------------------------

### 💸 Operações financeiras

#### Depósito

-   Apenas valores positivos

#### Saque

-   Não permite saldo negativo

#### Transferência

-   Apenas entre contas existentes
-   Não permite transferência para a mesma conta
-   Valida saldo suficiente
-   Tratamento de erro com mensagens claras

------------------------------------------------------------------------

### 📄 Extrato

-   Histórico completo de movimentações
-   Tipos:
    -   Depósito
    -   Saque
    -   Transferência enviada
    -   Transferência recebida

------------------------------------------------------------------------

### 📤 Exportação de Extrato

-   Exportação em TXT e JSON
-   Arquivos gerados automaticamente na pasta /Extratos

------------------------------------------------------------------------

## 💾 Persistência

-   Dados em memória
-   Persistência em arquivo local: database.json

------------------------------------------------------------------------

## 🚀 Como executar

cd ContaBancariaConsole dotnet run

------------------------------------------------------------------------

## 👨‍💻 Autor

Guilherme Barbarino
