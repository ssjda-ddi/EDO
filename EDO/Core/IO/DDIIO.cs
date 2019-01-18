using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using EDO.Core.ViewModel;
using System.Diagnostics;
using EDO.Core.Model;
using EDO.Core.Util;
using System.Text.RegularExpressions;
using System.Xml.Schema;
using System.Xml;

namespace EDO.Core.IO
{
    abstract public class DDIIO
    {
        public static DateUnit ParseDateUnit(string str)
        {
            DateUnit dateUnit = null;
            try
            {
                dateUnit = DateParser.Parse(str);
            }
            catch (Exception)
            {
            }
            return dateUnit;
        }

        public static XElement Create(XName name, object obj)
        {
            return new XElement(name, obj);
        }

        public static XElement CreateNullable(XName name, object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return new XElement(name, obj);
        }

        public static XElement CreateNullable(XName name, string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return new XElement(name, str);
        }

        public static XAttribute CreateNullableAttr(string name, string str)
        {
            if (string.IsNullOrEmpty(str))
            {
                return null;
            }
            return new XAttribute(name, str);
        }

        public static XAttribute CreateNullableAttr(string name, object obj)
        {
            if (obj == null)
            {
                return null;
            }
            return new XAttribute(name, obj);
        }

        public static XElement EmptyToNull(XElement element)
        {
            //IsEmpty=true if nothing is included such as child
            //IsEmpty=false when <title>abc</title><title></title>. IsEmpty=true when <title/>
            return element.IsEmpty ? null : element;
        }

        private static string TrimToNull(string str)
        {
            str = str.Trim();
            return string.IsNullOrEmpty(str) ? null : str;
        }

        private static List<string> SplitAndTrim(string str, char separator)
        {
            return str.Split(separator).Select(p => TrimToNull(p)).ToList();
        }

        public static string BuildPublisher(Book book)
        {
            if (string.IsNullOrEmpty(book.Publisher) && string.IsNullOrEmpty(book.City))
            {
                return "";
            }
            return book.Publisher + ", " + book.City;
        }
        public static void ParsePublisher(string str, Book book)
        {
            if (string.IsNullOrEmpty(str))
            {
                return;
            }
            List<string> elems = SplitAndTrim(str, ',');
            if (elems.Count != 2)
            {
                return;
            }
            book.Publisher = elems[0];
            book.City = elems[1];
        }

        private static string BuildPageIdentifier(Book book)
        {
            if (string.IsNullOrEmpty(book.StartPage) && string.IsNullOrEmpty(book.EndPage))
            {
                return "";
            }
            return "pp. " + book.StartPage + "-" + book.EndPage;
        }

        private static string BuildBookIdentifier(Book book)
        {
            if (string.IsNullOrEmpty(book.BookName) && string.IsNullOrEmpty(book.Chapter))
            {
                return "";
            }
            return book.BookName + ", " + book.Chapter + ": ";
        }

        private static string BuildMagazineIdentifier(Book book)
        {
            if (string.IsNullOrEmpty(book.MagazineName) && string.IsNullOrEmpty(book.Volume) && string.IsNullOrEmpty(book.Number))
            {
                return "";
            }
            return book.MagazineName + ", " + book.Volume + "(" + book.Number + "): ";
        }

        private static string BuildUniversityIdentifier(Book book)
        {
            if (string.IsNullOrEmpty(book.UniversityName) && string.IsNullOrEmpty(book.DepartmentName))
            {
                return "";
            }
            return book.UniversityName + " " + book.DepartmentName + ", ";
        }

        public static string BuildIdentifier(Book book)
        {
            StringBuilder buf = new StringBuilder();
            if (book.IsBookTypeBook || book.IsBookTypeSocietyAbstract || book.IsBookTypeReport || book.IsBookTypeOther)
            {
                //Book Title, Society Abstract, Report, etc.
                //pp. start page - end page
                buf.Append(BuildPageIdentifier(book));
            }
            else if (book.IsBookTypeBookChapter)
            {
                //Book Title(Chapter)
                //Book Title, chapter: pp. start page - end page
                buf.Append(BuildBookIdentifier(book));
                buf.Append(BuildPageIdentifier(book));
            }
            else if (book.IsBookTypeTreatiseWithPeerReview || book.IsBookTypeTreatiseWithoutPeerReview)
            {
                //Peer-reviewed academic papers, No peer-reviewed academic papers
                //Journal, issue: pp. start page - end page
                buf.Append(BuildMagazineIdentifier(book));
                buf.Append(BuildPageIdentifier(book));
            }
            else if (book.IsBookTypeThesis)
            {
                //Thesis
                //University, Department, pp. Start page - End page
                buf.Append(BuildUniversityIdentifier(book));
                buf.Append(BuildPageIdentifier(book));
            }
            return buf.ToString();
        }

        public static string ParsePageIdentifier(string str, Book book)
        {
            //            return "pp. " + book.StartPage + "-" + book.EndPage;

            Regex regex = new Regex(@"pp\. (\d+)-(\d+)$");
            Match m = regex.Match(str);
            if (!m.Success)
            {
                return str;
            }

            book.StartPage = m.Groups[1].Value;
            book.EndPage = m.Groups[2].Value;

            return str.Substring(0, m.Index);
        }

        public static void ParseBookIdentifier(string str, Book book)
        {
            //      return book.BookName + ", " + book.Chapter + ": ";
            Regex regex = new Regex(@"^(.*):\s*");
            Match m = regex.Match(str);
            if (!m.Success)
            {
                return;
            }
            List<string> elems = SplitAndTrim(m.Groups[1].Value, ',');
            if (elems.Count != 2)
            {
                return;
            }
            book.BookName = elems[0];
            book.Chapter = elems[1];
        }

        public static void ParseMagazineIdentifier(string str, Book book)
        {
            //book.MagazineName + ", " + book.Volume + "(" + book.Number + "): ";
            Regex regex = new Regex(@"^(.*):\s*");
            Match m = regex.Match(str);
            if (!m.Success)
            {
                return;
            }
            List<string> elems = SplitAndTrim(m.Groups[1].Value, ',');
            if (elems.Count != 2)
            {
                return;
            }
            book.MagazineName = elems[0];

            string rest = elems[1];
            regex = new Regex(@"^(.*)\((.*)\)$");
            m = regex.Match(rest);
            if (!m.Success)
            {
                return;
            }
            book.Volume = TrimToNull(m.Groups[1].Value);
            book.Number = TrimToNull(m.Groups[2].Value);
        }

        public static void ParseUniversityIdentifier(string str, Book book)
        {
            // book.UniversityName + " " + book.DepartmentName + ", ";
            Regex regex = new Regex(@"^(.*),\s*");
            Match m = regex.Match(str);
            if (!m.Success)
            {
                return;
            }
            string rest = m.Groups[1].Value;
            List<string> elems = SplitAndTrim(rest, ' ');
            if (elems.Count != 2)
            {
                return;
            }
            book.UniversityName = elems[0];
            book.DepartmentName = elems[1];
        }

        public static void ParseIdentifier(string identifier, Book book)
        {
            if (string.IsNullOrEmpty(identifier))
            {
                return;
            }
            if (book.IsBookTypeBook || book.IsBookTypeSocietyAbstract || book.IsBookTypeReport || book.IsBookTypeOther)
            {
                string rest = ParsePageIdentifier(identifier, book);
            }
            else if (book.IsBookTypeBookChapter)
            {
                string rest = ParsePageIdentifier(identifier, book);
                ParseBookIdentifier(rest, book);
            }
            else if (book.IsBookTypeTreatiseWithPeerReview || book.IsBookTypeTreatiseWithoutPeerReview)
            {
                string rest = ParsePageIdentifier(identifier, book);
                ParseMagazineIdentifier(rest, book);
            }
            else if (book.IsBookTypeThesis)
            {
                string rest = ParsePageIdentifier(identifier, book);
                ParseUniversityIdentifier(rest, book);
            }
        }


        private List<IOError> errors;

        public DDIIO()
        {
            errors = new List<IOError>();
        }

        [Obsolete("Error check at export was canceled.", false)]
        protected void AddError(IOError error)
        {
            errors.Add(error);
        }

        [Obsolete("Error check at export was canceled.", false)]
        protected bool ContainsError(IOError error)
        {
            return errors.Contains(error);
        }

        [Obsolete("Error check at export was canceled.", false)]
        protected bool HasError
        {
            get
            {
                return errors.Count > 0;
            }
        }

        [Obsolete("Error check at export was canceled.", false)]
        protected int RemoveError(Func<IOError, bool> cond)
        {
            int errorCount = 0;
            for (int i = errors.Count - 1; i >= 0; i--)
            {
                if (cond(errors[i]))
                {
                    errors.RemoveAt(i);
                    errorCount++;
                }
            }
            return errorCount;
        }

        [Obsolete("Error check at export was canceled.", false)]
        protected void DumpError()
        {
            foreach (IOError error in errors)
            {
                Debug.WriteLine(error.ToString());
            }
        }

        protected abstract void AddAllSchema(XmlSchemaSet sc);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        /// <exception cref="System.Xml.XmlException" />
        public ValidationResult Validate(string xml)
        {
            var result = new ValidationResult();
            XmlReaderSettings settings = new XmlReaderSettings();
            AddAllSchema(settings.Schemas);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ProcessIdentityConstraints;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationEventHandler += (object sender, ValidationEventArgs args) =>
            {
                result.AddError(new ValidationError($"<{((XmlReader)sender).Name}>", args.Message, args.Exception.LineNumber, args.Exception.LinePosition));
            };

            using (var reader = XmlReader.Create(xml, settings))
            {
                while (reader.Read()) ;
            }

            return result;
        }

        public class ValidationError
        {
            private readonly string name;
            private readonly string message;
            private readonly int line;
            private readonly int position;

            public ValidationError(string name, string message, int line, int position)
            {
                this.name = name;
                this.message = message;
                this.line = line;
                this.position = position;
            }

            public string Name => name;

            public string Message => message;

            public int Line => line;

            public int Position => position;
        }

        public class ValidationResult
        {
            private bool valid = true;
            public bool Valid { get => valid; }

            private readonly List<ValidationError> errors = new List<ValidationError>();
            public List<ValidationError> Errors => errors;

            public void AddError(ValidationError error)
            {
                valid = false;
                Errors.Add(error);
            }
        }

        public class ValidationException : Exception
        {
            public readonly List<ValidationError> errors;

            public ValidationException(List<ValidationError> errors)
            {
                this.errors = errors;
            }
        }

        protected void CanImport(string path)
        {
            var validationResult = Validate(path);
            if (!validationResult.Valid)
            {
                throw new ValidationException(validationResult.Errors);
            }
        }
    }
}
