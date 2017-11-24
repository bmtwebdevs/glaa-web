using System;
using System.Linq.Expressions;

namespace GLAA.Web.FormLogic
{
    public class FormPageDefinition
    {
        public FormPageDefinition()
        {
            SubModelName = string.Empty;
            OverrideViewCondition = false;
        }

        public FormPageDefinition(string subModelName, bool overrideViewCondition = false)
        {
            SubModelName = subModelName;
            OverrideViewCondition = overrideViewCondition;
        }

        public string SubModelName { get; }

        public bool OverrideViewCondition { get; }

        public object GetViewModelExpressionForPage<TParent>(TParent parent)
        {
            if (string.IsNullOrEmpty(SubModelName))
            {
                return parent;
            }

            var propExpression = Expression.Property(Expression.Constant(parent), SubModelName);
            var lambda = Expression.Lambda<Func<object>>(propExpression);
            return lambda.Compile()();
        }
    }
}