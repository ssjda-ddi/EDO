using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using EDO.Core.Util;
using EDO.Core.Model.Layout;

namespace EDO.Core.Model
{
    public class Question :ICloneable, IIDPropertiesProvider
    {
        public static void ChangeConceptId(List<Question> questions, string oldId, string newId)
        {
            foreach (Question question in questions)
            {
                if (question.ConceptId == oldId)
                {
                    question.ConceptId = newId;
                }
            }
        }

        public static List<Response> GetResponses(List<Question> questions)
        {
            List<Response> responses = new List<Response>();
            foreach (Question question in questions)
            {
                responses.Add(question.Response);
            }
            return responses;
        }

        public static Question Find(List<Question> questions, string questionId)
        {
            foreach (Question question in questions)
            {
                if (question.Id == questionId)
                {
                    return question;
                }
            }
            return null;
        }

        public string[] IdProperties
        {
            get
            {
                return new string[] { "Id" };
            }
        }
       
        public Question()
        {
            Id = IDUtils.NewGuid();
            Response = new Response();
        }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Text { get; set; }

        private Response response;
        public Response Response { 
            get
            {
                return response;
            }
            set
            {
                if (response != value)
                {
                    response = value;
                }
            }
        }

        public string ConceptId { get; set; }

        public bool IsCreatedVariable { get; set; } //Flag whether generated from the Variable this Question
        public bool IsCreatedConstruct { get; set; } //Flag whether generated Construct from this Question
        public VariableGenerationInfo VariableGenerationInfo { get; set; }

        public VariableGenerationInfo CreateVariableGenerationInfo() {
            VariableGenerationInfo info = new VariableGenerationInfo();
            info.ResponseTypeCode = Response.TypeCode;
            info.ChoicesLayoutMesurementMethod = ChoicesLayoutMesurementMethod.Single;
            if (Response.IsTypeChoices) {
                ResponseLayout layout = response.Layout;
                if (layout is ChoicesLayout) {
                    ChoicesLayout choicesLayout = (ChoicesLayout)layout;
                    info.ChoicesLayoutMesurementMethod = choicesLayout.MeasurementMethod;
                }
            }
            return info;
        }

        #region ICloneable Member

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        #endregion

    }
}
