using System.Reflection;

namespace RoslynCSharp.Compiler
{
    public sealed class AssemblyOutput
    {
        // Private
        private Assembly outputAssembly = null;
        private string assemblyFilePath = null;
        private string assemblyPDBFilePath = null;
        private byte[] assemblyImage = null;
        private byte[] assemblyPDBImage = null;

        // Properties
        public Assembly OutputAssembly
        {
            get { return outputAssembly; }
            internal set { outputAssembly = value; }
        }

        public bool HasFilePath
        {
            get { return assemblyFilePath != null; }
        }

        public string AssemblyFilePath
        {
            get { return assemblyFilePath; }
            internal set { assemblyFilePath = value; }
        }

        public string AssemblyPDBFilePath
        {
            get { return assemblyPDBFilePath; }
            internal set { assemblyPDBFilePath = value; }
        }

        public byte[] AssemblyImage
        {
            get { return assemblyImage; }
            internal set { assemblyImage = value; }
        }

        public byte[] AssemblyPDBImage
        {
            get { return assemblyPDBImage; }
            internal set { assemblyPDBImage = value; }
        }

        // Constructor
        internal AssemblyOutput() { }

        // Methods
        public void PatchAssemblyFilePath(string newAssemblyFilePath)
        {
            this.assemblyFilePath = newAssemblyFilePath;
        }

        public void PatchAssemblyPDBFilePath(string newAssemblyPDBFilePath)
        {
            this.assemblyPDBFilePath = newAssemblyPDBFilePath;
        }

        public void PatchAssemblyImage(byte[] newAssemblyImage)
        {
            this.assemblyImage = newAssemblyImage;
        }

        public void PatchAssemblyPDBImage(byte[] newAssemblyPDBImage)
        {
            this.assemblyPDBImage = newAssemblyPDBImage;
        }
    }
}
