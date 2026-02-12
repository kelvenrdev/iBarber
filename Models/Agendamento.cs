using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace iBarber.Models
{
    [Table(Agendamento)]
    public class Agendamento
    {
        [Key]
        public int Id { get; set; }
    }
}
