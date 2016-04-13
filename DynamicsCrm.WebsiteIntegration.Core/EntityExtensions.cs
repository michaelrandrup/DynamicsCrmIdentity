using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xrm.Sdk.Query;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class EntityExtensions
    {
        public static void SetAttributeMetaValue(this Entity entity, KeyValuePair<string, string> kv, EntityMetadata metadata = null)
        {
            if (metadata == null)
            {
                entity[kv.Key] = kv.Value;
                return;
            }

            AttributeMetadata att = metadata.Attributes.FirstOrDefault(x => x.LogicalName.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));
            if (att == null)
            {
                throw new InvalidOperationException(string.Format("Unknown attibute for {2}: '{0} with the value of {1}", kv.Key, kv.Value, entity.LogicalName));
            }
            else if (!att.AttributeType.HasValue)
            {
                entity[kv.Key] = kv.Value;
                return;
            }

            switch (att.AttributeType.Value)
            {
                case AttributeTypeCode.Boolean:
                    entity.SetAttributeValue(kv.Key, Convert.ToBoolean(kv.Value));
                    break;
                
                case AttributeTypeCode.DateTime:
                    DateTime dt;
                    if (DateTime.TryParse(kv.Value, out dt))
                    {
                        entity.SetAttributeValue(kv.Key, dt);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Decimal:
                case AttributeTypeCode.Money:
                    decimal dec;
                    if (decimal.TryParse(kv.Value, out dec))
                    {
                        if (att.AttributeType.Value == AttributeTypeCode.Money)
                        {
                            entity.SetAttributeValue(kv.Key, new Money(dec));
                        }
                        else
                        {
                            entity.SetAttributeValue(kv.Key, dec);
                        }
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Double:
                    double dou;
                    if (double.TryParse(kv.Value, out dou))
                    {
                        entity.SetAttributeValue(kv.Key, dou);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Integer:
                    int i;
                    if (int.TryParse(kv.Value, out i))
                    {
                        entity.SetAttributeValue(kv.Key, i);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.BigInt:
                    long l;
                    if (long.TryParse(kv.Value, out l))
                    {
                        entity.SetAttributeValue(kv.Key, l);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                    }
                    break;

                case AttributeTypeCode.Lookup:
                case AttributeTypeCode.Customer:
                    OneToManyRelationshipMetadata mto1 = metadata.ManyToOneRelationships.FirstOrDefault(x => x.ReferencedAttribute.Equals(kv.Key, StringComparison.OrdinalIgnoreCase));
                    if (mto1 == null)
                    {
                        throw new InvalidOperationException(string.Format("The Many-To-One relationship for the attribute '{0}' does not exist", kv.Key));
                    }
                    Entity lookup = null;
                    Guid gLookup;
                    EntityMetadata mto1Meta = XrmCore.RetrieveMetadata(mto1.ReferencingEntity, EntityFilters.Entity);
                    if (Guid.TryParse(kv.Value, out gLookup))
                    {
                        lookup = XrmCore.Retrieve(mto1.ReferencingEntity, gLookup, new ColumnSet(mto1Meta.PrimaryIdAttribute));
                    }
                    else
                    {
                        lookup = XrmCore.RetrieveByAttribute(mto1.ReferencingEntity, mto1Meta.PrimaryNameAttribute, kv.Value, new ColumnSet(mto1Meta.PrimaryIdAttribute)).Entities.FirstOrDefault();
                    }
                    if (lookup == null && att.AttributeType.Value == AttributeTypeCode.Lookup)
                    {
                        lookup = new Entity(mto1Meta.LogicalName, Guid.NewGuid());
                        lookup[mto1Meta.PrimaryNameAttribute] = kv.Value;
                        lookup.Id = XrmCore.CreateEntity(lookup);
                    }
                    entity.SetAttributeValue(kv.Key, lookup.ToEntityReference());
                    break;

                case AttributeTypeCode.Memo:
                case AttributeTypeCode.String:
                    entity.SetAttributeValue(kv.Key, kv.Value);
                    break;

                case AttributeTypeCode.Owner:
                    Guid g;
                    if (Guid.TryParse(kv.Value, out g))
                    {
                        entity.SetAttributeValue(kv.Key, new EntityReference("systemuser", g));
                    }
                    else
                    {
                        Entity owner = XrmCore.RetrieveByAttribute("systemuser", "fullname", kv.Value, new ColumnSet("systemuserid")).Entities.FirstOrDefault();
                        if (owner == null)
                        {
                            throw new InvalidOperationException(string.Format("The system user '{0}' does not exist", kv.Value));
                        }
                        entity.SetAttributeValue(kv.Key, owner.ToEntityReference());
                    }
                    break;

                case AttributeTypeCode.Picklist:
                case AttributeTypeCode.State:
                case AttributeTypeCode.Status:
                    int iOption;
                    if (int.TryParse(kv.Value, out iOption))
                    {
                        entity.SetAttributeValue(kv.Key, new OptionSetValue(iOption));
                    }
                    else
                    {
                        OptionMetadata opMeta = null;
                        if (att.AttributeType.Value == AttributeTypeCode.Picklist)
                        {
                            opMeta = (att as PicklistAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase));
                        }
                        else if (att.AttributeType.Value == AttributeTypeCode.State)
                        {
                            opMeta = (att as StateAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase));
                        }
                        else if (att.AttributeType.Value == AttributeTypeCode.Status)
                        {
                            opMeta = (att as StatusAttributeMetadata).OptionSet.Options.FirstOrDefault(x => x.Label.LocalizedLabels.Any(lab => lab.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase)) || x.Label.UserLocalizedLabel.Label.Equals(kv.Value, StringComparison.OrdinalIgnoreCase));
                        }
                        if (opMeta == null)
                        {
                            throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
                        }
                        else
                        {
                            entity.SetAttributeValue(kv.Key, opMeta.Value.Value);
                        }
                    }
                    break;

                case AttributeTypeCode.Uniqueidentifier:
                    if (Guid.TryParse(kv.Value, out g))
                    {
                        entity.SetAttributeValue(kv.Key, g);
                    }
                    else
                    {
                        throw new InvalidOperationException(string.Format("The {2} attribute '{0}' contains an invalid value of {1}", kv.Key, kv.Value, att.AttributeType.Value.ToString()));
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
    }
}
