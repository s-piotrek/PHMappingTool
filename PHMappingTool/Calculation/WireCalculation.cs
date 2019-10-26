using PHMappingTool.Duplication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Xml.XPath;

namespace PHMappingTool.Calculation
{
    class WireCalculation : OtherObjCalculation
    {
        private static XElement _wireDeviceList = new XElement("WiresList");
        public static List<CorrectDuplicatedTag> _correctDupTagNEO = new List<CorrectDuplicatedTag>();
        public static List<CorrectDuplicatedTag> _correctDupTagRATP = new List<CorrectDuplicatedTag>();

        public WireCalculation(IOtherObjDuplication otherObjDuplication) : base(otherObjDuplication) { }
        public static void CalculateContentFileForWire(XDocument contentFile, XMLDefContentFileName xMLDefContent)
        {
            if (contentFile == null)
                throw new ArgumentNullException(nameof(contentFile));

            var attrNameAttrValue = from elem in contentFile.Descendants("Wire")?.Elements("UserAttribute")
                                    where elem.Attribute("AttributeName")?.Value == "NeoWireTAG"
                                    select new Device
                                    {
                                        ratp = elem.Parent.Attribute("Tag")?.Value,
                                        tagNEO = elem.Attribute("AttributeValue")?.Value,
                                        contentFileName = xMLDefContent.contentFileName
                                    };

            lock (_wireDeviceList)
            {
                AddElements(attrNameAttrValue, "Wire", xMLDefContent.roots, _wireDeviceList);
            }
        }
        public static XElement GetElement() => _wireDeviceList;
        public List<string> GenerateErrorListAndCorrectDupTagList() =>
            base.GenerateErrorListAndCorrectDupTagList(_wireDeviceList, _correctDupTagNEO, _correctDupTagRATP);
        public static void AddCorrectTagsToElement()
        {
            foreach (var tag in _correctDupTagNEO)
            {
                _wireDeviceList.Add(new XElement("Wire",
                    new XAttribute("TagNEO", tag.tag),
                    new XAttribute("RATP", tag.correspondingTag),
                    new XAttribute("Validity", tag.validities)));
            }

            foreach (var tag in _correctDupTagRATP)
            {
                _wireDeviceList.Add(new XElement("Wire",
                    new XAttribute("TagNEO", tag.correspondingTag),
                    new XAttribute("RATP", tag.tag),
                    new XAttribute("Validity", tag.validities)));
            }
        }
    }
}
