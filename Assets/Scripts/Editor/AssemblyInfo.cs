using System.Runtime.CompilerServices;

// Tests exercise internal editor tooling directly rather than widening the
// public API just to make it reachable. See the unity-testing skill.
[assembly: InternalsVisibleTo("Fungiiiii.Tests.EditMode")]
