using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EDO.Main;
using System.IO;
using System.Diagnostics;
using EDO.Core.ViewModel;

namespace EDO.Core.IO
{
    public class IOUtils
    {
        public enum DialogType
        {
            Open, Save,
        }

        [Obsolete("should use FileIODialog")]
        public static string QuerySavePathName(string title, string initPathName, string filter, bool askPathName)
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();
            dlg.Title = title;
            dlg.Filter = filter;
            if (!string.IsNullOrEmpty(initPathName))
            {
                dlg.InitialDirectory = Path.GetDirectoryName(initPathName);
                dlg.FileName = Path.GetFileName(initPathName);
            }
            bool? result = dlg.ShowDialog();
            string path = null;
            if (result == true)
            {
                path = dlg.FileName;
            }
            return path;
        }

        [Obsolete("should use FileIODialog")]
        public static string QueryOpenPathName(string filter)
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = filter;
            bool result = dlg.ShowDialog() ?? false;
            if (!result)
            {
                return null;
            }
            return dlg.FileName;
        }

        public static T OpenFileIODialog<T>(string filter, DialogType type = DialogType.Open) where T : FileDialogResult, new()
        {
            return OpenFileIODialog<T>(null, null, filter, type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="title"></param>
        /// <param name="initPathName"></param>
        /// <param name="filter"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <exception cref="EDOCanceledException" />
        public static T OpenFileIODialog<T>(string title, string initPathName, string filter, DialogType type = DialogType.Open) where T : FileDialogResult, new()
        {
            Microsoft.Win32.FileDialog dialog;
            if (type == DialogType.Open)
            {
                dialog = new Microsoft.Win32.OpenFileDialog();
            }
            else
            {
                dialog = new Microsoft.Win32.SaveFileDialog();
            }
            dialog.Title = title;
            dialog.Filter = filter;
            if (!string.IsNullOrEmpty(initPathName))
            {
                dialog.InitialDirectory = Path.GetDirectoryName(initPathName);
                dialog.FileName = Path.GetFileName(initPathName);
            }
            bool result = dialog.ShowDialog() ?? false;
            if (!result || string.IsNullOrEmpty(dialog.FileName))
            {
                throw new EDOCanceledException();
            }
            T fileDialogResult = new T
            {
                FileName = dialog.FileName,
                FilterIndex = dialog.FilterIndex
            };
            return fileDialogResult;
        }
    }
}
