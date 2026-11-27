using System.Runtime.CompilerServices;

// Tests exercise internal types directly rather than widening the public API
// just to make them reachable. See the unity-testing skill.
[assembly: InternalsVisibleTo("Fungiiiii.Tests.EditMode")]
[assembly: InternalsVisibleTo("Fungiiiii.Tests.PlayMode")]

// The editor tooling wires internal components into the scenes it writes.
[assembly: InternalsVisibleTo("Fungiiiii.Editor")]

// The prototype benches build on the game's internal types; the game never uses theirs.
[assembly: InternalsVisibleTo("Fungiiiii.Prototypes")]
