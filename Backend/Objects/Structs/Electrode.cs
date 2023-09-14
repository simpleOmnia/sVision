using UnityEngine;

namespace svision_internal
{
    public readonly struct Electrode
    {
        public readonly int electrodeNumber; 
        public readonly float x;
        public readonly float y;
        public readonly float z;

        public Electrode(int electrodeNumber, float x, float y, float z, float current=0)
        {
            this.electrodeNumber = electrodeNumber;
            this.x = x;
            this.y = y;
            this.z = z;
        }

        //Used to convert to ComputeElectrode for use in ComputeBuffers on 1D image array
        public int Calculate1DPixelIndex()
        {
            float screenX = UnitConverter.micronToScreenPos(x);
            float screenY = UnitConverter.micronToScreenPos(y);
            Debug.Log(electrodeNumber+": "+screenX+","+screenY);
            
            int xPixel = Mathf.CeilToInt((screenX - svision.Instance.minimumScreenPositionX) * svision.Instance.xResolution*
                                         (svision.Instance.maximumScreenPositionX-svision.Instance.minimumScreenPositionX)*svision.Instance.xResolution);
            int yPixel = Mathf.CeilToInt((screenY - svision.Instance.minimumScreenPositionY) * svision.Instance.yResolution *
                         (svision.Instance.maximumScreenPositionY-svision.Instance.minimumScreenPositionY));
        
            xPixel = Mathf.Clamp(xPixel, 0, svision.Instance.xResolution - 1);
            yPixel = Mathf.Clamp(yPixel, 0, svision.Instance.yResolution - 1);
        
            return yPixel * svision.Instance.yResolution + xPixel;
        }
        
        public override string ToString()
        {
            return this.electrodeNumber + " - "+this.x + ", " + this.y + ", " + this.z; 
        }
    }
}