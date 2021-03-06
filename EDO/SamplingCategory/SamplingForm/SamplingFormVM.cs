﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using EDO.Core.ViewModel;
using EDO.Core.Model;
using EDO.Main;
using System.Collections.Specialized;
using System.Windows.Input;
using EDO.Core.Util;
using System.Windows;
using EDO.StudyCategory.MemberForm;
using EDO.VariableCategory.VariableForm;
using EDO.Core.View;
using EDO.Properties;

namespace EDO.SamplingCategory.SamplingForm
{
    public class SamplingFormVM : FormVM
    {
        private static readonly string PREFIX = Resources.SamplingTitle;

        public SamplingFormVM(StudyUnitVM studyUnit)
            : base(studyUnit)
        {
            samplings = new ObservableCollection<SamplingVM>();

            //change how to name the title
            //*Find the unique title like unsed 
            //*The title such as Concept and Amount can be changed manually,
            //Cannot do here
            HashSet<string> titles = Sampling.CollectTitles(studyUnit.SamplingModels);
            foreach (Sampling samplingModel in studyUnit.SamplingModels)
            {
                int uniqIndex = EDOUtils.UniqOrderNo(titles, samplingModel.Title, PREFIX);
                SamplingVM sampling = new SamplingVM(samplingModel)
                {
                    Parent = this,
                    OrderNo = uniqIndex,
                    OrderPrefix = PREFIX
                };
                sampling.Init();
                samplings.Add(sampling);
                titles.Add(sampling.Title); //Add in title set.
            }
            modelSyncher = new ModelSyncher<SamplingVM, Sampling>(this, samplings, studyUnit.SamplingModels);
        }


        public override VMState SaveState()
        {
            if (SelectedSampling == null)
            {
                return null;
            }
            return new VMState(SelectedSampling.Id);
        }

        protected override void Reload(VMState state)
        {
            if (state != null)
            {
                selectedSamplingItem = EDOUtils.Find(Samplings, state.State1);
            }
            if (selectedSamplingItem == null)
            {
                selectedSamplingItem = Samplings.FirstOrNull();
            }
        }


        private ObservableCollection<SamplingVM> samplings;
        private ModelSyncher<SamplingVM, Sampling> modelSyncher;

        public ObservableCollection<SamplingVM> Samplings { get { return samplings; } }

        public object selectedSamplingItem;
        public object SelectedSamplingItem
        {
            get
            {
                return selectedSamplingItem;
            }
            set
            {
                if (selectedSamplingItem != value)
                {
                    selectedSamplingItem = value;
                    NotifyPropertyChanged("SelectedSamplingItem");
                }
            }
        }

        public SamplingVM SelectedSampling
        {
            get
            {
                return SelectedSamplingItem as SamplingVM;
            }
        }
        public ObservableCollection<UniverseVM> AllUniverses
        {
            get
            {
                ObservableCollection<UniverseVM> allUniverses = new ObservableCollection<UniverseVM>();
                foreach (SamplingVM sampling in Samplings)
                {
                    foreach (UniverseVM universe in sampling.Universes)
                    {
                        allUniverses.Add(universe);
                    }
                }
                return allUniverses;
            }
        }

        public string DefaultUniverseGuid
        {
            get
            {
                Universe universe = Sampling.FindDefaultUniverse(StudyUnit.SamplingModels);
                if (universe == null)
                {
                    return null;
                }
                return universe.Id;
            }
        }

        private ICommand addCommand;
        public ICommand AddCommand
        {
            get
            {
                if (addCommand == null)
                {
                    addCommand = new RelayCommand(param => AddSampling(), param => CanAddSampling);
                }
                return addCommand;
            }
        }

        public bool CanAddSampling
        {
            get
            {
                return true;
            }
        }

        public void AddSampling()
        {
            HashSet<string> titles = Sampling.CollectTitles(StudyUnit.SamplingModels);
            int uniqIndex = EDOUtils.UniqOrderNo(titles, null, PREFIX);

            SamplingVM sampling = new SamplingVM();
            sampling.Parent = this;
            sampling.OrderNo = uniqIndex;
            sampling.OrderPrefix = PREFIX;
            sampling.Init();
            samplings.Add(sampling);
            SelectedSamplingItem = sampling;
            Memorize();
        }

        private ICommand removeCommand;
        public ICommand RemoveCommand
        {
            get
            {
                if (removeCommand == null)
                {
                    removeCommand = new RelayCommand(param => RemoveSampling(), param => CanRemoveSampling);
                }
                return removeCommand;
            }
        }

        public bool CanRemoveSampling
        {
            get
            {
                if (SelectedSamplingItem == null)
                {
                    return false;
                }
                return samplings.Count > 0;
            }
        }

        public void RemoveSampling()
        {
            samplings.Remove((SamplingVM)SelectedSamplingItem);
            SelectedSamplingItem = samplings.LastOrNull();
        }

        private ICommand removeUniverseDelegateCommand;
        public ICommand RemoveUniverseDelegateCommand
        {
            get
            {
                if (removeUniverseDelegateCommand == null)
                {
                    removeUniverseDelegateCommand = new RelayCommand(param => RemoveUniverseDelegate(), param => CanRemoveUniverseDelegate);
                }
                return removeUniverseDelegateCommand;
            }
        }

        public bool CanRemoveUniverseDelegate
        {
            get
            {
                if (SelectedSampling == null)
                {
                    return false;
                }
                return SelectedSampling.CanRemoveUniverse;
            }
        }

        public void RemoveUniverseDelegate()
        {
            SelectedSampling.RemoveUniverse();
        }

        private ICommand removeSamplingNumberDelegateCommand;
        public ICommand RemoveSamplingNumberDelegateCommand
        {
            get
            {
                if (removeSamplingNumberDelegateCommand == null)
                {
                    removeSamplingNumberDelegateCommand = new RelayCommand(param => RemoveSamplingNumberDelegate(), param => CanRemoveSamplingNumberDelegate);
                }
                return removeSamplingNumberDelegateCommand;
            }
        }

        public bool CanRemoveSamplingNumberDelegate
        {
            get
            {
                if (SelectedSampling == null)
                {
                    return false;
                }
                return SelectedSampling.CanRemoveSamplingNumber;
            }
        }

        public void RemoveSamplingNumberDelegate()
        {
            SelectedSampling.RemoveSamplingNumber();
        }

        protected override Action GetCompleteAction(VMState state)
        {
            return () => {
                foreach (SamplingVM sampling in Samplings)
                {
                    sampling.GenerateCollector();
                }
            };
        }


        public UniverseVM FindUniverse(string universeId)
        {
            ObservableCollection<UniverseVM> universes = AllUniverses;
            foreach (UniverseVM universe in universes)
            {
                if (universe.Id == universeId)
                {
                    return universe;
                }
            }
            return null;
        }
    }
 
}
