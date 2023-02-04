public static class TestGamePath
{
    public static DirectoryInfo Directory =>
        new(Environment.GetEnvironmentVariable("OPENGTA2_PATH", EnvironmentVariableTarget.User)!);

    public static Stream OpenFile(string path) => File.OpenRead(Path.Combine(Directory.FullName, path));
}