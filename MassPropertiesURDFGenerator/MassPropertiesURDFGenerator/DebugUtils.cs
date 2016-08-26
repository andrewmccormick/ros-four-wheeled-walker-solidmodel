using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassPropertiesURDFGenerator
{
    class DebugUtils
    {
        static void OutputToConsole(double[] centreOfMass, double mass, double[] massMomentsOfInertia)
        {
            string[] COM_TITLES = { "x", "y", "z" };
            string[] MOI_TITLES = {
                "Mass Moment of Inertia XX",
                "Mass Moment of Inertia XY",
                "Mass Moment of Inertia XZ",
                "Mass Moment of Inertia YX",
                "Mass Moment of Inertia YY",
                "Mass Moment of Inertia YZ",
                "Mass Moment of Inertia ZX",
                "Mass Moment of Inertia ZY",
                "Mass Moment of Inertia ZZ"
            };

            if (centreOfMass.Length == 3)
            {
                for (int i = 0; i < 3; i++)
                    Console.WriteLine(COM_TITLES[i] + ": " + centreOfMass[i]);
            }
            else
            {
                Console.WriteLine("Expected 3 values for COM, got: " + centreOfMass.Length);
            }

            Console.WriteLine("Mass: " + mass);

            if (massMomentsOfInertia.Length == 9)
            {
                for (int i = 0; i < 9; i++)
                {
                    Console.WriteLine(MOI_TITLES[i] + ": " + massMomentsOfInertia[i]);
                }
            }
            else
            {
                Console.WriteLine("Expected 9 mass moments of inertia, got: " + massMomentsOfInertia.Length);
            }

            return;
        }
    }
}
