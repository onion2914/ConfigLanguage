using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace VSIXProject1
{
    [Export(typeof(ITaggerProvider))]
    [Name("ConfigBaseTag")]
    [ContentType("config")]
    [TagType(typeof(ConfigTokenTag))]
    internal sealed class ConfigTokenTagProvider : ITaggerProvider
    {

        
        public ITagger<T> CreateTagger<T>(ITextBuffer buffer) where T : ITag
        {
            return new ConfigTokenTagger(buffer) as ITagger<T>;
        }
        
    }

    public class ConfigTokenTag : ITag
    {
        public ConfigTokenTypes type { get; private set; }

        public ConfigTokenTag(ConfigTokenTypes type)
        {
            this.type = type;
        }
    }

    internal sealed class ConfigTokenTagger : ITagger<ConfigTokenTag>
    {

        ITextBuffer _buffer;
        IDictionary<string, ConfigTokenTypes> _configTypes;
        HashSet<string> _configKeywordType;



        internal ConfigTokenTagger(ITextBuffer buffer)
        {
            _buffer = buffer;
            _configTypes = new Dictionary<string, ConfigTokenTypes>();
            _configTypes["keyword"] = ConfigTokenTypes.KeyWord;
            _configTypes["="] = ConfigTokenTypes.Operator;

            //KeyWord集合
            _configKeywordType = new HashSet<string> { "type", "struct", "gui", "const", "#define", "code", "rule", "func", "private", "type", "struct", "dictionary", "const" };

        }
        
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged
        {
            add { }
            remove { }
        }

        public IEnumerable<ITagSpan<ConfigTokenTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            foreach (SnapshotSpan curSpan in spans) {
                
                ITextSnapshotLine containingLine = curSpan.Start.GetContainingLine();
                string lineText = containingLine.GetText();
                string tokenText = "";
                string commentText = "";
                int curLoc = containingLine.Start.Position;

                //まずはコメント//を探す
                int commentIndex = lineText.IndexOf("//");
                if (commentIndex > -1) {
                    commentText = lineText.Substring(commentIndex);
                    if(commentIndex > 0) {
                        tokenText = lineText.Substring(0, commentIndex);
                    }
                } else {
                    tokenText = lineText;
                }

                //トークンリスト生成
                string[] tokens = tokenText.ToLower().Split(null);   //nullを与えると空白文字で分割

                foreach (string configToken in tokens) {
                    if (_configKeywordType.Contains(configToken)) {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, configToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                                                                  new ConfigTokenTag(_configTypes["keyword"]));
                    }
                    else if (_configTypes.ContainsKey(configToken)) {
                        var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc, configToken.Length));
                        if (tokenSpan.IntersectsWith(curSpan))
                            yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                                                                  new ConfigTokenTag(_configTypes[configToken]));
                    }

                    //add an extra char location because of the space
                    curLoc += configToken.Length + 1;
                }


                //コメントを分類
                if (!string.IsNullOrEmpty(commentText)) {
                    var tokenSpan = new SnapshotSpan(curSpan.Snapshot, new Span(curLoc - 1, commentText.Length));
                    yield return new TagSpan<ConfigTokenTag>(tokenSpan,
                                                                  new ConfigTokenTag(ConfigTokenTypes.Comment));

                    curLoc += commentText.Length;
                }
            }

        }

    }
}
