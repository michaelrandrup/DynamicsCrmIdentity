using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class XrmCore
    {
        #region Public properties

        private static string _WebUserCounterName = "WebUser";

        public static string WebUserCounterName
        {
            get
            {
                return _WebUserCounterName;
            }

            set
            {
                _WebUserCounterName = value;
            }
        }

        private static string _CounterEntityName = "appl_counter";


        #endregion

        public static void HashAllPasswords(Func<string, string> PasswordHasher)
        {
            OrganizationService srv = new OrganizationService(XrmConnection.Connection);
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
            {
                IQueryable<Entity> query = from member in service.CreateQuery("appl_webuser")
                                           select member;


                foreach (Entity e in query.ToList())
                {
                    string password = e.GetAttributeValue<string>("appl_passwordhash");
                    if (!string.IsNullOrEmpty(password) && password.Length < 15)
                    {
                        string hash = PasswordHasher(password);
                        e["appl_passwordhash"] = hash;
                        // e.EntityState = EntityState.Changed;
                        service.UpdateObject(e);
                    }
                }
                service.SaveChanges();
            }


        }
        public static Entity Retrieve(string EntityName, Guid Id, ColumnSet Columns = null, CrmConnection connection = null, bool CacheResults = true)
        {
            Columns = Columns ?? new ColumnSet(true);

            if (CacheResults)
            {
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
                {
                    return service.Retrieve(EntityName, Id, Columns);
                }
            }
            else
            {
                OrganizationService srv = new OrganizationService(connection ?? XrmConnection.Connection);
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
                {
                    return service.Retrieve(EntityName, Id, Columns);
                }
            }


        }

        public static EntityMetadata RetrieveMetadata(string entityName, Microsoft.Xrm.Sdk.Metadata.EntityFilters filter = EntityFilters.All, CrmConnection connection = null)
        {
            RetrieveEntityRequest request = new RetrieveEntityRequest()
            {
                EntityFilters = filter,
                LogicalName = entityName,
                RetrieveAsIfPublished = false
            };

            RetrieveEntityResponse response = Execute<RetrieveEntityRequest, RetrieveEntityResponse>(request, connection);
            return response.EntityMetadata;
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


        public static EntityCollection RetrieveByAttribute(string EntityName, string AttributeName, string AttributeValue, ColumnSet columns = null, CrmConnection connection = null, bool CacheResults = true)
        {
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression(AttributeName, ConditionOperator.Equal, AttributeValue));
            return RetrieveByFilter(EntityName, filter, columns, connection, CacheResults);
        }

        public static EntityCollection RetrieveByFilter(string EntityName, FilterExpression Filter, ColumnSet columns = null, CrmConnection connection = null, bool CacheResults = true)
        {
            QueryExpression query = new QueryExpression(EntityName);
            query.ColumnSet = columns ?? new ColumnSet(true);
            query.Criteria = Filter;

            if (CacheResults)
            {
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(connection ?? XrmConnection.Connection))
                {
                    query.PageInfo = new PagingInfo() { PageNumber = 1, PagingCookie = null, Count = 5000 };
                    EntityCollection ResultCollection = new EntityCollection();
                    EntityCollection col = null;
                    while (true)
                    {
                        try
                        {
                            col = service.RetrieveMultiple(query);
                        }
                        catch (System.ServiceModel.Security.MessageSecurityException ex1)
                        {
                            throw ex1;
                        }
                        catch (FaultException ex)
                        {

                            throw ex;
                        }

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




        public static bool BulkSave(EntityCollection entities, CrmConnection connection = null)
        {
            OrganizationService srv = new OrganizationService(connection ?? XrmConnection.Connection);
            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
            {
                foreach (Entity e in entities.Entities)
                {
                    if (e.Id.Equals(Guid.Empty))
                    {
                        e.Id = Guid.NewGuid();
                        // service.Attach(e);
                        service.AddObject(e);
                    }
                    else
                    {
                        service.Attach(e);
                        service.UpdateObject(e);
                    }
                }
                SaveChangesResultCollection result = service.SaveChanges(Microsoft.Xrm.Sdk.Client.SaveChangesOptions.None);
                return !result.HasError;
            }
        }
        public static Guid ApplyWorkFlow(Entity entity, string workflowName, InputArgumentCollection args = null, CrmConnection connection = null)
        {
            EntityCollection col = RetrieveByAttribute("workflow", "name", workflowName);
            if (col.Entities.Count != 1)
            {
                throw new ArgumentException(string.Format("{0} workflows with the name {1} exist. Expected 1.", col.Entities.Count, workflowName), "workflowName");
            }
            return ApplyWorkFlow(entity, col.Entities.First().Id, args, connection);
        }

        public static Guid ApplyWorkFlow(Entity entity, Guid workflowId, InputArgumentCollection args = null, CrmConnection connection = null)
        {
            ExecuteWorkflowRequest request = new ExecuteWorkflowRequest()
            {
                EntityId = entity.Id,
                WorkflowId = workflowId,
                InputArguments = args
            };
            ExecuteWorkflowResponse response = Execute<ExecuteWorkflowRequest, ExecuteWorkflowResponse>(request, connection);
            return response.Id;
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

        public static void SetAttributeField(Entity entity, EntityMetadata metadata, AttributeMetadata att, string attributeName, string attributeValue)
        {
            Guid g;
            switch (att.AttributeType.Value)
            {
                case AttributeTypeCode.Boolean:
                    entity[attributeName] = Convert.ToBoolean(attributeValue);
                    break;

                case AttributeTypeCode.DateTime:
                    DateTime dt;
                    if (DateTime.TryParse(attributeValue, out dt))
                    {
                        entity[attributeName] = dt;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                    decimal dec;
                    if (decimal.TryParse(attributeValue, out dec))
                    {
                        if (att.AttributeType.Value == AttributeTypeCode.Money)
                        {
                            entity[attributeName] = new Money(dec);
                        }
                        else
                        {
                            entity[attributeName] = dec;
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Double:
                    double dou;
                    if (double.TryParse(attributeValue, out dou))
                    {
                        entity[attributeName] = dou;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Integer:
                    int i;
                    if (int.TryParse(attributeValue, out i))
                    {
                        entity[attributeName] = i;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.BigInt:
                    long l;
                    if (long.TryParse(attributeValue, out l))
                    {
                        entity[attributeName] = l;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    //Guid g;
                    //string[] v = attributeValue.Split(',');
                    //if (Guid.TryParse(v[0], out g))
                    //{
                    //    entity[attributeName] = new EntityReference(v[1], g));
                    //}
                    //else
                    //{
                    //    throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    //}
                    OneToManyRelationshipMetadata mto1 = metadata.ManyToOneRelationships.FirstOrDefault(x => x.ReferencedAttribute.Equals(attributeName, StringComparison.OrdinalIgnoreCase));
                    if (mto1 == null)
                    {
                        throw new InvalidOperationException(string.Format("The Many-To-One relationship for the attribute '{0}' does not exist", attributeName));
                    }
                    Entity lookup = null;
                    Guid gLookup;
                    EntityMetadata mto1Meta = XrmCore.RetrieveMetadata(mto1.ReferencedEntity, EntityFilters.Entity);
                    if (Guid.TryParse(attributeValue, out gLookup))
                    {
                        lookup = XrmCore.Retrieve(mto1.ReferencingEntity, gLookup, new ColumnSet(mto1Meta.PrimaryIdAttribute));
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(attributeValue))
                        {
                            lookup = XrmCore.RetrieveByAttribute(mto1.ReferencedEntity, mto1Meta.PrimaryNameAttribute, attributeValue, new ColumnSet(mto1Meta.PrimaryIdAttribute)).Entities.FirstOrDefault();
                        }

                    }
                    if (!string.IsNullOrEmpty(attributeValue) && lookup == null && att.AttributeType.Value == AttributeTypeCode.Lookup)
                    {
                        lookup = new Entity(mto1Meta.LogicalName, Guid.NewGuid());
                        lookup[mto1Meta.PrimaryNameAttribute] = attributeValue;
                        lookup.Id = XrmCore.CreateEntity(lookup);
                    }
                    if (lookup != null)
                    {
                        entity[attributeName] = lookup.ToEntityReference();
                    }
                    break;

                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    {
                        entity[attributeName] = attributeValue;
                    }
                    break;

                case AttributeTypeCode.Owner:
                    if (Guid.TryParse(attributeValue, out g))
                    {
                        entity[attributeName] = new EntityReference("systemuser", g);
                    }
                    else
                    {
                        Entity owner = XrmCore.RetrieveByAttribute("systemuser", "fullname", attributeValue, new ColumnSet("systemuserid")).Entities.FirstOrDefault();
                        if (owner == null)
                        {
                            throw new InvalidOperationException(string.Format("The system user '{0}' does not exist", attributeValue));
                        }
                        entity[attributeName] = owner.ToEntityReference();
                    }
                    break;

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    int iOption;
                    if (int.TryParse(attributeValue, out iOption))
                    {
                        entity[attributeName] = new OptionSetValue(iOption);
                    }
                    else
                    {
                        OptionMetadata opMeta = null;
                        if (att.AttributeType.Value == AttributeTypeCode.Picklist)
                        {
                            opMeta = (att as PicklistAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase));
                        }
                        else if (att.AttributeType.Value == AttributeTypeCode.State)
                        {
                            opMeta = (att as StateAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase));
                        }
                        else if (att.AttributeType.Value == AttributeTypeCode.Status)
                        {
                            opMeta = (att as StatusAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(attributeValue, StringComparison.OrdinalIgnoreCase));
                        }
                        if (opMeta == null)
                        {
                            throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                        }
                        else
                        {
                            entity[attributeName] = opMeta.Value.Value;
                        }
                    }
                    break;

                case AttributeTypeCode.Uniqueidentifier:
                    if (Guid.TryParse(attributeValue, out g))
                    {
                        entity[attributeName] = g;
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", attributeName, attributeValue, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.PartyList:
                case AttributeTypeCode.CalendarRules:
                case AttributeTypeCode.Virtual:
                case AttributeTypeCode.ManagedProperty:
                case AttributeTypeCode.EntityName:
                    throw new NotImplementedException(string.Format("The attribute type {0} is not supported", att.AttributeType.Value.ToString()));

                default:
                    break;
            }
        }


        public static int GetNextId(string name)
        {
            EntityCollection col = RetrieveByAttribute("appl_counter", "appl_name", WebUserCounterName, CacheResults: false);
            if (col.Entities.Count == 0)
            {
                Entity e = new Entity("appl_counter", Guid.NewGuid());
                e.SetAttributeValue<string>("appl_name", "appl_counter");
                e.SetAttributeValue<int>("appl_current", 1);
                CreateEntity(e);
                return 1;
            }
            else if (col.Entities.Count == 1)
            {
                Entity e = col.Entities.First();
                int i = e.GetAttributeValue<int>("appl_current");
                i = i + 1;
                e.SetAttributeValue<int>("appl_current", i);
                UpdateEntity(e);
                return i;
            }
            else
            {
                throw new InvalidOperationException(string.Format("The counter {0} more than one records", WebUserCounterName));
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
                                           where login.GetAttributeValue<string>("appl_loginprovider") == LoginProvider && login.GetAttributeValue<string>("appl_providerkey") == ProviderKey
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
