
namespace OneCog.Io.Owl.Intuition.Device
{
    public class Type
    {
        public static readonly Type IntuitionC = new Type("CMR180");


        private Type(string name)
        {
            Name = name;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Type that = obj as Type;

            return (that != null && string.Equals(that.Name, this.Name, System.StringComparison.CurrentCultureIgnoreCase));
        }

        public string Name { get; private set; }
    }
}
