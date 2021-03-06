﻿using Microsoft.Xrm.Client;
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
            public DateTime ResultDate { get; set; } = DateTime.UtcNow;
            public string Version { get; set; } = "1";
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
                    lead["parentcontactid"] = contact.ToEntityReference();
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
                    lead["parentaccountid"] = account.ToEntityReference();
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

        public static XrmLeadResult CaptureLead(Dictionary<string, string> properties, IDictionary<string, string> settings, IDictionary<string, string> actions, CrmConnection connection = null)
        {
            XrmLeadResult result = new XrmLeadResult();
            bool match = Convert.ToBoolean(settings.GetValueOrDefault<string>("match", bool.FalseString));
            Entity lead = new Entity("lead", Guid.NewGuid());
            string email = properties.GetValueOrDefault<string>("emailaddress1", "");
            result.Email = email;

            string accountId = properties.GetValueOrDefault<string>("accountid", properties.GetValueOrDefault<string>("companyname", ""));
            Entity contact = null;
            Entity account = null;

            contact = XrmCore.RetrieveByAttribute("contact", "emailaddress1", email,CacheResults: false).Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).FirstOrDefault();
            if (contact != null)
            {
                result.ContactId = contact.Id.ToString();
                result.FullName = contact.GetAttributeValue<string>("fullname");
                if (contact.Contains("parentcustomerid"))
                {
                    EntityReference accountReference = contact.GetAttributeValue<EntityReference>("parentcustomerid");
                    result.AccountId = accountReference.Id.ToString();
                    result.CompanyName = accountReference.Name;
                    lead["parentaccountid"] = accountReference;
                }

                // return matched contact result
                return result;
            }
            else
            {
                // No contact match. Match against an existing lead
                EntityCollection leadCollection = XrmCore.RetrieveByAttribute("lead", "emailaddress1", email, new Microsoft.Xrm.Sdk.Query.ColumnSet("createdon", "fullname", "parentcontactid", "parentaccountid", "companyname"), CacheResults: false);
                if (leadCollection.Entities.Count > 0)
                {
                    lead = leadCollection.Entities.OrderByDescending(x => x.GetAttributeValue<DateTime>("createdon")).First();
                    result.LeadId = lead.Id.ToString();
                    if (lead.Contains("fullname"))
                    {
                        result.FullName = lead.GetAttributeValue<string>("fullname");
                    }
                    EntityReference er = null;
                    if (lead.Contains("parentaccountid"))
                    {
                        er = lead.GetAttributeValue<EntityReference>("parentaccountid");
                        Entity parentAccount = XrmCore.Retrieve("account", er.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("statecode"));
                        if (parentAccount != null && parentAccount.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                        {
                            result.AccountId = er.Id.ToString();
                            result.CompanyName = er.Name;
                        }
                    }
                    else if (lead.Contains("companyname"))
                    {
                        result.CompanyName = lead.GetAttributeValue<string>("companyname");
                    }
                    if (lead.Contains("parentcontactid"))
                    {
                        er = lead.GetAttributeValue<EntityReference>("parentcontactid");
                        Entity parentContact = XrmCore.Retrieve("contact", er.Id, new Microsoft.Xrm.Sdk.Query.ColumnSet("statecode"));
                        if (parentContact != null && parentContact.GetAttributeValue<OptionSetValue>("statecode").Value == 0)
                        {
                            result.ContactId = er.Id.ToString();
                        }
                    }

                    // result matched lead result
                    return result;
                }
            }

            //
            // No contact or lead match. Create a new lead in the system
            //
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

                if (account != null)
                {
                    lead["parentaccountid"] = account.ToEntityReference();
                    properties.Remove("accountid");
                    result.AccountId = account.Id.ToString();
                    result.CompanyName = account.GetAttributeValue<string>("name");
                }
            }

            if (account == null)
            {
                result.CompanyName = properties.GetValueOrDefault<string>("companyname", "");
            }

            result.FullName = properties.GetValueOrDefault<string>("fullname", "");
            if (string.IsNullOrEmpty(result.FullName))
            {
                if (properties.ContainsKey("firstname") && properties.ContainsKey("lastname"))
                {
                    result.FullName = string.Concat(properties.GetValueOrDefault<string>("firstname", ""), " ", properties.GetValueOrDefault<string>("lastname", ""));
                }
            }

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
