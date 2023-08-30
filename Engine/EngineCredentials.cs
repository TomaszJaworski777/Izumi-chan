namespace Engine
{
    public static class EngineCredentials
    {
        public static string Header => $"Konnyichiwaa, watashi wa'm chesies enginye {Name} v{Version}. Copywight (c) 2023 {Author}.\n" +
                                       "Souwce code c-can be found at https://github.com/TomaszJaworski777/Izumi-chan. \n\n" +
                                       "Use of thiies souwce code is guvwnyed by an MIT~stywe\n" +
                                       "wicense that c-can be found in the x3 LICENSE fiwe ow (・`w´・) at\n" +
                                       "https://opensource.org/licenses/MIT. \n";

        public static string FullName => $"{Name} v{Version}";

        public const string Name = "Izumi-chan";
        public const string Author = "Tomasz Jaworski";

        public static string Version => $"{MajorPatchNumber}.{FeatureCountSiceLastMajorPatch}.{CommitsSinceLastFeature}";

        private const int MajorPatchNumber = 0;
        private const int FeatureCountSiceLastMajorPatch = 4;
        private const int CommitsSinceLastFeature = 0;
    }
}