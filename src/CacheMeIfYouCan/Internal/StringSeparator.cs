using System;

namespace CacheMeIfYouCan.Internal
{
    internal ref struct StringSeparator
    {
        private readonly ReadOnlySpan<char> _separator;
        private ReadOnlySpan<char> _remaining;
        private bool _complete;

        public StringSeparator(string input, string separator)
        {
            if (input == null)
                throw new ArgumentNullException(nameof(input));
            
            if (string.IsNullOrEmpty(separator))
                throw new ArgumentException(nameof(separator));
            
            _separator = separator.AsSpan();
            _remaining = input.AsSpan();
            _complete = false;
        }

        public bool TryGetNext(out string value)
        {
            if (_complete)
            {
                value = null;
                return false;
            }

            var index = _remaining.IndexOf(_separator, StringComparison.Ordinal);

            if (index < 0)
            {
                value = _remaining.ToString();
                _complete = true;
            }
            else
            {
                value = _remaining.Slice(0, index).ToString();
                _remaining = _remaining.Slice(index + _separator.Length);
            }

            return true;
        }
    }
}