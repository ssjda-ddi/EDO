using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Util;

namespace EDO.Core.Model
{
    public class Member :ICloneable, IIDPropertiesProvider
    {
        public static void ChangeOrganizationId(List<Member> members, string oldId, string newId)
        {
            //Change the referred member ID since the one of Organization is changed
            foreach (Member member in members)
            {
                if (member.OrganizationId == oldId)
                {
                    member.OrganizationId = newId;
                }
            }
        }

        public static Member FindByName(List<Member> members, string lastName, string firstName)
        {
            foreach (Member member in members)
            {
                if (member.LastName == lastName && member.FirstName == firstName)
                {
                    return member;
                }
            }
            return null;
        }

        public static Member Find(List<Member> members, string memberId)
        {
            foreach (Member member in members)
            {
                if (member.Id == memberId)
                {
                    return member;
                }
            }
            return null;
        }

        public static string FindRoleName(string roleCode)
        {
            return Option.FindLabel(Options.Roles, roleCode);
        }

        public static string FindRoleNameDaihyoSha()
        {
            return Option.FindLabel(Options.Roles, Options.ROLE_DAIHYOSHA_CODE);
        }

        public string[] IdProperties
        {
            get
            {
                return new string[] { "Id" };
            }
        }

        public Member()
        {
            Id = IDUtils.NewGuid();
        }
        public string Id { get; set; }
        private string roleCode;
        public string RoleCode
        {
            get
            {
                return roleCode;
            }
            set
            {
                roleCode = value;
            }
        } // for backward compatibility
        public string RoleName { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Position {get; set; }
        public string OrganizationId { get; set; }

        public override string ToString()
        {
            return string.Format("Id={0}, RoleCode={1} LastName={2} FirstName={3} Position={4} OrganizationId={5}",
                Id,
                RoleCode, LastName, FirstName, Position, OrganizationId);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public bool IsLeader
        {
            get
            {
                return RoleName == FindRoleNameDaihyoSha();

            }
        }

        internal void ConvertRoleCodeToRoleName()
        {
            if (string.IsNullOrEmpty(RoleCode))
            {
                return;
            }
            if (string.IsNullOrEmpty(RoleName))
            {
                //Convert RoleCode to RoleName(since 1.6.0)
                RoleName = FindRoleName(RoleCode);
            }
            RoleCode = null;
        }
    }
}
