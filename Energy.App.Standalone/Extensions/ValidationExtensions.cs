namespace Energy.App.Standalone.Extensions;

public static class ValidationExtensions
{
    public static bool eIsValidMpan(this string meterIdentifier)
    {
        const int MPAN_LENGTH = 13;

        if (meterIdentifier == null || meterIdentifier.Length != MPAN_LENGTH)
            return false;

        var primeNumbers = new List<int>() { 3, 5, 7, 13, 17, 19, 23, 29, 31, 37, 41, 43 };
        var digitCheckSumResults = new List<int>();

        int primeNumberIdx = 0;
        meterIdentifier
            .Substring(0, MPAN_LENGTH - 1)
            .ToCharArray()
            .Where(x => int.TryParse(x.ToString(), out int convertedInt))
            .Select(x => Convert.ToInt16(x.ToString()))
            .ToList()
            .ForEach(x => digitCheckSumResults.Add(x * primeNumbers[primeNumberIdx++]));

        var checkDigit = Convert.ToUInt16(meterIdentifier.Substring(MPAN_LENGTH - 1, 1));

        return checkDigit == digitCheckSumResults.Sum() % 11 % 10;
    }

    public static bool eIsNotValidMpan(this string meterIdentifier)
    {
        return !meterIdentifier.eIsValidMpan();
    }

    public static bool eIsValidMprn(this string meterIdentifier)
    {
        var characters = meterIdentifier.ToCharArray();

        var checkChars = characters[^2..^0];
        var toCheck = characters[..^2];
        int length = toCheck.Length;

        int sumToCheck = 0;

        for (var index = 0; index < toCheck.Length; index++)
        {
            var charToCheck = toCheck[index];
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
}