using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace BF2ModToolsCopyPaster
{
    class ODFHandler
    {
        private static Regex tstRx = new Regex("^(?<property>\\w+)\\s+=\\s+\"(?<value>.*)\"\\s*$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private Dictionary<string, string> properties;
        private string path;


        public static ODFHandler GetODFHandler(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            Dictionary<string, string> propertiesMap = new Dictionary<string, string>();

            foreach (string line in System.IO.File.ReadAllLines(path))
            {
                //Console.WriteLine("Line: {0}", line);

                foreach (Match match in tstRx.Matches(line))
                { 
                    GroupCollection groups = match.Groups;

                    string propertyName = groups["property"].Value;
                    string propertyValue = groups["value"].Value;

                    //Console.WriteLine("\tMATCH:   Property = {0}, Value = {1}",
                                        //propertyName, propertyValue);

                    propertiesMap[propertyName] = propertyValue;
                }
            }

            return new ODFHandler(propertiesMap, path);
        }
           

        private ODFHandler(Dictionary<string,string> propMap, string filePath)
        {
            properties = propMap;
            path = filePath;
        }


            


        public string GetPropertyValue(string propertyName)
        {
            try
            {
                return properties[propertyName];
            } catch (Exception e)
            {
                return null;
            }
        }
    }
}
