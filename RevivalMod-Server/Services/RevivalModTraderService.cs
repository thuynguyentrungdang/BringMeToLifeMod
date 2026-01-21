using SPTarkov.DI.Annotations;
using SPTarkov.Server.Core.DI;
using SPTarkov.Server.Core.Extensions;
using SPTarkov.Server.Core.Models.Common;
using SPTarkov.Server.Core.Models.Eft.Common.Tables;
using SPTarkov.Server.Core.Models.Enums;
using SPTarkov.Server.Core.Models.Spt.Mod;
using SPTarkov.Server.Core.Models.Utils;
using SPTarkov.Server.Core.Services;

namespace RevivalModServer.Services;

[Injectable]
public class RevivalModTraderService(ISptLogger<RevivalModTraderService> logger, DatabaseService databaseService)
{
    public void AddNewTradeOffer(string traderName, string currencyType, string itemId, int price, int loyaltyLevel)
    {
        try
        {
            string traderId = traderName switch
            {
                "Therapist" => "54cb57776803fa99248b456e",
                "Prapor" => "54cb50c76803fa8b248b4571",
                "Mechanic" => "5a7c2eca46aef81a7ca2145d",
                "Ragman" => "5ac3b934156ae10c4430e83c",
                "Jaeger" => "5c0647fdd443bc2504c2d371",
                "Peacekeeper" => "5935c25fb3acc3127c3d8cd9",
                "Ref" => "6617beeaa9cfa777ca915b7c",
                "Skier" => "58330581ace78e27b8b10cee",
                //default to therapist if the trader name is incorrect
                _ => "54cb57776803fa99248b456e"
            };
            string currencyId = currencyType switch
            {
                "USD" => ItemTpl.MONEY_DOLLARS,
                "RUB" => ItemTpl.MONEY_ROUBLES,
                "EUR" => ItemTpl.MONEY_EUROS,
                "GP" => ItemTpl.MONEY_GP_COIN,
                //default to rub if the currency id is incorrect
                _ => ItemTpl.MONEY_ROUBLES
            };
            MongoId uid = new();
            Item item = new()
            {
                Upd = new Upd
                {
                    UnlimitedCount = true,
                    StackObjectsCount = 99999
                },
                Id = uid,
                Template = itemId,
                ParentId = "hideout",
                SlotId = "hideout"
            };
            List<List<BarterScheme>> barterScheme = new() // taken from svm, idk why bsg made it like this
            {
                new List<BarterScheme>
                {
                    new BarterScheme
                    {
                        Count = Convert.ToDouble(price),
                        Template = currencyId
                    }
                }
            };
            var traders = databaseService.GetTraders();
            traders[traderId].Assort.Items.Add(item);
            traders[traderId].Assort.BarterScheme.Add(uid, barterScheme);
            traders[traderId].Assort.LoyalLevelItems.Add(uid, loyaltyLevel);
        }
        catch (Exception ex)
        {
            logger.Error("[Revival Mod] Unable to add custom defib offer to traders \n" + ex);
        }

    }
}