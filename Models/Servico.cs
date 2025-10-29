using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iBarber.Models
{
    [Table("Servico")]
    public class Servico
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do serviço é obrigatório.")]
        [Display(Name = "Serviço")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "O preço é obrigatório.")]
        [Column(TypeName = "decimal(10, 2)")] // Bom para valores monetários
        public decimal Preco { get; set; }

        [Required(ErrorMessage = "A duração é obrigatória.")]
        [Display(Name = "Duração (em minutos)")]
        public int DuracaoEmMinutos { get; set; } // Essencial para calcular a agenda

        // Chave estrangeira para a Barbearia
        [ForeignKey("Barbearia")]
        [Display(Name = "Barbearia")]
        public int BarbeariaId { get; set; }

        // Propriedade de navegação
        public Barbearia Barbearia { get; set; }
    }
}