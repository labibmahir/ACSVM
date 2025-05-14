using System.Web.Http.ModelBinding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto.PaginationFiltersDto
{
    [ModelBinder(typeof(PaginationDtoBinder))]
    public class PaginationDto
    {
        public string? search { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public string orderBy { get; set; } = "desc";
    }
}
