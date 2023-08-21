namespace Izumi
{
    internal static class UciConfig
    {
        public static string Header = $"Konnyichiwaa, watashi wa'm chesies enginye {Name} v{Version}. Copywight (c) 2023 {Author}.\n" +
                                       "Souwce code c-can be found at https://github.com/TomaszJaworski777/Izumi-chan. \n\n" +
                                       "Use of thiies souwce code is guvwnyed by an MIT~stywe\n" +
                                       "wicense that c-can be found in the x3 LICENSE fiwe ow (・`w´・) at\n" +
                                       "https://opensource.org/licenses/MIT. \n";

        public const string Name = "Izumi-chan";
        public const string Author = "Tomasz Jaworski";

        public static string Version => $"{_majorPatchNumber}.{_featureCountSiceLastMajorPatch}.{_commitsSinceLastFeature}";

        private const int _majorPatchNumber = 0;
        private const int _featureCountSiceLastMajorPatch = 7;
        private const int _commitsSinceLastFeature = 0;
    }
}