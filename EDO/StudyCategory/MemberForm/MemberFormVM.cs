using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using EDO.Core.ViewModel;
using EDO.Main;
using EDO.Core.Model;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Input;
using EDO.Core.Util;
using System.Windows;
using EDO.Properties;
using EDO.Core.View;

namespace EDO.StudyCategory.MemberForm
{
    public class MemberFormVM :FormVM
    {
        private static void InitRole(MemberVM member, bool isLeader)
        {
            //if (isLeader)
            //{
            //    member.Roles = Options.FirstRoles;
            //}
            //else
            //{
            //    member.Roles = Options.OtherRoles;
            //}
            member.Roles = Options.Roles;
        }

        private static void InitRoleName(MemberVM member, bool isLeader)
        {
            //if (isLeader)
            //{
            //    member.RoleCode = Options.ROLE_DAIHYOSHA_CODE;
            //}
            //else
            //{
            //    member.RoleCode = Options.ROLE_OTHER_CODE;
            //}
            member.RoleName = Options.FindLabel(Options.Roles, Options.ROLE_OTHER_CODE);
        }

        public MemberFormVM(StudyUnitVM studyUnit) :base(studyUnit)
        {
            members = new ObservableCollection<MemberVM>();
            organizations = new ObservableCollection<OrganizationVM>();

            //Create the list of OrganizationVM(showed at the bottom of the window and used by Organization of Members combo)
            int i = 1;
            foreach (Organization organizationModel in studyUnit.OrganizationModels)
            {
                OrganizationVM organization = new OrganizationVM(organizationModel);
                InitExistOrganization(organization, i++);
                organizations.Add(organization);
            }

            //Create MemberVM list
            i = 1;
            foreach (Member memberModel in studyUnit.MemberModels)
            {
                OrganizationVM organization = OrganizationVM.Find(organizations, memberModel.OrganizationId);
                MemberVM member = new MemberVM(memberModel, organization.OrganizationName);
                InitExistMember(member, i++);
                members.Add(member);
            }
            memberSyncher = new ModelSyncher<MemberVM, Member>(this, members, studyUnit.MemberModels);
            organizationSyncher = new ModelSyncher<OrganizationVM, Organization>(this, organizations, studyUnit.OrganizationModels);
        }

        #region Field Property

        private ModelSyncher<MemberVM, Member> memberSyncher;
        private ModelSyncher<OrganizationVM, Organization> organizationSyncher;

        private ObservableCollection<MemberVM> members;
        public ObservableCollection<MemberVM> Members { get { return members; } }

        private ObservableCollection<OrganizationVM> organizations;
        public ObservableCollection<OrganizationVM> Organizations { get { return organizations; } }

        private object selectedMemberItem;
        public object SelectedMemberItem {
            get
            {
                return selectedMemberItem;
            }
            set
            {
                if (selectedMemberItem != value)
                {
                    selectedMemberItem = value;
                    NotifyPropertyChanged("SelectedMemberItem");
                }
            }
        }

        private object selectedOrganizationItem;
        public object SelectedOrganizationItem
        {
            get
            {
                return selectedOrganizationItem;
            }
            set
            {
                if (selectedOrganizationItem != value)
                {
                    selectedOrganizationItem = value;
                    NotifyPropertyChanged("SelectedOrganizationItem");
                }
            }
        }

        public MemberVM SelectedMember
        {
            get
            {
                return SelectedMemberItem as MemberVM;
            }
        }

        public OrganizationVM SelectedOrganization
        {
            get
            {
                return SelectedOrganizationItem as OrganizationVM;
            }
        }

        private int NextMemberNo
        {
            get
            {
                return MemberVM.GetMaxNo(members) + 1;
            }
        }

        private int NextOrganizationNo
        {
            get
            {
                return OrganizationVM.GetMaxNo(organizations) + 1;
            }
        }

        #endregion

        #region Process about Organization

        public void organization_ItemEndEdit(IEditableObject x)
        {
            OrganizationVM organization = (OrganizationVM)x;
            List<MemberVM> relatedMembers = MemberVM.FindByOrganizationId(Members, organization.Id);
            foreach (MemberVM member in relatedMembers)
            {
                member.OrganizationName = organization.OrganizationName;
            }
        }

        private void InitOrganization(OrganizationVM organization, int no)
        {
            organization.Parent = this;
            organization.No = no;
            organization.EndEditAction = new Action<IEditableObject>(organization_ItemEndEdit);
        }

        private void InitExistOrganization(OrganizationVM organization, int no)
        {
            InitOrganization(organization, no);
        }

        private void InitNewOrganization(OrganizationVM organization)
        {
            InitOrganization(organization, NextOrganizationNo);
        }

        private OrganizationVM FindOrganizationByName(string name)
        {
            return OrganizationVM.FindByName(organizations, name);
        }

        #endregion

        #region Process about Member

        private void InitMember(MemberVM member, bool isLeader, int no)
        {
            InitRole(member, isLeader);
            member.Parent = this;
            member.No = no;
            member.EndEditAction = new Action<IEditableObject>(member_ItemEndEdit);
        }

        private void InitExistMember(MemberVM member, int no)
        {
            InitMember(member, member.IsLeader, no);
        }

        public override void InitRow(object newItem)
        {
            if (newItem is MemberVM)
            {
                InitNewMember((MemberVM)newItem);
            }
            else if (newItem is OrganizationVM)
            {
                InitNewOrganization((OrganizationVM)newItem);
            }
        }

        public void InitNewMember(MemberVM member)
        {
            InitMember(member, members.Count == 1, NextMemberNo);
        }

        public void member_ItemEndEdit(IEditableObject x)
        {
            ///// Process after ending of editing MemberVM
            MemberVM editedMember = (MemberVM)x;
            if (string.IsNullOrEmpty(editedMember.OrganizationName))
            {
                return;
            }
            OrganizationVM existOrganization = FindOrganizationByName(editedMember.OrganizationName);
            if (existOrganization == null)
            {
                ////Create and add to VM if there is no match to existing organization.
                existOrganization = new OrganizationVM(editedMember.OrganizationName);
                InitNewOrganization(existOrganization);
                organizations.Add(existOrganization);
            }
            editedMember.OrganizationId = existOrganization.Id;

            //The screen also displays to follow because there may be members that refer to the same organization
            //List<MemberVM> relatedMembers = this.GetSameOrganizationMemberVMs(editedMember);
            //foreach (MemberVM member in relatedMembers)
            //{
            //    member.NotifyPropertyChanged("OrganizationName");
            //}
        }

        #endregion

        #region Creeate member(called from Data Collection screen) 
        public MemberVM AppendMember(string memberId, string lastName, string firstName, string organizationName, string position)
        {
            //This three are required?
            if (string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(firstName) || string.IsNullOrEmpty(organizationName)) {
                return null;
            }

            MemberVM existMember = MemberVM.FindByNames(Members, lastName, firstName, organizationName, position);
            if (existMember != null)
            {
                return existMember;
            }

            //other than the above is regarded as a new member and newly created.
            MemberVM newMember = new MemberVM();
            members.Add(newMember);
            InitNewMember(newMember);
            newMember.LastName = lastName;
            newMember.FirstName = firstName;
            newMember.OrganizationName = organizationName;
            newMember.Position = position;
            InitRoleName(newMember, members.Count == 1);
            member_ItemEndEdit(newMember);
            return newMember;
        }

        #endregion


        #region Delete command of Member

        private ICommand removeMemberCommand;
        public ICommand RemoveMemberCommand { 
            get 
            {
                if (removeMemberCommand == null) {
                    removeMemberCommand = new RelayCommand(param => RemoveMember(), param => CanRemoveMember);
                }
                return removeMemberCommand;
            }
        }

        public bool CanRemoveMember
        {
            get
            {
                MemberVM member = SelectedMember;
                if (member == null)
                {
                    return false;
                }
                if (member.IsLeader)
                {
                    return false;
                }
                return !member.InEdit;
            }
        }

        public void RemoveMember()
        {
            members.Remove(SelectedMember);
            SelectedMemberItem = null;
        }

        #endregion


        #region Delete command of Organization
        private ICommand removeOrganizationCommand;
        public ICommand RemoveOrganizationCommand
        {
            get
            {
                if (removeOrganizationCommand == null)
                {
                    removeOrganizationCommand = new RelayCommand(param => this.RemoveOrganization(), param => this.CanRemoveOrganization);
                }
                return removeOrganizationCommand;
            }
        }

        public bool CanRemoveOrganization
        {
            get
            {
                OrganizationVM organization = SelectedOrganization;
                if (organization == null)
                {
                    return false;
                }
                return true;
            }
        }

        public void RemoveOrganization()
        {
            List<MemberVM> usedMembers = MemberVM.FindByOrganizationName(members, SelectedOrganization.OrganizationName);
            if (usedMembers.Count > 0)
            {
                string msg = EDOUtils.CannotDeleteError(Resources.Member, usedMembers, param => param.FullName);
                MessageBox.Show(msg);
                return;
            }
            organizations.Remove(SelectedOrganization);
            SelectedOrganizationItem = null;
        }

        public OrganizationVM AppendOrganization(string memberId, string organizationName)
        {
            if ( string.IsNullOrEmpty(organizationName))
            {
                return null;
            }

            OrganizationVM existOrganization = OrganizationVM.FindByName(organizations, organizationName);
            if (existOrganization != null)
            {
                return existOrganization;
            }

            OrganizationVM organization = new OrganizationVM();
            organizations.Add(organization);
            organization.OrganizationName = organizationName;
            return organization;
        }
        #endregion 


        protected override Action GetCompleteAction(VMState state)
        {
            return () => { StudyUnit.CompleteMember(); };
        }
    }
}
