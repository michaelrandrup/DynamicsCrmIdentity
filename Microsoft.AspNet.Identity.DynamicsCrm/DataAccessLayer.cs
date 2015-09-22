using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm.DAL
{
    // Url=https://contoso.crm.dynamics.com; Username=jsmith@live-int.com; Password=passcode; DeviceID=contoso-ba9f6b7b2e6d; DevicePassword=passcode
    public static class XrmConnection
    {


        private static CrmConnection _Connection = null;
        public static CrmConnection Connection
        {
            get
            {
                if (_Connection == null)
                {
                    InitializeConnection(null);
                }
                return _Connection;
            }
            internal set
            {
                _Connection = value;
            }
        }

        private static void InitializeConnection(string Name)
        {
            string connectionString = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[Name ?? ConnectionName].ConnectionString;
            _Connection = CrmConnection.Parse(connectionString);


        }



        #region Public Properties

        private static string _ConnectionName = "CrmServices";
        public static string ConnectionName
        {
            get { return _ConnectionName; }
            set { _ConnectionName = value; }
        }

        #endregion

    }

    public static class XrmMarketing
    {
        public static void AddToMarketingList(Guid[] listMembers, Guid listId)
        {
            AddListMembersListRequest request = new Crm.Sdk.Messages.AddListMembersListRequest();
            request.MemberIds = listMembers;
            request.ListId = listId;
            AddListMembersListResponse response = XrmCore.Execute<AddListMembersListRequest, AddListMembersListResponse>(request);
        }
    }
    public static class XrmLead
    {
        public static bool CreateLead(string subject, string firstName, string lastName, string companyName, string email, CrmConnection connection = null)
        {
            throw new NotImplementedException();
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

    public static class XrmCore
    {
        public static Entity Retrieve(string EntityName, Guid Id, ColumnSet Columns = null, CrmConnection connection = null)
        {
            Columns = Columns ?? new ColumnSet(true);
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                return service.Retrieve(EntityName, Id, Columns);
            }
        }

        public static TResponse Execute<TRequest, TResponse>(TRequest request, CrmConnection connection = null)
            where TRequest : OrganizationRequest
            where TResponse : OrganizationResponse
        {
            OrganizationService srv = new OrganizationService(connection ?? XrmConnection.Connection);
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
            {
                return (TResponse)service.Execute(request);
            }
        }


        public static EntityCollection RetrieveByAttribute(string EntityName, string AttributeName, string AttributeValue, CrmConnection connection = null, bool CacheResults = true)
        {
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression(AttributeName, ConditionOperator.Equal, AttributeValue));
            return RetrieveByFilter(EntityName, filter, connection, CacheResults);
        }

        public static EntityCollection RetrieveByFilter(string EntityName, FilterExpression Filter, CrmConnection connection = null, bool CacheResults = true)
        {
            QueryExpression query = new QueryExpression(EntityName);
            query.ColumnSet = new ColumnSet(true);
            query.Criteria = Filter;

            if (CacheResults)
            {
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
                {
                    query.PageInfo = new PagingInfo() { PageNumber = 1, PagingCookie = null, Count = 5000 };
                    EntityCollection ResultCollection = new EntityCollection();
                    while (true)
                    {
                        EntityCollection col = service.RetrieveMultiple(query);
                        ResultCollection.EntityName = col.EntityName;
                        ResultCollection.Entities.AddRange(col.Entities);
                        if (col.MoreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = col.PagingCookie;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return ResultCollection;
                }
            }
            else
            {
                OrganizationService srv = new OrganizationService(connection ?? XrmConnection.Connection);
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
                {
                    query.PageInfo = new PagingInfo() { PageNumber = 1, PagingCookie = null, Count = 5000 };
                    EntityCollection ResultCollection = new EntityCollection();
                    while (true)
                    {
                        EntityCollection col = service.RetrieveMultiple(query);
                        ResultCollection.EntityName = col.EntityName;
                        ResultCollection.Entities.AddRange(col.Entities);
                        if (col.MoreRecords)
                        {
                            query.PageInfo.PageNumber++;
                            query.PageInfo.PagingCookie = col.PagingCookie;
                        }
                        else
                        {
                            break;
                        }
                    }
                    return ResultCollection;
                }
            }
        }

        public static Guid CreateEntity(Entity entity, CrmConnection connection = null)
        {
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                Guid id = service.Create(entity);
                entity.Id = id;
                return id;
            }
        }

        public static void UpdateEntity(Entity entity, CrmConnection connection = null)
        {
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                service.Update(entity);
            }
        }

        public static void DeleteEntity(string entityName, Guid id)
        {
            Entity e = new Entity(entityName, id);
            DeleteEntity(e, XrmConnection.Connection);
        }

        public static void DeleteEntity(Entity entity, CrmConnection connection = null)
        {
            connection.Timeout = new TimeSpan(0, 10, 0);
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                try
                {

                    service.Delete(entity.LogicalName, entity.Id);
                }
                catch (FaultException<OrganizationServiceFault> fault)
                {
                    throw fault;
                }

                catch (Exception ex)
                {
                    throw ex;
                }

            }
        }

        public static Entity GetWebUserFromLogin(string LoginProvider, string ProviderKey, CrmConnection connection = null)
        {
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                IQueryable<Entity> query = from entity in service.CreateQuery("appl_webuser")
                                           join login in service.CreateQuery("appl_webuserlogin") on
                                           entity["appl_webuserid"] equals login["appl_webuserid"]
                                           where login["appl_loginprovider"] == LoginProvider && login["appl_providerkey"] == ProviderKey
                                           select entity;

                List<Entity> result = query.ToList();
                if (result.Count > 0)
                {
                    return result.First();
                }
            }
            return null;
        }

        public static EntityCollection GetRelated(Entity PrimaryEntity, string RelatedEntityName, string ForeignKeyField, CrmConnection connection = null, bool CacheResults = true)
        {
            if (CacheResults)
            {
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
                {
                    IQueryable<Entity> query = from entity in service.CreateQuery(RelatedEntityName)
                                               where (Guid)entity[ForeignKeyField] == PrimaryEntity.Id
                                               select entity;

                    EntityCollection col = new EntityCollection(query.ToList());
                    col.EntityName = RelatedEntityName;
                    return col;
                }
            }
            else
            {
                OrganizationService srv = new OrganizationService(connection ?? XrmConnection.Connection);
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
                {
                    IQueryable<Entity> query = from entity in service.CreateQuery(RelatedEntityName)
                                               where (Guid)entity[ForeignKeyField] == PrimaryEntity.Id
                                               select entity;

                    EntityCollection col = new EntityCollection(query.ToList());
                    col.EntityName = RelatedEntityName;
                    return col;
                }
            }
        }

        public static void AddRelated(Entity PrimaryEntity, EntityCollection RelatedEntities, string RelationshipName, CrmConnection connection = null)
        {
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
            {
                EntityReferenceCollection col = new EntityReferenceCollection();
                foreach (Entity ent in RelatedEntities.Entities)
                {
                    col.Add(ent.ToEntityReference());
                }
                AssociateRequest request = new AssociateRequest()
                {
                    Target = PrimaryEntity.ToEntityReference(),
                    RelatedEntities = col,
                    Relationship = new Relationship(RelationshipName)
                };
                AssociateResponse response = (AssociateResponse)service.Execute(request);
            }
        }
    }
}
