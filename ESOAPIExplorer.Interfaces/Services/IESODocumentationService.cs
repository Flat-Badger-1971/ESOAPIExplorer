using ESOAPIExplorer.Models;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Services;

public interface IESODocumentationService
{
    EsoUIDocumentation Documentation { get; set; }
    EsoUIXMLElement CurrentElement { get; set; }
    List<string> CurrentEnum { get; set; }
    EsoUIFunction CurrentFunction { get; set; }
    string CurrentLine { get; set; }
    EsoUIObject CurrentObject { get; set; }
    EsoUIDocumentation Data { get; set; }
    string FileName { get; set; }
    ReaderState State { get; set; }
    ReaderState FindNextState();
    string GetFirstMatch(Regex pattern);
    List<string> GetMatches(Regex pattern);
    EsoUIXMLElement GetOrCreateElement(string name);
    List<string> GetOrCreateGlobal(string name);
    EsoUIObject GetOrCreateObject(string name);
    Task InitialiseAsync();
    void InjectCustomData();
    bool LineEndsWith(string suffix);
    bool LineStartsWith(string prefix);
    Task<EsoUIDocumentation> ParseAsync(string fileName);
    Task<EsoUIDocumentation> ParseFileAsync();
    bool ReadApiVersion();
    bool ReadEvents();
    void ReadFunction(Dictionary<string, EsoUIFunction> functions);
    bool ReadGameApi();
    bool ReadGlobals();
    bool ReadObjectApi();
    bool ReadXmlAttributes();
    bool ReadXmlLayout();
}
