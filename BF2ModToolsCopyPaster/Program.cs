using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Collections.Generic;

using LibSWBF2.MSH;
using LibSWBF2.MSH.Chunks;
using System.Linq;

using System.Windows.Forms;

namespace BF2ModToolsCopyPaster
{
    class Program
    {
        static string depsTXTPath;

        static void CopyWithDeps(string path_)
        {
            HashSet<string> fileDeps = new HashSet<string>();


            string filePath = path_;
            if (!filePath.EndsWith(".odf") && !filePath.EndsWith(".msh"))
            {
                MessageBox.Show(filePath + " is not an ODF file...", "Warning");
            }

            ODFHandler handle = ODFHandler.GetODFHandler(filePath);

            MSHHandler mshHandle = null;
            if (handle != null)
            {
                fileDeps.Add(filePath);
                mshHandle = MSHHandler.SearchFromFolder(filePath, handle.GetPropertyValue("GeometryName"));

                if (mshHandle == null)
                {
                    return;
                }
            }

            fileDeps.UnionWith(mshHandle.FindTextureFiles());
            fileDeps.Add(mshHandle.filePath);

            
            System.IO.File.WriteAllLines(depsTXTPath, fileDeps.ToArray());

            Console.WriteLine("\nPaths to be copied: ");
            foreach (string copied in fileDeps)
            {
                Console.WriteLine(copied);
            }
        }


        static void PasteWithDeps(string path)
        {
            string mshDestDir, odfDestDir, clickedDir = path;

            if (!File.GetAttributes(clickedDir).HasFlag(FileAttributes.Directory))
            {
                clickedDir = Path.GetDirectoryName(path);
            }

            odfDestDir = clickedDir;
            if (Directory.Exists(Path.Combine(clickedDir,"odf")))
            {
                odfDestDir = Path.Combine(clickedDir,"odf");
            }

            mshDestDir = clickedDir;
            if (Directory.Exists(Path.Combine(clickedDir,"msh")))
            {
                mshDestDir = Path.Combine(clickedDir,"msh");
            }

            Console.WriteLine("Clicked dir: {0}, MSH destination dir: {1}", clickedDir, mshDestDir);

            foreach (string line in File.ReadAllLines(depsTXTPath))
            {
                string fileName = Path.GetFileName(line);

                if (fileName.EndsWith(".odf"))
                {
                    File.Copy(line, Path.Combine(odfDestDir, fileName));
                }
                else
                {
                    File.Copy(line, Path.Combine(mshDestDir, fileName));
                }
            }
        }



        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide path(s) to ODF file...");
                return;
            }

            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string exeDir = exePath.StartsWith("file:\\") ? exePath.Substring(6) : exePath;
            depsTXTPath = exeDir + "\\deps.txt";

            if (args.Length == 2)
            {
                if (args[1].Contains("c"))
                {
                    CopyWithDeps(args[0]);
                }
                else
                {
                    PasteWithDeps(args[0]);
                }
            }

            //copy
            //get linked odfs
            //get odf model dependencies
            //for each model, get texture deps
            //write all paths to temp file

            //paste
            //if odf and msh subfolders exist  
            //for each file
            //check if file exists if not paste
        }
    }
}
