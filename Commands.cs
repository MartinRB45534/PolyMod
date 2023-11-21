namespace PolyMod
{
	internal static class Commands
	{
		private static Dictionary<string, Action> _commands = new();

		internal static bool Add(string command, Action action) 
		{
			return _commands.TryAdd(command, action);
		}

		internal static bool Execute(string command) 
		{
			if (_commands.TryGetValue(command, out Action? action)) 
			{
				DebugConsole.Write(">" + command);
				action();
				return true;
			}
			return false;
		}

		internal static void Help()
		{
			foreach(var command in _commands.Keys) 
			{
				DebugConsole.Write(" " + command);
			}
		}
	}
}
