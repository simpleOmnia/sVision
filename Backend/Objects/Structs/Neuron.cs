namespace svision_internal
{
    public struct Neuron {
        public float x;
        public float y;
        public float z;
        public float brightnessContribution;


        public Neuron(float x, float y, float z, float brightnessContribution) {
            this.x = x;
            this.y = y;
            this.z = z;
            this.brightnessContribution = brightnessContribution; }

        public override string ToString()
        { return x + ", " + y + ", " + z + ", " + ": " + brightnessContribution; }
    }
}