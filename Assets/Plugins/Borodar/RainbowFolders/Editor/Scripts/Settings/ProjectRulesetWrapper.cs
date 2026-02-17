namespace Borodar.RainbowFolders
{
    internal sealed class ProjectRulesetWrapper
    {
        #pragma warning disable 414
        public int Version = ProjectRuleset.VERSION;
        #pragma warning restore 414

        public string RulesetJson;
    }
}