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
    class EquipmentCalculation : InvariantCalculation
    {
        private static XElement _eqDeviceList = new XElement("DeviceList");
        public EquipmentCalculation(IInvObjDuplication invObjDuplication) : base(invObjDuplication)
        {
        }
        public static void CalculateContentFileForEquipment(XDocument contentFile, XMLDefContentFileName xMLDefContent)
        {
            if (contentFile == null)
                throw new ArgumentNullException(nameof(contentFile));

            var attrNameAttrValue = from elem in contentFile.Descendants("ConnectiveDevice")?.Elements("UserAttribute")
                                    where elem.Attribute("AttributeName")?.Value == "TagNEO" && elem.Parent.Attribute("Type")?.Value == "Equipment"
                                    select new Device
                                    {
                                        ratp = elem.Parent.Attribute("Tag")?.Value,
                                        tagNEO = elem.Attribute("AttributeValue")?.Value,
                                        contentFileName = xMLDefContent.contentFileName
                                    };

            lock (_eqDeviceList)
            { 
                AddElements(attrNameAttrValue, "Device", _eqDeviceList);
            }
        }
        public static XElement GetElement() => _eqDeviceList;
        public List<string> GetErrorList() => base.GetErrorList(_eqDeviceList);
    }
}
