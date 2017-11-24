using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;
using ExpressiveAnnotations.Attributes;
using GLAA.ViewModels;
using GLAA.ViewModels.Attributes;
using GLAA.ViewModels.LicenceApplication;

public static class HtmlHelpers
{
    public static IHtmlString ExternalStatusFor<TModel>(this HtmlHelper<TModel> html, LicenceStatusViewModel status)
    {
        var header = new TagBuilder("h3");
        header.AddCssClass(status.Status.GetExternalClassNames());
        header.SetInnerText(status.ExternalDescription);

        return new HtmlString(header.ToString());
    }

    public static IHtmlString InternalStatusFor<TModel>(this HtmlHelper<TModel> html, LicenceStatusViewModel status)
    {
        var header = new TagBuilder("h3");
        header.AddCssClass(status.Status.GetInternalClassNames());
        header.SetInnerText(status.InternalStatus);

        return new HtmlString(header.ToString());
    }

    public static IHtmlString TextFormGroupFor<TModel, TValue>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, TValue>> expression)
    {
        var control = html.TextBoxFor(expression, new { @class = "form-control" });
        return BuildFormGroupForControl(html, expression, control);
    }

    public static IHtmlString TextAreaFormGroupFor<TModel, TValue>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, TValue>> expression)
    {
        var control = html.TextAreaFor(expression, 6, 60, new { @class = "form-control" });
        return BuildFormGroupForControl(html, expression, control);
    }

    public static IHtmlString DateFormGroupFor<TModel>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, DateViewModel>> expression)
    {
        if (HasUIHint(expression))
        {
            return BuildFormGroupForControl(html, expression, html.EditorFor(expression));
        }
        return html.TextFormGroupFor(expression);
    }

    public static IHtmlString TimeSpanFormGroupFor<TModel>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, TimeSpanViewModel>> expression)
    {
        return BuildFormGroupForControl(html, expression, html.EditorFor(expression, "_NullableTimeSpan"));
    }

    public static IHtmlString RequiredCheckbox<TModel>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, bool>> expression)
    {
        var fieldName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        var hasErrors = html.ViewData.ModelState[fieldName]?.Errors != null &&
                        html.ViewData.ModelState[fieldName].Errors.Any();

        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

        //var label = new TagBuilder("span");
        //label.AddCssClass("body-text");
        //label.InnerHtml = metadata.Description;

        var errorMsg = new TagBuilder("span");
        errorMsg.AddCssClass("error-message");
        errorMsg.InnerHtml = html.ValidationMessageFor(expression).ToString();

        var legend = new TagBuilder("legend");
        legend.Attributes.Add("id", GenerateLegendId(html, expression));
        legend.InnerHtml = $"{errorMsg}";

        //var checkbox = html.CheckBoxFor(expression);

        var cbx = html.CheckBoxFor(expression);
        var lbl = html.LabelFor(expression, metadata.Description);

        var div = new TagBuilder("div");
        div.AddCssClass("multiple-choice");
        div.InnerHtml = $"{cbx}{lbl}";

        var fieldset = new TagBuilder("fieldset")
        {
            InnerHtml = $"{legend}{div}"
        };

        var parent = new TagBuilder("div");
        parent.AddCssClass("form-group");
        parent.InnerHtml = fieldset.ToString();

        if (hasErrors)
        {
            parent.AddCssClass("form-group-error");
        }

        return new HtmlString(parent.ToString());
    }

    public static IHtmlString CheckBoxFormGroupFor<TModel>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, List<CheckboxListItem>>> expression, IList<CheckboxListItem> values)
    {
        var fieldName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        var hasErrors = html.ViewData.ModelState[fieldName]?.Errors != null &&
                        html.ViewData.ModelState[fieldName].Errors.Any();

        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);

        var label = new TagBuilder("span");
        label.AddCssClass("body-text");
        label.InnerHtml = metadata.Description;

        var errorMsg = new TagBuilder("span");
        errorMsg.AddCssClass("error-message");
        errorMsg.InnerHtml = html.ValidationMessageFor(expression).ToString();

        var legend = new TagBuilder("legend");
        legend.Attributes.Add("id", GenerateLegendId(html, expression));
        legend.InnerHtml = $"{label}{errorMsg}";

        var checkboxes = html.CheckboxListFor(expression, values);

        var fieldset = new TagBuilder("fieldset")
        {
            InnerHtml = $"{legend}{checkboxes}"
        };

        var parent = new TagBuilder("div");
        parent.AddCssClass("form-group");
        parent.InnerHtml = fieldset.ToString();
        if (hasErrors)
        {
            parent.AddCssClass("form-group-error");
        }

        return new HtmlString(parent.ToString());
    }

    public static IHtmlString CheckboxListFor<TModel>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, List<CheckboxListItem>>> expression, IList<CheckboxListItem> values)
    {
        var sb = new StringBuilder();

        for (var i = 0; i < values.Count; i++)
        {
            var indexExpression = Expression.Call(expression.Body, typeof(List<CheckboxListItem>).GetMethod("get_Item"), Expression.Constant(i));

            var checkedAccessExpression = Expression.Property(indexExpression, typeof(CheckboxListItem), "Checked");
            var checkedLambda = Expression.Lambda<Func<TModel, bool>>(checkedAccessExpression, expression.Parameters[0]);

            var idAccessExpression = Expression.Property(indexExpression, typeof(CheckboxListItem), "Id");
            var idLambda = Expression.Lambda<Func<TModel, int>>(idAccessExpression, expression.Parameters[0]);

            var nameAccessExpression = Expression.Property(indexExpression, typeof(CheckboxListItem), "Name");
            var nameLambda = Expression.Lambda<Func<TModel, string>>(nameAccessExpression, expression.Parameters[0]);

            var idHid = html.HiddenFor(idLambda);
            var nameHid = html.HiddenFor(nameLambda);
            var cbx = html.CheckBoxFor(checkedLambda);
            var lbl = html.LabelFor(checkedLambda, values[i].Name);

            var div = new TagBuilder("div");
            div.AddCssClass("multiple-choice");
            div.InnerHtml = $"{idHid}{nameHid}{cbx}{lbl}";

            sb.Append(div);
        }

        return new HtmlString(sb.ToString());
    }

    public static IHtmlString RadioButtonFormGroupFor<TModel, TValue>(this HtmlHelper<TModel> html,
        Expression<Func<TModel, TValue>> expression, IList<SelectListItem> values, string subHeading = null)
    {
        var fieldName = html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        var hasErrors = html.ViewData.ModelState[fieldName]?.Errors != null &&
                        html.ViewData.ModelState[fieldName].Errors.Any();

        string sub;
        if (!string.IsNullOrEmpty(subHeading))
        {
            var subTag = new TagBuilder("span");
            subTag.AddCssClass("body-text");
            subTag.InnerHtml = subHeading;
            sub = subTag.ToString();
        }
        else
        {
            sub = string.Empty;
        }

        var label = html.LabelWithHintFor(expression);

        var errorMsg = new TagBuilder("span");
        errorMsg.AddCssClass("error-message");
        errorMsg.InnerHtml = html.ValidationMessageFor(expression).ToString();

        var legend = new TagBuilder("legend");
        legend.Attributes.Add("id", GenerateLegendId(html, expression));

        legend.InnerHtml = $"{sub}{label}{errorMsg}";

        var fieldset = new TagBuilder("fieldset");
        var sb = new StringBuilder();
        sb.Append(legend);

        foreach (var item in values)
        {
            var rbx = html.RadioButtonFor(expression, item.Value);
            var lbl = html.Label(item.Text);
            var div = new TagBuilder("div");
            div.AddCssClass("multiple-choice");
            div.InnerHtml = $"{rbx}{lbl}";

            sb.Append(div);
        }

        fieldset.InnerHtml = sb.ToString();

        var parent = new TagBuilder("div");
        parent.AddCssClass("form-group");
        parent.InnerHtml = fieldset.ToString();
        if (hasErrors)
        {
            parent.AddCssClass("form-group-error");
        }

        return new HtmlString(parent.ToString());
    }

    public static IHtmlString LabelWithHintFor<TModel, TValue>(this HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
    {
        var htmlFieldName = ExpressionHelper.GetExpressionText(expression);
        var metadata = ModelMetadata.FromLambdaExpression(expression, html.ViewData);
        var resolvedLabelText = metadata.DisplayName ?? metadata.PropertyName ?? htmlFieldName.Split('.').Last();
        var optionalText = metadata.IsRequired || 
            IsRequiredIf(expression) || 
            IsAssertThat(expression) || 
            IsRequireTrue(expression) ||
            IsRequiredForShellfish(expression, html.ViewData.Model as IShellfishSection) ||
            IsRequiredIfUk(expression, html.ViewData.Model as IUkOnly) ? string.Empty : "(optional)";

        var span = new TagBuilder("span");
        span.AddCssClass("form-hint");
        span.InnerHtml = metadata.Description;

        var label = new TagBuilder("label");
        label.Attributes.Add("for", TagBuilder.CreateSanitizedId(html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(htmlFieldName)));
        label.AddCssClass("form-label");
        label.InnerHtml = $"{resolvedLabelText} {optionalText} {span}";

        return new HtmlString(label.ToString());
    }



    private static IHtmlString BuildFormGroupForControl<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression, IHtmlString control)
    {
        var fieldName =
            html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));
        var hasErrors = html.ViewData.ModelState[fieldName]?.Errors != null &&
                        html.ViewData.ModelState[fieldName].Errors.Any();

        var legend = new TagBuilder("legend");
        legend.Attributes.Add("id", GenerateLegendId(html, expression));

        var error = new TagBuilder("span");
        error.AddCssClass("error-message");

        error.InnerHtml = html.ValidationMessageFor(expression).ToString();
        legend.InnerHtml = $"{html.LabelWithHintFor(expression)} {error}";

        var fieldset = new TagBuilder("fieldset") { InnerHtml = $"{legend} {control}" };

        var parent = new TagBuilder("div");
        parent.AddCssClass("form-group");
        parent.InnerHtml = fieldset.ToString();
        if (hasErrors)
        {
            parent.AddCssClass("form-group-error");
        }

        return new HtmlString(parent.ToString());
    }

    private static bool IsRequireTrue<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<RequireTrueAttribute>() != null;
    }

    private static bool IsRequiredIf<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<RequiredIfAttribute>() != null;
    }

    private static bool IsRequiredForShellfish<TModel, TValue>(Expression<Func<TModel, TValue>> expression, IShellfishSection model)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<RequiredForShellfishAttribute>() != null && model.IsShellfish;
    }

    private static bool IsRequiredIfUk<TModel, TValue>(Expression<Func<TModel, TValue>> expression, IUkOnly model)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<RequiredIfUkAddressAttribute>() != null && model.IsUk;
    }

    private static bool IsAssertThat<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<AssertThatAttribute>() != null;
    }

    private static bool HasUIHint<TModel, TValue>(Expression<Func<TModel, TValue>> expression)
    {
        var memberExpression = expression.Body as MemberExpression;
        if (memberExpression == null)
        {
            throw new InvalidOperationException("Expression must be a member expression");
        }

        return memberExpression.Member.GetCustomAttribute<UIHintAttribute>() != null;
    }

    private static string GenerateLegendId<TModel, TValue>(HtmlHelper<TModel> html, Expression<Func<TModel, TValue>> expression)
    {
        var htmlFieldId =
            html.ViewContext.ViewData.TemplateInfo.GetFullHtmlFieldName(ExpressionHelper.GetExpressionText(expression));

        return GenerateLegendId(htmlFieldId);
    }

    private static string GenerateLegendId(string fieldName)
    {
        return $"legend_{fieldName}";
    }
}
