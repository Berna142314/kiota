using System;
using System.IO;
using System.Linq;
using Kiota.Builder.Extensions;
using Xunit;

namespace Kiota.Builder.Writers.Go.Tests {
    public class CodePropertyWriterTests: IDisposable {
        private const string DefaultPath = "./";
        private const string DefaultName = "name";
        private readonly StringWriter tw;
        private readonly LanguageWriter writer;
        private readonly CodeProperty property;
        private readonly CodeClass parentClass;
        private const string PropertyName = "propertyName";
        private const string TypeName = "Somecustomtype";
        public CodePropertyWriterTests() {
            writer = LanguageWriter.GetLanguageWriter(GenerationLanguage.Go, DefaultPath, DefaultName);
            tw = new StringWriter();
            writer.SetTextWriter(tw);
            var root = CodeNamespace.InitRootNamespace();
            parentClass = new CodeClass {
                Name = "parentClass"
            };
            root.AddClass(parentClass);
            property = new CodeProperty {
                Name = PropertyName,
            };
            property.Type = new CodeType {
                Name = TypeName
            };
            parentClass.AddProperty(property);
        }
        public void Dispose() {
            tw?.Dispose();
            GC.SuppressFinalize(this);
        }
        [Fact]
        public void WritesRequestBuilder() {
            property.PropertyKind = CodePropertyKind.RequestBuilder;
            Assert.Throws<InvalidOperationException>(() => writer.Write(property));
        }
        [Fact]
        public void WritesCustomProperty() {
            property.PropertyKind = CodePropertyKind.Custom;
            writer.Write(property);
            var result = tw.ToString();
            Assert.Contains($"{PropertyName.ToFirstCharacterUpperCase()} *{TypeName}", result);
        }
        [Fact(Skip = "flag enum support needs to be added in Go")]
        public void WritesFlagEnums() {
            property.PropertyKind = CodePropertyKind.Custom;
            property.Type = new CodeType {
                Name = "customEnum",
            };
            (property.Type as CodeType).TypeDefinition = new CodeEnum {
                Name = "customEnumType",
                Flags = true,
            };
            writer.Write(property);
            var result = tw.ToString();
            Assert.Contains("EnumSet", result);
        }
        [Fact]
        public void WritesNonNull() {
            property.PropertyKind = CodePropertyKind.Custom;
            (property.Type as CodeType).IsNullable = false;
            writer.Write(property);
            var result = tw.ToString();
            Assert.DoesNotContain("*", result);
        }
    }
}
