using WordleLibrary;

Console.WriteLine("Simple Wordle Puzzler & Solver");
Console.WriteLine("Mode?");
Console.WriteLine("   Interactive (i)");
Console.WriteLine("   Solver (s[:{Iterations}][:{StartWord}]:[{Dictionary}])");
Console.WriteLine("      Iterations is the number of iterations (default: 10)");
Console.WriteLine("      StartWord is the word to start solving with (default: CRANE)");
Console.WriteLine("      Dictionary is the dictionary to solve against, either Complete or Solutions (default: Complete)");
Console.WriteLine("   Solve for all solution words (a)");
Console.WriteLine("   Letter Occurrences (o)");

var mode = Console.ReadLine();
if (!string.IsNullOrWhiteSpace(mode) && (mode.Equals("o", StringComparison.OrdinalIgnoreCase)))
{
    ModeOccurrences();
}
else if (!string.IsNullOrWhiteSpace(mode) && (mode.Equals("i", StringComparison.OrdinalIgnoreCase)))
{
    ModeInteractive();
}
else if (!string.IsNullOrWhiteSpace(mode) && (mode.Equals("a", StringComparison.OrdinalIgnoreCase)))
{
    ModeSolveAll();
}
else
{
    int iterations = 10;
    string startWord = "crane";
    WordsLibrary.WordleDictionary dictionary = WordsLibrary.WordleDictionary.Complete;
    if (mode != null && mode.IndexOf(":") != -1)
    {
        var modeParts = mode.Split(":");

        if (modeParts.Length > 1 && int.TryParse(modeParts.ElementAt(1), out int i))
        {
            iterations = i;
        }
        if (modeParts.Length > 2)
        {
            var w = Word.Create(modeParts.ElementAt(2));
            if (w == null || !w.IsValid())
            {
                Console.WriteLine($"Start word not recognized; using {startWord}");
            }
            else
            {
                startWord = w.ToString();
            }
        }
        if (modeParts.Length > 3)
        {
            if (Enum.TryParse(modeParts.ElementAt(3), true, out WordsLibrary.WordleDictionary result))
            {
                dictionary = result;
            }
        }
    }

    ModeSolver(iterations, startWord, dictionary);
}

Console.WriteLine();
Console.WriteLine("Press any key to quit.");
Console.ReadLine();

static void ModeInteractive()
{
    Console.WriteLine("Enter a start word to begin a game");

    Game:
    var game = Game.Create();
    // Console.WriteLine($"Hint: {game.Solution}");

    while (!game.IsComplete())
    {
    Word:
        var word = Console.ReadLine();

        try
        {
            var result = game.Guess(word);
            if (result?.Word == null)
            {
                Console.WriteLine("Not recognized");
            }
            else
            {
                WriteFormattedWord(result.Word);
                if (game.IsSolved())
                {
                    Console.WriteLine($"Solved!");
                    break;
                }
                else if (game.IsComplete())
                {
                    Console.WriteLine($"You lose; the answer was {result.Solution}");
                    break;
                }
            }

            // Helpful when debugging
            // var solver = Solver.Create();
            // var suggested = solver.SolveOne(game);
            // Console.WriteLine($"Suggested: {suggested}");
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception.Message);
            goto Word;
        }
    }

    Console.WriteLine("Play again? (yes | no)");
    var play = Console.ReadLine();
    if (!string.IsNullOrWhiteSpace(play) && (play.Equals("y", StringComparison.OrdinalIgnoreCase) || play.Equals("yes", StringComparison.OrdinalIgnoreCase)))
    {
        goto Game;
    }
}

static void ModeOccurrences()
{
    Console.WriteLine();
    Console.WriteLine("The occurrences of each letter in the Wordle dictionaries");
    Console.WriteLine(WordsLibrary.Instance.LetterPositionOccurrences.ToCsv());
}

static void ModeSolveAll()
{
    var solver = Solver.Create();
    var results = GamePlayResults.Create();

    foreach (var word in WordsLibrary.Instance.Solutions)
    {
        var game = Game.Create();
        while (!game.IsComplete())
        {
            var guess = game.Words.Count().Equals(0) ? word.ToString() : solver.SolveOne(game)?.ToString();
            game.Guess(guess);

            if (game.IsSolved())
            {
                break;
            }
        }

        results.AddGame(game);
        Console.WriteLine($"{game.Words.First()}\t{game.IsSolved()} ({game.Words.Count()})");
    }

    Console.WriteLine();
    var group = results.Games.Where(g => g.IsSolved()).GroupBy(g => g.Words.Count()).Where(g => g.Key > 1).OrderBy(g => g.Key).ToList();
    group.ForEach(item => { Console.WriteLine($"Solved in {item.Key}: {item.Count()}"); });
    Console.WriteLine($"Not solved: {results.Games.Count(g => !g.IsSolved())}");
}

static void ModeSolver(int iterations, string startWord, WordsLibrary.WordleDictionary dictionary)
{
    var solver = Solver.Create(dictionary);
    var results = GamePlayResults.Create();

    for (int i = 0; i < iterations; i++)
    {
        var game = Game.Create();
        while (!game.IsComplete())
        {
            var guess = game.Words.Count().Equals(0) ? startWord : solver.SolveOne(game)?.ToString();
            game.Guess(guess);

            if (game.IsSolved())
            {
                break;
            }
        }

        results.AddGame(game);

        Console.WriteLine($"Success? {(game.IsSolved() ? "Yes" : "No")} ({(game.IsSolved() ? game.Words.Count() : Game.WordLimit)})");
    }

    Console.WriteLine();
    foreach (var lost in results.Games.Where(g => !g.IsSolved()))
    {
        foreach (var word in lost.Words)
        {
            WriteFormattedWord(word);
        }
        Console.WriteLine("===============");
        WriteFormattedWord(lost.Solution);
        Console.WriteLine();
    }

    Console.WriteLine($"Dictionary: {dictionary}");
    Console.WriteLine($"Average words per solved game: {Math.Round(results.WordsPerSolvedGameAverage, 4)}");
    Console.WriteLine($"Solved games percentile: {Math.Round(results.WinLoss, 2)}%");
}

static void WriteFormattedWord(Word? word)
{
    if (word?.Letters == null)
    {
        return;
    }

    var current = Console.ForegroundColor;

    try
    {
        word.Letters.ToList().ForEach(v =>
        {
            switch (v.Correctness)
            {
                case Letter.CorrectnessLevel.InWordCorrectPosition:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Letter.CorrectnessLevel.InWordDifferentPosition:
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    break;
                default:
                    Console.ForegroundColor = current;
                    break;
            }
            Console.Write($" {v.Value} ");
        });
    }
    finally
    {
        Console.ForegroundColor = current;
        Console.WriteLine();
    }
}
