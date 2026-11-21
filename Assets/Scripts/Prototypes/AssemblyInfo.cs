using System.Runtime.CompilerServices;

// The prototype benches: scenes of Assets/Scenes/Prototype that try a mechanic alone.
// The game never depends on them; only their tests reach their internal types.
[assembly: InternalsVisibleTo("Fungiiiii.Tests.EditMode")]
[assembly: InternalsVisibleTo("Fungiiiii.Tests.PlayMode")]
