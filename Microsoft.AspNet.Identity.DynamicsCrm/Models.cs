using DynamicsCrm.WebsiteIntegration.Core;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
   public class XrmProfileModel : DynamicObject
   {
       public XrmProfileModel()
       {

       }

       public XrmProfileModel(NameValueCollection collection)
       {
           foreach (string key in collection.AllKeys)
           {
               string propname = GetPropName(key);
               properties.Add(key, collection[key]);
           }
       }
       public XrmProfileModel(UserProfile profiles)
       {
           foreach (UserProfileField field in profiles.Fields)
           {
               string propname = GetPropName(field.Name);
               properties.Add(propname, field.Value);
               properties.Add(propname + "id", field.Id);
               properties.Add(propname + "name", propname);
           }
           properties.Add("__profile", profiles.ProfileDefinition);
           properties.Add("__id", profiles.Id);

       }

       public void UpdateUserProfile(UserProfile profiles)
       {
           foreach (UserProfileField field in profiles.Fields)
           {
               string propname = GetPropName(field.Name);
               field.Value = Convert.ToString(properties[propname]);
           }
       }

       private Dictionary<string, object> properties = new Dictionary<string, object>();

       public Dictionary<string, object> Properties
       {
           get { return properties; }
           set { properties = value; }
       }

       private string GetPropName(string name)
       {
           Regex rgx = new Regex("[^a-zA-Z0-9 _]");
           return rgx.Replace(name, "").Replace(" ", "").ToLower();
       }

       public override IEnumerable<string> GetDynamicMemberNames()
       {
           foreach (string key in properties.Keys)
           {
               yield return key;
           }
       }

       public override bool TryGetMember(GetMemberBinder binder, out object result)
       {
           string propname = GetPropName(binder.Name);
           if (properties.ContainsKey(propname))
           {
               result = properties[propname];
               return true;
           }
           else
           {
               result = null;
               return false;
           }
       }

       public override bool TrySetMember(SetMemberBinder binder, object value)
       {
           string propname = GetPropName(binder.Name);
           if (properties.ContainsKey(propname))
           {
               properties[propname] = Convert.ToString(value);
           }
           else
           {
               properties.Add(propname, Convert.ToString(value));
           }
           return true;
       }

   }
}
