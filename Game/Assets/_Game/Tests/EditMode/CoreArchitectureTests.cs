using System.Linq;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class CoreArchitectureTests
    {
        // Guards the architecture rule: Core must stay engine-independent so the game logic
        // survives an engine change. (The asmdef's noEngineReferences already enforces it at compile time.)
        [Test]
        public void CoreAssembly_DoesNotReferenceUnity()
        {
            var referenced = typeof(Board).Assembly.GetReferencedAssemblies().Select(a => a.Name);

            Assert.That(referenced, Has.None.StartsWith("UnityEngine"));
            Assert.That(referenced, Has.None.StartsWith("UnityEditor"));
        }
    }
}
