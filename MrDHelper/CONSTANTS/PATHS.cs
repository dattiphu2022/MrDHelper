using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MrDHelper
{
    public class PATHS
    {
        /// <summary>
        /// System.IO.Directory.GetParent(<see cref="BasePath"/>)?.FullName
        /// </summary>
        private static string? parentFolderWithoutSlash => System.IO.Directory.GetParent(BasePath)?.FullName;

        /// <summary>
        /// System.IO.Directory.GetParent(parentFolderWithoutSlash ?? <see cref="BasePath"/>)?.FullName;
        /// </summary>
        public static string? ParrentFolderPath { get => System.IO.Directory.GetParent(parentFolderWithoutSlash ?? BasePath)?.FullName; }

        /// <summary>
        /// AppDomain.CurrentDomain.BaseDirectory
        /// </summary>
        public static string BasePath { get => AppDomain.CurrentDomain.BaseDirectory; }

        /// <summary>
        /// Path.Combine(BasePath, "Data")
        /// </summary>
        public static string DataFolderPath { get => Path.Combine(BasePath, "Data"); }

        /// <summary>
        /// Path.Combine(BasePath, "Bin")
        /// </summary>
        public static string BinFolderPath { get => Path.Combine(BasePath, "Bin"); }

        /// <summary>
        /// Path.Combine(BasePath, "Logs")
        /// </summary>
        public static string LogFolderPath { get => Path.Combine(BasePath, "Logs"); }

        /// <summary>
        /// Path.Combine(BasePath, "ClientLogs")
        /// </summary>
        public static string ClientLogFolderPath { get => Path.Combine(BasePath, "ClientLogs"); }

        /// <summary>
        /// Path.Combine(BasePath, "MasterLogs")
        /// </summary>
        public static string MasterLogFolderPath { get => Path.Combine(BasePath, "MasterLogs"); }

        /// <summary>
        /// Eg: Data, X.json will produce something like: ~/Data/x.json
        /// </summary>
        /// <param name="folder"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Get_FullFilePath_FromFolderAndFileName(string folder, string fileName)
        {
            if (fileName.IsNullOrWhiteSpace())
            {
                throw new ArgumentNullException(nameof(fileName));
            }
            string folderPath = Path.Combine(BasePath, folder);
            string filePath = Path.Combine(folderPath, fileName);
            return filePath;
        }

        /// <summary>
        /// Eg: X.json will produce something like: ~[startuppath]/x.json
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static string Get_FullFilePath(string fileName)
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
