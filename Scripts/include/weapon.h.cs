using System.Collections.Generic;
using include.math_h;

public struct uid_t {
  public string id;

  public static uid_t gen() {
    return new uid_t {
      id = id_generator.item_id()
    };
  }
}

namespace weapon {
  public enum selector_mode_t {
    SAVE, SEMI, AUTO
  }

  public enum calliber_t {
    _9x19, 
    _9x18, 
    _12x70, 
    _545x39, 
    _556x45, 
  }

  public struct ammo_t {
    public uid_t uid_t;
    public string uid {
      get {
        return uid_t.id; 
      }
    }
    public int dmg;
    public int penetr_power;
  }

  public class magazine_t {
    public uid_t uid_t;
    public string uid {
      get {
        return uid_t.id; 
      }
    }
    public int max_ammo;
    public List<ammo_t> ammo; 
    public calliber_t calliber;
  }

  public enum type_t {
    SEMI, AUTO, MANUAL
  }

  /* global weapon stats*/
  [System.Serializable]
  public struct stats_t {
    public btoi has_chambered_round;
    public ammo_t chambered_round;
    public magazine_t mag;
    public selector_mode_t mode;
    public type_t type;
  }
    
  public interface h_weapon {
    public void shoot();

    public void reload();
    public ammo_t reload_chamber();

    public void insert_mag(magazine_t mag);
    public void insert_chamber_ammo(ammo_t ammo);
    public magazine_t extract_mag();

    public void set_selector(selector_mode_t mode);
    public selector_mode_t get_selector();

    public void check_chamber();
    public void check_magazine();

    public stats_t get_stats();
  }
}