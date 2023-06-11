using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using DG.Tweening;

// ������� - ����������� � ACTION, ���������, �������, _agent!!!

public class Enemy : MonoBehaviour
{
    private NavMeshAgent _agent; // https://habr.com/ru/post/646039/

    [SerializeField]
    PlayerController _player; // ����� - �� ��� �������� 

    [SerializeField]
    private float _distanceToPlayer = 10F; // ���������, �� ������� ��� ������������� 

    // �������� - �������� �� ���� ��� �� ����� �� ����� 
    private const float EPSILON = 0.1F;

    private float _health;

    [SerializeField]
    private float _maxHealth = 20F;

    private BoxBar _healthBar;

    // ���������� ��� ������ ������, ������� ��������� �� ������ 
    private readonly string PLAYER_WEAPON = "PlayerWeapon";

    // � ��� ������ ������ 
    private readonly string PLAYER = "Player";

    // ������ ��� ���?
    private bool _isVulnerable = false;

    // ����� ��� ��� 
    private bool _isDead = false;

    // ������� �� 
    private bool _isAttacking = false;

    // �������� ����� 
    private float _attackSpeed = 0.5F;

    // ������ - ��� ���� � ���� ������ 
    private Weapon _weapon;

    // ������ ���� ��� ����� 
    private Tween _attackTween; // ��� ��������

    // ������� ������ ����
    public static System.Action OnDeath;

    [SerializeField]
    private ParticleSystem _bloodParticle; // ������� ����� ��� ����� 

    [SerializeField]
    private Transform _respawnTransform; // ����� �����������
    private float _activeMoveRadius = 30f; // ������, � �������� �������� ���� ����� ������������ ������
    private float _passiveMoveRadius = 10f; // ������ �������� ����� ������ ����� ������ 

    private const float RESPAWN_TIME = 20f;

    private Tween _respawnTween; // ���� ��� �������� (���� ��, ����� �� ������)
    private Vector3 _lastMovePosition; // ���� �������� ����

    private bool _isAngry; // ������� �� ���� ������

    private const float MOVE_EPSILON = 2f; // ����� �� ���� �� ���� 

    private float _lootProbability = 0.1f; // ����������� ��������� ������� 

    void Start()
    {
        _agent = GetComponent<NavMeshAgent>();
        Debug.Log("enemy awake");
        // ������������� ������������ ���������� ����� 
        _health = _maxHealth;
        _healthBar = GetComponentInChildren<BoxBar>();
       // _healthBar.SetValue(_health, _maxHealth);

        // ������ ������ 
        _weapon = GetComponent<Weapon>();

        // ������� �����
        _bloodParticle = GetComponentInChildren<ParticleSystem>();

        _isAngry = false; // ���������� ���� ������������ 

        _respawnTransform = transform.parent; // ����� �������� - ������ ������������
        _lastMovePosition = _respawnTransform.position; // ���� � ������ ���� - ����� �������� 

        _agent.speed = 2f; // ������ �������� ��� ���� �����������
    }

    private void OnEnable() // ����������� ��� ������� �������� ������� 
    {
        // ������ �������� �� ������ ������ 
        PlayerController.OnDeath += OnPlayerDead;  
    }

    private void OnDisable() // ������� - �����, ���� ���� �������  
    {
        PlayerController.OnDeath -= OnPlayerDead;
    }

    // ���� �� ���� 
    void OnPlayerDead()
    {
        _isAttacking = false;
        _attackTween.Kill();
    }

    private bool IsNavMeshMoving
    {
        get
        {
            return _agent.velocity.magnitude > EPSILON; // ��������� �� ���� 
        }
    }

    void Update()
    {
        float playerDistance = Vector3.Distance(_player.transform.position, transform.position); // ���������� �� ������ 
        float respawnDistance = Vector3.Distance(_respawnTransform.position, transform.position); // ���������� �� ����� ��������  

        // ���� ��������� �� ��������� ������ �������� - �������� ������������� 
        if (playerDistance < _distanceToPlayer && respawnDistance < _activeMoveRadius)
        {
            _isAngry = true;
            Vector3 playerPos = _player.transform.position; // ������ ���������� ��������� 
            _agent.SetDestination(playerPos); // ��������� ����� ��������� � ������� ���������� ������ ���� � �����, ��� ����� ���� 
        } else
        {
            if (_isAngry)
            {
                _agent.SetDestination(_lastMovePosition); // ���� ������������ �� ����� - ���� ������������ �� ���� ��������� ����� 
            }
            _isAngry = false; // ������ �� ������ 
            MoveRandomly(); // ��������� ��������
        }
    }

    private void MoveRandomly() // ��������� ��������
    {
        if (Vector3.Distance(_lastMovePosition, transform.position) < MOVE_EPSILON)
        {
            _lastMovePosition = GetRandomPassivePoint(); // ������ ����� ����� 
            _agent.SetDestination(_lastMovePosition); 
        }
    }

    private Vector3 GetRandomPassivePoint() // ����� ����� ��������� ����� 
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
    // ������������ ���� ��������� 
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == PLAYER_WEAPON) // ���� ���� ���������� � ����� 
        {
            GetDamage (other.gameObject.GetComponent<Weapon>().GetDamage); // ���� �������� ���� �� ���� 
        }

        if (other.gameObject.tag == PLAYER && !_player.IsDead)
        {
            _isAttacking = true;
            Attack();
        } 
    }

    private void OnTriggerExit(Collider other) // ���������� ��������� ��, ���� �� ����� �� �������� 
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

            // ��������������� ����� �����, ������� ���������� 
            if (_attackTween != null && _attackTween.IsActive())
            {
                _attackTween.Kill();
            }

            // ��������� ����� - ����� ������ ������� - �� ������� OnComplete ��������� ���������� �����
            // �������� - ���������� ���������������� ��������  
            _attackTween = DOVirtual.DelayedCall(_attackSpeed, () => {}).OnComplete(delegate 
            {
                Attack(); // �������� 

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

    // ������� ������ 
    private void Die()
    {
        _isDead = true;
        _attackTween.Kill();
        gameObject.SetActive(false); // ��������� ����� 

        if (OnDeath != null) // ���� ��� �� �������� �� ������� 
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

    private void BloodEffect() // ������ ����� ����� (�����)
    {
        _bloodParticle.Stop(); // ���� ��� ���� ������� - ��������� �� 
        _bloodParticle.Play(); // ��������� �������
    }
}
