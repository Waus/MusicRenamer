using System;
using System.IO;
using System.Text.RegularExpressions;

namespace MusicRenamer
{
    class Program
    {
        static void Main(string[] args)
        {
            RenameFiles();
        }

        public static void RenameFiles() // to format "02 - Hey hey Oh!"
        {
            Regex regex1 = new Regex(@"^\d\d\s[\w* [(\.]+");     //02 Hey hey Oh!
            Regex regex2 = new Regex(@"^\d\d\.\s[\w* [(\.]+");        //02. Hey hey Oh!
            Regex regex3 = new Regex(@"^\d\d\-[\w* [(\.]+");        //02-Hey hey Oh!
            Regex regex4 = new Regex(@"^\d\d\-\s[\w* [(\.]+");        //02- Hey hey Oh!
            Regex regex5 = new Regex(@"^\d\d\.[\w* [(\.]+");        //02.Hey hey Oh!

            Regex regex1a = new Regex(@"^\d\s[\w* [(\.]+");     //2 Hey hey Oh!
            Regex regex2b = new Regex(@"^\d\.\s[\w* [(\.]+");        //2. Hey hey Oh!
            Regex regex3c = new Regex(@"^\d\-[\w* [(\.]+");        //2-Hey hey Oh!
            Regex regex4d = new Regex(@"^\d\-\s[\w* [(\.]+");        //2- Hey hey Oh!
            Regex regex5e = new Regex(@"^\d\.[\w* [(\.]+");        //2.Hey hey Oh!

            string path = Directory.GetCurrentDirectory();
            foreach (string filePath in Directory.GetFiles(path, "*", SearchOption.AllDirectories))
            {
                string fileName = Path.GetFileName(filePath);
                string pathWithoutName = filePath.Replace(fileName, "");
                string fileNameRenamed = "";

                if (regex1.IsMatch(fileName))
                {
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3, fileName.Length - 3);
                }
                else if (regex2.IsMatch(fileName))
                {
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(4, fileName.Length - 4);
                }
                else if (regex3.IsMatch(fileName))
                {
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3, fileName.Length - 3);
                }
                else if (regex4.IsMatch(fileName))
                {
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(4, fileName.Length - 4);
                }
                else if (regex5.IsMatch(fileName))
                {
                    fileNameRenamed = fileName.Substring(0, 2) + " - " + fileName.Substring(3, fileName.Length - 3);
                }
                else if (regex1a.IsMatch(fileName))
                {
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2, fileName.Length - 2);
                }
                else if (regex2b.IsMatch(fileName))
                {
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(3, fileName.Length - 3);
                }
                else if (regex3c.IsMatch(fileName))
                {
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2, fileName.Length - 2);
                }
                else if (regex4d.IsMatch(fileName))
                {
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(3, fileName.Length - 3);
                }
                else if (regex5e.IsMatch(fileName))
                {
                    fileNameRenamed = "0" + fileName.Substring(0, 1) + " - " + fileName.Substring(2, fileName.Length - 2);
                }

                if (!String.IsNullOrEmpty(fileNameRenamed))
                {
                    string filePathRenamed = pathWithoutName + fileNameRenamed;
                    string oldFileNameWithPath = pathWithoutName + fileName;
                    try
                    {
                        File.Move(oldFileNameWithPath, filePathRenamed);
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
