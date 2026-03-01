using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iBarber.Models
{
    [Table("Usuarios")]
    public class Usuario
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage ="Obrigatorio inserir um nome")]
        public string Nome { get; set; }

        [Required(ErrorMessage = "Obrigatorio inserir uma senha")]
        [DataType(DataType.Password)]
        public string Senha{ get; set; }

        [Required(ErrorMessage = "Obrigatorio inserir um perfil de usuario")]
        public Perfil Perfil { get; set; }
    }

    public enum Perfil
    {
        Admin,
        User
    }
}
