using System.Collections;
using Attributes;
using include.input_h;
using include.math_h;
using include.stdio_h;
using include.time_h;
using TMPro;
using UnityEngine;
using weapon;

public class weapon_handler : MonoBehaviour, h_weapon {
  /* fields: debug / knowledge */
  [Header("Knowledge Status")]
  [SerializeField, GreyOut] private btoi mag_inserted = false;
  [SerializeField, GreyOut] private string ammo_in_chamber = "?";
  [SerializeField, GreyOut] private string ammo_in_mag = "No magazine";     
  [SerializeField, GreyOut] private selector_mode_t selector = selector_mode_t.SAVE;

  [Header("References")]
  [SerializeField] private GameObject weapon_obj;

  [Header("UI")]
  [SerializeField] private TextMeshProUGUI ammo_text;
  
  /* fields: internal state */
  private h_weapon curr_weapon;
  private magazine_t tmp_magazine;
  private ammo_t tmp_ammo;

  /* properties */
  private stats_t w_stats => curr_weapon.get_stats();
  private btoi has_tmp_ammo;
  private btoi has_tmp_magazine;

  private btoi show_ammo_text;
  private btoi show_chamber_text;

  private btoi is_busy;

  private float ammo_text_alpha {
    get { return ammo_text.GetComponent<CanvasGroup>().alpha; }
    set { ammo_text.GetComponent<CanvasGroup>().alpha = value; }
  }

  /* life cycle */
  void Start() {
    if (weapon_obj != null) {
      curr_weapon = weapon_obj.GetComponent<h_weapon>();
    }
  }

  void Update() {
    if (curr_weapon == null) return;
    handle_input();
    handle_ui();
  }

  /* logic ui */
  private void handle_ui() {
    /* ammo text */
    if(ammo_text != null ) {
      if(show_ammo_text && w_stats.mag != null) {
        ammo_text.text = stdio.snprintf("%s\n%s", 
          get_mag_count(w_stats.mag), 
          get_caliber_name(w_stats.caliber));
      } 
      if(show_chamber_text) {
        ammo_text.text = stdio.snprintf("%d\n%s", 
          (int)w_stats.has_chambered_round, 
          w_stats.has_chambered_round ? get_caliber_name(w_stats.chambered_round.caliber) : "None");
      }
      float target_alpha = (show_ammo_text || show_chamber_text) ? 1f : 0f;
      ammo_text_alpha = math.lerp(ammo_text_alpha, target_alpha, time.delta * 5f); 
    }
  }

  /* logic handling */
  private void handle_input() {
    if(is_busy) return;
    /* reload manipulation (R) */
    if (input.key_down(KeyCode.R)) {
      /* mag manipulation (Shift + R) */
      if (input.key(KeyCode.LeftShift)) {
        if (w_stats.mag != null) {
          extract_mag(); 
        } else if (tmp_magazine != null) {
          insert_mag(tmp_magazine);
          tmp_magazine = null;
        }
      }
      /* chamber manipulation (Alt + R) */
      else if (input.key(KeyCode.LeftAlt)) {
        if (w_stats.has_chambered_round) {
          tmp_ammo = reload_chamber();
          has_tmp_ammo = true;
        } else if (has_tmp_ammo) {
          insert_chamber_ammo(tmp_ammo);
          has_tmp_ammo = false;
        }
      } 
      else reload();
    }

    /* inspection (T) */
    if (input.key_down(KeyCode.T)) {
      if (input.key(KeyCode.LeftAlt)) check_chamber();
      else if (input.key(KeyCode.LeftShift)) check_magazine();
    }

    /* fire mode (B) */
    if (input.key_down(KeyCode.B)) {
      cycle_selector();
    }

    /* shooting */
    bool attempt_shoot = (w_stats.mode == selector_mode_t.AUTO) ? input.mouse(0) : input.mouse_down(0);
    if (attempt_shoot && get_selector() != selector_mode_t.SAVE) {
      shoot();
    }
  }

  private void cycle_selector() {
    selector_mode_t current = get_selector();
    if (current == selector_mode_t.SAVE) set_selector(selector_mode_t.SEMI); 
    else if (current == selector_mode_t.SEMI && w_stats.type == type_t.AUTO) set_selector(selector_mode_t.AUTO); 
    else set_selector(selector_mode_t.SAVE); 
    
    selector = get_selector();
  }

  /* interface implementation (h_weapon wrappers) */
  public void shoot() { 
    curr_weapon.shoot(); 
    ammo_in_mag = "?"; 
    ammo_in_chamber = "?"; 
  }

  public void reload() { 
    curr_weapon.reload(); 
    ammo_in_mag = "Full?"; 
    ammo_in_chamber = "?"; 
    mag_inserted = true;
  }

  public ammo_t reload_chamber() {
    ammo_t ammo = curr_weapon.reload_chamber();
    ammo_in_chamber = "0"; 
    return ammo;
  }

  public void insert_mag(magazine_t mag) { 
    curr_weapon.insert_mag(mag); 
    mag_inserted = true;
    has_tmp_magazine = false;
    ammo_in_mag = "?"; 
  }

  public void insert_chamber_ammo(ammo_t ammo) { 
    curr_weapon.insert_chamber_ammo(ammo); 
    ammo_in_chamber = "1"; 
  }

  public magazine_t extract_mag() { 
    tmp_magazine = curr_weapon.extract_mag(); 
    mag_inserted = false;
    has_tmp_magazine = true;
    ammo_in_mag = "-"; 
    return tmp_magazine; 
  }

  public void set_selector(selector_mode_t mode) => curr_weapon.set_selector(mode);
  public selector_mode_t get_selector() => curr_weapon.get_selector();

  public void check_chamber() { 
    StartCoroutine(check_cham_timer());
    curr_weapon.check_chamber(); 
    ammo_in_chamber = w_stats.has_chambered_round ? "1" : "0"; 
  }

  public void check_magazine() { 
    StartCoroutine(check_mag_timer());
    curr_weapon.check_magazine(); 
    ammo_in_mag = get_mag_count(w_stats.mag); 
  }

  public stats_t get_stats() => curr_weapon.get_stats();

  /* coroutines (timers) */
  private IEnumerator check_mag_timer() {
    is_busy = true;
    show_ammo_text = true;
    yield return new WaitForSeconds(1);
    show_ammo_text = false;
    is_busy = false;
  }
  private IEnumerator check_cham_timer() {
    is_busy = true;
    show_chamber_text = true;
    yield return new WaitForSeconds(1);
    show_chamber_text = false;
    is_busy = false;
  }

  /* helping functions */
  public static string get_mag_count(magazine_t mag) {
    if (mag == null) return "No magazine";
    int current = mag.ammo.Count;
    int max = mag.max_ammo;

    if (current <= 0) return "Empty";
    if (current == max) return "Full";
    
    float ratio = (float)current / max;
    if (ratio > 0.85f) return "Almost full";
    if (ratio > 0.60f) return "More than half";
    if (ratio >= 0.45f && ratio <= 0.55f) return "About half";
    if (ratio > 0.15f) return "Less than half";
    return "Almost empty";
  }
  public static string get_caliber_name(caliber_t caliber) {
    switch(caliber) {
      case caliber_t._9x19:
        return "9x19 PARA";
      case caliber_t._9x18:
        return "9x18";
      case caliber_t._12x70:
        return "12x70";
      case caliber_t._545x39:
        return "5.45x39";
      case caliber_t._556x45:
        return "5.56x45";
      default:
        return "Indefinite";
    }
  }
}