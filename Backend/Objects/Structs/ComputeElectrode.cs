using UnityEngine;

namespace svision_internal
{
    public struct ComputeElectrode
    {
        public int electrodeNumber;
        public int location1D;
        public float current;

        public ComputeElectrode(Electrode inputElectrode)
        {
            electrodeNumber = inputElectrode.electrodeNumber;
            location1D = inputElectrode.Calculate1DPixelIndex();
            current = 0;
        }

        public override string ToString()
        { return electrodeNumber + " (" + location1D + "): " + current; }
    }
}