using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MrDHelper
{
    public class PATHS
    {
        private static string? parentFolderWithoutSlash => System.IO.Directory.GetParent(BasePath)?.FullName;
        public static string? ParrentFolderPath => System.IO.Directory.GetParent(parentFolderWithoutSlash ?? BasePath)?.FullName;
        
        public static string BasePath = AppDomain.CurrentDomain.BaseDirectory;
        
        public static string DataFolderPath = Path.Combine(BasePath, "Data");
        public static string BinFolderPath = Path.Combine(BasePath, "Bin");
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
