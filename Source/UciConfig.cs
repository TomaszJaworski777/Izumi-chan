namespace Greg
{
    internal static class UciConfig
    {
        public static string Header = $"Chess engine \"Greg\" v{Version}. Copyright (c) 2023 Tomasz Jaworski.\n" +
                                      $"Source code can be found at https://github.com/TomaszJaworski777/Greg. \n\n" +
                                      $"Use of this source code is governed by an MIT-style\nlicense that can be found in the LICENSE file or at\nhttps://opensource.org/licenses/MIT. \n";

        public const string Name = "Greg";
        public const string Author = "Tomasz Jaworski";

        public static string Version => $"{_majorPatchNumber}.{_featureCountSiceLastMajorPatch}.{_commitsSinceLastFeature}";

        private const int _majorPatchNumber = 0;
        private const int _featureCountSiceLastMajorPatch = 3;
        private const int _commitsSinceLastFeature = 11;
    }
}
