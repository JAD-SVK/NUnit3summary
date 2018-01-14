// MIT License
//
// Code:
//    Atrip.NUnit3summary
//
// Author:
//    JAD <jad.1.svk+git@gmail.com>
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Xml;

namespace Atrip.NUnit3summary
{
  internal static class Program
  {
    [SuppressMessage("Gendarme.Rules.Exceptions", "DoNotSwallowErrorsCatchingNonSpecificExceptionsRule",
      Justification = "Do not let fall exception to system.")]
    static int Main(string[] args)
    {
      try
      {
        if (args.Length > 2)
        {
          DisplayHelp(Console.Error, false);
          return ((int)ApplicationResult.InvalidCommandLine);
        }
        XmlDocument document = LoadXmlDocument(args);
        if (ReferenceEquals(document, null))
        {
          return ((int)ApplicationResult.InvalidCommandLine);
        }
        TestResultNode result = new TestResultNode();
        result.Load(document.DocumentElement);
        Compress(result);
        if (args.Length == 2)
        {
          Save(result, args[1]);
        }
        else
        {
          Save(Console.Out, result, 0);
        }
        return ((int)ApplicationResult.OK);
      }
#pragma warning disable S2221 // Do not let fall exception to system
      catch (Exception ex)
#pragma warning restore S2221 // "Exception" should not be caught when not required by called methods
      {
        Console.Error.WriteLine(ex.GetType());
        Console.Error.WriteLine(ex.Message);
        return ((int)ApplicationResult.Exception);
      }
    }

    private static XmlDocument LoadXmlDocument(string[] args)
    {
      XmlDocument document = new XmlDocument();
      if (args.Length < 1)
      {
        if (!IsInputRedirected())
        {
          DisplayHelp(Console.Out, true);
          return (null);
        }
        document.Load(Console.In);
      }
      else if (string.Equals(args[0], "--console-in", StringComparison.OrdinalIgnoreCase))
      {
        document.Load(Console.In);
      }
      else
      {
        if (!File.Exists(args[0]))
        {
          DisplayHelp(Console.Error, noArgument: false);
          return (null);
        }
        document.Load(args[0]);
      }
      return (document);
    }

    private static bool IsInputRedirected()
    {
      if (IsRedirectedInLinux())
      {
        return (Console.KeyAvailable);
      }
      else
      {
        return (IsInputRedirectedWindows());
      }
    }

    private static bool IsRedirectedInLinux()
    {
      try
      {
        return (0 == (Console.WindowHeight + Console.WindowWidth));
      }
      catch (IOException)
      {
        return (false);
      }
    }

    private static bool IsInputRedirectedWindows()
    {
      try
      {
        if (Console.KeyAvailable)
        {
          return (false);
        }
      }
      catch (InvalidOperationException)
      {
        return (true);
      }
      return (false);
    }

    private static void Save(TestResultNode result, string fileName)
    {
      using (StreamWriter sw = new StreamWriter(fileName, false, Encoding.UTF8))
      {
        Save(sw, result, 0);
      }
    }

    private static void Save(TextWriter tw, TestResultNode result, int padding)
    {
      int newPadding = padding;
      if (!string.IsNullOrEmpty(result.Name))
      {
        if (padding > 0)
        {
          tw.Write(new string(' ', padding));
        }
        tw.Write(result.Name);
        if (!string.IsNullOrEmpty(result.Result))
        {
          tw.Write(" [");
          tw.Write(result.Result);
          tw.Write(']');
        }
        newPadding += 2;
        tw.WriteLine();
        if (!string.IsNullOrEmpty(result.Message))
        {
          WriteMessage(tw, result.Message, newPadding);
        }
      }
      foreach (TestResultNode node in result)
      {
        Save(tw, node, newPadding);
      }
    }

    private static void WriteMessage(TextWriter tw, string message, int padding)
    {
      string paddingSpace = (new string(' ', padding) + "'");
      using (StringReader sr = new StringReader(message))
      {
        while (sr.Peek() >= 0)
        {
          tw.Write(paddingSpace);
          string line = sr.ReadLine();
          tw.Write(line.Trim());
          tw.WriteLine('\'');
        }
      }
    }

    private static void Compress(TestResultNode parent)
    {
      int index = 0;
      while (index < parent.Count)
      {
        TestResultNode node = parent[index];
        if (node.SuiteType == TestSuiteType.Namespace)
        {
          bool repeat = true;
          while (repeat && (node.Count == 1))
          {
            TestResultNode child = node[0];
            repeat = (child.SuiteType == TestSuiteType.Namespace);
            if (repeat)
            {
              string combinedName = (node.Name + "." + child.Name);
              TestResultNode combined = new TestResultNode(combinedName, child);
              parent[index] = combined;
              node = combined;
            }
          }
        }
        Compress(node);
        index++;
      }
    }

    private static void DisplayHelp(TextWriter tw, bool noArgument)
    {
      bool mono = IsMono();
      if (mono)
      {
        tw.WriteLine("Usage: NUnit3summary <result-file.xml> [summary-file.txt]");
      }
      else
      {
        tw.WriteLine("Usage: NUnit3summary [result-file.xml [summary-file.txt]]");
      }
      tw.WriteLine("        -or-");
      tw.WriteLine("       NUnit3summary --console-in [summary-file.txt]");
      if (mono && noArgument && IsRedirectedInLinux())
      {
        Console.Error.WriteLine("Error: Redirection without '--console-in' is not supported in Linux/Mono.");
      }
    }

    private static bool IsMono()
    {
      Type type = Type.GetType("Mono.Runtime");
      return (!ReferenceEquals(type, null));
    }
  }
}
