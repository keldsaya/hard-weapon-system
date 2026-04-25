using UnityEngine;
using include.stdio_h;
using include.time_h;
using System.Collections.Generic;
using include.math_h;

namespace weapon.s {
  public class g17 : MonoBehaviour, h_weapon {
    private stats_t stats;

    [Header("Specifical")]
    public float rounds_per_minute = 600f;
    [SerializeField] private type_t type;
    public caliber_t caliber;

    [Header("Values")]
    [SerializeField] private btoi console_out = true;

    private float next_shoot_time = 0f;

    void Start() {
      stats.type = type;
      stats.caliber = caliber;
    }

    /* interface implementation */
    public void shoot() {
      float fire_delay = 60f / rounds_per_minute;
      if (time.val < next_shoot_time) return;
      if (stats.mode == selector_mode_t.SAVE) return;

      if (!stats.has_chambered_round) {
        if(console_out) stdio.printf("Click..."); 
        next_shoot_time = time.val + fire_delay; 
        return;
      }

      stats.has_chambered_round = false;
      if(console_out) stdio.printf("Bang %s!", stats.chambered_round.uid); 

      next_shoot_time = time.val + fire_delay;
      reload_chamber();
    }

    public void reload() {
      extract_mag();
      magazine_t mag = new magazine_t {
        uid_t = uid_t.gen(),
        max_ammo = 17,
        ammo = new List<ammo_t>(),
        caliber = caliber
      };
      for (int i = 0; i < mag.max_ammo; i++) mag.ammo.Add(gen_ammo()); 
      insert_mag(mag);
      if(!stats.has_chambered_round) reload_chamber();
      if(console_out) stdio.printf("Reloaded");
    }

    public ammo_t reload_chamber() {
      if (stats.has_chambered_round) {
        string ammo_uid = stats.chambered_round.uid;
        ammo_t tmp = stats.chambered_round;
        stats.has_chambered_round = false;
        stats.chambered_round = default;
        if(console_out) stdio.printf("Round %s ejected from chamber", ammo_uid);
        return tmp; 
      }
      if (stats.mag != null && stats.mag.ammo.Count > 0) {
        stats.chambered_round = get_last_round_mag();
        drain_ammo();
        stats.has_chambered_round = true;
        string ammo_uid = stats.chambered_round.uid;
        if(console_out) stdio.printf("Round chambered %s", ammo_uid); 
      }
      return default;
    }

    public void insert_mag(magazine_t mag) {
      if (stats.mag != null) {
        if(console_out) stdio.printf("Insert failed: Mag present");
        return;
      }
      stats.mag = mag;
      if(console_out) stdio.printf("Magazine: %s inserted", mag.uid);
    }

    public void insert_chamber_ammo(ammo_t ammo){
      if(stats.has_chambered_round) return;
      stats.chambered_round = ammo;
      stats.has_chambered_round = true;
      if(console_out) stdio.printf("Manual chambering: Round %s inserted", ammo.uid);
    }

    public magazine_t extract_mag() {
      if(stats.mag == null) return null;
      magazine_t tmp = stats.mag;
      if(console_out) stdio.printf("Magazine: %s extracted", stats.mag.uid);
      stats.mag = null;
      return tmp;
    }

    public void set_selector(selector_mode_t mode) {
      stats.mode = mode;
      if(console_out) stdio.printf("Selected mode: %s", mode.ToString());
    }

    public selector_mode_t get_selector() => stats.mode;

    public void check_chamber() {
      if(console_out) stdio.printf("Chamber... %d %s", (int)stats.has_chambered_round, stats.has_chambered_round ? stats.chambered_round.uid : ""); 
    }

    public void check_magazine() {
      string result = weapon_handler.get_mag_count(stats.mag);
      if(console_out) stdio.printf("Magazine... %s in %s", result, stats.mag != null ? stats.mag.uid : "{no mag}"); 
    }

    public stats_t get_stats() => stats;

    /* Helpers */
    private void drain_ammo() => stats.mag.ammo.RemoveAt(stats.mag.ammo.Count - 1);
    private ammo_t get_last_round_mag() => stats.mag.ammo[stats.mag.ammo.Count - 1];
    private ammo_t gen_ammo() => new ammo_t { uid_t = uid_t.gen(), dmg = 56, penetr_power = 18, caliber = caliber };
  }
}