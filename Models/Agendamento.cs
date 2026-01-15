using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iBarber.Models
{
    [Table("Agendamento")]
    public class Agendamento
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "A data e hora são obrigatórias.")]
        [Display(Name = "Horário")]
        public DateTime DataHora { get; set; }

        // --- Chaves Estrangeiras ---

        // 1. Quem é o CLIENTE que está agendando?
        [ForeignKey("Usuario")]
        [Display(Name = "Cliente")]
        public int UsuarioId { get; set; } // FK para Usuario (o seu "cliente final")

        // 2. Com qual PROFISSIONAL?
        [ForeignKey("Profissional")]
        [Display(Name = "Profissional")]
        public int ProfissionalId { get; set; }

        // 3. Qual SERVIÇO?
        [ForeignKey("Servico")]
        [Display(Name = "Serviço")]
        public int ServicoId { get; set; }

        // --- Propriedades de Navegação ---
        // Permitem acessar os dados completos (ex: Agendamento.Usuario.Nome)

        public virtual Usuario Usuario { get; set; }
        public virtual Profissional Profissional { get; set; }
        public virtual Servico Servico { get; set; }
    }
}