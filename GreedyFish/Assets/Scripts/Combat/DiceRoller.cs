using UnityEngine;

/// <summary>
/// Stateless utility for dice rolls.
/// Returns both the total and individual die values for the history log.
/// </summary>
public static class DiceRoller
{
    /// <summary>
    /// Rolls <paramref name="count"/> dice each with <paramref name="sides"/> faces, adds <paramref name="bonus"/>.
    /// Returns the total and exposes individual die results via <paramref name="individualResults"/>.
    /// </summary>
    public static int Roll(int count, int sides, int bonus, out int[] individualResults)
    {
        individualResults = new int[count];
        int total = bonus;

        for (int i = 0; i < count; i++)
        {
            int result = Random.Range(1, sides + 1);
            individualResults[i] = result;
            total += result;
        }

        return total;
    }

    /// <summary>Convenience overload — rolls a single die with <paramref name="sides"/> faces.</summary>
    public static int RollSingle(int sides)
    {
        return Random.Range(1, sides + 1);
    }
}
