using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using KeepCoding;
using UnityEngine;

public class TemplateScript : ModuleScript 
{
    private void Start()
    {
        // Log(AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetName().Name.StartsWith("Dependency")));

        //var testAssembly = Assembly.Load(File.ReadAllBytes(@"C:\Users\Emik\source\repos\DependencyTest\DependencyTest\bin\Debug\DependencyTester.dll"));
        //var trueAssembly = Assembly.Load(File.ReadAllBytes(@"C:\Users\Emik\source\repos\DependencyTest\DependencyTest\bin\Debug\DependencyTestTrue.dll"));
        //var falseAssembly = Assembly.Load(File.ReadAllBytes(@"C:\Users\Emik\source\repos\DependencyTest\DependencyTest\bin\Debug\DependencyTestFalse.dll"));

        //Log(testAssembly.Call()
        //    .GetType("DependencyTester.Test").Call()
        //    .GetMethod("Bar", BindingFlags.Public | BindingFlags.Static).Call()
        //    .Invoke(null, null).Call(), LogType.Error);

        //Log(falseAssembly.Call()
        //    .GetType("DependencyTest.Foo").Call()
        //    .GetMethod("Bar", BindingFlags.Public | BindingFlags.Static).Call()
        //    .Invoke(null, null).Call(), LogType.Error);

        //Log(trueAssembly.Call()
        //    .GetType("DependencyTest.Foo").Call()
        //    .GetMethod("Bar", BindingFlags.Public | BindingFlags.Static).Call()
        //    .Invoke(null, null).Call(), LogType.Error);
    }
}
