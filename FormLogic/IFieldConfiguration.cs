using System.Collections.Generic;
using GLAA.Web.Controllers;

namespace GLAA.Web.FormLogic
{
    public interface IFieldConfiguration
    {
        IDictionary<FormSection, FormPageDefinition[]> Fields { get; set; }
    }
}