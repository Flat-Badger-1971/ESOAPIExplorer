// TODO: loadza debugging required

//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.IO.Compression;
//using System.Text.RegularExpressions;
//using ESOAPIExplorer.Models;

//namespace ESOAPIExplorer.ViewModels;

//public class EsoUILuaEEGeneratorService
//{
//    public EsoUIDocumentation Documentation { get; set; }

//    private const string INDENT = "    ";
//    private const string EOL = "\r\n";
//    private const string TEMP_DIR = "temp";
//    private const string TEMPLATE_DIR = "template";

//    private EsoUIDocumentation documentation;
//    private StreamWriter writer;

//    private string CreateIndent(int level)
//    {
//        return new string(' ', level * INDENT.Length);
//    }

//    private string CreateLineBreak(int num = 1)
//    {
//        return new string('\n', num);
//    }

//    private void FinaliseWriter()
//    {
//        WriteLine("return nil");
//        writer.Flush();
//        writer.Close();
//    }

//    private void WriteLine(string line, int indent = 0)
//    {
//        line = CreateIndent(indent) + line + CreateLineBreak();
//        writer.Write(line);
//    }

//    private void StartSection()
//    {
//        WriteLine("-------------------------------------------------------------------------------");
//    }

//    private void EndSection()
//    {
//        WriteLine("");
//    }

//    private void WriteGlobalType(string type)
//    {
//        StartSection();
//        WriteLine($"-- {type} type.");
//        WriteLine($"-- @type {type}");
//        EndSection();
//    }

//    private void WriteGlobalField(string name, string type, string value)
//    {
//        StartSection();
//        WriteLine($"-- `{name}` = {value}");
//        WriteLine($"-- This is a global variable which holds one of the possible values for @{type}.");
//        WriteLine($"-- @field[parent=#globals] #{type} {name}");
//        EndSection();
//    }

//    private void WriteGlobals()
//    {
//        StartSection();
//        WriteLine("-- @module globals");
//        EndSection();

//        Dictionary<string, List<string>> globals = Documentation.Globals;

//        foreach (KeyValuePair<string, List<string>> kvp in globals)
//        {
//            string type = kvp.Key;
//            List<string> values = kvp.Value;
//            WriteGlobalType(type);

//            foreach (string name in values)
//            {
//                string value = "unknown"; // TODO: add value from ingame dump
//                WriteGlobalField(name, type, value);
//            }
//        }
//    }

//    private void WriteArguments(List<EsoUIArgument> args)
//    {
//        foreach (EsoUIArgument arg in args)
//        {
//            WriteLine($"-- @param #{arg.Type.Type} {arg.Name}");
//        }
//    }

//    private void WriteReturns(List<EsoUIArgument> returns)
//    {
//        foreach (EsoUIArgument ret in returns)
//        {
//            WriteLine($"-- @return {ret.Type.Type}#{ret.Type.Type} {ret.Name}");
//        }
//    }

//    private void WriteFunction(EsoUIFunction data, string parent = "global")
//    {
//        StartSection();
//        WriteLine($"-- {data.Access} `{data.Name}`");

//        if (data.HasVariableReturns)
//        {
//            WriteLine("-- This function uses variable return values.");
//        }

//        WriteLine($"-- @function [parent=#{parent}] {data.Name}");

//        if (parent != "global")
//        {
//            WriteLine($"-- @param #{parent} self");
//        }

//        if (data.Args != null)
//        {
//            WriteArguments(data.Args);
//        }

//        if (data.Returns != null)
//        {
//            WriteReturns(data.Returns);
//        }

//        EndSection();
//    }

//    private void WriteGameApi()
//    {
//        List<EsoUIFunction> api = Documentation.Functions;

//        foreach (EsoUIFunction func in api)
//        {
//            WriteFunction(func);
//        }
//    }

//    private void WriteEvent(EsoUIEvent esoevent)
//    {
//        string value = "unknown"; // TODO: add value from ingame dump

//        StartSection();
//        WriteLine($"-- `{esoevent.Name}` = {value}");
//        WriteLine("-- ");
//        WriteLine("-- This is one of the available event types which can be used with the @{EVENT_MANAGER}.");
//        WriteLine("-- <b>Callback Parameters</b>");
//        WriteLine("-- <ul>");
//        WriteLine("-- <li>#Event eventType</li>");

//        if (esoevent.Args != null)
//        {
//            foreach (EsoUIArgument arg in esoevent.Args)
//            {
//                WriteLine($"-- <li>#{arg.Type.Type} {arg.Name}</li>");
//            }
//        }

//        WriteLine("-- </ul>");
//        WriteLine($"-- @field[parent=#global] #Event {esoevent.Name}");
//        EndSection();
//    }

//    private void WriteEvents()
//    {
//        List<EsoUIEvent> events = Documentation.Events;

//        foreach (EsoUIEvent esoevent in events)
//        {
//            WriteEvent(esoevent);
//        }
//    }

//    private void WriteObject(EsoUIObject obj)
//    {
//        StartSection();
//        WriteLine($"-- @module {obj.Name}");

//        if (obj.Parent != null)
//        {
//            WriteLine($"-- @extends {obj.Parent.Name}#{obj.Parent.Name}");
//        }

//        EndSection();

//        List<EsoUIFunction> functions = obj.Functions;

//        foreach (EsoUIFunction func in functions)
//        {
//            WriteFunction(func, obj.Name);
//        }
//    }

//    private bool TryCreatePath(string path)
//    {
//        try
//        {
//            Directory.CreateDirectory(path);
//            return true;
//        }
//        catch (Exception ex)
//        {
//            if (!ex.Message.Contains("already exists"))
//            {
//                throw;
//            }
//        }

//        return false;
//    }

//    private void ClearPath(string path)
//    {
//        string[] content = Directory.GetFileSystemEntries(path);

//        foreach (string child in content)
//        {
//            if (Directory.Exists(child))
//            {
//                Console.WriteLine("enter", child);
//                ClearPath(child);
//                Console.WriteLine("rmdir", child);
//                Directory.Delete(child);
//            }
//            else
//            {
//                Console.WriteLine("unlink", child);
//                File.Delete(child);
//            }
//        }
//    }

//    private void CopyAllFiles(string sourcePath, string targetPath)
//    {
//        Console.WriteLine($"copy all files in \"{sourcePath}{Path.DirectorySeparatorChar}\" to \"{targetPath}{Path.DirectorySeparatorChar}\"");
//        TryCreatePath(targetPath);

//        string[] content = Directory.GetFileSystemEntries(sourcePath);

//        foreach (string child in content)
//        {
//            string sourceChildPath = Path.Combine(sourcePath, child);
//            string targetChildPath = Path.Combine(targetPath, child);
//            Console.WriteLine($"copy \"{sourceChildPath}\" to \"{targetChildPath}\"");

//            if (Directory.Exists(sourceChildPath))
//            {
//                CopyAllFiles(sourceChildPath, targetChildPath);
//            }
//            else
//            {
//                File.Copy(sourceChildPath, targetChildPath, true);
//            }
//        }
//    }

//    private void ReplaceTokens(string file, Dictionary<string, string> replacements)
//    {
//        Console.WriteLine($"replace tokens in {file}");
//        string content = File.ReadAllText(file);

//        foreach (KeyValuePair<string, string> kvp in replacements)
//        {
//            Console.WriteLine($"replace \"##{kvp.Key}##\" with \"{kvp.Value}\"");
//            Regex expr = new Regex($"##{kvp.Key}##", RegexOptions.Compiled);
//            content = expr.Replace(content, kvp.Value);
//        }

//        File.WriteAllText(file, content);
//    }

//    private void CreateArchive(string sourcePath, string targetFile)
//    {
//        string absPath = Path.GetFullPath(sourcePath);
//        string absTargetFile = Path.GetFullPath(targetFile);
//        Console.WriteLine("archive", absPath, "to", absTargetFile);

//        using (ZipArchive archive = ZipFile.Open(absTargetFile, ZipArchiveMode.Create))
//        {
//            string[] files = Directory.GetFiles(absPath, "*", SearchOption.AllDirectories);

//            foreach (string file in files)
//            {
//                string relativePath = file.Substring(absPath.Length + 1);
//                archive.CreateEntryFromFile(file, relativePath);
//            }
//        }
//    }

//    public void Generate(string outputDir)
//    {
//        if (!TryCreatePath(TEMP_DIR))
//        {
//            ClearPath(TEMP_DIR);
//        }

//        CopyAllFiles(TEMPLATE_DIR, TEMP_DIR);

//        string apiDir = Path.Combine(TEMP_DIR, "api");

//        using (StreamWriter writer = new StreamWriter(new FileStream(Path.Combine(apiDir, "global.doclua"), FileMode.Append, FileAccess.Write)))
//        {
//            this.writer = writer;
//            WriteGameApi();
//            WriteEvents();
//            FinaliseWriter();
//        }

//        using (StreamWriter writer = new StreamWriter(new FileStream(Path.Combine(apiDir, "globals.doclua"), FileMode.Create, FileAccess.Write)))
//        {
//            this.writer = writer;
//            WriteGlobals();
//            FinaliseWriter();
//        }

//        List<EsoUIObject> objects = Documentation.Objects;

//        foreach (EsoUIObject obj in objects)
//        {
//            using (StreamWriter writer = new StreamWriter(new FileStream(Path.Combine(apiDir, $"{obj.Name}.doclua"), FileMode.Create, FileAccess.Write)))
//            {
//                this.writer = writer;
//                WriteObject(obj);
//                FinaliseWriter();
//            }
//        }

//        string version = documentation.ApiVersion.ToString();

//        Dictionary<string, string> replacements = new Dictionary<string, string>
//        {
//            { "APIVERSION", version }
//        };

//        ReplaceTokens(Path.Combine(TEMP_DIR, "esolua.rockspec"), replacements);

//        CreateArchive(apiDir, Path.Combine(TEMP_DIR, "api.zip"));
//        ClearPath(apiDir);
//        Directory.Delete(apiDir);

//        CreateArchive(TEMP_DIR, Path.Combine(outputDir, $"esolua{version}.zip"));
//        ClearPath(TEMP_DIR);
//        Directory.Delete(TEMP_DIR);
//    }
//}
