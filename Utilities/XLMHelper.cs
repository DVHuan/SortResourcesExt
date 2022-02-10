using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Utilities
{
    public class XLMHelper
    {
        /// <summary>
     /// Sort element by attribute "name" (not recursively).
     /// </summary>
     /// <param name="element"></param>
        public static void SortXML(XElement element)
        {
            // Hàm chỉ sort các node "data", attribute "name" tăng dần.
            const string sortedNode = "data";
            const string sortedAttr = "name";

            IEnumerable<XNode> nodesToBePreserved = element.Nodes().Where(P => P.GetType() != typeof(XElement));
            var dataNodes = element.Elements()
                .Where(n => n.Name == sortedNode)
                .OrderBy(x => (string)x.Attribute(sortedAttr));

            var otherNodes = element.Elements().Where(n => n.Name != sortedNode);

            element.ReplaceNodes(nodesToBePreserved.Concat(otherNodes.Concat(dataNodes)));
        }
    }
}
