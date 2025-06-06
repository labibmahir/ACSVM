using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Dto
{
    public class PersonExcelImportResult
    {
        public List<PersonDto> Persons { get; set; } = new();
        public List<PersonExcelImportValidations> ValidationErrors { get; set; } = new();
    }

}
