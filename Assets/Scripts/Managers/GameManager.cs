using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : Singleton<GameManager>
{
    [Header("Configuration"), SerializeField] private uint startingLife = 10;
    [SerializeField] private ulong startingMoney = 150;
    [SerializeField] private Map[] maps;
    [SerializeField] private Tower[] towers;
    [SerializeField] private float gameOverDelay = 1f;

    [Header("UI"), SerializeField] private CanvasGroup mainUI;
    [SerializeField] private GameOverPanel gameOverPanel;

    public ulong Score { get; private set; }
    public ulong Highscore { get; private set; }
    public ulong Money { get; private set; }
    public uint Life { get; private set; }
    public uint Wave { get; private set; }
    public int EnemyAmount => _enemyList.Count;
    public float WaveTimer { get; private set; } = -1;
    public float EarlyWaveMoneyBonus => Mathf.Floor(Mathf.Max(WaveTimer / 2f, 0));

    public float DPS => CurrentMap.SpawnedTowerList.Count != 0
        ? CurrentMap.SpawnedTowerList
            .Sum(x => x.Value.Reports.DPS)
        : 0;

    public ulong Kills { get; private set; }

    public IReadOnlyList<Map> MapList => maps;
    public IReadOnlyList<Tower> TowerList => towers;
    public IReadOnlyList<Enemy> EnemyList => _enemyList;
    public Map CurrentMap => _map;

    private List<Enemy> _enemyList = new();

    private Map _map;
    private Coroutine _enemyCo;
    private Camera _sceneCamera;
    private Vector3 _mousePosInitial = Vector3.zero;
    private bool _isGameOver;

    public event Action<float> GameOver;
    
    // Start is called before the first frame update
    private void Start()
    {
        Life = startingLife;
        Money = startingMoney;
        Score = 0;
        Wave = 0;
        Highscore = ulong.Parse(PlayerPrefs.GetString("Highscore", "0"));
        
        Enemy.Death += EnemyOnDeath;
        Enemy.ReachedBase += EnemyOnReachedBase;
        
        _sceneCamera = Camera.main;

        _map = Instantiate(maps[Random.Range(0, maps.Length)].gameObject, Vector3.zero, Quaternion.identity).GetComponent<Map>();
    }

    private void EnemyOnDeath(Enemy e)
    {
        Score += Convert.ToUInt64(Mathf.CeilToInt(e.MaxHealth));
        Money += Convert.ToUInt64(Mathf.CeilToInt(e.Bounty));
        Kills++;
        _enemyList.Remove(e);
    }

    private void EnemyOnReachedBase(Enemy e, uint lifecost)
    {
        Life -= lifecost;
        _enemyList.Remove(e);
    }

    // Update is called once per frame
    private void Update()
    {
        if (_isGameOver) return;
        if (Life <= 0)
        {
            //Game Over
            GameOver?.Invoke(gameOverDelay);
            mainUI.interactable = false;
            mainUI.blocksRaycasts = false;
            mainUI.DOFade(0f, gameOverDelay).SetUpdate(true);
            DOTween.To(() => Time.timeScale, value => Time.timeScale = value, 0.1f, gameOverDelay)
                .SetUpdate(true).onComplete += () =>
            {
                Time.timeScale = 1;
                gameOverPanel.ShowPanel(Score, Wave, 0, 0);
                if (Score > Highscore) PlayerPrefs.SetString("Highscore", Score.ToString());
            };
            _isGameOver = true;
            return; 
        }
        
        if (WaveTimer > 0) WaveTimer -= Time.deltaTime;
        else if (WaveTimer > -1) NextWave();
        
        if (Input.GetMouseButtonDown(1))
            _mousePosInitial = _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

        if (Input.GetMouseButton(1))
        {
            var dir = _mousePosInitial - _sceneCamera.ScreenToWorldPoint(Input.mousePosition);

            _sceneCamera.transform.position += dir;
        }
        
        _enemyList.Sort();
    }

    private IEnumerator StartEnemySpawn()
    {
        WaveTimer = -2;
        yield return _map.SpawnEnemy(Wave, (e) => _enemyList.Add(e));
        
        WaveTimer = _map.GetSpawnTiming(Wave).amount * _map.GetSpawnTiming(Wave).waitTimeMultiplier;
        _enemyCo = null;
    }

    public void NextWave()
    {
        if (Math.Abs(WaveTimer - (-2)) < 0.001f) return;
        if (_enemyCo != null) StopCoroutine(_enemyCo);
        Money += (ulong)Mathf.RoundToInt(EarlyWaveMoneyBonus);
        Wave++;
        if (_map.ExpandThisWave(Wave))
            _map.ExpandMap(Wave);
        
        _enemyCo = StartCoroutine(StartEnemySpawn());
    }

    public bool Purchase(ulong amount)
    {
        if (Money < amount) return false;
        Money -= amount;
        return true;
    }
    
    public void AddMoney(ulong amount)
    {
        Money += amount;
    }
}
