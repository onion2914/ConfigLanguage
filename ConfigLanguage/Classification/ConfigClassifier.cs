using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VSIXProject1.Classification
{
    /// <summary>
    /// Classifier provider. It adds the classifier to the set of classifiers.
    /// </summary>
    [Export(typeof(ITaggerProvider))]
    //[ContentType("text")] // This classifier applies to all text files.
    [ContentType("config")] // This classifier applies to all text files.
    [TagType(typeof(ClassificationTag))]
    internal class ConfigClassifierProvider : ITaggerProvider
    {
        [Export]
        [Name("config")]
        [BaseDefinition("csharp")]
        internal static ContentTypeDefinition ConfigContentType = null;

        [Export]
        [FileExtension(".cfg")]
        [ContentType("config")]
        internal static FileExtensionToContentTypeDefinition ConfigFileType = null;

        [Import]
        internal IClassificationTypeRegistryService ClassificationTypeRegistry = null;

        [Import]
        internal IBufferTagAggregatorFactoryService aggregatorFactory = null;

        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {

            ITagAggregator<ConfigTokenTag> configTagAggregator =
                                            aggregatorFactory.CreateTagAggregator<ConfigTokenTag>(buffer);

            return new ConfigClassifier(buffer, configTagAggregator, ClassificationTypeRegistry) as ITagger<T>;
        }
    }



    /// <summary>
    /// Classifier that classifies all text as an instance of the "ConfigClassifier" classification type.
    /// </summary>
    internal class ConfigClassifier : ITagger<ClassificationTag>
    {
        ITextBuffer _buffer;
        ITagAggregator<ConfigTokenTag> _aggregator;
        IDictionary<ConfigTokenTypes, IClassificationType> _configTypes;

        /// <summary>
        /// Construct the classifier and define search tokens
        /// </summary>
        internal ConfigClassifier(ITextBuffer buffer,
                               ITagAggregator<ConfigTokenTag> tagAggregator,
                               IClassificationTypeRegistryService typeService)
        {
            _buffer = buffer;
            _aggregator = tagAggregator;
            _configTypes = new Dictionary<ConfigTokenTypes, IClassificationType>();
            _configTypes[ConfigTokenTypes.KeyWord] = typeService.GetClassificationType("ConfigKeyWord");
            _configTypes[ConfigTokenTypes.Operator] = typeService.GetClassificationType("ConfigOperator");
            _configTypes[ConfigTokenTypes.Comment] = typeService.GetClassificationType("ConfigComment");
        }
        

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        /// <summary>
        /// Search the given span for any instances of classified tags
        /// </summary>
        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (var tagSpan in _aggregator.GetTags(spans)) {
                var tagSpans = tagSpan.Span.GetSpans(spans[0].Snapshot);
                yield return
                    new TagSpan<ClassificationTag>(tagSpans[0],
                                                   new ClassificationTag(_configTypes[tagSpan.Tag.type]));
            }
        }
    }
}
