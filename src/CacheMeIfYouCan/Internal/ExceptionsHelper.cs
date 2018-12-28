using System;

namespace CacheMeIfYouCan.Internal
{
    public static class ExceptionsHelper
    {
        public static bool TryGetInnerExceptionOfType<T>(Exception ex, out T innerException) where T : Exception
        {
            innerException = null;
            
            var current = ex.InnerException;

            while (current != null)
            {
                if (current is T match)
                {
                    innerException = match;
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }
    }
}