namespace svision_internal
{
    public struct Electrode
    {
        public int electrodeNumber; 
        public float x;
        public float y;
        public float z;
        public float current;

        public Electrode(int electrodeNumber, float x, float y, float z, float current=0)
        {
            this.electrodeNumber = electrodeNumber;
            this.x = x;
            this.y = y;
            this.z = z;
            this.current = current; 
        }

        public override string ToString()
        {
            return this.electrodeNumber + " - "+this.x + ", " + this.y + ", " + this.z + ": " + current; 
        }
    }
}