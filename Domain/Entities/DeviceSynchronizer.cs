using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class DeviceSynchronizer:BaseModel
    {
        [Key]
        public Guid Oid { get; set; }
        public Guid? PersonId { get; set; }
        public Guid? VisitorId { get; set; }

        [ForeignKey("VisitorId")]
        public virtual Visitor Visitor { get; set; }

        [ForeignKey("PersonId")]
        public virtual Person Person { get; set; }


    }
}
