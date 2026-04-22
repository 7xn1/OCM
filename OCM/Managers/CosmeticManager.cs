using GorillaNetworking;
using static GorillaNetworking.CosmeticsController;

namespace OCM.Managers;

public static class CosmeticManager {
    public static void ShowCosmetics(bool enable) {
        IEnumerable<CosmeticItem> headItems = instance.currentWornSet.items.Where(
                item => item.itemCategory is CosmeticCategory.Face or CosmeticCategory.Hat);

        foreach (CosmeticItem item in headItems) {
            CosmeticItemRegistry? registry = GorillaTagger.Instance.offlineVRRig.cosmeticsObjectRegistry;
            CosmeticItemInstance? cosmetic = registry.Cosmetic(item.displayName);

            CosmeticSlots slot;
                
            switch (item.itemCategory) {
                case CosmeticCategory.Face: slot = CosmeticSlots.Face; break;
                case CosmeticCategory.Hat:  slot = CosmeticSlots.Hat; break;

                case CosmeticCategory.None:
                case CosmeticCategory.Badge:
                case CosmeticCategory.Paw:
                case CosmeticCategory.Chest:
                case CosmeticCategory.Fur:  
                case CosmeticCategory.Shirt:
                case CosmeticCategory.Back:
                case CosmeticCategory.Arms:
                case CosmeticCategory.Pants:
                case CosmeticCategory.TagEffect:
                case CosmeticCategory.Count:
                case CosmeticCategory.Set:
                default: continue;
            }

            if (enable) cosmetic.EnableItem(slot, VRRig.LocalRig);
            else cosmetic.DisableItem(slot);
        }
    }
}