namespace Energy.App.Standalone.Extensions;

public static class ValidationExtensions
{
    public static bool eIsValidMpan(this string meterIdentifier)
    {
        const int MPAN_LENGTH = 13;

        if (meterIdentifier == null || meterIdentifier.Length != MPAN_LENGTH)
            return false;

        List<int> primeNumbers = new List<int>() { 3, 5, 7, 13, 17, 19, 23, 29, 31, 37, 41, 43 };
        List<int> digitCheckSumResults = new List<int>();

        int primeNumberIdx = 0;
        meterIdentifier
            .Substring(0, MPAN_LENGTH - 1)
            .ToCharArray()
            .Where(x => int.TryParse(x.ToString(), out int convertedInt))
            .Select(x => Convert.ToInt16(x.ToString()))
            .ToList()
            .ForEach(x => digitCheckSumResults.Add(x * primeNumbers[primeNumberIdx++]));

        ushort checkDigit = Convert.ToUInt16(meterIdentifier.Substring(MPAN_LENGTH - 1, 1));

        return checkDigit == digitCheckSumResults.Sum() % 11 % 10;
    }

    public static bool eIsNotValidMpan(this string meterIdentifier)
    {
        return !meterIdentifier.eIsValidMpan();
    }

    public static bool eIsDigitsOnly(this string str)
    {
        foreach (char c in str)
        {
            if (c < '0' || c > '9')
                return false;
        }

        return true;
    }

    public static bool eIsValidMprn(this string meterIdentifier)
    {
        if (meterIdentifier == null || meterIdentifier.Length < 6)
        {
            return false;
        }

        if (!meterIdentifier.eIsDigitsOnly())
        {
            return false;
        }

        char[] characters = meterIdentifier.ToCharArray();

        char[] checkChars = characters[^2..^0];
        char[] toCheck = characters[..^2];
        int length = toCheck.Length;

        int sumToCheck = 0;

        for (int index = 0; index < toCheck.Length; index++)
        {
            char charToCheck = toCheck[index];
            int digit = int.Parse(charToCheck.ToString());
            int digitCheckResult = digit * (length - index);
            sumToCheck += digitCheckResult;
        }

        string checkCharsString = new string(checkChars);
        int checkSum = int.Parse(checkCharsString);

        bool result = sumToCheck % 11 == checkSum;
        return result;
    }


    public static bool eIsNotValidMprn(this string meterIdentifier)
    {
        return !meterIdentifier.eIsValidMprn();
    }

    public static bool eIsValidOutcode(this string outcode)
    {
        if (String.IsNullOrWhiteSpace(outcode) || outcode.Length > 5)
        {
            return false;
        }
        // check starts with letter and contains at least one digit
        if(outcode[0].eIsLetter() && outcode.eContainsDigit())
        {
            return true;
        }
        return false;
    }

    public static bool eContainsDigit(this string str)
    {
        foreach (char c in str)
        {
            if (c >= '0' && c <= '9')
                return true;
        }

        return false;
    }

    public static bool eIsLetter(this char c)
    {
        return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
    }
}