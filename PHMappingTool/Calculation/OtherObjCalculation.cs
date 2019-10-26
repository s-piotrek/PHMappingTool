using PHMappingTool.Duplication;
using PHMappingTool.Duplication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace PHMappingTool.Calculation
{
    struct CorrectDuplicatedTag
    {
        public string tag;
        public string correspondingTag;
        public string validities;
    }
    class OtherObjCalculation
    {
        private IOtherObjDuplication _otherObjDuplication;
        private static List<CorrectDuplicatedTag> _correctDupTagNEO = new List<CorrectDuplicatedTag>();
        private static List<CorrectDuplicatedTag> _correctDupTagRATP = new List<CorrectDuplicatedTag>();

        public OtherObjCalculation(IOtherObjDuplication otherObjDuplication) => _otherObjDuplication = otherObjDuplication;

        public static void AddElements(IEnumerable<Device> attrNameAttrValue, string elementName, string roots, XElement elementList)
        {
            if (elementList == null)
                throw new ArgumentNullException(nameof(elementList));

            try
            { 
                foreach (var attr in attrNameAttrValue)
                {
                    elementList.Add(new XElement(elementName,
                            new XAttribute("TagNEO", attr.tagNEO),
                            new XAttribute("RATP", attr.ratp),
                            new XAttribute("Validity", roots),
                            new XAttribute("PHContentFileName", attr.contentFileName)));
                }
            }
            catch(Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }

        public List<string> GenerateErrorListAndCorrectDupTagList(XElement element, List<CorrectDuplicatedTag> correctDupTagNEO, List<CorrectDuplicatedTag> correctDupTagRATP)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            var tagNEODuplication = _otherObjDuplication?.CheckTagDuplication(element, "TagNEO");
            var tagRATPDuplication = _otherObjDuplication?.CheckTagDuplication(element, "RATP");

            var tagNEOErrorMessages = GenerateErrorListAndCorrectDupTagList(tagNEODuplication, "TagNEO", correctDupTagNEO);
            var tagRATPErrorMessages = GenerateErrorListAndCorrectDupTagList(tagRATPDuplication, "RATP", correctDupTagRATP);

            //element.Elements().GroupBy(g => g.Attribute("TagNEO").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();
            //element.Elements().GroupBy(g => g.Attribute("RATP").Value).Where(x => x.Count() > 1).SelectMany(m => m).Remove();

            return tagNEOErrorMessages.Union(tagRATPErrorMessages).ToList();
        }

        private List<string> GenerateErrorListAndCorrectDupTagList(HashSet<LogMessageWithValidityInfo> logMessages, string tag, List<CorrectDuplicatedTag> corrDupTag)
        {
            List<string> errorMessages = new List<string>();

            foreach (var log in logMessages)
            {
                if(log.cTagValidityFiles?.Count > 1)
                {
                    var setValidities = new List<HashSet<string>>();
                    foreach(var cTag in log.cTagValidityFiles)
                        setValidities.Add(cTag.setValidites);

                    var commonValidities = FindCommonValidities(setValidities);
                    var commonValiditesForCorrespondingTag = new List<string>();

                    foreach (var duplicatedTag in log.cTagValidityFiles)
                    {
                        commonValiditesForCorrespondingTag = duplicatedTag.setValidites.Intersect(commonValidities).ToList();
                        var dif = duplicatedTag.setValidites.Except(commonValiditesForCorrespondingTag);
                        if (dif.Any())
                        {
                            corrDupTag.Add(new CorrectDuplicatedTag
                            {
                                tag = log.tag,
                                correspondingTag = duplicatedTag.correspondingTag,
                                validities = string.Join(", ", dif)
                            });
                        }
                        if (commonValiditesForCorrespondingTag.Any())
                        {
                            errorMessages.Add($"{tag} '{log.tag}' has name {duplicatedTag.correspondingTag} for root(s) '{string.Join(", ", commonValiditesForCorrespondingTag)}' " +
                                $"in files '{string.Join(", ", duplicatedTag.setContentFileNames)}'.");
                        }
                    }
                }
                else if(log.cTagValidityFiles?.Count == 1)
                {
                    corrDupTag.Add(new CorrectDuplicatedTag
                    {
                        tag = log.tag,
                        correspondingTag = log.cTagValidityFiles.First().correspondingTag,
                        validities = string.Join(", ", log.cTagValidityFiles.First().setValidites)
                    });
                }
            }
            return errorMessages;
        }

        private HashSet<string> FindCommonValidities(List<HashSet<string>> setValidities)
        {
            var commonValidities = new HashSet<string>();
            for(int idx = 0; idx < setValidities.Count; ++idx)
            {
                for (int idy = 0; idy < setValidities.Count; ++idy)
                {
                    if (idx == idy)
                        continue;

                    commonValidities.UnionWith(setValidities[idx].Intersect(setValidities[idy]).ToHashSet());
                }
            }
            return commonValidities;
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

        //public void AddCorrectTagsToElement(XElement element, string nodeName, List<CorrectDuplicatedTag> correctDupTagNEO, List<CorrectDuplicatedTag> correctDupTagRATP)
        //{
        //    foreach (var tag in correctDupTagNEO)
        //    {
        //        element.Add(new XElement(nodeName,
        //            new XAttribute("TagNEO", tag.tag),
        //            new XAttribute("RATP", tag.correspondingTag),
        //            new XAttribute("Validity", tag.validities)));
        //    }

        //    foreach (var tag in correctDupTagRATP)
        //    {
        //        element.Add(new XElement(nodeName,
        //            new XAttribute("RATP", tag.tag),
        //            new XAttribute("TagNEO", tag.correspondingTag),
        //            new XAttribute("Validity", tag.validities)));
        //    }
        //}
    }
}
