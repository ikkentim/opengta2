using System;
using System.IO;

namespace OpenGta2.Client;

public static class TestGamePath
{
    public static DirectoryInfo Directory => new(Environment.GetEnvironmentVariable("OPENGTA2_PATH", EnvironmentVariableTarget.User)!);

    public static Stream OpenFile(string path)
    {
        return File.OpenRead(Path.Combine(Directory.FullName, path));
    }
}