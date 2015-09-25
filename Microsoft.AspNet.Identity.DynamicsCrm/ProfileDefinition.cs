using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Identity.DynamicsCrm
{
    public enum ProfileFieldDataTypes : int
    {
        Text = 771780000,
        WholeNumber = 771780001,
        DecimalNumber = 771780002,
        DateTime = 771780003,
        Boolean = 771780004,
        OptionListSingleSelect = 771780005,
        OptionListMultiSelect = 771780006
    }

    public enum ProfileFieldRequirementLevels : int
    {
        None = 771780000,
        Recommended = 771780001,
        Required = 771780002
    }

    public class ProfileField
    {
        public Guid Id { get; set; }
        public ProfileFieldDataTypes DataType { get; set; }
        public ProfileFieldRequirementLevels RequirementLevel { get; set; }
        public string Name { get; set; }
        public string Options { get; set; }
        internal static ProfileField Factory(Entity e)
        {
            ProfileField field = new ProfileField();
            field.Id = e.GetAttributeValue<Guid>("appl_profilefieldid");
            field.Name = e.GetAttributeValue<string>("appl_name");
            field.Options = e.GetAttributeValue<string>("appl_options");
            field.RequirementLevel = (ProfileFieldRequirementLevels)e.GetAttributeValue<OptionSetValue>("appl_requirementlevel").Value;
            field.DataType = (ProfileFieldDataTypes)e.GetAttributeValue<OptionSetValue>("appl_datatype").Value;
            return field;
        }

    }

    public class Profile
    {
        public Profile()
        {
            
        }

        public Guid Id { get; set; }
        public List<ProfileField> Fields = new List<ProfileField>();

        internal static Profile Factory(EntityCollection Fields, Guid ProfileId)
        {
            Profile profile = new Profile() { Id = ProfileId };
            foreach (Entity e in Fields.Entities)
            {
                profile.Fields.Add(ProfileField.Factory(e));
            }
            return profile;

        }

       
    }

    public class UserProfile
    {
        public Guid Id { get; set; }

    }

    public class UserProfileField
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid FieldId { get; set; }
        public Guid ContactId {get; set;}
        public string Value { get; set; }
        public bool Boolean { get; set; }
        public DateTime Date { get; set; }
        public decimal Decimal { get; set; }
        public int Number { get; set; }

        internal static UserProfileField Factory(Entity e)
        {
            UserProfileField f = new UserProfileField();
            f.Id = e.GetAttributeValue<Guid>("appl_membershipprofileid");
            f.ProfileId = e.GetAttributeValue<Guid>("appl_profiledefinitionid");
            f.FieldId = e.GetAttributeValue<Guid>("appl_profilefieldid");
            f.ContactId = e.GetAttributeValue<Guid>("appl_contactid");
            f.Value = e.GetAttributeValue<string>("appl_value");

            // TODO: Fill out the rest of the fields
            return f;



        }

    }
}
