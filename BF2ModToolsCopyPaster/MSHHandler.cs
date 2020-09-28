using System;
using System.IO;
using System.Reflection;
using System.Net;
using System.Collections.Generic;

using LibSWBF2.MSH;
using LibSWBF2.MSH.Chunks;


namespace BF2ModToolsCopyPaster
{
    class MSHHandler
    {
        private MSH mesh;
        public string filePath, folderPath;

        public static MSHHandler SearchFromFolder(string directory, string mshName)
        {
            if (directory == null || mshName == null)
            {
                return null;
            }

            string mshFileName = mshName.EndsWith(".msh") ? mshName : mshName + ".msh";

            DirectoryInfo di = new DirectoryInfo(Path.GetDirectoryName(Path.GetFullPath(directory)));

            string possibleMSHPath = di.FullName + "\\" + mshFileName;
            if (File.Exists(possibleMSHPath))
            {
                return new MSHHandler(possibleMSHPath);
            }

            string worldFolderPath = di.Parent.FullName;
            possibleMSHPath = worldFolderPath + "\\msh\\" + mshFileName;
            if (File.Exists(possibleMSHPath))
            {
                return new MSHHandler(possibleMSHPath);
            }

            possibleMSHPath = worldFolderPath + "\\msh\\PC\\" + mshFileName;
            if (File.Exists(possibleMSHPath))
            {
                return new MSHHandler(possibleMSHPath);
            }

            return null;
        }


        public MSHHandler(string pathIn)
        {
            mesh = MSH.LoadFromFile(pathIn);
            filePath = pathIn;
            folderPath = Path.GetDirectoryName(filePath);
        }

        public HashSet<string> FindTextureFiles()
        {
            HashSet<string> fileDeps = new HashSet<string>();

            foreach (MATD mat in mesh.Materials)
            {
                foreach (string texName in mat.Textures)
                {
                    if (texName == null || !texName.EndsWith(".tga")) continue;

                    string texturePath = folderPath + "\\" + texName;

                    if (!File.Exists(texturePath))
                    {
                        texturePath = folderPath + "\\PC\\" + texName;
                        if (!File.Exists(texturePath)) continue;
                    }

                    fileDeps.Add(texturePath);
                    string optionFilePath = texturePath + ".option";
                    if (File.Exists(optionFilePath)) fileDeps.Add(optionFilePath);
                }
            }

            return fileDeps;
        }

    }
}
