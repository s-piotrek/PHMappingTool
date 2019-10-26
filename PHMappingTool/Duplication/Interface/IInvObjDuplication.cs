using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace PHMappingTool.Duplication.Interface
{
    interface IInvObjDuplication
    {
        HashSet<LogMessageInfo> CheckTagDuplication(XElement element, string duplicatedTag);
    }
}
