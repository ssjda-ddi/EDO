using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using EDO.Core.ViewModel;
using System.ComponentModel;
using EDO.Core.Model;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using EDO.Core.Util;

namespace EDO.StudyCategory.MemberForm
{
    public class OrganizationVM :BaseVM, IEditableObject
    {
        public static OrganizationVM Find(Collection<OrganizationVM> items, string organizationId)
        {
            foreach (OrganizationVM item in items)
            {
                if (item.Organization.Id == organizationId)
                {
                    return item;
                }
            }
            return null;
        }

        public static OrganizationVM FindByName(Collection<OrganizationVM> items, string organizationName)
        {
            foreach (OrganizationVM item in items)
            {
                if (item.OrganizationName != null && item.OrganizationName == organizationName)
                {
                    return item;
                }
            }
            return null;
        }

        public static int GetMaxNo(Collection<OrganizationVM> items)
        {
            int maxNo = 0;
            foreach (OrganizationVM organization in items)
            {
                if (organization.No > maxNo)
                {
                    maxNo = organization.No;
                }
            }
            return maxNo;
        }

        public static OrganizationVM FindById(ObservableCollection<OrganizationVM> organizations, string organizationId)
        {
            if (organizationId == null)
            {
                return null;
            }
            foreach (OrganizationVM organization in organizations)
            {
                if (organization.Id == organizationId)
                {
                    return organization;
                }
            }
            return null;
        }

        public OrganizationVM() :this(new Organization())
        {
        }

        public OrganizationVM(string organizationName) :this(new Organization(organizationName))
        {
        }

        public OrganizationVM(Organization organization)
        {
            this.organization = organization;
            this.bakOrganization = null;
        }

        #region Field Property

        private Organization organization;

        private Organization bakOrganization = null;

        public Organization Organization
        {
            get
            {
                return organization;
            }
        }

        public override object Model
        {
            get
            {
                return organization;
            }
        }

        private int no;
        public int No { 
            get
            {
                return no;
            }
            set
            {
                if (no != value) {
                    no = value;
                    NotifyPropertyChanged("No");
                }
            }
        }

        public string Id
        {
            get
            {
                return organization.Id;
            }
        }

        public string OrganizationName {
            get
            {
                return organization.OrganizationName;
            }
            set
            {
                if (organization.OrganizationName != value)
                {
                    organization.OrganizationName = value;
                    NotifyPropertyChanged("OrganizationName");
                }
            }
        }

        #endregion

        #region IEditableObject Member

        private bool inEdit = false;
        public void BeginEdit()
        {
            if (inEdit)
            {
                return;
            }
            inEdit = true;
            bakOrganization = organization.Clone() as Organization;
        }

        public void CancelEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            this.OrganizationName = bakOrganization.OrganizationName;
            bakOrganization = null;
        }

        public Action<IEditableObject> EndEditAction { get; set; }

        public void EndEdit()
        {
            if (!inEdit)
            {
                return;
            }
            inEdit = false;
            bakOrganization = null;
            //orgOrganization.OrganizationName = bakOrganization.OrganizationName;
            //bakOrganization = null;
            //curOrganization = orgOrganization;
            if (this.EndEditAction != null)
            {
                this.EndEditAction(this);
            } 
            Memorize();
        }

        #endregion

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            //builder.Append("OrganizationVM No=").Append(No);
            //builder.Append(" IsEditing=").Append(IsEditing);
            //builder.Append(" curOrganization=").Append(curOrganization.ToDebugString());
            //builder.Append(" orgOrganization=").Append(orgOrganization.ToDebugString());
            //builder.Append(" bakOrganization=").Append(orgOrganization.ToDebugString());
            return builder.ToString();
        }

        protected override void PrepareValidation()
        {
            if (string.IsNullOrEmpty(OrganizationName))
            {
                OrganizationName = EMPTY_VALUE;
            }
        }
    }
}
