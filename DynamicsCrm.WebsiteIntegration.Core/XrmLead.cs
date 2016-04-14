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
        public static Guid CreateLead(Dictionary<string, string> properties, IDictionary<string, string> settings, IDictionary<string, string> actions, CrmConnection connection = null)
        {
            bool match = Convert.ToBoolean(settings.GetValueOrDefault<string>("match", bool.FalseString));
            Entity lead = new Entity("lead", Guid.NewGuid());
            string email = properties.GetValueOrDefault<string>("email", "");
            string accountId = properties.GetValueOrDefault<string>("accountid", "");
            if (match && !string.IsNullOrEmpty(email))
            {
                Entity contact = null;
                if (!string.IsNullOrEmpty(accountId))
                {
                    Guid g;
                    Entity account = null;
                    if (Guid.TryParse(accountId, out g))
                    {
                        account = XrmCore.Retrieve("account", g);
                    }
                    else
                    {
                        account = XrmCore.RetrieveByAttribute("account", "name", accountId).Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
                    }
                    if (account != null)
                    {
                        lead["accountid"] = account.ToEntityReference();
                    }
                }
                contact = XrmCore.RetrieveByAttribute("contact", "emailaddress1", email).Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
                if (contact != null)
                {
                    lead["contactid"] = contact.ToEntityReference();
                    if (string.IsNullOrEmpty(accountId))
                    {
                        accountId = contact.GetAttributeValue<string>("parentcustomerid_name");
                    }
                }
            }

            // Apply properties
            EntityMetadata meta = XrmCore.RetrieveMetadata("lead", EntityFilters.All, connection);
            foreach (KeyValuePair<string, string> kv in properties)
            {
                lead.SetAttributeMetaValue(kv,settings, meta);
            }
            Guid Id = XrmCore.CreateEntity(lead);
            
            // Apply actions
            foreach (KeyValuePair<string, string> kv in actions)
            {
                lead.ApplyAction(kv);
            }

            return lead.Id;

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
