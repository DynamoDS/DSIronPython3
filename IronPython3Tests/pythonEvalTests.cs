using Dynamo;
using NUnit.Framework;
using System.Collections;


namespace IronPython3Tests
{
    public class PythonEvalTests : UnitTestBase
    {
        public delegate object PythonEvaluatorDelegate(string code, IList bindingNames, IList bindingValues);

        public IEnumerable<PythonEvaluatorDelegate> Evaluators = new List<PythonEvaluatorDelegate> {
            IronPython3.Evaluator.IronPython3Evaluator.EvaluateIronPythonScript,
        };

        [Test]
        [Category("UnitTests")]
        public void EvaluatorWorks()
        {
            foreach (var pythonEvaluator in Evaluators)
            {
                var empty = new ArrayList();
                var output = pythonEvaluator("OUT = 0", empty, empty);
                Assert.AreEqual(0, output);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void BindingsWork()
        {
            const string expected = "Hi!";

            var names = new ArrayList { "test" };
            var vals = new ArrayList { expected };

            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(
                    "OUT = test",
                    names,
                    vals
                );

                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        [Category("UnitTests")]
        public void SysDiagProccess_AndOtherShimmedCLRTypesWork()
        {

          
            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(
                    @"
import clr
from System.Reflection import Assembly
from System.Diagnostics import Process
dynamoCore = Assembly.Load(""DynamoCore"")
version_long = dynamoCore.GetName().Version.Major.ToString()
proc = Process.GetCurrentProcess().ProcessName
OUT = (version_long,proc)
",
                    new ArrayList(),
                    new ArrayList()
                );

                Assert.AreEqual(new []{ "3","dotnet"}, output);
            }
        }


        [Test]
        [Category("UnitTests")]
        public void DataMarshaling_Output()
        {
            var marshaler = IronPython3.Evaluator.IronPython3Evaluator.OutputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            const string script = "OUT = ['', ' ', '  ']";

            object output = IronPython3.Evaluator.IronPython3Evaluator.EvaluateIronPythonScript(
                script,
                new ArrayList(),
                new ArrayList());

            Assert.AreEqual(new[] { 0, 1, 2 }, output);

            marshaler.UnregisterMarshalerOfType<string>();
        }

        [Test]
        [Category("UnitTests")]
        public void DataMarshaling_Input()
        {
            var marshaler = IronPython3.Evaluator.IronPython3Evaluator.InputMarshaler;
            marshaler.RegisterMarshaler((string s) => s.Length);

            const string script = "OUT = sum(IN)";

            object output = IronPython3.Evaluator.IronPython3Evaluator.EvaluateIronPythonScript(
                script,
                new ArrayList { "IN" },
                new ArrayList { new ArrayList { " ", "  " } });

            Assert.AreEqual(3, output);

            marshaler.UnregisterMarshalerOfType<string>();
        }


        [Test]
        public void SliceOperator_Output()
        {
            var names = new ArrayList { "indx" };
            var vals = new ArrayList { 3 };

            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(
                "OUT = [1,2,3,4,5,6,7][indx:indx+2]",
                names,
                vals);

                var expected = new ArrayList { 4, 5 };

                Assert.AreEqual(expected, output);
            }
        }

        [Test]
        public void IronPythonGivesCorrectErrorLineNumberAndLoadsStdLib()
        {
            var code = @"
from xml.dom.minidom import parseString
my_xml = parseString('invalid XML!')
";
            try
            {
                IronPython3.Evaluator.IronPython3Evaluator.EvaluateIronPythonScript(code, new ArrayList(), new ArrayList());
                Assert.Fail("An exception was expected");
            }
            catch (Exception exc)
            {
                StringAssert.StartsWith(@"Traceback (most recent call last):
  File ""<string>"", line 3, in <module>", exc.Message);
                StringAssert.EndsWith("Data at the root level is invalid. Line 1, position 1.", exc.Message);
            }
        }

        [Test]
        public void NonListIterablesCanBeOutput()
        {
            var code = @"
s = { 'hello' }
fs = frozenset({ 'world' })
d = { 'one': 1 }
dk = d.keys()
dv = d.values()
di = d.items()

OUT = s,fs,dk,dv,di
";
            var expected = new ArrayList
            {
                new ArrayList { "hello" },
                new ArrayList { "world" },
                new ArrayList { "one" },
                new ArrayList { 1 },
                new ArrayList { new ArrayList { "one", 1 } }
            };
            var empty = new ArrayList();
            foreach (var pythonEvaluator in Evaluators)
            {
                var output = pythonEvaluator(code, empty, empty);
                Assert.AreEqual(expected, output);
            }
        }
    }
}