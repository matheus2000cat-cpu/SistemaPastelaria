using System.ComponentModel.DataAnnotations;

namespace SistemaPastelaria.Models;

public class LoginViewModel
{
    public IReadOnlyList<FuncionarioOpcaoViewModel> Funcionarios { get; init; } = [];

    [Range(1, int.MaxValue, ErrorMessage = "Selecione um funcionário.")]
    public int IdOperario { get; set; }

    [Required(ErrorMessage = "Informe seu PIN.")]
    [RegularExpression(@"^\d{4}$", ErrorMessage = "O PIN deve ter 4 números.")]
    public string SenhaPin { get; set; } = string.Empty;
}

public class FuncionarioOpcaoViewModel
{
    public int IdOperario { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cargo { get; init; } = string.Empty;
}
