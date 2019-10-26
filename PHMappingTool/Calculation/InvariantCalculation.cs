using PHMappingTool.Duplication;
using PHMappingTool.Duplication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PHMappingTool.Calculation
{
    class InvariantCalculation
    {
        private IInvObjDuplication _invObjDuplication;
        public InvariantCalculation(IInvObjDuplication invObjDuplication)
        {
            _invObjDuplication = invObjDuplication;
        }
        public static void AddElements(IEnumerable<Device> attrNameAttrValue, string elementName, XElement elementList)
        {
            if (elementList == null)
                throw new ArgumentNullException(nameof(elementList));

            foreach (var attr in attrNameAttrValue)
            {
                elementList.Add(new XElement(elementName,
                        new XAttribute("TagNEO", attr.tagNEO),
                        new XAttribute("RATP", attr.ratp),
                        new XAttribute("PHContentFileName", attr.contentFileName)));
            }
        }

        public List<string> GetErrorList(XElement element)
        {
            var tagNEODuplication = _invObjDuplication?.CheckTagDuplication(element, "TagNEO");
            var tagNEOErrorMessages = RemoveAndRaportDuplication(tagNEODuplication, element, "TagNEO");
            var tagRATPDuplication = _invObjDuplication?.CheckTagDuplication(element, "RATP");
            var tagRATPErrorMessages = RemoveAndRaportDuplication(tagRATPDuplication, element, "RATP");

            return tagNEOErrorMessages.Union(tagRATPErrorMessages).ToList();
        }

        private List<string> RemoveAndRaportDuplication(HashSet<LogMessageInfo> logMessages, XElement element, string tag)
        {
            List<string> errorMessages = new List<string>();
            foreach (var log in logMessages)
            {
                if (log.setTags.Count > 1)
                {
                    element.Elements().Where(e => e.Attribute(tag).Value == log.tag)?.Remove();
                    errorMessages.Add($"{tag} '{log.tag}' has names '{string.Join(", ", log.setTags)}' in file(s) '{string.Join(", ", log.setContentFileNames)}'.");
                }
            }
            return errorMessages;
        }

        public static void RemoveDuplication(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var nodes = new List<XNode>();
            nodes = element.Elements().Distinct(new XNodeEqualityComparer()).ToList();
            element.Elements().Remove();
            element.Add(nodes);
        }

        public static void RemovePHContentFileNameAttribute(XElement element)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            foreach (var el in element.Elements())
                el.Attribute("PHContentFileName")?.Remove();
        }
    }
}
