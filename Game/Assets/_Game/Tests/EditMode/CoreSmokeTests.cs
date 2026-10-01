using System.Linq;
using NUnit.Framework;

namespace SweetBazaar.Core.Tests
{
    public class CoreSmokeTests
    {
        [Test]
        public void Add_ReturnsSum()
        {
            Assert.AreEqual(5, CoreSmoke.Add(2, 3));
        }

        // Permanent guard for the architecture rule: Core must stay engine-independent.
        // When CoreSmoke is deleted, point this at any other type from the Core assembly.
        [Test]
        public void CoreAssembly_DoesNotReferenceUnityEngine()
        {
            var referenced = typeof(CoreSmoke).Assembly.GetReferencedAssemblies().Select(a => a.Name);

            Assert.That(referenced, Has.None.StartsWith("UnityEngine"));
            Assert.That(referenced, Has.None.StartsWith("UnityEditor"));
        }
    }
}
