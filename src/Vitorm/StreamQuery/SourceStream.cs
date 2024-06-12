using System;

namespace Vitorm.StreamQuery
{
    public class SourceStream : IStream
    {
        public SourceStream(object source, string alias)
        {
            this.source = source;
            this.alias = alias;
        }
        public string alias { get; private set; }
        private object source;

        public int? hashCode
        {
            get => source?.GetHashCode();
        }

        public object GetSource() => source;

        public Type GetEntityType() => source?.GetType().GenericTypeArguments[0];
    }
}
