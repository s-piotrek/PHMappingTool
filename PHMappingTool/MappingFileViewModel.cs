using GalaSoft.MvvmLight.Command;
using PHMappingTool.Calculation;
using PHMappingTool.Duplication;
using PHMappingTool.Log;
using PHMappingTool.Properties;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.XPath;

namespace PHMappingTool
{
    struct XMLDefContentFileName
    {
        public string defFileName;
        public string contentFileName;
        public string roots;
    }
    class XMLDefContentFileNameComparer : IEqualityComparer<XMLDefContentFileName>
    {
        public bool Equals(XMLDefContentFileName x1, XMLDefContentFileName x2) => x1.contentFileName == x2.contentFileName;
        public int GetHashCode(XMLDefContentFileName x) => x.contentFileName.GetHashCode();
    }
    struct Device
    {
        public string ratp;
        public string tagNEO;
        public string contentFileName;
    }

    class MappingFileViewModel
    {
        private static Object _xmlPH = new Object();
        private static Object _xmlValidation = new Object();
        private List<string> _errorMessages = new List<string>();
        private List<string> _errorXMLValidation = new List<string>();
        public void CalculateMappingFile(string directoryPathXMLFiles)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            HashSet<XMLDefContentFileName> xMLDefContentFileNames = FindPHPairFiles(directoryPathXMLFiles);
            //       MessageBox.Show("FindPHPairFiles time is " + watch.ElapsedMilliseconds.ToString());

            var watch2 = System.Diagnostics.Stopwatch.StartNew();
            CreateElementsInMappingFileXMLAsync(directoryPathXMLFiles, xMLDefContentFileNames);
            //     MessageBox.Show("CreateElementsInMappingFileXMLAsync time is " + watch2.ElapsedMilliseconds.ToString());

            Loger.log(_errorXMLValidation, "LogXMLValidation");
            Loger.log(_errorMessages.Union(GetErrorListForXMLObjects()), "LogPHMappingError");
            CreateXMLMappingFileDoc()?.Save("mapingfile_test.xml");
        }

        private static XDocument CreateXMLMappingFileDoc()
        {
            var mappingFile = new XDocument();

            mappingFile.Add(new XElement("MAPPING"));
            RemovePHContentFileNameAttribute();
            RemoveDuplication();
            UpdateDuplicatedTagForOtherObject();

            mappingFile.Root.Add(ConnectiveDeviceCalculation.GetElement());
            mappingFile.Root.Add(EquipmentCalculation.GetElement());
            mappingFile.Root.Add(NetCalculation.GetElement());
            mappingFile.Root.Add(WireCalculation.GetElement());

            return mappingFile;
        }

        private static void UpdateDuplicatedTagForOtherObject()
        {
            NetCalculation.GetElement().Elements().GroupBy(g => g.Attribute("TagNEO").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();
            NetCalculation.GetElement().Elements().GroupBy(g => g.Attribute("RATP").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();
            WireCalculation.GetElement().Elements().GroupBy(g => g.Attribute("TagNEO").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();
            WireCalculation.GetElement().Elements().GroupBy(g => g.Attribute("RATP").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();

            NetCalculation.AddCorrectTagsToElement();
            WireCalculation.AddCorrectTagsToElement();
            RemoveDuplication();
        }

        private static void RemoveDuplication()
        {
            ConnectiveDeviceCalculation.RemoveDuplication(ConnectiveDeviceCalculation.GetElement());
            EquipmentCalculation.RemoveDuplication(EquipmentCalculation.GetElement());
            NetCalculation.RemoveDuplication(NetCalculation.GetElement());
            WireCalculation.RemoveDuplication(WireCalculation.GetElement());

        }
        private static void RemovePHContentFileNameAttribute()
        {
            ConnectiveDeviceCalculation.RemovePHContentFileNameAttribute(ConnectiveDeviceCalculation.GetElement());
            EquipmentCalculation.RemovePHContentFileNameAttribute(EquipmentCalculation.GetElement());
            NetCalculation.RemovePHContentFileNameAttribute(NetCalculation.GetElement());
            WireCalculation.RemovePHContentFileNameAttribute(WireCalculation.GetElement());
        }

        private static List<string> GetErrorListForXMLObjects()
        {
            var eqErrorList = new EquipmentCalculation(new InvObjDuplication()).GetErrorList();
            var cdevErrorList = new ConnectiveDeviceCalculation(new InvObjDuplication()).GetErrorList();
            var netErrorList = new NetCalculation(new OtherObjDuplication()).GenerateErrorListAndCorrectDupTagList();
            var wireErrorList = new WireCalculation(new OtherObjDuplication()).GenerateErrorListAndCorrectDupTagList();

            return eqErrorList.Union(cdevErrorList).Union(netErrorList).Union(wireErrorList).ToList();
        }

        private void CreateElementsInMappingFileXMLAsync(string directoryPathXMLFiles, HashSet<XMLDefContentFileName> xMLDefContentFileNames)
        {
            CleanXMLElements();
            XmlSchemaSet schema = GetXmlSchemaSet();

            List<Task> listOfTasks = new List<Task>();
            foreach (var defFileContentFile in xMLDefContentFileNames)
                listOfTasks.Add(Task.Run(() => CreateElementsInMappingFile(directoryPathXMLFiles, defFileContentFile, schema)));

            Task.WaitAll(listOfTasks.ToArray());
        }

        private static void CleanXMLElements()
        {
            ConnectiveDeviceCalculation.GetElement()?.RemoveAll();
            EquipmentCalculation.GetElement()?.RemoveAll();
            NetCalculation.GetElement()?.RemoveAll();
            WireCalculation.GetElement()?.RemoveAll();

            NetCalculation._correctDupTagNEO.Clear();
            NetCalculation._correctDupTagRATP.Clear();

            WireCalculation._correctDupTagNEO.Clear();
            WireCalculation._correctDupTagRATP.Clear();
        }

        private static XmlSchemaSet GetXmlSchemaSet()
        {
            XmlSchemaSet schema = new XmlSchemaSet();
            string runningPath = AppDomain.CurrentDomain.BaseDirectory;
            schema.Add("", @"runningPath + /../../../" + "Resources/IGE-XAO Physical Harness 6.19 complete.xsd");
            //schema.ValidationEventHandler += new ValidationEventHandler(ValidationInfo);
            return schema;
        }
        //private static void ValidationInfo(object sender, ValidationEventArgs args)
        //{
        //    if (args.Severity == XmlSeverityType.Warning)
        //        Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
        //    else
        //        Console.WriteLine("\tValidation error: " + args.Message);

        //    //msg += e.Message + Environment.NewLine;

        //}

        private void CreateElementsInMappingFile(string directoryPathXMLFiles, XMLDefContentFileName xMLDefContent, XmlSchemaSet schema)
        {
            var contentFile = XDocument.Load(directoryPathXMLFiles + "\\" + xMLDefContent.contentFileName);

            if (xMLDefContent.contentFileName == null)
                throw new ArgumentNullException(nameof(xMLDefContent.contentFileName));

            if (ValidateXMLContentFile(contentFile, xMLDefContent.contentFileName, schema))
            {
                ConnectiveDeviceCalculation.CalculateContentFileForConnectiveDevice(contentFile, xMLDefContent);
                EquipmentCalculation.CalculateContentFileForEquipment(contentFile, xMLDefContent);
                NetCalculation.CalculateContentFileForNet(contentFile, xMLDefContent);
                WireCalculation.CalculateContentFileForWire(contentFile, xMLDefContent);
            }
        }

        private bool ValidateXMLContentFile(XDocument contentFile, string contentFileName, XmlSchemaSet schema)
        {
            if (contentFile == null)
                throw new ArgumentNullException(nameof(contentFile));
            if (schema == null)
                throw new ArgumentNullException(nameof(schema));

            string validationError = string.Empty;
            bool success = true;
            lock (_xmlValidation)
            {
                contentFile.Validate(schema, (o, e) =>
                {
                    _errorXMLValidation.Add($"XML file '{contentFileName}' validation failed: {e.Message}");
                    success = false;
                });
            }
            return success;
        }

        private string CalculateDefinitionFile(XDocument defFile) =>
            defFile.XPathSelectElement("IGE-XAO_DIAGRAM_DEFINITION/Diagram/Validity/ValidityFixed/NamesList")?.Attribute("Roots")?.Value;

        private HashSet<XMLDefContentFileName> FindPHPairFiles(string directoryPathXMLFiles)
        {
            DirectoryInfo taskDirectory = new DirectoryInfo(directoryPathXMLFiles);
            IEnumerable<FileInfo> xmlFiles = taskDirectory.GetFiles("*.xml", SearchOption.TopDirectoryOnly);

            HashSet<XMLDefContentFileName> setXMLDefContent = new HashSet<XMLDefContentFileName>(new XMLDefContentFileNameComparer());
            foreach (var fileName in xmlFiles)
            {
                if (fileName == null)
                {
                    _errorMessages.Add("File name is null for one of the xml files");
                    continue;
                }

                if (!setXMLDefContent.Contains(new XMLDefContentFileName { contentFileName = fileName.Name }))
                {
                    XMLDefContentFileName xmlDefContent = new XMLDefContentFileName();
                    if (FindDefAndContentFileName(directoryPathXMLFiles, fileName.Name, out xmlDefContent))
                        setXMLDefContent.Add(xmlDefContent);
                }
            }
            return setXMLDefContent;
        }

        private HashSet<XMLDefContentFileName> FindPHPairFilesAsync(string directoryPathXMLFiles)
        {
            DirectoryInfo taskDirectory = new DirectoryInfo(directoryPathXMLFiles);
            IEnumerable<FileInfo> xmlFiles = taskDirectory.GetFiles("*.xml", SearchOption.TopDirectoryOnly);

            return GetPHDefContentFilesAsync(xmlFiles, directoryPathXMLFiles);
        }

        private HashSet<XMLDefContentFileName> GetPHDefContentFilesAsync(IEnumerable<FileInfo> xmlFiles, string directoryPathXMLFiles)
        {
            List<Task> tasks = new List<Task>();
            HashSet<XMLDefContentFileName> setXMLDefContent = new HashSet<XMLDefContentFileName>(new XMLDefContentFileNameComparer());
            foreach (var fileName in xmlFiles)
            {
                if (fileName == null)
                {
                    _errorMessages.Add("File name is null for one of the xml files");
                    continue;
                }

                XMLDefContentFileName xmlDefContent = new XMLDefContentFileName();
                var result = Task.Run(() => FindDefAndContentFileName(directoryPathXMLFiles, fileName.Name, out xmlDefContent));
                tasks.Add(result);
                if (result.Result)
                {
                    lock (_xmlPH)
                    {
                        setXMLDefContent.Add(xmlDefContent);
                    }
                }
            }
            Task.WaitAll(tasks.ToArray());
            return setXMLDefContent;
        }

        private bool FindDefAndContentFileName(string dirPath, string xmlFileName, out XMLDefContentFileName xMLDefContent)
        {
            xMLDefContent = default;
            var xmlFile = XDocument.Load(dirPath + "\\" + xmlFileName);
            IEnumerable<XMLDefContentFileName> elems = new List<XMLDefContentFileName>();

            elems = from el in xmlFile.Descendants("ValidityFixed")?.Elements("NamesList")
                    select new XMLDefContentFileName
                    {
                        roots = el.Attribute("Roots")?.Value.Replace(" ", string.Empty),
                        defFileName = xmlFileName,
                        contentFileName = xmlFile.XPathSelectElement("IGE-XAO_DIAGRAM_DEFINITION/Diagram/ContentFile/Content")?.
                        Attribute("FileName")?.Value.Replace(@".\", string.Empty)
                    };

            if (elems?.Count() > 0)
            {
                xMLDefContent = elems.First();
                if (xMLDefContent.contentFileName == null)
                {
                    _errorMessages.Add($"Content file does not exist for definition file {xMLDefContent.defFileName}");
                    return false;
                }
                if (elems?.Count() > 1)
                {
                    _errorMessages.Add($"For def file {xMLDefContent.defFileName} cannot be more than 1 element 'NameList'");
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}
