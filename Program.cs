using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using kasthack.vksharp.generator.Templates;

namespace kasthack.vksharp.generator
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Starting up");

            var serializer = new JsonSerializer()
            {
                MetadataPropertyHandling = MetadataPropertyHandling.Ignore
            };

            Console.WriteLine("Reading objects");

            var objectsSource = File.ReadAllText("objects.json");
            var objects = serializer.Deserialize<ResponsesSchema>(new JsonTextReader(new StringReader(objectsSource)));

            Console.WriteLine($"Successfully read objects schema. {objects.Definitions.Count} definitions found");

            //------------------------------------------------------
            Console.WriteLine("Reading responses");

            var responsesSource = File.ReadAllText("responses.json");
            var responses = serializer.Deserialize<ResponsesSchema>(new JsonTextReader(new StringReader(responsesSource)));

            Console.WriteLine($"Successfully read responses schema. {responses.Definitions.Count} definitions found");

            //------------------------------------------------------

            Console.WriteLine("Reading methods");

            var methodsSource = File.ReadAllText("methods.json");
            var methods = serializer.Deserialize<MethodsSchema>(new JsonTextReader(new StringReader(methodsSource)));
            methods.Methods = methods.Methods.OrderBy(a => a.Name).ToArray();

            Console.WriteLine($"Successfully read methods schema. {methods.Methods.Length} definitions found");

            //------------------------------------------------------

            Console.WriteLine("Building output");
            
            //------------------------------------------------------

            Console.WriteLine("Generating object mappings");

            var objectsTemplate = new Objects(objects);
            var objectsOutput = objectsTemplate.TransformText();
            File.WriteAllText("objects.cs", objectsOutput);

            Console.WriteLine($"Successfully generated objects mappings. Output written to objects.cs");

            //------------------------------------------------------

            Console.WriteLine("Generating methods");

            var methodsTemplate = new Methods(methods, responses, objects);
            var methodsOutput = methodsTemplate.TransformText();
            File.WriteAllText("methods.cs", methodsOutput);

            Console.WriteLine("Successfully generated methods. Output written to methods.cs");

            //------------------------------------------------------

            Console.WriteLine("Complete");

            if (Debugger.IsAttached)
            {
                //Process.Start("explorer", ".");
            }
        }

        private static void Stuff(ResponsesSchema responses)
        {

            #region Temp / research
            var props = responses
                .Definitions
                .SelectMany(
                    a => a
                        .Value
                        .Properties
                        .Values
                        .Where(b => b.Properties != null)
                        .SelectMany(
                            b => b
                                .Properties
                                .Keys
                                .Select(c => new
                                {
                                    keys = c,
                                    type = a.Key
                                })
                        )
                )
                .GroupBy(a => a.keys)
                .Select(a => new { a.Key, cnt = a.Count(), values = new HashSet<string>(a.Select(b => b.type)) })
                .Where(a => a.cnt > 1)
                .OrderBy(a => -a.cnt)
                .ToArray();

            var pms = props
                .SelectMany(
                    a => props
                        .Where(b => a.Key.CompareTo(b.Key) < 0)
                        .Select(b =>
                        {
                            var matches = a.values.Count(sourceType => b.values.Contains(sourceType));
                            if (matches == 0) return null;
                            return new { a = a.Key, b = b.Key, x = matches, ac = a.cnt, bc = b.cnt };
                        })
                        .Where(b => b != null)
                )
                //.OrderByDescending(a => a.x / (double)Math.Min(a.ac, a.bc))
                .Where(a => a.x > 1)
                .OrderByDescending(a => a.x)
                .ToList();

            var subsets = pms
                .Where(a => a.x >= (double)Math.Min(a.ac, a.bc))
                .ToList();
            pms = pms
                .Where(a => a.x < (double)Math.Min(a.ac, a.bc))
                .ToList();

            #endregion
        }
    }
}
