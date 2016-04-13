using Microsoft.Xrm.Client;
using Microsoft.Xrm.Client.Services;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Linq;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class XrmProfile
    {
        private static EntityCollection GetUserProfiles(Guid ContactId, Guid? ProfileId)
        {
            if (ProfileId.HasValue)
            {
                OrganizationService srv = new OrganizationService(XrmConnection.Connection);
                using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(srv))
                {
                    IQueryable<Entity> query = from entity in service.CreateQuery("appl_membershipprofile")
                                               where (Guid)entity["appl_contactid"] == ContactId && (Guid)entity["appl_profiledefinitionid"] == ProfileId.Value
                                               select entity;

                    EntityCollection col = new EntityCollection(query.ToList());
                    col.EntityName = "appl_membershipprofile";
                    return col;
                }
            }
            else
            {
                return XrmCore.GetRelated(new Entity("contact", ContactId), "appl_membershipprofile", "appl_contactid", CacheResults: false);
            }
        }
        public static UserProfile GetUserProfile(Guid ContactId, Profile ProfileDefinition)
        {
            return UserProfile.Factory(ContactId, GetUserProfiles(ContactId, ProfileDefinition.Id), ProfileDefinition);
        }

        public static UserProfile GetUserProfile(Guid ContactId, string ProfileDefinition)
        {
            Profile profile = GetProfile(ProfileDefinition);
            return UserProfile.Factory(ContactId, GetUserProfiles(ContactId, profile.Id), profile);
        }

        public static UserProfile GetUserProfile(Profile ProfileDefinition)
        {
            return UserProfile.Factory(ProfileDefinition);
        }

        public static bool UpdateUserProfile(UserProfile Profiles)
        {
            EntityCollection collection = new EntityCollection();
            collection.EntityName = "appl_membershipprofile";
            Profiles.Fields.ForEach(profile => collection.Entities.Add(profile.ToEntity()));
            return XrmCore.BulkSave(collection);
        }

        public static Profile GetProfile(string ProfileName)
        {
            FilterExpression filter = new FilterExpression(LogicalOperator.And);
            filter.AddCondition(new ConditionExpression("appl_name", ConditionOperator.Equal, ProfileName));
            EntityCollection col = XrmCore.RetrieveByFilter("appl_profiledefinition", filter);
            if (col.Entities.Count != 1)
            {
                throw new Exception(string.Format("There are {0} profiles with the name {1}. 1 was expected", col.Entities.Count, ProfileName));
            }

            Guid ProfileDefinitionId = col.Entities.First().GetAttributeValue<Guid>("appl_profiledefinitionid");

            using (CrmOrganizationServiceContext service = new CrmOrganizationServiceContext(XrmConnection.Connection))
            {
                IQueryable<Entity> query = from field in service.CreateQuery("appl_profilefield")
                                           join related in service.CreateQuery("appl_profilefield_appl_profiledefinitio") on field["appl_profilefieldid"] equals related["appl_profilefieldid"]
                                           join profiledef in service.CreateQuery("appl_profiledefinition") on related["appl_profiledefinitionid"] equals profiledef["appl_profiledefinitionid"]
                                           where (string)profiledef["appl_name"] == ProfileName
                                           select field;

                return Profile.Factory(new EntityCollection(query.ToList()), ProfileDefinitionId);
            }
        }
    }
}
