using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.ViewModel;
using EDO.Core.Model;
using System.Collections.ObjectModel;
using EDO.Core.Util;
using EDO.StudyCategory.MemberForm;
using EDO.Main;
using System.Windows.Input;
using System.Windows;
using System.Diagnostics;
using EDO.Core.View;
using EDO.Properties;

namespace EDO.SamplingCategory.SamplingForm
{
    public class SamplingVM :BaseVM, IOrderedObject, IStringIDProvider
    {
        public SamplingVM() :this(new Sampling())
        {
        }

        public SamplingVM(Sampling sampling)
        {
            this.sampling = sampling;
            this.universes = new ObservableCollection<UniverseVM>();
            foreach (Universe universeModel in sampling.Universes)
            {
                UniverseVM universe = new UniverseVM(universeModel);
                universe.Parent = this;
                universes.Add(universe);
            }
            this.samplingNumbers = new ObservableCollection<SamplingNumberVM>();
            foreach (SamplingNumber samplingNumberModel in sampling.SamplingNumbers)
            {
                SamplingNumberVM samplingNumber = new SamplingNumberVM(samplingNumberModel);
                samplingNumber.Parent = this;
                samplingNumbers.Add(samplingNumber);
            }

            samplingMethods = Options.SamplingMethods;
            collectorTypes = Options.CollectorTypes;
            if (string.IsNullOrEmpty(CollectorTypeCode))
            {
                CollectorTypeCode = Sampling.DefaultCollectorType;
            }
            modelSyncher = new ModelSyncher<UniverseVM, Universe>(this, universes, sampling.Universes);
            modelSyncher2 = new ModelSyncher<SamplingNumberVM, SamplingNumber>(this, samplingNumbers, sampling.SamplingNumbers);
        }

        public void Init()
        {
            InitTitle();

            //if (IsCollectorTypeIndividual)
            //{
            //    MemberVM member = StudyUnit.FindMember(sampling.MemberId);
            //    UpdateIndividual(member);
            //}
            //else
            //{
            //    OrganizationVM organization = StudyUnit.FindOrganization(sampling.MemberId);
            //    UpdateOrganization(organization);
            //}
        }

        private Sampling sampling;
        private ObservableCollection<UniverseVM> universes;
        private ObservableCollection<SamplingNumberVM> samplingNumbers;
        private ModelSyncher<UniverseVM, Universe> modelSyncher;
        private ModelSyncher<SamplingNumberVM, SamplingNumber> modelSyncher2;
        private ObservableCollection<Option> samplingMethods;
        private ObservableCollection<Option> collectorTypes;

        public Sampling Sampling { get { return sampling; } }
        public override object Model { get { return sampling; } }
        public string Id
        {
            get { return sampling.Id; }
        }
        public string MemberId { get { return sampling.MemberId; } }
        public ObservableCollection<UniverseVM> Universes { get { return universes; } }
        public ObservableCollection<SamplingNumberVM> SamplingNumbers { get { return samplingNumbers; } }
        public ObservableCollection<Option> SamplingMethods { get { return samplingMethods; } }
        public ObservableCollection<Option> CollectorTypes { get { return collectorTypes; } }

        public void InitTitle()
        {
            if (string.IsNullOrEmpty(sampling.Title))
            {
                sampling.Title = EDOUtils.OrderTitle(this);
            }
        }

        public string Title
        {
            get
            {
                return sampling.Title;
            }
            set
            {
                if (sampling.Title != value)
                {
                    sampling.Title = value;
                    InitTitle();
                    NotifyPropertyChanged("Title");
                    Memorize();
                }
            }
        }


        public string LastName
        {
            get
            {
                return sampling.LastName;
            }
            set
            {
                if (sampling.LastName != value)
                {
                    sampling.LastName = value;
                    NotifyPropertyChanged("LastName");
                    Memorize();
                }
            }
        }

        public string FirstName
        {
            get
            {
                return sampling.FirstName;
            }
            set
            {
                if (sampling.FirstName != value)
                {
                    sampling.FirstName = value;
                    NotifyPropertyChanged("FirstName");
                    Memorize();
                }
            }
        }

        public string OrganizationName
        {
            get
            {
                return sampling.OrganizationName;
            }
            set
            {
                if (sampling.OrganizationName != value)
                {
                    sampling.OrganizationName = value;
                    NotifyPropertyChanged("OrganizationName");
                    Memorize();
                }
            }
        }

        public string Position
        {
            get
            {
                return sampling.Position;
            }
            set
            {
                if (sampling.Position != value)
                {
                    sampling.Position = value;
                    NotifyPropertyChanged("Position");
                    Memorize();
                }
            }
        }


        public DateRange DateRange
        {
            get
            {
                return sampling.DateRange;
            }
            set
            {
                if (!DateRange.EqualsDateRange(sampling.DateRange, value))
                {
//                    Debug.WriteLine("From = " + sampling.DateRange.ToSafeString());
//                    Debug.WriteLine("To = " + value.ToSafeString());
                    sampling.DateRange = value;
                    NotifyPropertyChanged("DateRange");
                    Memorize();
                }
            }
        }

        public string Situation
        {
            get
            {
                return sampling.Situation;
            }
            set
            {
                if (sampling.Situation != value)
                {
                    sampling.Situation = value;
                    NotifyPropertyChanged("Situation");
                    Memorize();
                }
            }
        }

        //public string MethodCode
        //{
        //    get
        //    {
        //        return sampling.MethodCode;
        //    }
        //    set
        //    {
        //        if (sampling.MethodCode != value)
        //        {
        //            sampling.MethodCode = value;
        //            NotifyPropertyChanged("SelectedSamplingMethod");
        //            Memorize();
        //        }
        //    }
        //}

        public string MethodName
        {
            get
            {
                return sampling.MethodName;
            }
            set
            {
                if (sampling.MethodName != value)
                {
                    sampling.MethodName = value;
                    NotifyPropertyChanged("MethodName");
                    Memorize();
                }
            }
        }

//        public string Method
//        {
//            //Make in use of Codebook output
//            get
//            {
//                return Option.FindLabel(samplingMethods, MethodCode);
//            }
//       }

        public void GenerateCollector()
        {
            StudyUnitVM studyUnit = StudyUnit;
            MemberFormVM memberForm = studyUnit.MemberForm;
            if (IsCollectorTypeIndividual)
            {
                MemberVM newMember = memberForm.AppendMember(sampling.MemberId, this.LastName, this.FirstName, this.OrganizationName, this.Position);
                UpdateIndividual(newMember);
            }
            else
            {
                OrganizationVM organization = memberForm.AppendOrganization(sampling.MemberId, this.OrganizationName);
                UpdateOrganization(organization);
            }
        }

        private ICommand selectIndividualCommand;

        public ICommand SelectIndividualCommand
        {
            get
            {
                if (selectIndividualCommand == null)
                {
                    selectIndividualCommand = new RelayCommand(param => this.SelectIndividual(), param => this.CanSelectIndividual);
                }
                return selectIndividualCommand;
            }
        }

        public bool CanSelectIndividual
        {
            get
            {
                return true;
            }
        }

        public void UpdateIndividual(MemberVM member)
        {
            if (member == null)
            {
                return;
            }

            string memberId = null;
            string lastName = null;
            string firstName = null;
            string organizationName = null;
            string position = null;
            if (member != null)
            {
                memberId = member.Member.Id;
                lastName = member.LastName;
                firstName = member.FirstName;
                organizationName = member.OrganizationName;
                position = member.Position;
            }
            sampling.MemberId = memberId;
            this.LastName = lastName;
            this.FirstName = firstName;
            this.OrganizationName = organizationName;
            this.Position = position;
        }

        public void SelectIndividual()
        {
            SelectMemberWindow dlg = new SelectMemberWindow(StudyUnit);
            dlg.Owner = Application.Current.MainWindow;
            dlg.ShowDialog();
            if (dlg.DialogResult == true)
            {
                MemberVM member = dlg.SelectedMember;
                UpdateIndividual(member);
            }
        }

        private ICommand selectOrganizationCommand;
        public ICommand SelectOrganizationCommand
        {
            get
            {
                if (selectOrganizationCommand == null)
                {
                    selectOrganizationCommand = new RelayCommand(param => this.SelectOrganization(), param => this.CanSelectOrganization);
                }
                return selectOrganizationCommand;
            }
        }

        public bool CanSelectOrganization
        {
            get
            {
                return true;
            }
        }

        public void UpdateOrganization(OrganizationVM organization)
        {
            if (organization == null)
            {
                return;
            }
            sampling.MemberId = organization.Id;
            this.OrganizationName = organization.OrganizationName;
        }

        public void SelectOrganization()
        {
            SelectObjectWindowVM<OrganizationVM> vm = new SelectObjectWindowVM<OrganizationVM>(StudyUnit.Organizations, "OrganizationName");
            SelectObjectWindow window = new SelectObjectWindow(vm);
            OrganizationVM organization = SelectObjectWindow.Select(Resources.SelectMember, vm) as OrganizationVM;
            if (organization != null)
            {
                UpdateOrganization(organization);
            }
        }

        public string DefaultUniverseGuid 
        {
            get
            {
                if (universes.Count == 0)
                {
                    return null;
                }
                foreach (UniverseVM universe in universes)
                {
                    if (universe.IsMain)
                    {
                        return universe.Id;
                    }
                }
                return universes[0].Id;
            }
        }

        private object selectedUniverseItem;
        public object SelectedUniverseItem
        {
            get
            {
                return selectedUniverseItem;
            }
            set
            {
                if (selectedUniverseItem != value)
                {
                    selectedUniverseItem = value;
                    NotifyPropertyChanged("SelectedUniverseItem");
                }
            }
        }

        public UniverseVM SelectedUniverse
        {
            get
            {
                return SelectedUniverseItem as UniverseVM;
            }
        }

        private ICommand removeUniverseCommand;
        public ICommand RemoveUniverseCommand
        {
            get
            {
                if (removeUniverseCommand == null)
                {
                    removeUniverseCommand = new RelayCommand(param => RemoveUniverse(), param => CanRemoveUniverse);
                }
                return removeUniverseCommand;
            }
        }

        public bool CanRemoveUniverse
        {
            get
            {
                if (SelectedUniverse == null)
                {
                    return false;
                }
                return true;
            }
        }

        public void RemoveUniverse()
        {
            StudyUnit.RemoveUniverseFromVariable(SelectedUniverse);
            Universes.Remove(SelectedUniverse);
            SelectedUniverseItem = null;
        }

        private object selectedSamplingNumberItem;
        public object SelectedSamplingNumberItem
        {
            get
            {
                return selectedSamplingNumberItem;
            }
            set
            {
                if (selectedSamplingNumberItem != value)
                {
                    selectedSamplingNumberItem = value;
                    NotifyPropertyChanged("SelectedSamplingNumberItem");
                }
            }
        }

        public SamplingNumberVM SelectedSamplingNumber
        {
            get
            {
                return SelectedSamplingNumberItem as SamplingNumberVM;
            }
        }


        private ICommand removeSamplingNumberCommand;
        public ICommand RemoveSamplingNumberCommand
        {
            get
            {
                if (removeSamplingNumberCommand == null)
                {
                    removeSamplingNumberCommand = new RelayCommand(param => RemoveSamplingNumber(), param => CanRemoveSamplingNumber);
                }
                return removeSamplingNumberCommand;
            }
        }

        public bool CanRemoveSamplingNumber
        {
            get
            {
                if (SelectedSamplingNumber == null)
                {
                    return false;
                }
                return true;
            }
        }

        public void RemoveSamplingNumber()
        {
//            StudyUnit.RemoveUniverseFromVariable(SelectedUniverse);
            SamplingNumbers.Remove(SelectedSamplingNumber);
            SelectedSamplingNumberItem = null;
        }


        public String CollectorTypeCode
        {
            get
            {
                return sampling.CollectorTypeCode;
            }
            set
            {
                if (sampling.CollectorTypeCode != value)
                {
                    sampling.CollectorTypeCode = value;
                    NotifyPropertyChanged("CollectorTypeCode");
                    Memorize();
                }
            }
        }

        public bool IsCollectorTypeIndividual
        {
            get
            {
                return Options.COLLECTOR_TYPE_INDIVIDUAL == CollectorTypeCode;
            }
        }

        #region IOrderedObject Member

        public int OrderNo { get; set; }
        public string OrderPrefix { get; set; }
        
        #endregion
    }
}
