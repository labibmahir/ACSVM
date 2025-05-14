using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;

namespace Domain.Dto.PaginationFiltersDto
{
    public class PaginationDtoBinder : IModelBinder
    {
        public bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (bindingContext == null)
                throw new ArgumentNullException(nameof(bindingContext));

            // Initialize the model
            var model = new PaginationDto();


            //var pageValue = bindingContext.ValueProvider.GetValue("Page");
            //model.Page = pageValue != null && int.TryParse(pageValue.AttemptedValue, out var page) ? page : 1;

            //var pageSizeValue = bindingContext.ValueProvider.GetValue("PageSize");
            //model.PageSize = pageSizeValue != null && int.TryParse(pageSizeValue.AttemptedValue, out var pageSize) ? pageSize : 10;

            var orderByValue = bindingContext.ValueProvider.GetValue("orderBy");
            model.orderBy = orderByValue?.AttemptedValue ?? "desc";

            bindingContext.Model = model;

            return true;
        }
    }
}
