using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using EDO.Core.Model;
using EDO.Core.Util;

namespace EDO.Core.IO
{
    public class DDI31SamplingNumberIO
    {
        #region write
        public static string Escape(string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return "";
            }
            bool mustQuote = (str.Contains(",") || str.Contains("\"") || str.Contains("\r") || str.Contains("\n") || str.Contains("("));
            if (!mustQuote)
            {
                return str;
            }
            StringBuilder sb = new StringBuilder();
            //
            // must quote
            // 
            // a"b => "a""b" 
            //
            sb.Append("\"");
            foreach (char nextChar in str)
            {
                sb.Append(nextChar);
                if (nextChar == '"')
                    sb.Append("\"");
            }
            sb.Append("\"");
            return sb.ToString();
        }

        public static string ToString(SamplingNumber samplingNumber)
        {
            StringBuilder content = new StringBuilder();
            content.Append(Escape(samplingNumber.ResponseRate));
            content.Append("(");
            content.Append(Escape(samplingNumber.SampleSize));
            content.Append(",");
            content.Append(Escape(samplingNumber.NumberOfResponses));
            content.Append(",");
            content.Append(Escape(samplingNumber.Description));
            content.Append(")");
            return content.ToString();
        }

        #endregion

        #region read

        public static string[] GetFirstToken(string str)
        {
            Regex regex1 = new Regex(@"^""((?:[^""]|"""")*)""\((.*)\)$");
            Regex regex2 = new Regex(@"^([^""]*)\((.*)\)$");
            Match m = regex1.Match(str);
            if (m.Success)
            {
                // match escaped
                return new string[] {
                    m.Groups[1].Value.Replace("\"\"", "\""), //unescape escaped double quote
                    m.Groups[2].Value };
            }
            m = regex2.Match(str);
            if (m.Success)
            {
                return new string[] { m.Groups[1].Value, m.Groups[2].Value };
            }
            return new string[] { "", "" };
        }

        public static void Parse(string str, SamplingNumber samplingNumber)
        {
            string[] tokens = GetFirstToken(str);
            samplingNumber.ResponseRate = tokens[0];
            string[] restTokens = EDOUtils.ParseCSV(tokens[1]);
            if (restTokens == null || restTokens.Length < 3)
            {
                return;
            }
            samplingNumber.SampleSize = restTokens[0];
            samplingNumber.NumberOfResponses = restTokens[1];
            samplingNumber.Description = restTokens[2];
        }
        #endregion



    }
}
