using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ESOAPIExplorer.Models;

public class StatusInformation
{
    public int APIItems { get; set; }
    public int FunctionItems { get; set; }
    public int CFunctionItems { get; set; }
    public int EnumTypes { get; set; }
    public int EnumConstants { get; set; }
    public int Globals { get; set; }
}
