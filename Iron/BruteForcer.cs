using System;
using System.Collections.Generic;
using System.Text;

namespace IronWASP
{
    public class BruteForcer
    {
        string Characters = "";
        int MinLength = 0;
        int MaxLength = 0;

        int CurrrentLength = 0;
        List<int> Positions = new List<int>();

        int Count = 0;
        int CurrentPositionCount = 0;

        public int TotalCount
        {
            get
            {
                return Count;
            }
        }

        public BruteForcer(string CharactersP, int MinLengthP, int MaxLengthP)
        {
            if (CharactersP.Length == 0)
            {
                throw new Exception("Empty character set specified");
            }
            if (MaxLengthP < MinLengthP)
            {
                throw new Exception("Invalid range specified, maximum length cannot be less than minimum length");
            }
            if (MaxLengthP < 1)
            {
                throw new Exception("Invalid range specified, maximum length cannot be less than 1");
            }

            int i = 0;
            while(i < CharactersP.Length)
            {
                for (int j = 0; j < CharactersP.Length; j++)
                {
                    if (i != j)
                    {
                        if (CharactersP[i] == CharactersP[j])
                        {
                            throw new Exception(string.Format("The characters list is not unique, '{0}' appears more than once", CharactersP[i]));
                        }
                    }
                }
                i++;
            }

            this.Characters = CharactersP;
            if (MinLengthP > 0)
            {
                this.MinLength = MinLengthP;
            }
            else
            {
                this.MinLength = 1;
            }
            this.MaxLength = MaxLengthP;

            this.CurrrentLength = this.MinLength;
            this.Positions = new List<int>();
            for (int ii = 0; ii < this.MinLength; ii++)
            {
                this.Positions.Add(0);
            }
            this.Positions[this.Positions.Count - 1] = -1;

            Count = 0;
            for (int j = MinLength; j <= MaxLength; j++)
            {
                Count = Count + (int) Math.Pow(Characters.Length, j);
            }
        }

        public bool HasMore()
        {
            if (CurrentPositionCount < TotalCount)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public string GetNext()
        {
            MoveToNextPosition();
            return GetStringForCurrentPosition();
        }

        string GetStringForCurrentPosition()
        {
            StringBuilder SB = new StringBuilder();
            for (int i = 0; i < this.Positions.Count; i++)
            {
                SB.Append(Characters[this.Positions[i]]);
            }
            return SB.ToString();
        }

        void MoveToNextPosition()
        {
            if (HasMore())
            {
                this.Positions[this.Positions.Count - 1] = this.Positions[this.Positions.Count - 1] + 1;
                CurrentPositionCount++;
                for (int i = this.Positions.Count - 1; i >= 0; i--)
                {
                    if (this.Positions[i] >= Characters.Length)
                    {
                        for (int ii = i; ii < this.Positions.Count; ii++)
                        {
                            this.Positions[ii] = 0;
                        }
                        if (i == 0)
                        {
                            if (this.Positions.Count < this.MaxLength)
                            {
                                this.Positions.Insert(0, 0);
                                return;
                            }
                            else
                            {
                                throw new Exception("Reached the end of brute force list");
                            }
                        }
                        else
                        {
                            this.Positions[i - 1] = this.Positions[i - 1] + 1;
                        }
                    }
                }
            }
            else
            {
                throw new Exception("Reached the end of brute force list");
            }
        }
    }
}
