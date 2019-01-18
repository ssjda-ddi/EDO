using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using EDO.Core.Util;
using EDO.Core.Model.Layout;

//Class that is inherited by the one that has the Response as a property of the Question cannot be serialized.
//So make a fat base class of general-purpose for the time being
namespace EDO.Core.Model
{
    public class Response :ICloneable, IIDPropertiesProvider
    {
        public string[] IdProperties
        {
            get
            {
                //Implementation just in case
                return new string[] { "Id" };
            }
        }

        public Response()
            : this(Options.RESPONSE_TYPE_UNKNOWN)
        {
        }

        public Response(Option type)
        {
            Id = IDUtils.NewGuid();                    
            TypeCode = type.Code;
            MissingValues = new List<MissingValue>();
            Layout = null;
            Scale = EDOConstants.DEF_SCALE;
        }

        public string Id { get; set; }

        public string TypeCode {get; set; }
        public bool IsTypeChoices { get { return Options.RESPONSE_TYPE_CHOICES_CODE == TypeCode; } }
        public bool IsTypeUnknown { get { return Options.RESPONSE_TYPE_UNKNOWN_CODE == TypeCode; } }
        public bool IsTypeNumber { get { return Options.RESPONSE_TYPE_NUMBER_CODE == TypeCode; } }
        public bool IsTypeFree { get { return Options.RESPONSE_TYPE_FREE_CODE == TypeCode; } }
        public bool IsTypeDateTime { get { return Options.RESPONSE_TYPE_DATETIME_CODE == TypeCode; } }

        //Title
        public string Title { get; set; }

        public int Scale { get; set; }

        // for choices
        public string CodeSchemeId { get; set;  }

        // for DateTime and Number
        public string DetailTypeCode { get; set; }
        public string DetailTypeLabel { get; set; }

        // for Free
        public string Text { get; set; }

        // for Free and Number
        public decimal? Max { get; set; }
        public decimal? Min { get; set; }

        public List<MissingValue> MissingValues { get; set; }
        public ResponseLayout Layout { get; set; }

        public Response Dup()
        {
            Response newResponseModel = Clone() as Response;
            //replicate new answers itself
            newResponseModel.Id = IDUtils.NewGuid();
            return newResponseModel;
        }

        #region ICloneable Member

        public object Clone()
        {
            Response newResponse =  (Response)this.MemberwiseClone();
            newResponse.MissingValues = new List<MissingValue>();
            foreach (MissingValue mv in MissingValues)
            {
                newResponse.MissingValues.Add((MissingValue)mv.Clone());
            }
            if (Layout != null)
            {
                newResponse.Layout = Layout.Clone() as ResponseLayout;
            }
            return newResponse;
        }

        public string JoinMissingValuesContent()
        {
            return MissingValue.JoinContent(MissingValues, " ");
        }

        #endregion
    }
}
