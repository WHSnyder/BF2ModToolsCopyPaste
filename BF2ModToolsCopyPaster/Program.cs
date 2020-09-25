using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Collections.Generic;

using LibSWBF2.MSH;
using LibSWBF2.MSH.Chunks;
using System.Linq;

namespace BF2ModToolsCopyPaster
{
    class Program
    {

        static void Main(string[] args)
        {
            List<string> fileDeps = new List<string>();

            if (args.Length < 2)
            {
                Console.WriteLine("Please provide path(s) to ODF file...");
                return;
            }

            string filePath = args[0];

            ODFHandler handle = ODFHandler.GetODFHandler(filePath);
           
            MSHHandler mshHandle = null;
            if (handle != null)
            {
                mshHandle = MSHHandler.SearchFromFolder(filePath, handle.GetPropertyValue("GeometryName"));
            }  

            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string exeDir = exePath.StartsWith("file:\\") ? exePath.Substring(6) : exePath;
            string depsTXTPath = exeDir + "\\deps.txt";

            Console.WriteLine("Will write deps to {0}", depsTXTPath);

            fileDeps.AddRange(mshHandle.FindTextureFiles());



            List<string> noDups = fileDeps.Distinct().ToList();
            System.IO.File.WriteAllLines(depsTXTPath, noDups.ToArray());
            Console.WriteLine("\nPaths to be copied: ");
            foreach (string copied in noDups)
            {
                Console.WriteLine(copied);
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
