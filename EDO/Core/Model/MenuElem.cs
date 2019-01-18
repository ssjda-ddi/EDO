using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Properties;

namespace EDO.Core.Model
{
    //define for use in the menu screen of DDI import and menu screen of the main screen
    public class MenuElem
    {
        //Which starts with C is category, which starts with M is menu,
        public static MenuElem C_EVENT;
        public static MenuElem M_EVENT;

        public static MenuElem C_STUDY;
        public static MenuElem M_MEMBER;
        public static MenuElem M_ABSTRACT;
        public static MenuElem M_COVERAGE;
        public static MenuElem M_FUNDING_INFO;

        public static MenuElem C_SAMPLING;
        public static MenuElem M_SAMPLING;

        public static MenuElem C_QUESTION;
        public static MenuElem M_CONCEPT;
        public static MenuElem M_QUESTION;
        public static MenuElem M_CATEGORY;
        public static MenuElem M_CODE;
        public static MenuElem M_SEQUENCE;
        public static MenuElem M_QUESTION_GROUP;

        public static MenuElem C_VARIABLE;
        public static MenuElem M_VARIABLE;

        public static MenuElem C_DATA;
        public static MenuElem M_DATA_SET;
        public static MenuElem M_DATA_FILE;
        public static MenuElem M_BOOKS;
        public static MenuElem M_STATISTICS;

        public static MenuElem C_GROUP;
        public static MenuElem M_DETAIL;
        public static MenuElem M_COMPARE_DAI;
        public static MenuElem M_COMPARE_SHO;
        public static MenuElem M_COMPARE_VARIABLE;

        static MenuElem()
        {
            int id = 1;
            C_EVENT = new MenuElem(id++, Resources.EventManagement, true); //Data Lifecycle Events
            M_EVENT = new MenuElem(id++, Resources.EventManagement); //Data Lifecycle Events

            C_STUDY = new MenuElem(id++, Resources.StudyPlan, true); //Study Description
            M_MEMBER = new MenuElem(id++, Resources.StudyMember); //Study Member
            M_ABSTRACT = new MenuElem(id++, Resources.StudyAbstract); //Abstract
            M_COVERAGE = new MenuElem(id++, Resources.StudyRange); //Coverage
            M_FUNDING_INFO = new MenuElem(id++, Resources.StudyFund); //Funding

            C_SAMPLING = new MenuElem(id++, Resources.DataCollectionMethod, true); //Data Collection
            M_SAMPLING = new MenuElem(id++, Resources.DataCollectionMethod); //Data Collection

            C_QUESTION = new MenuElem(id++, Resources.QuestionDesign, true); //Question
            M_CONCEPT = new MenuElem(id++, Resources.VariableConcept); //Concept
            M_QUESTION = new MenuElem(id++, Resources.QuestionItemDesign); //Question Item
            M_CATEGORY = new MenuElem(id++, Resources.Category); //Category
            M_CODE = new MenuElem(id++, Resources.Code); //Code
            M_SEQUENCE = new MenuElem(id++, Resources.Sequence); //Sequence
            M_QUESTION_GROUP = new MenuElem(id++, Resources.QuestionGroup);

            C_VARIABLE = new MenuElem(id++, Resources.VariableManagement, true); //Variable
            M_VARIABLE = new MenuElem(id++, Resources.VariableInfo); //Variable Information

            C_DATA = new MenuElem(id++, Resources.DataManagement, true); //Data
            M_DATA_SET = new MenuElem(id++, Resources.DataSetDefinition); //Logical Data Definition
            M_DATA_FILE = new MenuElem(id++, Resources.DataFileDefinition); //Physical Data Description
            M_BOOKS = new MenuElem(id++, Resources.BookList); //Physical Data Description
            M_STATISTICS = new MenuElem(id++, Resources.StatisticsInfo); 

            C_GROUP = new MenuElem(id++, Resources.GroupManagement, true); //Group Information
            M_DETAIL = new MenuElem(id++, Resources.DetailItem); //Group Description
            M_COMPARE_DAI = new MenuElem(id++, Resources.CompareMajorDivision); //Concept Scheme Comparison
            M_COMPARE_SHO = new MenuElem(id++, Resources.CompareMinorDivision); //Concept Comparison
            M_COMPARE_VARIABLE = new MenuElem(id++, Resources.CompareVariable); //Variable Comparison
        }

        public MenuElem(int id, string title)
            : this(id, title, false)
        {
        }

        public MenuElem(int id, string title, bool isCategory)
        {
            this.id = id;
            this.title = title;
            this.isCategory = isCategory;
        }

        private int id;
        public int Id { get { return id; } }

        private string title;
        public string Title { get { return title; } }

        private bool isCategory;
        public bool IsCategory { get { return isCategory; } }
    }
}
