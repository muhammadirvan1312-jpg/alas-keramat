using AlasKeramat.AI;
using AlasKeramat.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace AlasKeramat.EditorTools
{
    // Langkah 7s (T9): Sutradara Teror (+ sosok siluet, pengawas Ki Rogo, layar kedip), Penjadwal Kejadian Dunia.
    public static partial class NusantaraMasterBuilder
    {
        static void LangkahTeror()
        {
            var td = Anak(_sistem, "TerrorDirector").AddComponent<TerrorDirector>();
            var sosok = AssetDatabase.LoadAssetAtPath<GameObject>(FolderPrefabMusuh + "/Hantu_PocongHutan.prefab");
            var pengawas = AssetDatabase.LoadAssetAtPath<GameObject>(FolderPrefabMusuh + "/Musuh_PemimpinPatroli.prefab");
            Image kedip = null;
            var kanvas = _ui != null ? _ui.transform.Find("Canvas_HUD") : null;
            if (kanvas != null)
            {
                var k = UIStretch(kanvas.gameObject, "KedipLayar", Vector2.zero, Vector2.one);
                kedip = k.AddComponent<Image>();
                kedip.color = Color.black; kedip.raycastTarget = false; kedip.enabled = false;
                k.transform.SetAsLastSibling();
            }
            td.Pasang(sosok, pengawas, AmbilMat("M_Batu"), AmbilMat("M_CatMerahPudar"), kedip);
            EditorUtility.SetDirty(td);
            Anak(_sistem, "WorldEventScheduler").AddComponent<WorldEventScheduler>();
            Catat("Teror: TerrorDirector (" + TerrorDirector.Katalog().Count + " kejadian), WorldEventScheduler, 20 catatan Deden");
        }
    }
}
