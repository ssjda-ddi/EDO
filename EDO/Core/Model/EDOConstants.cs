using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Properties;
using System.Windows.Input;

namespace EDO.Core.Model
{
    public class EDOConstants
    {
        public const string TAG_UNDOABLE = "Undoable";
        public static readonly string ERR_UNEXPECTED = Resources.UnexpectedErrorOccurred; //Unexpected Error Occurred.
        public static readonly string LABEL_ALL = Resources.All; //All
        public static readonly KeyGesture KEY_DELETE = new KeyGesture(Key.Delete);
        public static readonly KeyGesture KEY_PAGE_UP = new KeyGesture(Key.PageUp);
        public static readonly KeyGesture KEY_PAGE_DOWN = new KeyGesture(Key.PageDown);
        public const string EMPTY_CURSOR = "-";
        public static readonly string LABEL_UNDEFINED = Resources.UnspecifiedValue;
        public static readonly string LABEL_EMPTY = Resources.Empty;
        public static int DEF_SCALE = 1;
        public static readonly string SELECTED_CODE_VALUE = "0";
        public static readonly string UNSELECTED_CODE_VALUE = "1";
    }
}
