using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    public enum ProfileFieldBooleanValues : int
    {
        False = 771780000,
        True = 771780001,
        NotDefined = 771780002
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

        public ProfileField Field(string name)
        {
            ProfileField field = Fields.FirstOrDefault(x => GetPropName(x.Name).Equals(GetPropName(name), StringComparison.OrdinalIgnoreCase)); 
            if (field == null)
            {
                field = Fields.FirstOrDefault(x => x.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            }
            if (field != null)
            {
                return field;
            }
            throw new ArgumentException(string.Format("A field with the name {0} was not found in the profile", name), "name");
        }

        private string GetPropName(string name)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9]");
            return rgx.Replace(name, "");

        }

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
        public UserProfile()
        {

        }

        public void Update(NameValueCollection collection)
        {
            foreach (string key in collection.AllKeys)
            {
                UserProfileField field = Fields.FirstOrDefault(x => GetPropName(x.Name).Equals(key, StringComparison.OrdinalIgnoreCase));
                if (field != null)
                {
                    field.Value = collection[key];
                }
            }
        }

        private string GetPropName(string name)
        {
            Regex rgx = new Regex("[^a-zA-Z0-9 _]");
            return rgx.Replace(name, "").Replace(" ", "").ToLower();
        }


        private Guid _Id;
        public Guid Id
        {
            get { return _Id; }
            set
            {
                _Id = value;
                if (!_Id.Equals(Guid.Empty) && Fields.Count > 0)
                {
                    foreach (UserProfileField field in Fields)
                    {
                        field.ContactId = _Id;
                    }
                }
            }
        }

        public List<UserProfileField> Fields = new List<UserProfileField>();
        public Profile ProfileDefinition { get; set; }
        internal static UserProfile Factory(Guid ContactId, EntityCollection Profiles, Profile ProfileDefinition)
        {
            UserProfile profile = new UserProfile() { Id = ContactId, ProfileDefinition = ProfileDefinition };
            foreach (Entity e in Profiles.Entities)
            {
                profile.Fields.Add(UserProfileField.Factory(e));
            }

            foreach (ProfileField field in ProfileDefinition.Fields.Except(ProfileDefinition.Fields.Where(x => Profiles.Entities.Any(y => y.GetAttributeValue<EntityReference>("appl_profilefieldid").Id.Equals(x.Id)))))
            {
                profile.Fields.Add(UserProfileField.Factory(ContactId, ProfileDefinition.Id, field));
            }

            return profile;
        }

        internal static UserProfile Factory(Profile ProfileDefinition)
        {
            UserProfile profile = new UserProfile() { Id = Guid.Empty, ProfileDefinition = ProfileDefinition };

            foreach (ProfileField field in ProfileDefinition.Fields)
            {
                profile.Fields.Add(UserProfileField.Factory(Guid.Empty, ProfileDefinition.Id, field));
            }

            return profile;

        }


    }

    public class UserProfileField
    {
        public Guid Id { get; set; }
        public Guid ProfileId { get; set; }
        public Guid FieldId { get; set; }
        public Guid ContactId { get; set; }
        public string Value { get; set; }
        public bool? Boolean { get; set; }
        public DateTime? Date { get; set; }
        public decimal? Decimal { get; set; }
        public int? Number { get; set; }
        public string Name { get; set; }

        internal static UserProfileField Factory(Guid contactId, Guid profileId, ProfileField profileField)
        {
            UserProfileField f = new UserProfileField();
            f.Id = Guid.Empty;
            f.ProfileId = profileId;
            f.FieldId = profileField.Id;
            f.ContactId = contactId;
            f.Name = profileField.Name;
            return f;
        }

        internal Entity ToEntity()
        {
            Entity e = new Entity("appl_membershipprofile", Id);
            e["appl_profiledefinitionid"] = new EntityReference("appl_profiledefinition", ProfileId);
            e["appl_profilefieldid"] = new EntityReference("appl_profilefield", FieldId);
            e["appl_contactid"] = new EntityReference("contact", ContactId);
            e["appl_name"] = Name;
            e["appl_value"] = Value;

            DateTime dt;
            int i;
            decimal d;
            bool b;

            if (!string.IsNullOrEmpty(Value))
            {
                if (DateTime.TryParse(Value, out dt))
                {
                    e["appl_datetime"] = dt;
                }
                else if (bool.TryParse(Value, out b))
                {
                    e["appl_boolean"] = b == true ? new OptionSetValue((int)ProfileFieldBooleanValues.True) : new OptionSetValue((int)ProfileFieldBooleanValues.False);
                }
                else if (int.TryParse(Value, out i))
                {
                    e["appl_wholenumber"] = i;
                }
                else if (decimal.TryParse(Value, out d))
                {
                    e["appl_decimalnumber"] = d;
                }
            }

            //if (Date.HasValue)
            //{
            //    e["appl_datetime"] = Date.Value;
            //}
            //if (Boolean.HasValue)
            //{
            //    e["appl_boolean"] = Boolean.Value == true ? new OptionSetValue((int)ProfileFieldBooleanValues.True) : new OptionSetValue((int)ProfileFieldBooleanValues.False);
            //}
            //else
            //{
            //    e["appl_boolean"] = new OptionSetValue((int)ProfileFieldBooleanValues.NotDefined);
            //}
            //if (Number.HasValue)
            //{
            //    e["appl_wholenumber"] = Number.Value;
            //}
            //if (Decimal.HasValue)
            //{
            //    e["appl_decimalnumber"] = Decimal.Value;
            //}




            return e;





        }


        internal static UserProfileField Factory(Entity e)
        {
            UserProfileField f = new UserProfileField();
            f.Id = e.GetAttributeValue<Guid>("appl_membershipprofileid");
            f.ProfileId = e.GetAttributeValue<EntityReference>("appl_profiledefinitionid").Id;
            f.FieldId = e.GetAttributeValue<EntityReference>("appl_profilefieldid").Id;
            f.ContactId = e.GetAttributeValue<EntityReference>("appl_contactid").Id;
            f.Value = e.GetAttributeValue<string>("appl_value");
            f.Name = e.GetAttributeValue<string>("appl_name");

            // TODO: Fill out the rest of the fields
            if (e.Contains("appl_datetime"))
            {
                f.Date = e.GetAttributeValue<DateTime>("appl_datetime");
            }
            if (e.Contains("appl_boolean"))
            {
                OptionSetValue v = e.GetAttributeValue<OptionSetValue>("appl_boolean");
                switch (v.Value)
                {
                    case (int)ProfileFieldBooleanValues.False:
                        f.Boolean = false;
                        break;

                    case (int)ProfileFieldBooleanValues.True:
                        f.Boolean = true;
                        break;

                    default:
                        break;
                }
            }
            if (e.Contains("appl_decimalnumber"))
            {
                f.Decimal = e.GetAttributeValue<decimal>("appl_decimalnumber");
            }
            if (e.Contains("appl_wholenumber"))
            {
                f.Number = e.GetAttributeValue<int>("appl_wholenumber");
            }
            return f;
        }

    }
}
