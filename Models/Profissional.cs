using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iBarber.Models
{
    [Table("Profissional")]
    public class Profissional
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O telefone é obrigatório.")]
        public string Telefone { get; set; }

        [Required(ErrorMessage = "O e-mail é obrigatório.")]
        [EmailAddress(ErrorMessage = "O e-mail informado não é válido.")]
        public string Email { get; set; }

        [ForeignKey("Barbearia")]
        [Display(Name = "Barbearia")]
        public int BarbeariaId { get; set; } // chave estrangeira que liga o profissional à barbearia

        // Propriedade de navegação
        public Barbearia Barbearia { get; set; } // propriedade que permite acessar os dados da barbearia diretamente a partir do profissional

    }
}
