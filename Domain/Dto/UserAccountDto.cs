using System.ComponentModel.DataAnnotations;

namespace Domain.Dto;

public class UserAccountDto
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
    
    public Guid ModifiedBy { get; set; }
}