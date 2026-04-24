using UnityEngine;
using include.input_h;
using include.stdio_h;
using include.time_h;
using System.Collections.Generic;
using include.math_h;
using Attributes;

namespace weapon.s {
  public class g17 : MonoBehaviour, h_weapon {
    private stats stats;

    [Header("Specifical")]
    public float rounds_per_minute = 600f;
    public type_t type;
    public calliber_t calliber;

    [Header("Values")]
    [SerializeField] private btoi console_out = true;
    [SerializeField, GreyOut] private btoi chambered = false;
    [SerializeField, GreyOut] private btoi mag_inserted = false;
    [SerializeField, GreyOut] private string ammo_in_mag = "-";
    [SerializeField, GreyOut] private selector_mode_t selector = selector_mode_t.SAVE;

    private float next_shoot_time = 0f;
    private btoi jammed;
    private magazine_t tmp_magazine;
    private ammo_t tmp_ammo;
    private btoi has_tmp_ammo;
    private btoi has_tmp_magazine;

    void Update() {
      has_tmp_magazine = tmp_magazine != null;

      /* Input handling */
      if (input.key_down(KeyCode.R)) {
        if (input.key(KeyCode.LeftShift)) {
          if (stats.mag != null) tmp_magazine = extract_mag();
          else if (tmp_magazine != null) insert_mag(tmp_magazine);
        }
        else if (input.key(KeyCode.LeftAlt)) {
          if (stats.has_chambered_round) reload_chamber();
          else if (has_tmp_ammo) insert_chamber_ammo(tmp_ammo);
        }
        else reload();
      }

      if (input.key_down(KeyCode.T)) {
        if (input.key(KeyCode.LeftAlt)) check_chamber();
        else if (input.key(KeyCode.LeftShift)) check_magazine();
      }

      if (input.key_down(KeyCode.B)) {
        selector_mode_t current = get_selector();
        if (current == selector_mode_t.SAVE) set_selector(selector_mode_t.SEMI); 
        else if (current == selector_mode_t.SEMI && type == type_t.AUTO) set_selector(selector_mode_t.AUTO); 
        else set_selector(selector_mode_t.SAVE); 
        selector = get_selector();
      }

      bool attempt_shoot = (stats.mode == selector_mode_t.AUTO) ? input.mouse(0) : input.mouse_down(0);
      if (attempt_shoot) shoot();
    }

    /* Interface Implementation */
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
      ammo_in_mag = "?";
    }

    public void reload() {
      extract_mag();
      magazine_t mag = new magazine_t {
        uid_t = uid_t.gen(),
        max_count = 17,
        ammo = new List<ammo_t>(),
        calliber = calliber
      };
      for (int i = 0; i < 17; i++) mag.ammo.Add(gen_ammo()); 
      insert_mag(mag);
      if(!stats.has_chambered_round) reload_chamber();
      if(console_out) stdio.printf("Reloaded");
    }

    public void reload_chamber() {
      if (stats.has_chambered_round) {
        tmp_ammo = stats.chambered_round;
        has_tmp_ammo = true;
        string ammo_uid = stats.chambered_round.uid;
        stats.has_chambered_round = false;
        stats.chambered_round = default;
        if(console_out) stdio.printf("Round %s ejected from chamber", ammo_uid);
      }
      if (stats.mag != null && stats.mag.ammo.Count > 0) {
        stats.chambered_round = get_last_round_mag();
        drain_ammo();
        stats.has_chambered_round = true;
        string ammo_uid = stats.chambered_round.uid;
        if(console_out) stdio.printf("Round chambered %s", ammo_uid); 
      }
      chambered = stats.has_chambered_round;
    }

    public void insert_mag(magazine_t mag) {
      if (stats.mag != null) {
        if(console_out) stdio.printf("Insert failed: Mag present");
        return;
      }
      stats.mag = mag;
      if(console_out) stdio.printf("Magazine: %s inserted", mag.uid);
      mag_inserted = true;
      ammo_in_mag = "?";
    }

    public void insert_chamber_ammo(ammo_t ammo){
      if(stats.has_chambered_round) return;
      stats.chambered_round = ammo;
      stats.has_chambered_round = true;
      if(ammo.Equals(tmp_ammo)) has_tmp_ammo = false;
      if(console_out) stdio.printf("Manual chambering: Round %s inserted", ammo.uid);
      chambered = true;
    }

    public magazine_t extract_mag() {
      if(stats.mag == null) return null;
      magazine_t tmp = stats.mag;
      if(console_out) stdio.printf("Magazine: %s extracted", stats.mag.uid);
      stats.mag = null;
      mag_inserted = false;
      ammo_in_mag = "-";
      return tmp;
    }

    public void set_selector(selector_mode_t mode) {
      stats.mode = mode;
      if(console_out) stdio.printf("Selected mode: %s", mode.ToString());
    }

    public selector_mode_t get_selector() => stats.mode;

    public void check_chamber() {
      if(console_out) stdio.printf("Chamber... %d %s", (int)stats.has_chambered_round, stats.has_chambered_round ? stats.chambered_round.uid : ""); 
      chambered = (int)stats.has_chambered_round;
    }

    public void check_magazine() {
      string result = get_mag_count();
      if(console_out) stdio.printf("Magazine... %s in %s", result, stats.mag.uid); 
      ammo_in_mag = result;
    }

    /* Helpers */
    private string get_mag_count() {
      if (stats.mag == null || stats.mag.max_count <= 0) return "No magazine";
      int current = stats.mag.ammo.Count;
      int max = stats.mag.max_count;
      if (current <= 0) return "Empty";
      if (current == max) return "Full";
      float ratio = (float)current / max;
      if (ratio > 0.85f) return "Almost full";
      if (ratio > 0.60f) return "More than half";
      if (ratio >= 0.45f && ratio <= 0.55f) return "About half";
      if (ratio > 0.15f) return "Less than half";
      return "Almost empty";
    }

    private void drain_ammo() => stats.mag.ammo.RemoveAt(stats.mag.ammo.Count - 1);
    private ammo_t get_last_round_mag() => stats.mag.ammo[stats.mag.ammo.Count - 1];
    private ammo_t gen_ammo() => new ammo_t { uid_t = uid_t.gen(), dmg = 56, penetr_power = 18 };
  }
}