using GLAA.ViewModels;
using GLAA.Web.Controllers;

namespace GLAA.Web.FormLogic
{
    public class LicenceApplicationFormDefinition : IFormDefinition
    {
        private readonly IFieldConfiguration fieldConfiguration;

        public LicenceApplicationFormDefinition(IFieldConfiguration fieldConfiguration)
        {
            this.fieldConfiguration = fieldConfiguration;
        }

        public object GetViewModel<TParent>(FormSection section, int id, TParent parent)
        {
            var page = GetPageDefinition(section, id);

            if (page == null || parent == null)
            {
                return null;
            }

            return page.GetViewModelExpressionForPage(parent);
        }

        public bool CanViewNextModel<TParent>(FormSection section, int id, TParent parent)
        {
            var page = GetPageDefinition(section, id);

            if (page.OverrideViewCondition)
            {
                return true;
            }

            var model = GetViewModel(page, parent) as ICanView<TParent>;

            return model == null || model.CanView(parent);
        }

        public int GetSectionLength(FormSection section)
        {
            return fieldConfiguration.Fields[section].Length;
        }

        private FormPageDefinition GetPageDefinition(FormSection section, int id)
        {
            var index = id - 1;
            if (!fieldConfiguration.Fields.ContainsKey(section) || index >= fieldConfiguration.Fields[section].Length)
            {
                return null;
            }
            return fieldConfiguration.Fields[section][index];
        }

        private static object GetViewModel<TParent>(FormPageDefinition page, TParent parent)
        {
            return page.GetViewModelExpressionForPage(parent);
        }
    }
}