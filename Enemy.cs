using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

// ВОПРОСЫ - РАЗОБРАТЬСЯ С ACTION, СОБЫТИЯМИ, ТВИНАМИ, _agent!!!

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _agent; // https://habr.com/ru/post/646039/

    [SerializeField]
    PlayerController _player; // игрок - за ним охотятся 

    [SerializeField]
    private float _distanceToPlayer = 10F; // дистанция, до которой нет преследования 

    // проверка - движется ли враг или он стоит на месте 
    private const float EPSILON = 0.1F;

    private float _health;

    [SerializeField]
    private float _maxHealth = 20F;

    private BoxBar _healthBar;

    // определяем тег оружия игрока, который находится на игроке 
    private readonly string PLAYER_WEAPON = "PlayerWeapon";

    // и тег самого игрока 
    private readonly string PLAYER = "Player";

    // уязвим или нет?
    private bool _isVulnerable = false;

    // мертв или нет 
    private bool _isDead = false;

    // атакует ли 
    private bool _isAttacking = false;

    // скорость атаки 
    private float _attackSpeed = 0.5F;

    // оружие - сам враг и есть оружие 
    private Weapon _weapon;

    // задаем твин для атаки 
    private Tween _attackTween; // для анимации

    // событие смерти моба
    public static System.Action OnDeath;

    [SerializeField]
    private ParticleSystem _bloodParticle; // частицы крови при ударе 

    [SerializeField]
    private Transform _respawnTransform; // точка возрождения
    private float _activeMoveRadius = 30f; // радиус, в пределах которого враг будет преследовать игрока
    private float _passiveMoveRadius = 10f; // радиус движения врага вокруг точки спавна 

    private const float RESPAWN_TIME = 20f;

    private Tween _respawnTween; // твин для респавна (пока хз, зачем он вообще)
    private Vector3 _lastMovePosition; // куда двигался враг

    private bool _isAngry; // заметил ли враг игрока

    private const float MOVE_EPSILON = 2f; // дошел ли враг до цели 

    private float _lootProbability = 0.1f; // вероятность выпадения аптечки 

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Debug.Log("enemy awake");
        // устанавливаем максимальное количество жизни 
        _health = _maxHealth;
        _healthBar = GetComponentInChildren<BoxBar>();
       // _healthBar.SetValue(_health, _maxHealth);

        // задаем оружие 
        _weapon = GetComponent<Weapon>();

        // частицы крови
        _bloodParticle = GetComponentInChildren<ParticleSystem>();

        _isAngry = false; // изначально враг неагрессивен 

        _respawnTransform = transform.parent; // точка респавна - объект родительский
        _lastMovePosition = _respawnTransform.position; // цель в начале пути - точка респавна 

        _agent.speed = 2f; // задаем скорость для всех противников
    }

    private void OnEnable() // срабатывает при запуске игрового объекта 
    {
        // делаем подписку на смерть игрока 
        PlayerController.OnDeath += OnPlayerDead;  
    }

    private void OnDisable() // отписка - нужна, если враг умирает  
    {
        PlayerController.OnDeath -= OnPlayerDead;
    }

    // если ГГ умер 
    void OnPlayerDead()
    {
        _isAttacking = false;
        _attackTween.Kill();
    }

    private bool IsNavMeshMoving
    {
        get
        {
            return _agent.velocity.magnitude > EPSILON; // двигается ли враг 
        }
    }

    void Update()
    {
        float playerDistance = Vector3.Distance(_player.transform.position, transform.position); // расстояние до игрока 
        float respawnDistance = Vector3.Distance(_respawnTransform.position, transform.position); // расстояние до точки респавна  

        // если дистанция до персонажа меньше заданной - начинаем преследование 
        if (playerDistance < _distanceToPlayer && respawnDistance < _activeMoveRadius)
        {
            _isAngry = true;
            Vector3 playerPos = _player.transform.position; // задали координаты персонажа 
            _agent.SetDestination(playerPos); // указываем врагу двигаться с помощью технологии поиска пути к месту, где стоит враг 
        } else
        {
            if (_isAngry)
            {
                _agent.SetDestination(_lastMovePosition); // если преследовать не нужно - враг возвращается на свою последнюю точку 
            }
            _isAngry = false; // больше не злимся 
            MoveRandomly(); // хаотичное движение
        }
    }

    private void MoveRandomly() // хаотичное движение
    {
        if (Vector3.Distance(_lastMovePosition, transform.position) < MOVE_EPSILON)
        {
            _lastMovePosition = GetRandomPassivePoint(); // задаем новую точку 
            _agent.SetDestination(_lastMovePosition); 
        }
    }

    private Vector3 GetRandomPassivePoint() // поиск новой случайной точки 
    {
        return new Vector3(_respawnTransform.position.x + Random.Range(0, _passiveMoveRadius), 
                            _respawnTransform.position.y,
                            _respawnTransform.position.z + Random.Range(0, _passiveMoveRadius)
            );
    }

    private void RespawnDelay()
    {
        if(_respawnTween != null)
        {
            _respawnTween.Kill();
        }
        _respawnTween = DOVirtual.DelayedCall(RESPAWN_TIME, () =>
        {
            if (Vector3.Distance(_player.transform.position, _respawnTransform.position) > _passiveMoveRadius)
            {
                Respawn();
            } else
            {
                RespawnDelay();
            }
        });
    }

    private void Respawn()
    {
        Debug.Log("enemy respawn");
        transform.position = _respawnTransform.position;
        gameObject.SetActive(true);
        _isDead = false;
        _health = _maxHealth;
        _healthBar.SetValue(_health, _maxHealth);
    }
    // столкновение двух триггеров 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == PLAYER_WEAPON) // если враг столкнулся с мечом 
        {
            GetDamage (other.gameObject.GetComponent<Weapon>().GetDamage); // враг получает урон от меча 
        }

        if (other.gameObject.tag == PLAYER && !_player.IsDead)
        {
            _isAttacking = true;
            Attack();
        } 
    }

    private void OnTriggerExit(Collider other) // прекращаем атаковать ГГ, если он вышел из коллизии 
    {
        if (other.gameObject.tag == PLAYER)
        {
            _isAttacking = false;
            _attackTween.Kill();
        }
    }

    private void Attack()
    {
        if (_isAttacking)
        {
            _player.GetDamage(_weapon.GetDamage);

            // еслизапустилась новая атака, убиваем предыдущую 
            if (_attackTween != null && _attackTween.IsActive())
            {
                _attackTween.Kill();
            }

            // выполняем атаку - потом пустой делегат - по событию OnComplete выполняем перезапуск атаки
            // корутина - выполнение последлвательных действий  
            _attackTween = DOVirtual.DelayedCall(_attackSpeed, () => {}).OnComplete(delegate 
            {
                Attack(); // рекурсия 

                Debug.Log("Attack" + name);
            });
        }
    }

    private void GetDamage(float value)
    {
        _health = Mathf.Clamp(_health - value, 0, _health);
        _healthBar.SetValue(_health, _maxHealth);

        if (_health <= 0)
        {
            Die();
        }

        BloodEffect();
    }

    // событие смерти 
    private void Die()
    {
        _isDead = true;
        _attackTween.Kill();
        gameObject.SetActive(false); // отключаем врага 

        if (OnDeath != null) // если кто то подписан на событие 
        {
            OnDeath();
        }
        DropLoot();
        RespawnDelay();
    }

    private void DropLoot() 
    {
        if (Random.value < _lootProbability)
        {
            // 
        }
    }

    private void BloodEffect() // эффект брызг крови (ахаха)
    {
        _bloodParticle.Stop(); // если уже были частицы - выключаем их 
        _bloodParticle.Play(); // запускаем частицы
    }
}
