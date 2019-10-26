using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PHMappingTool.Duplication.Interface
{
    interface IOtherObjDuplication
    {
        HashSet<LogMessageWithValidityInfo> CheckTagDuplication(XElement element, string duplicatedTag);
    }
}
