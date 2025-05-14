using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Utilities;
using Utilities.Constants;

namespace Domain.Entities;

public class UserAccount : BaseModel
{
    [Key]
    public Guid Oid { get; set; }
    
    [Required]
    [StringLength(100)]
    public string FirstName { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Surname { get; set; }
    
    [Required]
    [StringLength(50)]
    public string Email { get; set; }
    
    [StringLength(20)]
    public string CellPhone { get; set; }
    
    [StringLength(4)]
    public string CountryCode { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Username { get; set; }
    
    [Required]
    [StringLength(100)]
    public string Password { get; set; }
    
    public bool IsAccountActive { get; set; }
    
    [Required]
    public Enums.Role RoleId { get; set; }
}