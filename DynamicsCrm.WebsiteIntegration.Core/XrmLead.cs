using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class XrmLead
    {
        public class XrmLeadResult
        {
            public string LeadId { get; set; }
            public string ContactId { get; set; }
            public string AccountId { get; set; }
            public string Email { get; set; }
            public string FullName { get; set; }
            public string CompanyName { get; set; }


        }
        public static XrmLeadResult CreateLead(Dictionary<string, string> properties, IDictionary<string, string> settings, IDictionary<string, string> actions, CrmConnection connection = null)
        {
            XrmLeadResult result = new XrmLeadResult();
            bool match = Convert.ToBoolean(settings.GetValueOrDefault<string>("match", bool.FalseString));
            Entity lead = new Entity("lead", Guid.NewGuid());
            string email = properties.GetValueOrDefault<string>("emailaddress1", "");
            string accountId = properties.GetValueOrDefault<string>("accountid", properties.GetValueOrDefault<string>("companyname", ""));
            Entity contact = null;
            Entity account = null;
            if (match && !string.IsNullOrEmpty(email))
            {
                
                if (!string.IsNullOrEmpty(accountId))
                {
                    Guid g;
                    if (Guid.TryParse(accountId, out g))
                    {
                        account = XrmCore.Retrieve("account", g);
                    }
                    else
                    {
                        account = XrmCore.RetrieveByAttribute("account", "name", accountId).Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
                    }
                }

                contact = XrmCore.RetrieveByAttribute("contact", "emailaddress1", email).Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
                if (contact != null)
                {
                    lead["contactid"] = contact.ToEntityReference();
                    result.ContactId = contact.Id.ToString();
                    if (accountId == null && contact.Contains("parentcustomerid"))
                    {
                        account = XrmCore.Retrieve("account", contact.GetAttributeValue<EntityReference>("parentcustomerid").Id);
                    }
                    //if (string.IsNullOrEmpty(accountId))
                    //{
                    //    accountId = contact.GetAttributeValue<string>("parentcustomerid_name");
                    //}
                }

                if (account != null)
                {
                    lead["accountid"] = account.ToEntityReference();
                    result.AccountId = account.Id.ToString();
                }
            }

            result.CompanyName = properties.GetValueOrDefault<string>("companyname", account != null && account.Contains("name") ? account.GetAttributeValue<string>("name") : "");
            result.Email = email;
            result.FullName = properties.GetValueOrDefault<string>("fullname", contact != null && contact.Contains("fullname") ? contact.GetAttributeValue<string>("fullname") : "");

            // Apply properties
            EntityMetadata meta = XrmCore.RetrieveMetadata("lead", EntityFilters.All, connection);
            foreach (KeyValuePair<string, string> kv in properties)
            {
                lead.SetAttributeMetaValue(kv, settings, meta);
            }

            Guid Id = XrmCore.CreateEntity(lead);
            result.LeadId = Id.ToString();

            // Apply actions
            foreach (KeyValuePair<string, string> kv in actions)
            {
                lead.ApplyAction(kv);
            }


            return result;

        }

        public static Guid CreateLead(string subject, string description, string firstName, string lastName, string companyName, string email, NameValueCollection otherFields, CrmConnection connection = null)
        {
            Entity lead = new Entity("lead", Guid.NewGuid());
            if (!string.IsNullOrEmpty(email))
            {
                lead["emailaddress1"] = email;
                EntityCollection contacts = XrmCore.RetrieveByAttribute("contact", "emailaddress1", email);
                if (contacts.Entities.Count > 0)
                {
                    Entity contact = contacts.Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).First();
                    lead["contactid"] = contact.ToEntityReference();
                    if (contact.Contains("accountid"))
                    {
                        lead["accountid"] = contact.GetAttributeValue<EntityReference>("accountid");
                    }
                }
            }
            lead["subject"] = subject;
            lead["firstname"] = firstName;
            lead["lastname"] = lastName;
            lead["companyname"] = companyName;
            lead["description"] = description;

            if (otherFields != null && otherFields.Count > 0)
            {
                foreach (string key in otherFields.AllKeys)
                {
                    lead[key] = otherFields[key];
                }
            }

            return XrmCore.CreateEntity(lead);

        }



        public static Guid CreateLead(NameValueCollection formCollection)
        {
            string LeadId = formCollection.Get("leadid");
            Entity lead = new Entity("lead", Guid.NewGuid());
            Guid id = Guid.Empty;
            if (!string.IsNullOrEmpty(LeadId) && Guid.TryParse(LeadId, out id))
            {
                lead = XrmCore.Retrieve("lead", id);
            }

            string Email = formCollection.Get("emailaddress1");
            if (!string.IsNullOrEmpty(Email))
            {
                EntityCollection contacts = XrmCore.RetrieveByAttribute("contact", "emailaddress1", Email);
                if (contacts.Entities.Count > 0)
                {
                    Entity contact = contacts.Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).First();
                    lead["contactid"] = contact.ToEntityReference();
                    if (contact.Contains("accountid"))
                    {
                        lead["accountid"] = contact.GetAttributeValue<EntityReference>("accountid");
                    }
                }
            }
            foreach (string key in formCollection.AllKeys.Except(new string[] { "leadid" }))
            {
                lead[key] = formCollection[key];
            }
            if (id.Equals(Guid.Empty))
            {
                id = XrmCore.CreateEntity(lead);
            }
            else
            {
                XrmCore.UpdateEntity(lead);
                id = lead.Id;
            }
            return id;
        }
    }
}
