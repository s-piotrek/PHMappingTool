using PHMappingTool.Duplication;
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
    class ConnectiveDeviceCalculation : InvariantCalculation
    {
        private static XElement _cdevDeviceList = new XElement("ConnectiveDeviceList");

        public ConnectiveDeviceCalculation(IInvObjDuplication invObjDuplication) : base(invObjDuplication)
        {
        }
        public static void CalculateContentFileForConnectiveDevice(XDocument contentFile, XMLDefContentFileName xMLDefContent)
        {
            if (contentFile == null)
                throw new ArgumentNullException(nameof(contentFile));

            var attrNameAttrValue = from elem in contentFile.Descendants("ConnectiveDevice")?.Elements("UserAttribute")
                                    where elem.Attribute("AttributeName")?.Value == "TagNEO" &&
                                    (elem.Parent.Attribute("Type")?.Value == "Connector" ||
                                    elem.Parent.Attribute("Type")?.Value == "Shell" ||
                                    elem.Parent.Attribute("Type")?.Value == "ShellModule" ||
                                    elem.Parent.Attribute("Type")?.Value == "TerminalBlock" ||
                                    elem.Parent.Attribute("Type")?.Value == "GroundBlock" ||
                                    elem.Parent.Attribute("Type")?.Value == "Splice" ||
                                    elem.Parent.Attribute("Type")?.Value == "TerminalTrack")
                                    select new Device
                                    {
                                        ratp = elem.Parent.Attribute("Tag")?.Value,
                                        tagNEO = elem.Attribute("AttributeValue")?.Value,
                                        contentFileName = xMLDefContent.contentFileName
                                    };

            lock (_cdevDeviceList)
            {
                AddElements(attrNameAttrValue, "ConnectiveDevice", _cdevDeviceList);
            }
        }
        public static XElement GetElement() => _cdevDeviceList;
        public List<string> GetErrorList() => base.GetErrorList(_cdevDeviceList);
    }
}
