using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.DynamicsCrm;
using System.Web.Mvc;

namespace System.Web.MVC
{
    public static class ProfileExtensions
    {
        public static IEnumerable<SelectListItem> AsSelectList(this ProfileField Field, string SelectedValue)
        {
            foreach (string value in Field.Options.Split(new string[] {"\r\n"}, StringSplitOptions.RemoveEmptyEntries))
            {
                yield return new SelectListItem()
                {
                    Text = value,
                    Value = value,
                    Selected = !string.IsNullOrEmpty(SelectedValue) && SelectedValue.Equals(value, StringComparison.OrdinalIgnoreCase)
                };
            }
        }

        public static MvcHtmlString BooleanAsRadio(this HtmlHelper<UserProfile> html, ProfileField Field, bool? value)
        {
            throw new NotImplementedException();
        }
    }
}

namespace System.Web.Mvc.Html
{
    public static class ProfileExtensions
    {
        public static MvcHtmlString BooleanAsRadio(this HtmlHelper<UserProfile> html, ProfileField Field, bool? value)
        {
            throw new NotImplementedException();
        }
    }
}