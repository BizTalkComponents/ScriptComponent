using Microsoft.BizTalk.Component.Interop;
using Microsoft.BizTalk.Streaming;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Xml;
using Microsoft.BizTalk.XPath;
using System.ComponentModel;
using BizTalkComponents.Utils;

namespace BizTalkComponents.PipelineComponents.ScriptComponent
{
    [System.Runtime.InteropServices.Guid("D54B204D-37D5-4E4F-B0EB-4610295BA57C")]
    [ComponentCategory(CategoryTypes.CATID_PipelineComponent)]
    [ComponentCategory(CategoryTypes.CATID_Any)]
    public partial class ScriptComponent : IBaseComponent, IPersistPropertyBag, IComponentUI, Microsoft.BizTalk.Component.Interop.IComponent
    {
        private const string SnippetPropertyName = "Snippet";

        [DisplayName("Snippet")]
        [Description("C# code to execute.")]
        public string Snippet { get; set; }

        public Microsoft.BizTalk.Message.Interop.IBaseMessage Execute(IPipelineContext pContext, Microsoft.BizTalk.Message.Interop.IBaseMessage pInMsg)
        {
            var data = pInMsg.BodyPart.GetOriginalDataStream();
            const int bufferSize = 0x280;
            const int thresholdSize = 0x100000;
            var rss = new ReadOnlySeekableStream(data, new VirtualStream(bufferSize, thresholdSize), bufferSize);
            pContext.ResourceTracker.AddResource(data);

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using Microsoft.BizTalk.Component.Interop;");
            sb.AppendLine("using Microsoft.BizTalk.Streaming;");
            sb.AppendLine("using Microsoft.BizTalk.Message.Interop;");
            sb.AppendLine("using System.Xml;");
            sb.AppendLine("using Microsoft.BizTalk.XPath;");

            sb.AppendLine();
            sb.AppendLine("namespace BizTalkComponents");
            sb.AppendLine("{");

            sb.AppendLine("      public class GenericHelper");
            sb.AppendLine("      {");

            sb.AppendLine("              public Func<IBaseMessageContext, ReadOnlySeekableStream, ReadOnlySeekableStream> Execute = " + Snippet);
            sb.AppendLine("      }");
            sb.AppendLine("}");

            var instance = GetScriptInstance(sb.ToString());

            data = instance.Execute.Invoke(pInMsg.Context, rss);

            pInMsg.BodyPart.Data = data;

            return pInMsg;
        }

        private dynamic GetScriptInstance(string script)
        {
            XPathReader reader = null;
            CSharpCodeProvider provider = new CSharpCodeProvider(new Dictionary<string, string>
              {
                 { "CompilerVersion", "v4.0" }
              });

            CompilerParameters parameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = true,
                WarningLevel = 3,
                CompilerOptions = "/optimize",
                TreatWarningsAsErrors = false,

            };

            Assembly a = Assembly.GetExecutingAssembly();

            foreach (var referencedAssembly in a.GetReferencedAssemblies())
            {
                var asm = Assembly.Load(referencedAssembly);
                parameters.ReferencedAssemblies.Add(asm.Location);
            }
            parameters.ReferencedAssemblies.Add("System.Xml.dll");

            CompilerResults results = provider.CompileAssemblyFromSource(parameters, script);
            if (results.Errors.Count != 0)
            {
                Console.WriteLine(results.Errors);
            }

            dynamic instance = results.CompiledAssembly.CreateInstance("BizTalkComponents.GenericHelper");

            return instance;
        }

        public void Load(IPropertyBag propertyBag, int errorLog)
        {
            Snippet = PropertyBagHelper.ReadPropertyBag(propertyBag, SnippetPropertyName, Snippet);
        }

        public void Save(IPropertyBag propertyBag, bool clearDirty, bool saveAllProperties)
        {
            PropertyBagHelper.WritePropertyBag(propertyBag, SnippetPropertyName, Snippet);
        }
    }
}
