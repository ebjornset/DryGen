namespace DryGen.Docs
{
    public static class ReplaceTokenExtensions
    {
        public static string AsCommandLineReplaceToken(this string replaceToken)
        {
            return $".!.!.replace-token-for-{replaceToken.ToLowerInvariant()}-commandline.!.!.";
        }

        public static string AsGeneratedRepresentationReplaceToken(this string replaceToken)
        {
            return $".!.!.replace-token-for-{replaceToken.ToLowerInvariant()}.!.!.";
        }

        public static string AsHtmlEscapedGeneratedRepresentationReplaceToken(this string replaceToken)
        {
            return $".!.!.replace-token-for-{replaceToken.ToLowerInvariant()}-html-escaped.!.!.";
        }
    }
}
