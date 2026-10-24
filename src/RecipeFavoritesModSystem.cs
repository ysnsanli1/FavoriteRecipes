using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;

namespace RecipeFavorites
{
    public class RecipeFavoritesModSystem : ModSystem
    {
        public const string HarmonyId = "recipefavorites.patches";
        public const string HotkeyCode = "recipefavoritestoggle";

        /// <summary>
        /// Bu bayrakların hepsi static ve oyun süreci boyunca bir kez true olur.
        ///
        /// SEBEP: Ana menüye çıkıp tekrar dünyaya girildiğinde StartClientSide
        /// yeniden çalışır. Harmony patch'leri ve kısayol tuşu kaydı ise oyun
        /// süreci boyunca kalıcıdır — bunları ikinci kez yapmaya çalışmak hata
        /// fırlatıp StartClientSide'ın geri kalanının hiç çalışmamasına ve
        /// modun tamamen ölmesine yol açıyordu.
        /// </summary>
        private static bool harmonyPatched;
        private static bool hotkeyRegistered;

        /// <summary>
        /// Kısayol tuşunun handler'ı, kaydedildiği oturumun ModSystem örneğine
        /// bağlı kalabiliyor. Bu yüzden güncel API'yi static tutuyoruz ki eski
        /// bir handler bile her zaman YAŞAYAN oturumla konuşsun.
        /// </summary>
        private static ICoreClientAPI activeCapi;

        public override bool ShouldLoad(EnumAppSide side) => side == EnumAppSide.Client;

        public override void StartClientSide(ICoreClientAPI api)
        {
            activeCapi = api;

            StarIconRenderer.ResetForNewSession();

            // Bu oturumun kayıt dosyasına bağlı yeni favori deposu
            new FavoritesData(api);

            if (!harmonyPatched)
            {
                new Harmony(HarmonyId).PatchAll(Assembly.GetExecutingAssembly());
                harmonyPatched = true;
            }

            if (!hotkeyRegistered)
            {
                api.Input.RegisterHotKeyFirst(
                    HotkeyCode,
                    "Görüntülenen tarifi favorile",
                    GlKeys.F,
                    HotkeyType.HelpAndOverlays,
                    altPressed: false,
                    ctrlPressed: false,
                    shiftPressed: true
                );
                hotkeyRegistered = true;
            }

            // Handler her oturumda yeniden bağlanır (ucuz ve güvenli).
            api.Input.SetHotKeyHandler(HotkeyCode, OnToggleFavoriteHotkey);
        }

        private static bool OnToggleFavoriteHotkey(KeyCombination comb)
        {
            var capi = activeCapi;
            if (capi == null) return false;

            string pageCode = FindOpenHandbookPageCode(capi);

            if (pageCode == null)
            {
                // Sessizce başarısız olmak yerine kullanıcıya durumu söylüyoruz,
                // böylece "hiçbir şey olmuyor" belirsizliği ortadan kalkıyor.
                capi.ShowChatMessage("Favorilemek için önce el kitabında bir sayfa açın.");
                return true;
            }

            var data = FavoritesData.Instance;
            if (data == null)
            {
                capi.ShowChatMessage("Favori listesi yüklenemedi.");
                return true;
            }

            bool nowFavorited = data.Toggle(pageCode);
            capi.ShowChatMessage(nowFavorited ? "★ Favorilere eklendi." : "☆ Favorilerden çıkarıldı.");
            return true;
        }

        /// <summary>
        /// O an açık olan el kitabı penceresini bulur ve görüntülenen detay
        /// sayfasının kodunu döndürür. Hafızada durum tutmak yerine her tuşa
        /// basışta oyunun kendisine sorduğumuz için, oturumlar arası bozulma
        /// riski yok.
        /// </summary>
        private static string FindOpenHandbookPageCode(ICoreClientAPI capi)
        {
            var dlg = capi.OpenedGuis?.FirstOrDefault(g => g is GuiDialogHandbook) as GuiDialogHandbook;
            if (dlg == null) return null;

            var history = Traverse.Create(dlg).Field("browseHistory").GetValue<Stack<BrowseHistoryElement>>();
            if (history == null || history.Count == 0) return null;

            var current = history.Peek();

            // Arama sonuçları listesindeyiz, açık bir detay sayfası yok
            if (current.SearchText != null) return null;

            return current.Page?.PageCode;
        }
    }
}
