using System.Text.Json.Serialization;

namespace ContaBancaria.Domain.Entidades
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Cpf { get; private set; }
        public DateTime DataNascimento { get; private set; }

        public Cliente(string nome, string cpf, DateTime dataNascimento)
            : this(Guid.NewGuid(), nome, cpf, dataNascimento)
        {
        }

        [JsonConstructor]
        public Cliente(Guid id, string nome, string cpf, DateTime dataNascimento)
        {
            if (id == Guid.Empty)
                throw new ArgumentException("Identificador do cliente inválido.", nameof(id));

            if (string.IsNullOrWhiteSpace(nome))
                throw new ArgumentException("Nome é obrigatório.", nameof(nome));

            if (dataNascimento.Date > DateTime.Today)
                throw new ArgumentException("Data de nascimento não pode ser futura.", nameof(dataNascimento));

            Id = id;
            Nome = nome.Trim();
            Cpf = NormalizarCpf(cpf);
            DataNascimento = dataNascimento.Date;
        }

        public static string NormalizarCpf(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf))
                throw new ArgumentException("CPF é obrigatório.", nameof(cpf));

            var normalizado = cpf.Trim().Replace(".", string.Empty).Replace("-", string.Empty);

            if (normalizado.Length != 11 || normalizado.Any(c => c < '0' || c > '9'))
                throw new ArgumentException("CPF deve conter 11 dígitos, com ou sem pontuação.", nameof(cpf));

            return normalizado;
        }

        public int ObterIdade()
        {
            var hoje = DateTime.Today;
            var idade = hoje.Year - DataNascimento.Year;

            if (DataNascimento.Date > hoje.AddYears(-idade))
                idade--;

            return idade;
        }

        public string ObterCpfMascarado()
        {
            var cpfLimpo = new string(Cpf.Where(char.IsDigit).ToArray());

            if (cpfLimpo.Length != 11)
                return "***";

            return $"{cpfLimpo[..3]}.***.***-{cpfLimpo[^2..]}";
        }
    }
}
