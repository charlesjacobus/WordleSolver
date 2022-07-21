namespace WordleLibrary
{
    public class Word
    {
        public const uint LetterLimit = 5;

        private Word(Letter[] letters)
        {
            Letters = letters;
        }

        public Letter[] Letters { get; private set; }

        public bool IsRecogized()
        {
            return WordsLibrary.Instance.Exists(this);
        }

        public bool IsSolved()
        {
            return Letters.All(l => l.Correctness == Letter.CorrectnessLevel.InWordCorrectPosition);
        }

        public bool IsValid() => Letters.All(v => Letter.IsValid(v.Value));

        public bool HasAllLetters(IEnumerable<Letter> letters)
        {
            foreach (var letter in letters)
            {
                if (LetterPosition(letter) == null)
                {
                    return false;
                }
            }

            return true;
        }

        public bool HasAnyLetter(IEnumerable<Letter> letters)
        {
            foreach (var letter in letters)
            {
                if (LetterPosition(letter) != null)
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasAtLeastOneLetterInSamePosition(IEnumerable<Letter> letters)
        {
            foreach (var letter in letters)
            {
                var l = Letters.ElementAt((int)letter.Position - 1);
                if (l != null && l.Value == letter.Value)
                {
                    return true;
                }
            }

            return false;
        }

        public uint? LetterPosition(Letter letter)
        {
            if (letter == null)
            {
                return null;
            }

            var index = Letters.Select(v => v.Value).ToList().IndexOf(letter.Value);

            return index == -1 ? null : (uint)index + 1;
        }

        public bool MatchesOn(char letter, uint position)
        {
            var index = (int)position - 1;

            return Equals(letter, Letters?.AsEnumerable().ElementAt(index).Value);
        }

        public override string ToString()
        {
            return string.Join(string.Empty, Letters.Select(v => v.Value.ToString()));
        }

        public static Word? Create(string? value, bool validate = true)
        {
            if (string.IsNullOrEmpty(value) || value.Length != LetterLimit)
            {
                return null;
            }

            var letters = new List<Letter>();
            for (int i = 0; i < value.Length; i++)
            {
                letters.Add(Letter.Create(char.ToUpper(value[i]), (uint)i + 1, Letter.CorrectnessLevel.NotScored));
            }
            var word = Create(letters.ToArray());
            if (word == null || (validate && (!word.IsValid() || !word.IsRecogized())))
            {
                return null;
            }

            return word;
        }

        internal static Word? Create(Letter[] letters)
        {
            if (!IsValid(letters))
            {
                return null;
            }

            return new Word(letters);
        }

        internal static Word Score(Word word, Word solution)
        {
            ArgumentNullException.ThrowIfNull(nameof(word));
            ArgumentNullException.ThrowIfNull(nameof(solution));


            for (int i = 0; i < word.Letters.Length; i++)
            {
                if (word.Letters.ElementAt(i).Value == solution.Letters.ElementAt(i).Value)
                {
                    word.Letters.ElementAt(i).Correctness = Letter.CorrectnessLevel.InWordCorrectPosition;
                }
                else if (string.Join(string.Empty, solution.Letters.Select(v => v.Value)).IndexOf(word.Letters.ElementAt(i).Value.ToString()) != -1)
                {
                    word.Letters.ElementAt(i).Correctness = Letter.CorrectnessLevel.InWordDifferentPosition;
                }
                else
                {
                    word.Letters.ElementAt(i).Correctness = Letter.CorrectnessLevel.NotInWord;
                }
            }

            return word;
        }

        private static bool IsValid(Letter[] value)
        {
            return value != null
                &&
                value.Length == LetterLimit;
        }
    }
}
