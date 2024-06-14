


using System.Diagnostics;

namespace daily_worlde
{
    public partial class WordleForm : Form
    {
        private const string WordsTextFile = @"wordsForWordle.txt";
        private const int RowLength = 5;
        private const string PlayAgainMessage = "Play again?";

        private int previousRow = 0;
        private int hintsCount = 0;

        private string currentWord = string.Empty;
        private List<TextBox> currentBoxes = new List<TextBox>();

        public WordleForm()
        {
            InitializeComponent();

            StartNewGame();
            foreach (TextBox tb in this.Controls.OfType<TextBox>())
            {
                tb.MouseClick += FocusTextBox;
                tb.KeyDown += MoveCursor;
            }

        }

        private void MoveCursor(object? sender, KeyEventArgs e)
        {
            var pressedKey = e.KeyCode;
            var senderTextBox = sender as TextBox;
            var currentTextBoxIndex = int.Parse(senderTextBox.Name.Replace("textBox", ""));

            if (ShouldGoToLeftTextBox(pressedKey, currentTextBoxIndex))
            {
                currentTextBoxIndex--;
            }
            else if (ShouldGoToRightTextBox(pressedKey, currentTextBoxIndex))
            {
                currentTextBoxIndex++;
            }

            var textBox = GetTextBox(currentTextBoxIndex);
            textBox.Focus();
        }

        private TextBox GetTextBox(int index)
        {
            string textBoxName = $"textBox{index}";
            return this.Controls[textBoxName] as TextBox;
        }

        private bool ShouldGoToRightTextBox(Keys pressedKey, int currentTextBoxIndex)
            => (pressedKey == Keys.Right || IsAlphabetKeyPressed(pressedKey.ToString()))
            && !IsLastTextBox(currentTextBoxIndex);

        private bool IsLastTextBox(int currentTextBoxIndex)
            => currentTextBoxIndex % RowLength == 0;

        private bool IsAlphabetKeyPressed(string v)
            => v.Length == 1 && char.IsLetter(v[0]);

        private bool ShouldGoToLeftTextBox(Keys pressedKey, int currentTextBoxIndex)
            => pressedKey == Keys.Left && !IsFirstTextBox(currentTextBoxIndex);

        private bool IsFirstTextBox(int currentTextBoxIndex)
            => (currentTextBoxIndex + 4) % RowLength == 0;

        private void FocusTextBox(object? sender, MouseEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();
            }
        }

        private void StartNewGame()
        {
            var wordList = GetAllWords();
            var random = new Random();

            currentWord = wordList[random.Next(wordList.Count)];
            buttonSubmit.Enabled = true;
            buttonHint.Enabled = true;

            Debug.WriteLine(currentWord);
        }

        private List<string> GetAllWords()
        {
            return File.ReadAllLines(path: WordsTextFile)
                .Where(x => !string.IsNullOrWhiteSpace(x) && x.Length == RowLength)
                .ToList();
        }

        private void buttonSubmit_Click(object sender, EventArgs e)
        {
            var userWord = GetInput();
            if (!IsValidInput(userWord))
            {
                DisplayInvalidWordMessage();
                return;
            }

            ColorBoxes();

            if (IsWordGuessed(userWord))
            {
                FinalizeWinGame();
                return;
            }

            if (IsCurrentRowLast())
            {
                FinalizeGameLost();
                return;
            }

            ModifyTextBoxesAvailability(false);
            previousRow++;
            ModifyTextBoxesAvailability(true);

            var textBox = GetTextBox(GetFirstTextBoxIndexRow());
            textBox.Focus();
        }

        private void ModifyTextBoxesAvailability(bool shouldBeEnabled)
        {
            int firstTextBoxIndexOnRow = GetFirstTextBoxIndexRow();

            for (int i = 0; i < RowLength; i++)
            {
                var textBox = GetTextBox(firstTextBoxIndexOnRow + i);
                if (shouldBeEnabled)
                {
                    textBox.Enabled = true;
                    if (i == 0)
                    {
                        textBox.Focus();
                    }
                    textBox.ReadOnly = false;
                    textBox.TabStop = true;
                }
                else
                {
                    textBox.ReadOnly = true;
                    textBox.TabStop = false;
                }

            }
        }

        private void FinalizeGameLost()
        {
            MessageBox.Show($"The correct word is {currentWord}");
            this.buttonSubmit.Enabled = false;
            this.buttonHint.Enabled = false;
            this.buttonReset.Enabled = true;
        }

        private bool IsCurrentRowLast()
        {
            var columnsCount = 6;
            return this.previousRow == columnsCount - 1;
        }

        private void FinalizeWinGame()
        {
            MessageBox.Show("You win!");

            this.buttonSubmit.Enabled = false;
            this.buttonHint.Enabled = false;

            this.buttonReset.Text = PlayAgainMessage;

            ModifyTextBoxesAvailability(false);

        }

        private bool IsWordGuessed(string userWord)
        {
            return string.Compare(currentWord, userWord, true) == 0;
        }

        private void ColorBoxes()
        {
            for (int i = 0; i < this.currentBoxes.Count; i++)
            {
                var textBox = this.currentBoxes[i];
                var currentTextBoxChar = textBox.Text.ToLower().FirstOrDefault();

                if (!WordContainsChar(currentTextBoxChar))
                {
                    textBox.BackColor = Color.Gray;
                }
                else if (!IsCharOnCorrectIndex(i, currentTextBoxChar))
                {
                    textBox.BackColor = Color.Yellow;
                }
                else
                {
                    textBox.BackColor = Color.LightGreen;
                }
            }
        }

        private bool IsCharOnCorrectIndex(int i, char currentTextBoxChar)
            => this.currentWord[i] == currentTextBoxChar;

        private bool WordContainsChar(char currentTextBoxChar)
            => this.currentWord.IndexOf(currentTextBoxChar, StringComparison.InvariantCultureIgnoreCase) > -1;

        private void DisplayInvalidWordMessage()
        {
            MessageBox.Show("The word must have 5 letters!");
        }

        private bool IsValidInput(string input)
        {
            if (input.All(char.IsLetter) && input.Length == RowLength)
            {
                return true;
            }

            return false;
        }

        private string GetInput()
        {
            this.currentBoxes = new List<TextBox>();
            string tempString = string.Empty;

            int firstTextBoxIndex = GetFirstTextBoxIndexRow();
            for (int i = 0; i < RowLength; i++)
            {
                var textBox = GetTextBox(index: firstTextBoxIndex + i);
                if (string.IsNullOrEmpty(textBox.Text))
                {
                    return textBox.Text;
                }

                tempString += textBox.Text[0];
                this.currentBoxes.Add(textBox);
            }

            return tempString;
        }

        private int GetFirstTextBoxIndexRow()
            => this.previousRow * RowLength + 1;

        private void buttonReset_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void buttonHint_Click(object sender, EventArgs e)
        {
            this.hintsCount++;
            if (this.hintsCount >= 3)
            {
                this.buttonHint.Enabled = false;
            }

            var unavailbalePositions = GetUnavailablePositions();
            if (unavailbalePositions.Count == RowLength)
            {
                ShowInvalidUseOfHintMessage();
                return;
            }

            ReveralRandomWordLetter(unavailbalePositions);

        }

        private void ReveralRandomWordLetter(List<int> unavailbalePositions)
        {
            var random = new Random();

            while (true)
            {
                int randomIndex = random.Next(0, RowLength);
                int textBoxIndex = GetFirstTextBoxIndexRow() + randomIndex;
                var textBox = GetTextBox(textBoxIndex);
                if (textBox.Text != string.Empty)
                {
                    continue;
                }

                var hintLetter = this.currentWord[randomIndex].ToString();
                textBox.Text = hintLetter;
                unavailbalePositions.Add(textBoxIndex);

                break;
            }
        }

        private void ShowInvalidUseOfHintMessage()
        {
            MessageBox.Show("Free up space for a hint");
            this.buttonSubmit.Focus();
            this.hintsCount -= 1;
        }

        private List<int> GetUnavailablePositions()
        {
            int firstIndexOnRow = GetFirstTextBoxIndexRow();
            var positions = new List<int>();

            for (int i=0; i<RowLength; i++)
            {
                int textBoxIndex = firstIndexOnRow + i;
                var textBox = GetTextBox(textBoxIndex);
                if (!string.IsNullOrEmpty(textBox.Text))
                {
                    positions.Add(textBoxIndex);
                }
            }

            return positions;
        }
    }
}
