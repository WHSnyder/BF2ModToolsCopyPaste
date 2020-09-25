using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Collections.Generic;

using LibSWBF2.MSH;
using LibSWBF2.MSH.Chunks;


namespace BF2ModToolsCopyPaster
{
    class Program
    {
        //private static string 

        static void Main(string[] args)
        {
            List<string> fileDeps = new List<string>();

            if (args.Length < 1)
            {
                Console.WriteLine("Please provide path(s) to ODF file...");
                return;
            }

            string filePath = args[0];

            ODFHandler handle = ODFHandler.GetODFHandler(filePath);

            string geomName = handle.GetPropertyValue("GeometryName");

            if (geomName == null)
            {
                return;
            }


            string mshFileName = geomName.Contains(".msh") ? geomName : geomName + ".msh";

            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(filePath)));

            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);
            string exeDir = exePath.StartsWith("file:\\") ? exePath.Substring(6) : exePath;
            Console.WriteLine("Exe dir: {0}", exeDir);

            string worldFolderPath = di.Parent.FullName;
            string odfFolderPath = di.FullName;
            string mshFilePath = worldFolderPath + "\\msh\\" + mshFileName;

            Console.WriteLine("ODF folder: {0}", odfFolderPath);
            Console.WriteLine("World folder: {0}", worldFolderPath);

            if (File.Exists(mshFilePath))
            {
                Console.WriteLine("Found msh at {0}", mshFilePath);
                fileDeps.Add(mshFilePath);

                //Find texture dependencies
                MSH mesh = MSH.LoadFromFile(mshFilePath);

                foreach(MATD mat in mesh.Materials)
                {
                    foreach (string texName in mat.Textures)
                    {
                        if (texName == null || !texName.EndsWith(".tga")) continue;

                        string texturePath = Path.GetDirectoryName(mshFilePath) + "\\" + texName;

                        if (!fileDeps.Contains(texturePath))
                        {
                            fileDeps.Add(texturePath);
                        }
                    }
                }
            }

            string depsTXTPath = exeDir + "\\deps.txt";

            Console.WriteLine("Will write deps to {0}", depsTXTPath);

            System.IO.File.WriteAllLines(depsTXTPath, fileDeps.ToArray());
            //System.IO.File.WriteAllLines("deps.txt", fileDeps.ToArray());


            //MSH mesh = MSH.LoadFromFile();



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
