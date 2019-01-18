using System;

namespace EDO.Core.IO
{
    public class FileDialogResult
    {
        public FileDialogResult() { }

        [Obsolete]
        public FileDialogResult(string fileName, int filterIndex)
        {
            FileName = fileName;
            FilterIndex = filterIndex;
        }

        public string FileName { get; set; }
        public virtual int FilterIndex { get; set; }
    }

}