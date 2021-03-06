using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine.UI;
using System.Linq;
using Abilities;
using System.IO;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {
    public const string ABILITY_FILE = "abilities";
    //Instance - eliminates the requirement for lookups
    public static GameController instance;


    public static Ability[] abilityList {
        get {
            if (_abilityList == null)
                _abilityList = System.Reflection.Assembly.GetAssembly(typeof(Ability)).GetTypes()
          .Where(x => typeof(Ability).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract && x != typeof(Standard))
          .Select(x => (Ability)System.Activator.CreateInstance(x)).ToArray();
            return _abilityList;
        }
    }
    static Ability[] _abilityList;

    static RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();

    public GameObject pauseMenu;

    GameStats _score;
    public static GameStats score { get { return instance._score; } }

    public float sphereRespawnCooldown = 2;

    int _active;
    public int active { get { return _active; } set { _active = value; } }

    public int spawned { get { return score.spawned; } }

    int _destroyed;
    public static int destroyed { get { return instance._destroyed; } set { instance._destroyed = value; instance._score.SpawnedSphere(); } }

    public static bool paused;

    public static Vector3 randomPositionInSphere { get { return instance.transform.position + Random.insideUnitSphere * instance.spawnRadius; } }

    public static List<SphereStats> activeSpheres { get; private set; } 

    /*Spheres with abilities*/
    public List<AbilityInfo> abilities;
    bool initialized = false;

    public Standard standard = new Standard();
    float totalSpawnValue;

    const float INCREASE_CHANCE_BY = 0.1f;
    const float BASE_CHANCE_TO_SPAWN_SPECIAL = 0.1f;
    float chanceToSpawnSpecial = BASE_CHANCE_TO_SPAWN_SPECIAL;

    float spawnRadius;


    void Awake() {
        paused = false;
    }

    AbilityInfo[] LoadAbilities() {
        return JsonUtility.FromJson<AbilityInfoList>(Resources.Load<TextAsset>(ABILITY_FILE).text).array;
    }

    public void Initialize() {
        if (initialized)
            return;
        else
            initialized = true;

        instance = this;
        activeSpheres = new List<SphereStats>();

        abilities = new List<AbilityInfo>();

        var list = LoadAbilities();

        if (list != null) {
            if (!Application.isPlaying)
                abilities.AddRange(list);
            else
                abilities.AddRange(list.Where(x => x.enabled == true).ToArray());
        } else if (!Application.isPlaying) {
            foreach (var a in abilityList)
                abilities.Add(new AbilityInfo(a, 1, true));
        } else {
            throw new System.Exception("ABILITY DATA ARE NULL");
        }

        _score = new GameStats(this, transform.root.Find("/canvas"), PlayerStats.GetAvailablePower());
    }

    void Start() {
        Initialize();
        foreach (var ability in abilities)
            totalSpawnValue += ability.chanceToSpawn;

        spawnRadius = transform.localScale.x - 0.5f;
        ChangeSeed();
        StartCoroutine("Spawn");
    }

    /// <summary>
    /// Unity GUI does not support static methods
    /// </summary>
    public void uGUIPause() {
        Pause();
    }

    public static void Pause() {
        paused = !paused;
        instance.pauseMenu.SetActive(paused);
        Time.timeScale = paused ? 0 : 1;
        RenderSettings.fogDensity = paused ? 0.16f : 0;
    }

    Ability GetRandomAbility(List<AbilityInfo> al, ref float spawnValue) {
        var rand = Random.Range(0, spawnValue);
        float currentValue = 0;
        for (int i = 0; i < al.Count; i++) {
            var a = al[i];
            currentValue += a.chanceToSpawn;
            if (currentValue >= rand) {
                spawnValue -= a.chanceToSpawn;
                al.RemoveAt(i);
                return a.ability;
            }
        }
        return null;
    }

    IEnumerator Spawn() {
        while(true) {
            yield return new WaitForSeconds(sphereRespawnCooldown);
            if (!paused) {
                score.SpawnedSphere();
                GameObject g = Instantiate(GlobalManager.defaultSphere, randomPositionInSphere, new Quaternion());
                SphereStats s = g.GetComponent<SphereStats>();
                activeSpheres.Add(s);
                int value = 100;
                if (Random.value <= chanceToSpawnSpecial) {
                    float abilityChance = 1f;
                    List<AbilityInfo> ab = new List<AbilityInfo>(abilities);
                    float spawnValue = totalSpawnValue;

                    while (ab.Count > 0 && score.power > value) {
                        if (Random.value <= abilityChance) {
                            value += 50;
                            Ability a = GetRandomAbility(ab, ref spawnValue);
                            s.AddAbility(a);
                            if (a.GetType() == typeof(Parasite))
                                break;
                        } else break;
                        abilityChance /= 4;
                    }
                    chanceToSpawnSpecial = BASE_CHANCE_TO_SPAWN_SPECIAL;
                } else {
                    s.AddAbility(standard);
                    chanceToSpawnSpecial += INCREASE_CHANCE_BY;
                }
                score.AddPowerInstant(-value);
            }
        };
        
    }

    IEnumerator RestartIn() {
        yield return new WaitForSeconds(2);
        while (Input.touchCount == 0 || Input.GetMouseButtonDown(0)) yield return new WaitForFixedUpdate();
        Restart();
    }

    void Restart() {
        ChangeSeed();
        destroyed = 0;
        sphereRespawnCooldown = 2;
        _score = new GameStats(this, transform.root.Find("/canvas"), 0);
    }

    void ChangeSeed() {
        byte[] data = new byte[4];
        rng.GetBytes(data);
        Random.InitState(System.BitConverter.ToInt32(data, 0));
    }

    public static void Pop(SphereStats stats) {
        activeSpheres.Remove(stats);
        instance._score.AddPower(stats.Pop() + GlobalManager.bonusManager.CalculateBonus(stats));
        if(activeSpheres.Count == 0 && score.power < 100) {
            //todo end game
        }
    }
}

