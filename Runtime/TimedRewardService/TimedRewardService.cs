using System;
using System.Threading.Tasks;

namespace Beamable.Server
{
   [Microservice("TimedRewardService")]
   public class TimedRewardService : Microservice
   {
      /// <summary>
      /// Attempts to claim a timed reward for a player.
      /// Returns true if the claim was made; false otherwise
      /// </summary>
      /// <param name="rewardRef">
      /// The reference to the TimedReward content definition that should be claimed
      /// </param>
      /// <returns>True if the claim is wade; false otherwise</returns>
      [ClientCallable]
      public async Task<bool> Claim(TimeRewardRef rewardRef)
      {
         // resolve content
         var reward = await rewardRef.Resolve();

         // get the current reward cycle
         var now = DateTime.UtcNow;
         var anchor = DateTime.Parse(reward.AnchorTime);
         var timeDiff = now.Subtract(anchor);
         var currentCycle = (long)timeDiff.TotalSeconds / reward.CycleDurationInSeconds;

         // record a timestamp for this attempt
         await Services.Stats.SetProtectedPlayerStat(Context.UserId, reward.GetStatAttemptKey(), now.ToFileTimeUtc().ToString());

         // get the last cycle the player resolved
         var statKey = reward.GetStatKey();
         var lastCycleClaimStat = await Services.Stats.GetProtectedPlayerStat(Context.UserId, statKey);
         if (!long.TryParse(lastCycleClaimStat, out var lastCycleClaimed))
         {
            lastCycleClaimed = 0;
         }

         if (lastCycleClaimed >= currentCycle)
         {
            // already claimed.
            return false;
         }

         // we should do the claim!
         await Services.Inventory.Update(builder =>
         {
            foreach (var currency in reward.RewardCurrencies)
            {
               builder.CurrencyChange(currency.currency.Id, currency.amount);
            }

            foreach (var item in reward.RewardItems)
            {
               builder.AddItem(item.item.Id, item.properties.Value);
            }
         });

         // update the player stat
         await Services.Stats.SetProtectedPlayerStat(Context.UserId, statKey, currentCycle.ToString());
         return true;
      }
   }
}
