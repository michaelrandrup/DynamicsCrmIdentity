using Microsoft.Crm.Sdk.Messages;
using System;

namespace DynamicsCrm.WebsiteIntegration.Core
{
    public static class XrmMarketing
    {
        public static void AddToMarketingList(Guid[] listMembers, Guid listId)
        {
            AddListMembersListRequest request = new AddListMembersListRequest();
            request.MemberIds = listMembers;
            request.ListId = listId;
            AddListMembersListResponse response = XrmCore.Execute<AddListMembersListRequest, AddListMembersListResponse>(request);
        }

        public static void RemoveFromMarketingList(Guid listMember, Guid listId)
        {

            RemoveMemberListRequest request = new RemoveMemberListRequest();
            request.EntityId = listMember;
            request.ListId = listId;
            RemoveMemberListResponse response = XrmCore.Execute<RemoveMemberListRequest, RemoveMemberListResponse>(request);
        }
    }
}
