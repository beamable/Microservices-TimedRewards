using System.Collections.Generic;
using Beamable.Common.Content;
using Beamable.Common.Content.Validation;
using Beamable.Common.Inventory;

[ContentType("timedRewards")]
[System.Serializable]
public class TimeRewardContent : ContentObject
{
   public List<RewardCurrency> RewardCurrencies;
   public List<RewardItem> RewardItems;

   [MustBeDateString]
   public string AnchorTime = "2021-01-01T12:00:00Z";

   [MustBePositive]
   public long CycleDurationInSeconds = 86400; // one day's worth of seconds

   private const string TIMED_REWARD_STAT_PREFX = "timedReward";

   public string GetStatKey()
   {
      return $"{TIMED_REWARD_STAT_PREFX}-claimed-{Id}";
   }
   public string GetStatAttemptKey()
   {
      return $"{TIMED_REWARD_STAT_PREFX}-attempted-{Id}";
   }
}

[System.Serializable]
public class TimeRewardRef : ContentRef<TimeRewardContent>
{
   public TimeRewardRef(string id)
   {
      Id = id;
   }

   public TimeRewardRef()
   {

   }
}

[System.Serializable]
public class RewardCurrency
{
   public int amount;

   [MustReferenceContent]
   public CurrencyRef currency;
}

[System.Serializable]
public class RewardItem
{
   public OptionalSerializableDictionaryStringToString properties;

   [MustReferenceContent]
   public ItemRef item;
}
