using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Collections.Generic;

public class Program
{
    public static void Main()
    {
        var baseDir = Path.GetFullPath("..");
        Console.WriteLine(baseDir);

        var globalSettingsFile = Path.Combine(baseDir, "settings.json");
        if (!File.Exists(globalSettingsFile))
        {

            Console.Write("Enter user name: ");
            var userName = Console.ReadLine();

            Console.Write("Enter password: ");
            var password = Console.ReadLine();

            Console.Write("Enter game server name: ");
            var serverHostname = Console.ReadLine();

            // Write global settings file
            {
                Console.WriteLine(@"Creating ""{0}""...", globalSettingsFile);
                Directory.CreateDirectory(Directory.GetParent(globalSettingsFile).FullName);
                File.WriteAllText(globalSettingsFile, $@"
{{
	""ServerHostname"": ""{serverHostname}"",
	""Login"": ""{userName}"",
	""Password"": ""{password}"",
	""StateFilename"": ""state.json""
}}");
            }
        }

        Console.Write("Enter namespace name (i.e. team name using CamelCase): ");
        var namespaceName = Console.ReadLine();

        Console.Write("Enter assignment name (using CamelCase): ");
        var assignmentName = Console.ReadLine();

        Console.Write("Enter game servers port numbers separated with commas: ");
        var ports = Console.ReadLine().Split(',');

        var newProjectGuid = Guid.NewGuid().ToString().ToUpper();

        var ignoredPaths = @"(^.+[\\/]bin$)|(^.+[\\/]obj$)";

        var transformMap = new Dictionary<string, Dictionary<string, string>>()
        {
            {
                "files", new Dictionary<string, string>()
                {
                    { "FooBar", assignmentName },
                    { "Acme", namespaceName }
                }
            },
            {
                @"(^.+\.csproj$)|(^.+\.sln$)", new Dictionary<string, string>()
                {
                    { "FooBar", assignmentName },
                    { "Acme", namespaceName },
                    { "4507B685-EA7A-47FA-AB54-75DC5AE47080", newProjectGuid }
                }
            },
            {
                @"^.+\.cs$", new Dictionary<string, string>()
                {
                    { "FooBar", assignmentName },
                    { "Acme", namespaceName }
                }
            },
            {
                @"^.+\.cmd$", new Dictionary<string, string>()
                {
                    { "FooBar", assignmentName },
                    { "Acme", namespaceName }
                }
            }
        };

        Func<string, string> GetFullPath = (string relativeSource) => Path.GetFullPath(Path.Combine(baseDir, relativeSource));

        Func<string, Dictionary<string, string>> GetReplacementMap = (string filename) =>
        {
            foreach (var kv in transformMap)
            {
                if (Regex.IsMatch(filename, kv.Key)) return kv.Value;
            }

            return null;
        };

        Action<string> MirrorFile = (string inputSource) =>
        {
            var source = GetFullPath(inputSource);
            var namesMap = GetReplacementMap("files");
            var destination = GetFullPath(Transform(inputSource, namesMap));
            var map = GetReplacementMap(source);
            if (map == null)
            {
                Console.WriteLine(@"Copying ""{0}"" -> ""{1}""...", source, destination);
                File.Copy(source, destination, true);
            }
            else
            {
                Console.WriteLine(@"Transforming ""{0}"" -> ""{1}""...", source, destination);
                var text = File.ReadAllText(source);
                File.WriteAllText(destination, Transform(text, map));
            }
        };

        Action<string> MirrorDirectory = null;
        MirrorDirectory = (string inputSource) =>
        {
            var source = GetFullPath(inputSource);
            var namesMap = GetReplacementMap("files");
            var destination = GetFullPath(Transform(inputSource, namesMap));
            Directory.CreateDirectory(destination);
            foreach (var path in Directory.EnumerateFiles(source))
            {
                if (Regex.IsMatch(path, ignoredPaths)) continue;
                MirrorFile(path);
            }
            foreach (var path in Directory.EnumerateDirectories(source))
            {
                if (Regex.IsMatch(path, ignoredPaths)) continue;
                MirrorDirectory(path);
            }
        };

        // Copy sources
        MirrorFile(Path.Combine("src", "FooBarPlayer.sln"));
        MirrorDirectory(Path.Combine("src", "Acme.FooBarPlayer"));

        // Prepare deployment for each server
        foreach (var serverPort in ports)
        {
            transformMap["files"]["20000"] = serverPort;
            transformMap[@"^.+\.cmd$"]["20000"] = serverPort;
            MirrorFile("Deploy_FooBar_20000.cmd");
            MirrorFile("Run_FooBar_20000.cmd");

            // Write custom settings file
            {
                var file = GetFullPath(Transform(Path.Combine("players", "FooBar_20000", "settings.json"), GetReplacementMap("files")));
                Console.WriteLine(@"Creating ""{0}""...", file);
                Directory.CreateDirectory(Directory.GetParent(file).FullName);
                File.WriteAllText(file, $@"
{{
	""ServerPort"": {serverPort}
}}");
            }
        }
    }

    static string Transform(string source, Dictionary<string, string> replacers)
    {
        foreach (var kv in replacers)
        {
            source = Regex.Replace(source, kv.Key, kv.Value);
        }
        return source;
    }
}
