using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MassPropertiesURDFGenerator
{
    class MassPropertySet
    {
        private double[] centreOfMass;
        private double mass;
        private double[] massMomentsOfInertia;
        private bool isValid;

        public MassPropertySet(double[] COM, double m, double[] IOM)
        {
            centreOfMass = COM;
            mass = m;
            massMomentsOfInertia = IOM;

            isValid = COM.Length == 3 && IOM.Length == 9;

            double magnitudeThreshold = 0.000001;
            for(int i = 0; i < 9; i++)
            {
                //if magnitude is less than 1 * 10 ^ -6
                if(massMomentsOfInertia[i] < magnitudeThreshold && massMomentsOfInertia[i] > (-1 * magnitudeThreshold))
                {
                    massMomentsOfInertia[i] = 0;
                }
            }
        }

        public bool IsValid() { return isValid; }

        /*  This Class is just a wrapper so that you don't have to remember which index is for which property
         * 
        * see: http://help.solidworks.com/2015/English/api/sldworksapi/SolidWorks.Interop.sldworks~SolidWorks.Interop.sldworks.IModelDocExtension~CreateMassProperty.html
        * for details
        */
        public double Mass() { return mass; }

        public double CoM_X() { return centreOfMass[0]; }
        public double CoM_Y() { return centreOfMass[1]; }
        public double CoM_Z() { return centreOfMass[2]; }

        public double MoI_XX() { return massMomentsOfInertia[0]; }
        public double MoI_XY() { return massMomentsOfInertia[1]; }
        public double MoI_XZ() { return massMomentsOfInertia[2]; }

        public double MoI_YX() { return massMomentsOfInertia[3]; }
        public double MoI_YY() { return massMomentsOfInertia[4]; }
        public double MoI_YZ() { return massMomentsOfInertia[5]; }

        public double MoI_ZX() { return massMomentsOfInertia[6]; }
        public double MoI_ZY() { return massMomentsOfInertia[7]; }
        public double MoI_ZZ() { return massMomentsOfInertia[8]; }

    }
}
