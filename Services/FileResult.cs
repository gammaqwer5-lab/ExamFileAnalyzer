using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileAnalyzer.Services
{
    public sealed record FileResult(string FileName, int Lines, int Words, long Characters);
}
