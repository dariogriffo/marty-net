namespace Marty.Net.Internal;

using System.Text;

internal static class StringExtensions
{
    internal static byte[] ToUtf8Bytes(this string s)
    {
        return Encoding.UTF8.GetBytes(s);
    }
}
