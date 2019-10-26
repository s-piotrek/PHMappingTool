using PHMappingTool.Duplication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PHMappingTool.Duplication
{
    struct InvDuplicatedTag
    {
        public string duplicatedTag;
        public string correspondingTag;
        public string contentFileName;
    }

    struct LogMessageInfo
    {
        public string tag;
        public HashSet<string> setTags;
        public HashSet<string> setContentFileNames;

        public LogMessageInfo(string tag)
        {
            this.tag = tag;
            this.setTags = new HashSet<string>();
            this.setContentFileNames = new HashSet<string>();
        }
    }
    class LogMessageInfoComparer : IEqualityComparer<LogMessageInfo>
    {
        public bool Equals(LogMessageInfo n1, LogMessageInfo n2) => n1.tag == n2.tag;
        public int GetHashCode(LogMessageInfo n) => n.tag.GetHashCode();
    }
    class InvObjDuplication : IInvObjDuplication
    {
        public HashSet<LogMessageInfo> CheckTagDuplication(XElement element, string duplicatedTag)
        {
            if (duplicatedTag != "TagNEO" && duplicatedTag != "RATP")
                throw new ArgumentException($"{duplicatedTag} is different than 'TagNEO' and 'RATP'");

            HashSet<LogMessageInfo> setLogMessages = new HashSet<LogMessageInfo>(new LogMessageInfoComparer());
            foreach (var dp in GetDuplication(element, duplicatedTag))
            {
                LogMessageInfo logInfo = new LogMessageInfo(dp.duplicatedTag);
                if (setLogMessages.TryGetValue(logInfo, out LogMessageInfo existingTag))
                {
                    if (existingTag.setTags.Contains(dp.correspondingTag))
                        existingTag.setContentFileNames.Add(dp.contentFileName);
                    else
                    {
                        existingTag.setTags.Add(dp.correspondingTag);
                        existingTag.setContentFileNames.Add(dp.contentFileName);
                    }
                }
                else
                {
                    logInfo.setTags.Add(dp.correspondingTag);
                    logInfo.setContentFileNames.Add(dp.contentFileName);
                    setLogMessages.Add(logInfo);
                }
            }
            return setLogMessages;
        }

        private IEnumerable<InvDuplicatedTag> GetDuplication(XElement element, string groupingAttributeName)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if(groupingAttributeName != "TagNEO" && groupingAttributeName != "RATP")
                throw new ArgumentException($"{groupingAttributeName} is different than 'TagNEO' and 'RATP'");

            IEnumerable <InvDuplicatedTag> xx = element.Elements()?.
                GroupBy(g => g.Attribute(groupingAttributeName).Value)?.
                Where(grp => grp.Count() > 1)?.
                SelectMany(m => m)?.
                Select(x => new InvDuplicatedTag
                {
                    duplicatedTag = x.Attribute(groupingAttributeName).Value ?? string.Empty,
                    correspondingTag = x.Attribute(groupingAttributeName == "RATP" ? "TagNEO" : "RATP")?.Value ?? string.Empty,
                    contentFileName = x.Attribute("PHContentFileName")?.Value
                });
            return xx;
        }
    }
}
