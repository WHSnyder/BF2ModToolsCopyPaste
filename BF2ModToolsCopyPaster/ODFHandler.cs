using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BF2ModToolsCopyPaster
{
    class ODFHandler
    {
        private static Regex propertyValueRx = new Regex("^(?<property>\\w+)\\s+=\\s+\"(?<value>.*)\"\\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public string filePath, folderPath;

        private static HashSet<string> MSH_LINKED_PROPERTIES;
        private static HashSet<string> ODF_LINKED_PROPERTIES;
        private static HashSet<string> FX_LINKED_PROPERTIES;


        public HashSet<string> mshDependencies;
        public HashSet<string> odfDependencies;
        public HashSet<string> fxDependencies;

        public bool isValid = false;
                    

        static ODFHandler()
        {
            string[] MSH_LINKED_PROPERTIES_ARR = { "GeometryName", "DestroyedGeometryName", "ChunkGeometryName" };
            string[] ODF_LINKED_PROPERTIES_ARR = { "ExplosionName", "AttachOdf", "WeaponName", "ExplosionExpire", "OrdnanceName" };
            string[] FX_LINKED_PROPERTIES_ARR  = { "ChunkTerrainEffect", "DamageEffect", "Effect" };


            MSH_LINKED_PROPERTIES = new HashSet<string>();
            MSH_LINKED_PROPERTIES.UnionWith(MSH_LINKED_PROPERTIES_ARR);

            ODF_LINKED_PROPERTIES = new HashSet<string>();
            ODF_LINKED_PROPERTIES.UnionWith(ODF_LINKED_PROPERTIES_ARR);

            FX_LINKED_PROPERTIES = new HashSet<string>();
            FX_LINKED_PROPERTIES.UnionWith(FX_LINKED_PROPERTIES_ARR);
        }



        public ODFHandler(string path)
        {
            if (!File.Exists(path))
            {
                Console.WriteLine("{0} doesn't exist", path);
                return;
            }

            mshDependencies = new HashSet<string>();
            odfDependencies = new HashSet<string>();
            fxDependencies =  new HashSet<string>();

            foreach (string line in System.IO.File.ReadAllLines(path))
            {
                foreach (Match match in propertyValueRx.Matches(line))
                {
                    GroupCollection groups = match.Groups;

                    string propertyName  = groups["property"].Value;
                    string propertyValue = groups["value"].Value;

                    if (MSH_LINKED_PROPERTIES.Contains(propertyName))
                    {
                        mshDependencies.Add(propertyValue);
                    }
                    else if (ODF_LINKED_PROPERTIES.Contains(propertyName))
                    {
                        odfDependencies.Add(propertyValue);
                    }
                    else if (FX_LINKED_PROPERTIES.Contains(propertyName))
                    {
                        fxDependencies.Add(propertyValue);
                    }
                }
            }

            isValid = true;
            filePath = path;
            folderPath = Path.GetDirectoryName(path);
        }
    }
}
