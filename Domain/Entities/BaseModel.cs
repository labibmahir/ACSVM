using System.ComponentModel.DataAnnotations.Schema;

namespace Domain.Entities;

public class BaseModel
{
    public Guid? CreatedBy { get; set; }
    
    [Column(TypeName = "smalldatetime")]
    public DateTime? DateCreated { get; set; }
    
    public Guid? ModifiedBy { get; set; }
    
    [Column(TypeName = "smalldatetime")]
    public DateTime? DateModified { get; set; }
    public int? OrganizationId { get; set; }
    public bool? IsDeleted { get; set; }
}