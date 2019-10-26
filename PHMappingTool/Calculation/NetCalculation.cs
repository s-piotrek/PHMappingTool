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
    class NetCalculation : OtherObjCalculation
    {
        private static XElement _netDeviceList = new XElement("NetList");
        public static List<CorrectDuplicatedTag> _correctDupTagNEO = new List<CorrectDuplicatedTag>();
        public static List<CorrectDuplicatedTag> _correctDupTagRATP = new List<CorrectDuplicatedTag>();

        public NetCalculation(IOtherObjDuplication otherObjDuplication) : base(otherObjDuplication) { }
        public static void CalculateContentFileForNet(XDocument contentFile, XMLDefContentFileName xMLDefContent)
        {
            if (contentFile == null)
                throw new ArgumentNullException(nameof(contentFile));

            var attrNameAttrValue = from elem in contentFile.Descendants("Wire")?.Elements("UserAttribute")
                                    where elem.Attribute("AttributeName")?.Value == "NeoNetTAG"
                                    select new Device
                                    {
                                        ratp = elem.Parent.Attribute("Tag")?.Value ?? string.Empty,
                                        tagNEO = elem.Attribute("AttributeValue")?.Value ?? string.Empty,
                                        contentFileName = xMLDefContent.contentFileName
                                    };

            lock (_netDeviceList)
            {
                AddElements(attrNameAttrValue, "Net", xMLDefContent.roots, _netDeviceList);
            }
        }
        public static XElement GetElement() => _netDeviceList;
        public List<string> GenerateErrorListAndCorrectDupTagList() =>
            base.GenerateErrorListAndCorrectDupTagList(_netDeviceList, _correctDupTagNEO, _correctDupTagRATP);
        public static void AddCorrectTagsToElement()
        {
            foreach (var tag in _correctDupTagNEO)
            {
                _netDeviceList.Add(new XElement("Net",
                    new XAttribute("TagNEO", tag.tag),
                    new XAttribute("RATP", tag.correspondingTag),
                    new XAttribute("Validity", tag.validities)));
            }

            foreach (var tag in _correctDupTagRATP)
            {
                _netDeviceList.Add(new XElement("Net",
                    new XAttribute("TagNEO", tag.correspondingTag),
                    new XAttribute("RATP", tag.tag),
                    new XAttribute("Validity", tag.validities)));
            }
        }
    }
}
