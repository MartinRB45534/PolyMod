namespace PolyMod
{
	internal static class Commands
	{
		private static Dictionary<string, Tuple<string, Action<string[]>>> _commands = new();

		internal static bool Add(string command, string args, Action<string[]> action)
		{
			return _commands.TryAdd(command.ToLower(), new(args, action));
		}

		internal static bool Execute(string input)
		{
			List<string> parts = input.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
			for (int i = 0; i < parts.Count; i++)
			{
				string text = parts[i];
				if (text.Contains("\""))
				{
					for (int j = i + 1; j < parts.Count; j++)
					{
						string text2 = parts[j];
						text = text + " " + text2;
						parts.RemoveAt(j--);
						if (text2.Contains("\""))
						{
							break;
						}
					}
					parts[i] = text.Replace("\"", "");
				}
			}

			string command = parts[0].ToLower();
			string[] args = parts.Skip(1).ToArray();

			if (_commands.TryGetValue(command, out var data))
			{
				DebugConsole.Write(">" + input);
				data.Item2(args);
				return true;
			}
			return false;
		}

		internal static void Help()
		{
			foreach (var command in _commands.Distinct())
			{
				DebugConsole.Write(" " + command.Key + " " + command.Value.Item1);
			}
		}
	}
}
