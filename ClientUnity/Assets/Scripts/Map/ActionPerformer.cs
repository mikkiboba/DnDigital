using UnityEngine;

public static class ActionPerformer
{
    

    public static int PerformActionOnEntity(CreatureEntity.Action action, WorldEntity target)
    {
        if (target == null)
        {
            
            return -1; 
        }

        int damageAmount = CalculateDamage(action.damage);
        
        int pf = Mathf.Max(0, target.Current_Pf - damageAmount);

        Debug.Log($"{action.actionName} performed on {target.Name}. Dealt {damageAmount} damage. " +
                  $"Current PF: {pf}/{target.Max_Pf}");

        // Return the new PF value
        return pf;
    }

    private static int CalculateDamage(string damage)
    {
        if (int.TryParse(damage, out int numDamage))
        {
            return numDamage;
        }

        // notation "2d6+3", "1d8", "3d4-1" ecc
        return RollDiceNotation(damage);
    }

    private static int RollDiceNotation(string diceNotation)
    {
        diceNotation = diceNotation.Replace(" ", "");

        try
        {
            int dIndex = diceNotation.IndexOf('d');
            if (dIndex == -1)
            {
                Debug.LogWarning($"Invalid dice notation: {diceNotation}");
                return 0;
            }

            // Parse number of dice
            string numDiceStr = diceNotation.Substring(0, dIndex);
            if (!int.TryParse(numDiceStr, out int numDice))
            {
                Debug.LogWarning($"Invalid number of dice in: {diceNotation}");
                return 0;
            }

            int modifierIndex = diceNotation.IndexOfAny(new char[] { '+', '-' }, dIndex + 1);
            string diceSidesStr;
            int modifier = 0;

            if (modifierIndex == -1)
            {
                // No modifier
                diceSidesStr = diceNotation.Substring(dIndex + 1);
            }
            else
            {
                diceSidesStr = diceNotation.Substring(dIndex + 1, modifierIndex - dIndex - 1);
                string modifierStr = diceNotation.Substring(modifierIndex);
                if (!int.TryParse(modifierStr, out modifier))
                {
                    Debug.LogWarning($"Invalid modifier in: {diceNotation}");
                    return 0;
                }
            }

            if (!int.TryParse(diceSidesStr, out int diceSides))
            {
                Debug.LogWarning($"Invalid dice sides in: {diceNotation}");
                return 0;
            }

            // Roll 
            int total = 0;
            System.Text.StringBuilder rollDetails = new System.Text.StringBuilder();
            rollDetails.Append($"Rolling {numDice}d{diceSides}");

            for (int i = 0; i < numDice; i++)
            {
                int roll = UnityEngine.Random.Range(1, diceSides + 1);
                total += roll;

                if (i == 0)
                    rollDetails.Append($": {roll}");
                else
                    rollDetails.Append($", {roll}");
            }

            // Add modifier
            if (modifier != 0)
            {
                total += modifier;
                rollDetails.Append($" {(modifier > 0 ? "+" : "")}{modifier} = {total}");
            }
            else
            {
                rollDetails.Append($" = {total}");
            }

            Debug.Log(rollDetails.ToString());
            return total;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error parsing dice notation '{diceNotation}': {e.Message}");
            return 0;
        }
    }

}
