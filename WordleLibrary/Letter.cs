namespace WordleLibrary
{
    public class Letter
    {
        public const string Alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        public enum CorrectnessLevel
        {
            NotScored,
            NotInWord,
            InWordDifferentPosition,
            InWordCorrectPosition
        }

        private Letter() { }

        public uint Position { get; private set; }

        public char Value { get; private set; }

        public CorrectnessLevel Correctness { get; internal set; }

        internal static Letter Create(char value, uint position, CorrectnessLevel correctness)
        {
            return new Letter
            {
                Value = value,
                Position = position,
                Correctness = correctness
            };
        }

        public static bool IsValid(char value) => Alphabet.IndexOf(value.ToString().ToUpper()) != -1;
    }

    public class LetterOccurrence
    {
        private LetterOccurrence()
        {
            Count = 0;
        }

        public char Letter { get; private set; }

        public uint Count { get; private set; }

        public void AddOne()
        {
            Count++;
        }

        public static LetterOccurrence Create(char letter)
        {
            return new LetterOccurrence
            {
                Letter = letter
            };
        }
    }

    public class LetterOccurrences
        : List<LetterOccurrence>
    {
        private LetterOccurrences()
        {
            Letter.Alphabet.ToCharArray().ToList().ForEach(c => {
                this.Add(LetterOccurrence.Create(c));
            });
        }

        public void AddOne(Letter letter)
        {
            if (letter?.Value == null)
            {
                return;
            }

            var l = this.FirstOrDefault(o => o.Letter == letter.Value);
            if (l != null)
            {
                l.AddOne();
            }
        }

        public static LetterOccurrences Create()
        {
            return new LetterOccurrences();
        } 
    }
}