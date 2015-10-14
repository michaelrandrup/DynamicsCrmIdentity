using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity.DynamicsCrm;
using System.Web.Mvc;
using System.Text;

namespace System.Web.Mvc
{
    public static class ProfileExtensions
    {
        public static IEnumerable<SelectListItem> AsSelectList(this ProfileField Field, string[] SelectedValues, string UndefinedText = "(not selected)")
        {
            List<SelectListItem> items = new List<SelectListItem>();
            if (!string.IsNullOrEmpty(UndefinedText))
            {
                items.Add(new SelectListItem() { Value = "", Text = UndefinedText, Selected = (SelectedValues == null || SelectedValues.Length == 0) });
            }
            foreach (string value in Field.Options.Split(new string[] { "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries))
            {
                items.Add(new SelectListItem()
                {
                    Text = value,
                    Value = value,
                    Selected = SelectedValues.Contains(value)
                });
            }
            return items;
        }
        
        public static IEnumerable<SelectListItem> AsSelectList(this ProfileField Field, string SelectedValue, string UndefinedText = "(not selected)")
        {
            string[] selected = new string[0];
            if (!string.IsNullOrEmpty(SelectedValue))
            {
                selected = SelectedValue.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
            }
            return AsSelectList(Field, selected, UndefinedText);
        }

        public static MvcHtmlString AsOptionList(this ProfileField Field, string SelectedValue, string UndefinedText = "(not selected)")
        {
            IEnumerable<SelectListItem> list = AsSelectList(Field, SelectedValue, UndefinedText);
            StringBuilder html = new StringBuilder();
            foreach (SelectListItem item in list)
            {
                html.AppendFormat("<option value=\"{0}\" title=\"{1}\" {2}>{1}</option>\r\n",item.Value,item.Text, item.Selected ? "selected" : "");
            }
            return new MvcHtmlString(html.ToString());
        }

        public static MvcHtmlString BooleanAsRadio(this HtmlHelper<UserProfile> html, ProfileField Field, bool? value)
        {
            throw new NotImplementedException();
        }
    }
}

