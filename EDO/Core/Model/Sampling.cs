using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Core.Util;

namespace EDO.Core.Model
{
    public class Sampling :IIDPropertiesProvider, IStringIDProvider
    {
        public static List<Universe> GetUniverses(List<Sampling> samplings)
        {
            var universes = new List<Universe>();
            foreach (Sampling sampling in samplings)
            {
                universes.AddRange(sampling.Universes);
            }
            return universes;
        }

        public static Universe FindUniverse(List<Sampling> samplings, string universeId)
        {
            List<Universe> universes = GetUniverses(samplings);
            foreach (Universe universe in universes)
            {
                if (universe.Id == universeId)
                {
                    return universe;
                }
            }
            return null;
        }

        public static Universe FindDefaultUniverse(List<Sampling> samplings)
        {
            List<Universe> universes = GetUniverses(samplings);
            if (universes.Count == 0)
            {
                return null;
            }
            Universe universe =  Universe.FindMainUniverse(universes);
            if (universe != null)
            {
                return universe;
            }
            return universes[0];
        }

        public static Universe FindMainUniverse(List<Sampling> samplings)
        {
            List<Universe> universes = GetUniverses(samplings);
            return Universe.FindMainUniverse(universes);
        }

        public static HashSet<string> CollectTitles(List<Sampling> samplings)
        {
            HashSet<string> titles = new HashSet<string>();
            foreach (Sampling samplingModel in samplings)
            {
                if (samplingModel.Title != null)
                {
                    titles.Add(samplingModel.Title);
                }
            }
            return titles;
        }

        public static string DefaultCollectorType
        {
            get
            {
                return Options.COLLECTOR_TYPE_INDIVIDUAL;
            }        
        }

        public static void ChangeMemberId(List<Sampling> samplings, string oldId, string newId)
        {
            foreach (Sampling sampling in samplings)
            {
                if (sampling.MemberId == oldId)
                {
                    sampling.MemberId = newId;
                }
            }
        }

        public string[] IdProperties
        {
            get
            {
                return new string[] { "Id", "CollectionEventId", "ModeOfCollectionId", "CollectionSituationId" };
            }
        }

        public Sampling()
        {
            Id = IDUtils.NewGuid();
            DateRange = new DateRange();
            CollectionEventId = IDUtils.NewGuid();
            ModeOfCollectionId = IDUtils.NewGuid();
            CollectionSituationId = IDUtils.NewGuid();

            Universes = new List<Universe>();
            SamplingNumbers = new List<SamplingNumber>();
            CollectorTypeCode = DefaultCollectorType;
        }
        public string Id { get; set; }
        public DateRange DateRange { get; set; }
        public string MemberId { get; set; }
        public string CollectorTypeCode { get; set; }
        public string MethodCode {get; set; } // for backward compatibility
        public string MethodName { get; set; }
        public string Situation {get; set; }
        public string CollectionEventId { get; set; } // for DDI
        public string ModeOfCollectionId { get; set; } // for DDI
        public string CollectionSituationId { get; set; } // for DDI
        //Added for Undo/Redo
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string OrganizationName { get; set; }
        public string Position { get; set; }

        public string Title { get; set; }
        public List<Universe> Universes { get; set; }
        public List<SamplingNumber> SamplingNumbers { get; set; }
        public Universe MainUniverse
        {
            get
            {
                return Universe.FindMainUniverse(Universes);
            }
        }

        internal void ConvertMethodCodeToMethodName()
        {
            if (string.IsNullOrEmpty(MethodCode)) {
                return;
            }
            if (string.IsNullOrEmpty(MethodName))
            {
                // Convert MethodCode To MethodName(since 1.6.0)
                MethodName = Option.FindLabel(Options.SamplingMethods, MethodCode);
            }
            MethodCode = null;
        }
    }
}
