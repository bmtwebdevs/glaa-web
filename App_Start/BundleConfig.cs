using System.Diagnostics.CodeAnalysis;
using System.Web.Optimization;

namespace GLAA.Web
{
    [ExcludeFromCodeCoverage]
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*",
                        "~/Scripts/expressive.annotations*",
                        "~/Scripts/app/gdsValidation.js"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new ScriptBundle("~/bundles/govuk").Include(
                "~/Scripts/govuk/modules.js",
                "~/Scripts/govuk/show-hide-content.js",
                "~/Scripts/govuk/stop-scrolling-at-footer.js",
                "~/Scripts/govuk/stick-at-top-when-scrolling.js"));

            bundles.Add(new ScriptBundle("~/bundles/app").Include(
                      "~/Scripts/app/branchedQuestion.js",
                      "~/Scripts/app/hiddenContent.js"));

            bundles.Add(new StyleBundle("~/Content/glaa").Include(
                      "~/Content/styles/accordion.css",
                      "~/Content/styles/licence/statuses.css",
                      "~/Content/styles/glaa.css"));

            bundles.Add(new StyleBundle("~/Content/govuk").Include(
                      "~/Content/styles/govuk-elements/elements-documentation.css",                                         
                      "~/Content/styles/govuk-template/assets/stylesheets/govuk-template.css",
                      "~/Content/styles/govuk-elements/govuk-elements-styles.css",
                      "~/Content/styles/govuk-elements/govuk-pattern-task-list.css",
                      "~/Content/styles/govuk-elements/govuk-pattern-check-your-answers.css",
                      "~/Content/styles/govuk-overrides.css",
                      "~/Content/styles/sidebar-navigation.css"
                      ));

            bundles.Add(new StyleBundle("~/Content/administration").Include(
                      "~/Content/styles/admin/layout.css",                      
                      "~/Content/styles/admin/dashboard.css",
                      "~/Content/styles/admin/application-list.css"));
        }
    }
}
