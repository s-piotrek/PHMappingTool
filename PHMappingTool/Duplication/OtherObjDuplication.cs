using PHMappingTool.Duplication.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace PHMappingTool.Duplication
{
    struct OtherDuplicatedTag
    {
        public string duplicatedTag;
        public string correspondingTag;
        public string contentFileName;
        public HashSet<string> setValidities;
    }

    struct CTagValidityFile
    {
        public string correspondingTag;
        public HashSet<string> setValidites;
        public HashSet<string> setContentFileNames;

        public CTagValidityFile(string correspondingTag)
        {
            this.correspondingTag = correspondingTag;
            this.setValidites = new HashSet<string>();
            this.setContentFileNames = new HashSet<string>();
        }
    }

    struct LogMessageWithValidityInfo
    {
        public string tag;
        public HashSet<CTagValidityFile> cTagValidityFiles;

        public LogMessageWithValidityInfo(string tag)
        {
            this.tag = tag;
            this.cTagValidityFiles = new HashSet<CTagValidityFile>(new CTagValidityFile_Comparer());
        }
    }

    class CTagValidityFile_Comparer : IEqualityComparer<CTagValidityFile>
    {
        public bool Equals(CTagValidityFile n1, CTagValidityFile n2) => n1.correspondingTag == n2.correspondingTag;
        public int GetHashCode(CTagValidityFile n) => n.correspondingTag.GetHashCode();
    }

    class LogMessageWithValidityInfo_Comparer : IEqualityComparer<LogMessageWithValidityInfo>
    {
        public bool Equals(LogMessageWithValidityInfo n1, LogMessageWithValidityInfo n2) => n1.tag == n2.tag;
        public int GetHashCode(LogMessageWithValidityInfo n) => n.tag.GetHashCode();
    }

    class OtherObjDuplication : IOtherObjDuplication
    {
        public HashSet<LogMessageWithValidityInfo> CheckTagDuplication(XElement element, string duplicatedTag)
        {
            if (duplicatedTag != "TagNEO" && duplicatedTag != "RATP")
                throw new ArgumentException($"{duplicatedTag} is different than 'TagNEO' and 'RATP'");

            var setLogMessages = new HashSet<LogMessageWithValidityInfo>(new LogMessageWithValidityInfo_Comparer());
            foreach (var dp in GetDuplication(element, duplicatedTag))
            {
                var logInfo = new LogMessageWithValidityInfo(dp.duplicatedTag);

                if (setLogMessages.TryGetValue(logInfo, out LogMessageWithValidityInfo existingTag))
                {
                    var cTag = new CTagValidityFile(dp.correspondingTag);
                    if (existingTag.cTagValidityFiles.TryGetValue(cTag, out CTagValidityFile existingCorrespondingTag))
                    {
                        existingCorrespondingTag.setValidites.UnionWith(dp.setValidities);
                        existingCorrespondingTag.setContentFileNames.Add(dp.contentFileName);
                    }
                    else
                    {
                        cTag.setValidites.UnionWith(dp.setValidities);
                        cTag.setContentFileNames.Add(dp.contentFileName);
                        existingTag.cTagValidityFiles.Add(cTag);
                    }
                }
                else
                {
                    CTagValidityFile cTagValidityFile = new CTagValidityFile(dp.correspondingTag);
                    cTagValidityFile.setValidites.UnionWith(dp.setValidities);
                    cTagValidityFile.setContentFileNames.Add(dp.contentFileName);

                    logInfo.cTagValidityFiles.Add(cTagValidityFile);
                    setLogMessages.Add(logInfo);
                }
            }
            return setLogMessages;
        }

        public IEnumerable<OtherDuplicatedTag> GetDuplication(XElement element, string groupingAttributeName)
        {
            if (element == null)
                throw new ArgumentNullException(nameof(element));

            if (groupingAttributeName != "TagNEO" && groupingAttributeName != "RATP")
                throw new ArgumentException($"{groupingAttributeName} is different than 'TagNEO' and 'RATP'");

            IEnumerable<OtherDuplicatedTag> xx = element.Elements()?.
                GroupBy(g => g.Attribute(groupingAttributeName)?.Value)?.
                Where(grp => grp.Count() > 1)?.
                SelectMany(m => m)?.
                Select(x => new OtherDuplicatedTag
                {
                    duplicatedTag = x.Attribute(groupingAttributeName).Value ?? string.Empty,
                    correspondingTag = x.Attribute(groupingAttributeName == "RATP" ? "TagNEO" : "RATP")?.Value ?? string.Empty,
                    setValidities = x.Attribute("Validity").Value.Split(',').ToHashSet(),
                    contentFileName = x.Attribute("PHContentFileName")?.Value 
                });

            return xx;
        }
    }
}
