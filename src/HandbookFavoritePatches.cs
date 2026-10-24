using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace RecipeFavorites
{
    /// <summary>
    /// El kitabı (handbook) listesinde favorilenen satırların yanına küçük bir ★ ikonu çizer.
    /// Sadece görsel bir ekleme yaptığı için (orijinal metodu değiştirmiyor, sadece "postfix"
    /// olarak arkasından çalışıyor) en güvenli patch türüdür.
    /// </summary>
    internal static class StarIconRenderer
    {
        private static LoadedTexture starTexture;

        /// <summary>
        /// Dünyadan çıkıldığında ekran kartındaki doku (texture) geçersiz hale gelir.
        /// Referansı bırakmazsak, tekrar dünyaya girildiğinde kod "dokum zaten var"
        /// sanıp ölü bir dokuyu çizmeye çalışır ve yıldızlar görünmez olur.
        /// Dispose() çağırmıyoruz çünkü o noktada GL bağlamı çoktan yok olmuş olabilir.
        /// </summary>
        public static void ResetForNewSession()
        {
            starTexture = null;
        }

        private static void EnsureTexture(ICoreClientAPI capi)
        {
            if (starTexture != null) return;
            var font = CairoFont.WhiteMediumText().WithColor(new double[] { 1, 0.82, 0.15, 1 }).WithFontSize(22);
            starTexture = new TextTextureUtil(capi).GenTextTexture("★", font);
        }

        public static void DrawIfFavorited(ICoreClientAPI capi, string pageCode, double x, double y, double cellWidth, double cellHeight)
        {
            if (FavoritesData.Instance == null || !FavoritesData.Instance.IsFavorite(pageCode)) return;

            EnsureTexture(capi);
            if (starTexture == null) return;

            double drawX = x + cellWidth - starTexture.Width - GuiElement.scaled(14);
            double drawY = y + cellHeight / 2 - starTexture.Height / 2;

            capi.Render.Render2DTexturePremultipliedAlpha(
                starTexture.TextureId, (float)drawX, (float)drawY, starTexture.Width, starTexture.Height, 200
            );
        }
    }

    [HarmonyPatch(typeof(GuiHandbookItemStackPage), "RenderListEntryTo")]
    internal static class Patch_ItemStackPage_RenderListEntryTo
    {
        static void Postfix(GuiHandbookItemStackPage __instance, ICoreClientAPI capi, double x, double y, double cellWidth, double cellHeight)
        {
            StarIconRenderer.DrawIfFavorited(capi, __instance.PageCode, x, y, cellWidth, cellHeight);
        }
    }

    [HarmonyPatch(typeof(GuiHandbookGroupedItemstackPage), "RenderListEntryTo")]
    internal static class Patch_GroupedItemStackPage_RenderListEntryTo
    {
        static void Postfix(GuiHandbookGroupedItemstackPage __instance, ICoreClientAPI capi, double x, double y, double cellWidth, double cellHeight)
        {
            StarIconRenderer.DrawIfFavorited(capi, __instance.PageCode, x, y, cellWidth, cellHeight);
        }
    }

    [HarmonyPatch(typeof(GuiHandbookMealRecipePage), "RenderListEntryTo")]
    internal static class Patch_MealRecipePage_RenderListEntryTo
    {
        static void Postfix(GuiHandbookMealRecipePage __instance, ICoreClientAPI capi, double x, double y, double cellWidth, double cellHeight)
        {
            StarIconRenderer.DrawIfFavorited(capi, __instance.PageCode, x, y, cellWidth, cellHeight);
        }
    }

    /// <summary>
    /// Handbook'un sol sekme listesine bir "Favoriler" sekmesi ekler.
    /// </summary>
    [HarmonyPatch(typeof(GuiDialogSurvivalHandbook), "genTabs")]
    internal static class Patch_GenTabs
    {
        public const string FavoritesCategoryCode = "recipefavorites-tab";

        static void Postfix(ref GuiTab[] __result)
        {
            var list = __result.ToList();
            list.Add(new HandbookTab
            {
                DataInt = list.Count,
                Name = "★ Favoriler",
                CategoryCode = FavoritesCategoryCode
            });
            __result = list.ToArray();
        }
    }

    /// <summary>
    /// "Favoriler" sekmesine tıklanınca listeyi sadece favorilenen sayfaları gösterecek şekilde filtreler.
    /// Bu alanlar (shownHandbookPages, allHandbookPages, overviewGui) orijinal sınıfta protected/internal
    /// olduğu için Harmony'nin Traverse aracıyla erişiyoruz.
    /// </summary>
    [HarmonyPatch(typeof(GuiDialogHandbook), "OnTabClicked")]
    internal static class Patch_OnTabClicked
    {
        static bool Prefix(GuiDialogHandbook __instance, GuiTab tab)
        {
            if (!(tab is HandbookTab htab) || htab.CategoryCode != Patch_GenTabs.FavoritesCategoryCode)
            {
                return true; // favoriler sekmesi değil, orijinal davranış çalışsın
            }

            __instance.currentCatgoryCode = Patch_GenTabs.FavoritesCategoryCode;

            var trav = Traverse.Create(__instance);
            var shown = trav.Field("shownHandbookPages").GetValue<List<IFlatListItem>>();
            var all = trav.Field("allHandbookPages").GetValue<List<GuiHandbookPage>>();
            var overviewGui = trav.Field("overviewGui").GetValue<GuiComposer>();
            var listHeight = trav.Field("listHeight").GetValue<double>();

            shown.Clear();
            if (all != null && FavoritesData.Instance != null)
            {
                foreach (var page in all)
                {
                    if (page.IsDuplicate) continue;
                    if (FavoritesData.Instance.IsFavorite(page.PageCode)) shown.Add(page);
                }
            }

            var stacklist = overviewGui.GetFlatList("stacklist");
            stacklist.CalcTotalHeight();
            overviewGui.GetScrollbar("scrollbar").SetHeights((float)listHeight, (float)stacklist.insideBounds.fixedHeight);

            return false; // orijinal metodu çalıştırma, işimiz bitti
        }
    }


}
