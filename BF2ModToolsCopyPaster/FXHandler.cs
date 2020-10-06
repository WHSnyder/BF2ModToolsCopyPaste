using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BF2ModToolsCopyPaster
{
    class FXHandler
    {
        private static Regex texturePropRx = new Regex("^\\s*Texture\\(\"(?<texName>\\w+)\"\\);\\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string filePath, folderPath;
        public bool isValid = false;


        public FXHandler(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("{0} doesn't exist", path);
                return;
            }
                       
            isValid = true;
            filePath = path;
            folderPath = Path.GetDirectoryName(path);
        }



        public HashSet<string> FindTextureFiles()
        {
            HashSet<string> texDeps = new HashSet<string>();

            foreach (string line in System.IO.File.ReadAllLines(filePath))
            {
                foreach (Match match in texturePropRx.Matches(line))
                {
                    GroupCollection groups = match.Groups;

                    string texName = groups["texName"].Value;

                    Console.WriteLine("Found FX texture dep: " + texName);

                    string texturePath = folderPath + "\\" + texName + ".tga";

                    if (File.Exists(texturePath))
                    {
                        texDeps.Add(texturePath);
                        string optionFilePath = texturePath + ".option";
                        if (File.Exists(optionFilePath)) texDeps.Add(optionFilePath);
                    }
                }
            }

            return texDeps;
        }
    }
}
