using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BizTalkComponents.PipelineComponents.ScriptComponent
{
    public partial class ScriptComponent

    {
        public IntPtr Icon { get { return IntPtr.Zero; } }

        public System.Collections.IEnumerator Validate(object projectSystem)
        {
            throw new NotImplementedException();
        }

        public void GetClassID(out Guid classID)
        {
            classID = new Guid("748D25A3-A15D-48D8-8411-76BD351A4EDC");
        }

        public void InitNew()
        {
        }

        public string Name { get { return "ScriptComponent"; } }
        public string Version { get { return "1.0"; } }
        public string Description { get { return "Execute a C# snippet on a biztalk message or context."; } }
    }
}
