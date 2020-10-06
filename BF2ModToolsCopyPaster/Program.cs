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
        static string mshDepsTXTPath, odfDepsTXTPath, fxDepsTXTPath, worldDepsTXTPath;

        static List<HashSet<string>> GetAllDependencies(List<string> files)
        {
            HashSet<string> mshDependencies = new HashSet<string>();
            HashSet<string> odfDependencies = new HashSet<string>();
            HashSet<string> fxDependencies  = new HashSet<string>();

            HashSet<string> visitedFiles    = new HashSet<string>();

            Stack<string> dependencyStack   = new Stack<string>();
            foreach (string file in files) { dependencyStack.Push(file); }

            while (dependencyStack.Count > 0)
            {
                string filePath = dependencyStack.Pop();

                if (filePath.EndsWith(".odf"))
                {
                    ODFHandler handle = new ODFHandler(filePath);
                    if (handle.isValid)
                    {
                        odfDependencies.Add(filePath);

                        foreach (string msh_link in handle.mshDependencies)
                        {
                            string possibleMSHFile = MSHHandler.SearchFromFolder(filePath, msh_link);
                            if (possibleMSHFile != null && !visitedFiles.Contains(possibleMSHFile))
                            {
                                dependencyStack.Push(possibleMSHFile);
                            }
                        }

                        foreach (string odf_link in handle.odfDependencies)
                        {
                            string odfFile = Path.Combine(handle.folderPath, odf_link + ".odf");
                            if (File.Exists(odfFile) && !visitedFiles.Contains(odfFile))
                            {
                                dependencyStack.Push(odfFile);
                            }
                        }

                        foreach (string fx_link in handle.fxDependencies)
                        {
                            string path1 = Path.Combine(handle.folderPath, "..", "EFFECTS", fx_link + ".fx");

                            if (File.Exists(path1) && !visitedFiles.Contains(path1))
                            {
                                Console.WriteLine("Pushing fx path: " + path1);
                                dependencyStack.Push(path1);
                            }
                        }
                    }
                }
                else if (filePath.EndsWith(".msh"))
                {
                    MSHHandler mshHandle = new MSHHandler(filePath);
                    mshDependencies.Add(filePath);
                    mshDependencies.UnionWith(mshHandle.FindTextureFiles());

                    string optionPath = mshHandle.filePath + ".option";
                    if (File.Exists(optionPath))
                    {
                        mshDependencies.Add(optionPath);
                    }
                }

                else if (filePath.EndsWith(".fx"))
                {
                    FXHandler fxHandler = new FXHandler(filePath);
                    HashSet<string> texturePaths = fxHandler.FindTextureFiles();

                    fxDependencies.Add(filePath);
                    fxDependencies.UnionWith(texturePaths); 
                }

                else
                {
                    MessageBox.Show(filePath + " is not an ODF or MSH file...", "Warning");
                }

                visitedFiles.Add(filePath);
            }

            HashSet<string>[] depsArr = { odfDependencies, mshDependencies, fxDependencies };
            List<HashSet<string>> deps = new List<HashSet<string>>(depsArr);
            
            Console.WriteLine("\nPaths to be copied: ");
            for (int i = 0; i < 3; i++)
            {
                if (i == 0) Console.WriteLine("\nODF dependencies: ");
                if (i == 1) Console.WriteLine("\nMSH dependencies: ");
                if (i == 2) Console.WriteLine("\nFX dependencies: ");

                HashSet<string> depSet = deps[i];

                foreach (string copied in depSet)
                {
                    Console.WriteLine(copied);
                }
            }

            return deps;
        }


        static void PasteWithDeps(string path)
        {
            string clickedDir = path;

            if (!File.GetAttributes(clickedDir).HasFlag(FileAttributes.Directory))
            {
                clickedDir = Path.GetDirectoryName(path);
            }

            string mshDestDir, odfDestDir, fxDestDir;
            mshDestDir = odfDestDir = fxDestDir = clickedDir;

            //ODF
            if (Directory.Exists(Path.Combine(clickedDir,"odf")))
            {
                odfDestDir = Path.Combine(clickedDir,"odf");
            }

            //MSH
            if (Directory.Exists(Path.Combine(clickedDir,"msh")))
            {
                mshDestDir = Path.Combine(clickedDir,"msh");
            }

            //FX
            if (Directory.Exists(Path.Combine(clickedDir, "effects")))
            {
                fxDestDir = Path.Combine(clickedDir, "effects");
            }
            if (Directory.Exists(Path.Combine(clickedDir, "Effects")))
            {
                fxDestDir = Path.Combine(clickedDir, "Effects");
            }
            if (Directory.Exists(Path.Combine(clickedDir, "EFFECTS")))
            {
                fxDestDir = Path.Combine(clickedDir, "EFFECTS");
            }


            foreach (string line in File.ReadAllLines(odfDepsTXTPath))
            {
                string odfDestFile = Path.Combine(odfDestDir, Path.GetFileName(line));
                if (!File.Exists(odfDestFile)) File.Copy(line, odfDestFile);
            }

            foreach (string line in File.ReadAllLines(mshDepsTXTPath))
            {
                string mshDestFile = Path.Combine(mshDestDir, Path.GetFileName(line));
                if (!File.Exists(mshDestFile)) File.Copy(line, mshDestFile);
            }

            foreach (string line in File.ReadAllLines(fxDepsTXTPath))
            {
                string fxDestFile = Path.Combine(fxDestDir, Path.GetFileName(line));
                if (!File.Exists(fxDestFile)) File.Copy(line, fxDestFile);
            }
        }



        static void Main(string[] args)
        {
            if (args.Length < 1)
            {
                Console.WriteLine("Please provide path(s) to ODF/MSH file(s)...");
                return;
            }

            foreach (string arg in args)
            {
                Console.WriteLine("Arg: " + arg);
            }

            string exePath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase);

            Console.WriteLine("exe path: " + exePath);
            string exeDir = exePath.StartsWith("file:") ? exePath.Substring(5) : exePath;
            odfDepsTXTPath = Path.Combine(exeDir, "odfDeps.txt");
            mshDepsTXTPath = Path.Combine(exeDir, "mshDeps.txt");
            fxDepsTXTPath = Path.Combine(exeDir, "fxDeps.txt");

            List<string> pathsInput = new List<string>();
            pathsInput.Add(args[1]);

            if (args.Length >= 2)
            {
                if (args[args.Length - 1].Contains("c"))
                {
                    List<HashSet<string>> dependencies = GetAllDependencies(pathsInput);

                    Console.WriteLine("FINISHED SEARCH");

                    System.IO.File.WriteAllLines(odfDepsTXTPath, dependencies[0].ToArray());
                    System.IO.File.WriteAllLines(mshDepsTXTPath, dependencies[1].ToArray());
                    System.IO.File.WriteAllLines(fxDepsTXTPath,  dependencies[2].ToArray());
                }

                else
                {
                    PasteWithDeps(pathsInput[0]);
                }
            }
        }
    }
}
