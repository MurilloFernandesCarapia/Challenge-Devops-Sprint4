using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare360.Domain.Entities
{
    [Table("TB_USUARIO_PETCARE")]
    public class Usuario
    {
        [Key]
        [Column("ID_USUARIO")]
        public int IdUsuario { get; set; }

        [Required]
        [MaxLength(100)]
        [Column("NM_USUARIO")]
        public string NmUsuario { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        [Column("EMAIL")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MaxLength(256)]
        [Column("SENHA_HASH")]
        public string SenhaHash { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        [Column("PERFIL")]
        public string Perfil { get; set; } = PerfilUsuario.Usuario;

        [Column("DT_CRIACAO")]
        public DateTime DtCriacao { get; set; } = DateTime.UtcNow;
    }
}