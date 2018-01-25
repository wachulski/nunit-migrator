namespace NUnit.Migrator
{
    /// <summary>
    /// Contains common texts to be displayed/stored
    /// </summary>
    public static class Texts
    {
        private const string CodeActionPrefix = "[NUnit migration]";

        public static string CodeActionTitle(string titleBody) => $"{CodeActionPrefix} {titleBody}";
    }
}