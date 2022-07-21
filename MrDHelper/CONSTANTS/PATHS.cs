using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MrDHelper
{
    public class PATHS
    {
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        public static string DataFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data");
        public static string BinFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Bin");
        public static string GetFilePath(string folder, string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            string folderPath = Path.Combine(BasePath, folder);
            string filePath = Path.Combine(folderPath, fileName);
            return filePath;
        }
        public static string GetFilePath(string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            string filePath = Path.Combine(BasePath, fileName);
            return filePath;
        }
    }
}
