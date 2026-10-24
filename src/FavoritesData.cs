using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Vintagestory.API.Client;

namespace RecipeFavorites
{
    /// <summary>
    /// Favorilenen handbook sayfalarının (PageCode) diske kaydedilip okunmasından sorumlu.
    /// Her kayıt (savegame) için ayrı bir JSON dosyası tutulur, böylece farklı dünyalarda
    /// farklı favori listeleri olabilir.
    /// </summary>
    public class FavoritesData
    {
        public static FavoritesData Instance { get; private set; }


        private readonly ICoreClientAPI capi;
        private readonly string filePath;
        private HashSet<string> favoritePageCodes = new HashSet<string>();

        public FavoritesData(ICoreClientAPI capi)
        {
            this.capi = capi;
            Instance = this;

            string dir = capi.GetOrCreateDataPath(Path.Combine("ModData", "recipefavorites"));
            string rawName = capi.World.SavegameIdentifier ?? "default";
            char[] invalid = Path.GetInvalidFileNameChars();
            string safeName = new string(rawName.Where(c => !invalid.Contains(c)).ToArray());
            if (string.IsNullOrEmpty(safeName)) safeName = "default";

            filePath = Path.Combine(dir, safeName + ".json");
            Load();
        }

        public bool IsFavorite(string pageCode)
        {
            return pageCode != null && favoritePageCodes.Contains(pageCode);
        }

        /// <summary>
        /// Favori durumunu tersine çevirir. true dönerse eklendi, false dönerse çıkarıldı demektir.
        /// </summary>
        public bool Toggle(string pageCode)
        {
            if (pageCode == null) return false;

            bool nowFavorited;
            if (favoritePageCodes.Contains(pageCode))
            {
                favoritePageCodes.Remove(pageCode);
                nowFavorited = false;
            }
            else
            {
                favoritePageCodes.Add(pageCode);
                nowFavorited = true;
            }

            Save();
            return nowFavorited;
        }

        private void Load()
        {
            try
            {
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    var list = JsonConvert.DeserializeObject<List<string>>(json);
                    if (list != null) favoritePageCodes = new HashSet<string>(list);
                }
            }
            catch (Exception e)
            {
                capi.Logger.Warning("[RecipeFavorites] Favoriler dosyası okunamadı: " + e.Message);
            }
        }

        private void Save()
        {
            try
            {
                File.WriteAllText(filePath, JsonConvert.SerializeObject(favoritePageCodes.ToList()));
            }
            catch (Exception e)
            {
                capi.Logger.Warning("[RecipeFavorites] Favoriler kaydedilemedi: " + e.Message);
            }
        }
    }
}
