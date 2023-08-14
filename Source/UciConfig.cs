namespace Greg
{
    internal static class UciConfig
    {
        public static string Name = "Greg";
        public static string Author = "Tomasz Jaworski";

        public static string Version => $"{_majorPatchNumber}.{_featureCountSiceLastMajorPatch}.{_commitsSinceLastFeature}";

        private static int _majorPatchNumber = 0;
        private static int _featureCountSiceLastMajorPatch = 3;
        private static int _commitsSinceLastFeature = 2;
    }
}
