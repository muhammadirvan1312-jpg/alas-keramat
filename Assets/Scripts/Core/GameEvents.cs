using System;
using UnityEngine;

namespace AlasKeramat.Core
{
    // Event statis bersama (docs/tech/11 bagian 4). Semua Raise aman bila tanpa pendengar.
    public static class GameEvents
    {
        public static event Action<ItemID, int> OnItemChanged;
        public static event Action<int, TreeKind> OnTreeFelled;
        public static event Action<EnemyArchetype, bool> OnEnemyKilled;
        public static event Action<QuestID> OnQuestAdvanced;
        public static event Action<int> OnHourChanged;
        public static event Action<int> OnDayChanged;
        public static event Action OnMaghrib;
        public static event Action OnMalamDarahStart;
        public static event Action OnMalamDarahEnd;
        public static event Action<WeatherState> OnWeatherChanged;
        public static event Action<LoreID> OnLoreFound;
        public static event Action<DamageInfo> OnPlayerDamaged;
        public static event Action<BlueprintID> OnStructureBuilt;
        public static event Action<UndergroundId> OnEnterUnderground;
        public static event Action OnExitUnderground;
        public static event Action<string> OnSubtitle;
        public static event Action<string> OnToast;
        public static event Action<TerrorPhase> OnTerrorPhaseChanged;
        public static event Action<int> OnTerrorEvent;
        public static event Action<int> OnWorldEvent;
        public static event Action<EnemyArchetype, int> OnLimbSevered;
        public static event Action<StatusEffect> OnStatusChanged;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatis()
        {
            OnItemChanged = null; OnTreeFelled = null; OnEnemyKilled = null; OnQuestAdvanced = null;
            OnHourChanged = null; OnDayChanged = null; OnMaghrib = null; OnMalamDarahStart = null; OnMalamDarahEnd = null;
            OnWeatherChanged = null; OnLoreFound = null; OnPlayerDamaged = null; OnStructureBuilt = null;
            OnEnterUnderground = null; OnExitUnderground = null; OnSubtitle = null; OnToast = null;
            OnTerrorPhaseChanged = null; OnTerrorEvent = null; OnWorldEvent = null; OnLimbSevered = null; OnStatusChanged = null;
        }

        static void Aman(Action a, string nama)
        {
            try { a(); }
            catch (Exception e) { Debug.LogError("[AK][GameEvents] Pendengar " + nama + " melempar error: " + e); }
        }

        public static void RaiseItemChanged(ItemID id, int n) { var h = OnItemChanged; if (h != null) Aman(() => h(id, n), "OnItemChanged"); }
        public static void RaiseTreeFelled(int treeId, TreeKind k) { var h = OnTreeFelled; if (h != null) Aman(() => h(treeId, k), "OnTreeFelled"); }
        public static void RaiseEnemyKilled(EnemyArchetype a, bool spared) { var h = OnEnemyKilled; if (h != null) Aman(() => h(a, spared), "OnEnemyKilled"); }
        public static void RaiseQuestAdvanced(QuestID q) { var h = OnQuestAdvanced; if (h != null) Aman(() => h(q), "OnQuestAdvanced"); }
        public static void RaiseHourChanged(int jam) { var h = OnHourChanged; if (h != null) Aman(() => h(jam), "OnHourChanged"); }
        public static void RaiseDayChanged(int hari) { var h = OnDayChanged; if (h != null) Aman(() => h(hari), "OnDayChanged"); }
        public static void RaiseMaghrib() { var h = OnMaghrib; if (h != null) Aman(h, "OnMaghrib"); }
        public static void RaiseMalamDarahStart() { var h = OnMalamDarahStart; if (h != null) Aman(h, "OnMalamDarahStart"); }
        public static void RaiseMalamDarahEnd() { var h = OnMalamDarahEnd; if (h != null) Aman(h, "OnMalamDarahEnd"); }
        public static void RaiseWeatherChanged(WeatherState w) { var h = OnWeatherChanged; if (h != null) Aman(() => h(w), "OnWeatherChanged"); }
        public static void RaiseLoreFound(LoreID l) { var h = OnLoreFound; if (h != null) Aman(() => h(l), "OnLoreFound"); }
        public static void RaisePlayerDamaged(DamageInfo d) { var h = OnPlayerDamaged; if (h != null) Aman(() => h(d), "OnPlayerDamaged"); }
        public static void RaiseStructureBuilt(BlueprintID b) { var h = OnStructureBuilt; if (h != null) Aman(() => h(b), "OnStructureBuilt"); }
        public static void RaiseEnterUnderground(UndergroundId u) { var h = OnEnterUnderground; if (h != null) Aman(() => h(u), "OnEnterUnderground"); }
        public static void RaiseExitUnderground() { var h = OnExitUnderground; if (h != null) Aman(h, "OnExitUnderground"); }
        public static void RaiseSubtitle(string s) { var h = OnSubtitle; if (h != null) Aman(() => h(s), "OnSubtitle"); }
        public static void RaiseToast(string s) { var h = OnToast; if (h != null) Aman(() => h(s), "OnToast"); }
        public static void RaiseTerrorPhaseChanged(TerrorPhase p) { var h = OnTerrorPhaseChanged; if (h != null) Aman(() => h(p), "OnTerrorPhaseChanged"); }
        public static void RaiseTerrorEvent(int id) { var h = OnTerrorEvent; if (h != null) Aman(() => h(id), "OnTerrorEvent"); }
        public static void RaiseWorldEvent(int id) { var h = OnWorldEvent; if (h != null) Aman(() => h(id), "OnWorldEvent"); }
        public static void RaiseLimbSevered(EnemyArchetype a, int bone) { var h = OnLimbSevered; if (h != null) Aman(() => h(a, bone), "OnLimbSevered"); }
        public static void RaiseStatusChanged(StatusEffect s) { var h = OnStatusChanged; if (h != null) Aman(() => h(s), "OnStatusChanged"); }
    }
}
