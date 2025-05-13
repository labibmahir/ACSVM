namespace Domain.Entities;

public class BaseModel
{
    public Guid? CreatedBy { get; set; }
    
    public DateTime? DateCreated { get; set; }
    
    public Guid? ModifiedBy { get; set; }
    
    public DateTime? DateModified { get; set; }
    
    public bool? IsDeleted { get; set; }
}