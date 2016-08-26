using System;
using System.Collections.Generic;
using System.Linq;

namespace MassPropertiesURDFGenerator
{
    class Program
    {
        static List<string> GenerateXMLTags(string partName, MassPropertySet set)
        {
            List<string> result = new List<string>();

            //We create properties that can be used by urdf.xacro xml structure
            /* ex:
                    < xacro:property name = "proprerty_name" value = "0.01" />
             */

            string xacroPropertyTemplate = " <xacro:property name=\"{0}\" value=\"{1:F15}\" />";

            if(set.IsValid())
            {
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_Mass"), set.Mass()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_XX"), set.MoI_XX()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_XY"), set.MoI_XY()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_XZ"), set.MoI_XZ()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_YY"), set.MoI_YY()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_YZ"), set.MoI_YZ()));
                result.Add(String.Format(xacroPropertyTemplate, (partName + "_MoI_ZZ"), set.MoI_ZZ()));
                result.Add(" "); //Add an empty line 
            }
            else
            {
                Console.WriteLine(String.Format("Could not generate tags for part: {0}, could not retrieve good data", partName));
            }

            return result;
        }

        static MassPropertySet GetMassPropertiesFromDoc(SldWorks.ModelDoc2 doc)
        {
            /*  Open the model doc extension object
                I think they just made this so that they didnt have to change the original model object interface?
                Their API is a piece of work...                                                                            */
            SldWorks.ModelDocExtension docExtension = doc.Extension;

            /*  One option is to use the GetMassProperties function,
             *  but this will only return mass properties about the centre of mass, aligned with the assemblies default coordinate system
             *  see: http://help.solidworks.com/2015/English/api/sldworksapi/SOLIDWORKS.Interop.sldworks~SOLIDWORKS.Interop.sldworks.IModelDocExtension~GetMassProperties.html
             */
            /*  Instead, we probably want to create our own MassProperties object, 
             *  so that we can get mass properties with respect to a coordinate system
             *  of our choosing.
             *  see: http://help.solidworks.com/2015/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IModelDocExtension~CreateMassProperty.html
             */

            SldWorks.MassProperty massProperty = docExtension.CreateMassProperty();
            //Note: we don't call AddBodies or IAddBodies, so that all bodies are calculated by default
            //Note: we must set the coordinate system
            const string coordSystemName = "urdf_coordinate_system";
            SldWorks.MathTransform coordinateSystemTransform = docExtension.GetCoordinateSystemTransformByName(coordSystemName);

            MassPropertySet result = null;

            //Try to fill result
            if (coordinateSystemTransform == null)
            {
                Console.WriteLine("Coordinate System Transform was null.");
                Console.WriteLine("Coordinate System with name: '" + coordSystemName + "' was not found.");
            }
            else
            {
                massProperty.SetCoordinateSystem(coordinateSystemTransform);

                double[] centreOfMass = massProperty.CenterOfMass;
                double mass = massProperty.Mass;
                double[] momentsOfInertia = massProperty.GetMomentOfInertia((int)SwConst.swMassPropertyMoment_e.swMassPropertyMomentAboutCoordSys);

                result = new MassPropertySet(centreOfMass, mass, momentsOfInertia);
            }
            return result;
        }

        /* Straight forward method, opens the doc in the way we want it, debugs any errors */
        //Returns null on failure
        static SldWorks.ModelDoc2 OpenDoc(SldWorks.SldWorks app, string filePath)
        {
            int errors = 0;
            int warnings = 0;

            Console.WriteLine("Opening Doc: " + filePath);
            SldWorks.ModelDoc2 doc = app.OpenDoc6(
                filePath,
                (int)(SwConst.swDocumentTypes_e.swDocASSEMBLY),
                (int)(SwConst.swOpenDocOptions_e.swOpenDocOptions_Silent | SwConst.swOpenDocOptions_e.swOpenDocOptions_ReadOnly),
                "default",
                ref errors,
                ref warnings);

            if (errors != 0)
            {
                Console.WriteLine("Error: " + errors);
            }
            if (warnings != 0)
            {
                Console.WriteLine("Warnings: " + warnings);
            }

            return doc;
        }

        /*  Takes as input the file name of the index file
         *  Outputs a list of solidworks files and a list of corresponding titles
         *  These outputs are parallel (equal size, and each nth item in the first list corresponds to the nth item in the second list)
         *  
         *  Some sketchy stuff is done to allow for spaces in file paths. As such, if the index file is malformed, this will not work.
         */
        static bool ParseIndexFile(string fileName, ref List<string> listOfFiles, ref List<string> listOfMeshTitles)
        {
            listOfFiles.Clear();
            listOfMeshTitles.Clear();

            //Start by opening the file
            System.IO.StreamReader indexStream;
            try
            {
                indexStream = new System.IO.StreamReader(fileName);
            }
            catch
            {
                Console.WriteLine("Error opening index file: " + fileName);
                return false;
            }

            //Read in from the file
            try
            {
                int n = Int32.Parse(indexStream.ReadLine());
                for (int i = 0; i < n; i++)
                {
                    string s = indexStream.ReadLine();
                    char space = ' ';
                    string[] strings = s.Split(space);
                    string fileNameToParse = "";
                    for (int j = 0; j < strings.Length - 1; j++)
                    {
                        fileNameToParse += strings[j];
                        if (j < strings.Length - 2)
                            fileNameToParse += " ";
                    }

                    listOfFiles.Add(fileNameToParse);
                    listOfMeshTitles.Add(strings.Last());
                }
            }
            catch
            {
                Console.WriteLine("Problem while reading index file");
                return false;
            }

            return true;
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage: ");
            Console.WriteLine(" MassPropertiesURDFGenerator.exe -h :   help, show this dialog");
            Console.WriteLine(" MassPropertiesURDFGenerator.exe {index_file_name} {output_file_name}: runs the script, using the index file specified");
            Console.WriteLine("\n  The index file should contain on its first line, n:");
            Console.WriteLine("        the number of files to parse through");
            Console.WriteLine("  and then n lines, each with 2 strings: \n        first, the name of the solidworks file, second the title of the mesh to use for urdf property generation.");
            Console.WriteLine("\nPress any key to quit");
        }

        static bool OutputToXacroFile(string filename, List<string> tags)
        {
            try
            {
                System.IO.StreamWriter fileOut = new System.IO.StreamWriter(filename);
                fileOut.WriteLine("<? xml version=\"1.0\"?>");
                fileOut.WriteLine("<!-- Programmatically generated content -->");
                foreach (var tag in tags)
                {
                    fileOut.WriteLine(tag);
                }
                fileOut.Close();
            }
            catch
            {
                Console.WriteLine("Problem writing out to file");
                return false;
            }

            return true;
        }

        static void Main(string[] args)
        {
            if(args.Length != 2 || args[0] == "-h")
            {
                PrintUsage();
                return;
            }

            List<string> filesToParse = new List<string>();
            List<string> meshTitles = new List<string>();
            if (!ParseIndexFile(args[0], ref filesToParse, ref meshTitles)) {
                Console.WriteLine("Couldn't read index file");
                return;
            }

            if (filesToParse.Count != meshTitles.Count)
            {
                Console.WriteLine("Something amiss: Had different number of files than mesh names. Maybe the index file specified is malformed?");
                Console.WriteLine("Num Files: " + filesToParse.Count + ", Num Mesh Names: " + meshTitles.Count);
                return;
            }

            SldWorks.SldWorks swApp = new SldWorks.SldWorks();

            List<string> xmlTags = new List<string>();
            try {
                for (int i = 0; i < filesToParse.Count; i++)
                {
                    SldWorks.ModelDoc2 doc = OpenDoc(swApp, filesToParse[i]);
                    if(doc == null)
                    {
                        Console.WriteLine("Null Doc: " + filesToParse[i]);
                        Console.WriteLine("Check that the index file specified is properly formed.");
                    }
                    else
                    {
                        MassPropertySet mpSet = GetMassPropertiesFromDoc(doc);
                        xmlTags.AddRange(GenerateXMLTags(meshTitles[i], mpSet));
                        /*  It makes the most sense to close the solidworks document at this point...
                         *  but for some reason that was giving errors... maybe something about lazy loading?        
                         *  So instead we leave them all open until we are done with them                         */
                       // doc.Close();
                    }

                }
            }
            catch
            {
                Console.WriteLine("Problem while reading from solidworks files");
            }

            swApp.ExitApp();
            swApp = null;

            Console.WriteLine("Outputting to xacro");
            OutputToXacroFile(args[1], xmlTags);
        }
    }
}