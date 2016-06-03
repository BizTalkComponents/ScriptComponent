using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Winterdom.BizTalk.PipelineTesting;
using BizTalkComponents.PipelineComponents.ScriptComponent;
using System.Xml;
using Microsoft.BizTalk.XPath;
using System.Text;

namespace BizTalkComponents.PipelineComponents.Test.UnitTests
{
    [TestClass]
    public class ScriptComponentTest
    {
        [TestMethod]
        public void TestSetProperty()
        {
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();
            var scriptComponent = new ScriptComponent.ScriptComponent();
            scriptComponent.Snippet = "(ctx, msg) => {ctx.Promote(\"Name\",\"Namespace\", \"TEst\"); return msg;};";
            pipeline.AddComponent(scriptComponent, PipelineStage.Decode);
            var msg = MessageHelper.CreateFromString("<testmessage1>testmsg</testmessage1>");
            var result = pipeline.Execute(msg);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("TEst", result[0].Context.Read("Name", "Namespace"));
        }

        [TestMethod]
        public void TestSetPropertyFromXpath()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("(ctx, msg) => {");
            sb.AppendLine("XmlTextReader xmlTextReader = new XmlTextReader(msg);");
            sb.AppendLine("XPathCollection xPathCollection = new XPathCollection();");
            sb.AppendLine("XPathReader xPathReader = new XPathReader(xmlTextReader, xPathCollection);");
            sb.AppendLine("xPathCollection.Add(\"/*[local-name() = 'testmessage1']\");");
            sb.AppendLine("string value = string.Empty;");
            sb.AppendLine("while (xPathReader.ReadUntilMatch())");
            sb.AppendLine("{");
            sb.AppendLine("if (xPathReader.Match(0))");
            sb.AppendLine(" {");
            sb.AppendLine(" if (xPathReader.NodeType == XmlNodeType.Attribute)");
            sb.AppendLine("{");
            sb.AppendLine("value = xPathReader.GetAttribute(xPathReader.Name);");
            sb.AppendLine("}");
            sb.AppendLine("else");
            sb.AppendLine(" {");
            sb.AppendLine(" value = xPathReader.ReadString();");
            sb.AppendLine("}");

            sb.AppendLine("ctx.Promote(\"Name2\",\"Namespace\", value);");

            sb.AppendLine("break;");
            sb.AppendLine("}");
            sb.AppendLine(" }");

            sb.AppendLine("            if (string.IsNullOrEmpty(value))");
            sb.AppendLine("{");
            sb.AppendLine("throw new InvalidOperationException(\"The specified XPath did not exist or contained an empty value.\");");
            sb.AppendLine("}");

            sb.AppendLine(" msg.Position = 0;");

            sb.AppendLine("return msg;");
            sb.AppendLine("};");
           
            var pipeline = PipelineFactory.CreateEmptyReceivePipeline();

            var scriptComponent = new ScriptComponent.ScriptComponent();
            scriptComponent.Snippet = sb.ToString();
            var msg = MessageHelper.CreateFromString("<testmessage1>testmsg</testmessage1>");
            pipeline.AddComponent(scriptComponent, PipelineStage.ResolveParty);
            var result = pipeline.Execute(msg);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual("testmsg", result[0].Context.Read("Name2", "Namespace"));
            
        }
    }
}