namespace ContaBancaria.Domain.Entidades
{
    public class Cliente
    {
        public Guid Id { get; private set; }
        public string Nome { get; private set; }
        public string Cpf { get; private set; }
        public DateTime DataNascimento { get; private set; }

        public Cliente(string nome, string cpf, DateTime dataNascimento)
        {
            Id = Guid.NewGuid();
            Nome = nome;
            Cpf = cpf;
            DataNascimento = dataNascimento;
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